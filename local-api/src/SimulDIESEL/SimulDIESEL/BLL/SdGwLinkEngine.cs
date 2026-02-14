using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL
{
    public sealed class SdGwLinkEngine
    {
        public sealed class Config
        {
            public byte CmdAck { get; set; } = 0xF1;
            public byte CmdErr { get; set; } = 0xF2;

            public byte FlagAckReq { get; set; } = 0x01;
            public byte FlagIsEvt { get; set; } = 0x02;

            public int MaxRawFrameLen { get; set; } = 250;
            public bool DeliverAckErrToApp { get; set; } = false;
        }

        public readonly struct AppFrame
        {
            public readonly byte Cmd;
            public readonly byte Flags;
            public readonly byte Seq;
            public readonly byte[] Payload;

            public AppFrame(byte cmd, byte flags, byte seq, byte[] payload)
            {
                Cmd = cmd;
                Flags = flags;
                Seq = seq;
                Payload = payload ?? new byte[0];
            }
        }

        public enum SendOutcome
        {
            Enqueued,
            Acked,
            Nacked,
            Timeout,
            TransportDown,
            Busy
        }

        public sealed class SendOptions
        {
            public bool RequireAck { get; set; }
            public int TimeoutMs { get; set; } = 100;
            public int MaxRetries { get; set; } = 0;
            public bool IsEvent { get; set; }
            public byte AdditionalFlags { get; set; } = 0;
        }

        private readonly Config _cfg;
        private readonly Func<byte[], bool> _write;

        private readonly List<byte> _rx = new List<byte>(512);

        private byte _txSeq;

        private bool _hasLastRxSeq;
        private byte _lastRxSeq;

        private readonly object _swSync = new object();
        private bool _waitAck;
        private byte _waitSeq;
        private byte[] _lastTxFrame;
        private int _retriesLeft;
        private int _timeoutMs;
        private Timer _ackTimer;
        private TaskCompletionSource<SendOutcome> _tcs;

        public event Action<AppFrame> AppFrameReceived;
        public event Action<string> ProtocolError;
        public event Action<byte, SendOutcome> SendCompleted;

        // NOVO (opcional): indica falha física do transporte para log/ações externas
        public event Action<string> TransportFault;

        public SdGwLinkEngine(Config cfg, Func<byte[], bool> write)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _write = write ?? throw new ArgumentNullException(nameof(write));
        }

        // ======================================================
        // NOVO: chame isto quando a DAL/Serial cair (cabo puxado)
        // ======================================================
        public void OnTransportDown(string reason = null)
        {
            if (!string.IsNullOrEmpty(reason))
                TransportFault?.Invoke(reason);

            lock (_swSync)
            {
                // encerra qualquer stop-and-wait pendente imediatamente
                if (_waitAck)
                    CompleteWait_NoLock(_waitSeq, SendOutcome.TransportDown);
            }

            // limpa RX buffer (evita lixo acumulado)
            _rx.Clear();
        }

        // =========================
        // RX: Alimenta bytes do stream
        // =========================
        public void OnBytesReceived(byte[] chunk)
        {
            if (chunk == null || chunk.Length == 0) return;

            for (int i = 0; i < chunk.Length; i++)
            {
                byte b = chunk[i];

                if (b != 0x00)
                {
                    _rx.Add(b);

                    if (_rx.Count > _cfg.MaxRawFrameLen + 16)
                    {
                        _rx.Clear();
                        ProtocolError?.Invoke("RX overflow: frame sem delimitador 0x00 (descartado).");
                    }
                    continue;
                }

                if (_rx.Count == 0) continue;

                var encoded = _rx.ToArray();
                _rx.Clear();

                byte[] decoded;
                try
                {
                    decoded = CobsDecode(encoded);
                }
                catch (Exception ex)
                {
                    ProtocolError?.Invoke("COBS decode falhou: " + ex.Message);
                    continue;
                }

                if (decoded.Length < 4)
                {
                    ProtocolError?.Invoke("Frame curto (descartado).");
                    continue;
                }

                byte crcRx = decoded[decoded.Length - 1];
                byte crcCalc = Crc8Atm(decoded, 0, decoded.Length - 1);

                if (crcRx != crcCalc)
                {
                    ProtocolError?.Invoke("CRC inválido (descartado).");
                    continue;
                }

                byte cmd = decoded[0];
                byte flags = decoded[1];
                byte seq = decoded[2];
                int payloadLen = decoded.Length - 4;
                byte[] payload = payloadLen > 0 ? Slice(decoded, 3, payloadLen) : new byte[0];

                if (cmd == _cfg.CmdAck)
                {
                    HandleAck(seq);
                    if (_cfg.DeliverAckErrToApp)
                        AppFrameReceived?.Invoke(new AppFrame(cmd, flags, seq, payload));
                    continue;
                }
                if (cmd == _cfg.CmdErr)
                {
                    HandleErr(seq);
                    if (_cfg.DeliverAckErrToApp)
                        AppFrameReceived?.Invoke(new AppFrame(cmd, flags, seq, payload));
                    continue;
                }

                bool ackReq = (flags & _cfg.FlagAckReq) != 0;

                if (ackReq)
                {
                    if (_hasLastRxSeq && seq == _lastRxSeq)
                    {
                        SendTransportAck(seq);
                        continue;
                    }

                    _hasLastRxSeq = true;
                    _lastRxSeq = seq;

                    SendTransportAck(seq);
                }

                AppFrameReceived?.Invoke(new AppFrame(cmd, flags, seq, payload));
            }
        }

        // =========================
        // TX: Enviar frame de aplicação
        // =========================
        public Task<SendOutcome> SendAsync(byte cmd, byte[] payload, SendOptions opt)
        {
            if (opt == null) opt = new SendOptions();
            if (payload == null) payload = new byte[0];

            byte flags = opt.AdditionalFlags;
            if (opt.IsEvent) flags |= _cfg.FlagIsEvt;
            if (opt.RequireAck) flags |= _cfg.FlagAckReq;

            if (payload.Length + 4 > _cfg.MaxRawFrameLen)
                throw new ArgumentException("Payload excede MTU do frame cru (250).");

            if (!opt.RequireAck)
            {
                byte seq = NextTxSeq();
                var stream = BuildStreamFrame(cmd, flags, seq, payload);
                bool ok = _write(stream);
                return Task.FromResult(ok ? SendOutcome.Enqueued : SendOutcome.TransportDown);
            }

            lock (_swSync)
            {
                if (_waitAck) return Task.FromResult(SendOutcome.Busy);

                byte seq = NextTxSeq();
                var stream = BuildStreamFrame(cmd, flags, seq, payload);

                _waitAck = true;
                _waitSeq = seq;
                _lastTxFrame = stream;
                _retriesLeft = opt.MaxRetries;
                _timeoutMs = Math.Max(1, opt.TimeoutMs);

                _tcs = new TaskCompletionSource<SendOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
                var localTcs = _tcs; // <<< FIX: captura antes de qualquer CompleteWait

                bool ok = _write(stream);
                if (!ok)
                {
                    CompleteWait_NoLock(seq, SendOutcome.TransportDown);
                    return localTcs.Task; // <<< retorna a Task válida (não _tcs)
                }

                if (_ackTimer != null) _ackTimer.Dispose();
                _ackTimer = new Timer(AckTimeoutTick, null, _timeoutMs, Timeout.Infinite);

                return localTcs.Task;
            }
        }

        // =========================
        // Internos: ACK/ERR/Timeout
        // =========================
        private void HandleAck(byte seq)
        {
            lock (_swSync)
            {
                if (!_waitAck) return;
                if (seq != _waitSeq) return;

                CompleteWait_NoLock(seq, SendOutcome.Acked);
            }
        }

        private void HandleErr(byte seq)
        {
            lock (_swSync)
            {
                if (!_waitAck) return;
                if (seq != _waitSeq) return;

                CompleteWait_NoLock(seq, SendOutcome.Nacked);
            }
        }

        private void AckTimeoutTick(object _)
        {
            lock (_swSync)
            {
                if (!_waitAck) return;

                if (_retriesLeft > 0)
                {
                    _retriesLeft--;

                    bool ok = _write(_lastTxFrame);
                    if (!ok)
                    {
                        CompleteWait_NoLock(_waitSeq, SendOutcome.TransportDown);
                        return;
                    }

                    if (_ackTimer != null) _ackTimer.Change(_timeoutMs, Timeout.Infinite);
                    return;
                }

                CompleteWait_NoLock(_waitSeq, SendOutcome.Timeout);
            }
        }

        // >>> IMPORTANTE: este método assume que já está dentro de lock(_swSync)
        // >>> IMPORTANTE: este método assume que já está dentro de lock(_swSync)
        private void CompleteWait_NoLock(byte seq, SendOutcome outcome)
        {
            if (_ackTimer != null)
            {
                _ackTimer.Dispose();
                _ackTimer = null;
            }

            _waitAck = false;

            var localTcs = _tcs;
            _tcs = null;

            // capture o delegate para invocar fora do lock
            var sendCompleted = SendCompleted;

            if (localTcs != null)
                localTcs.TrySetResult(outcome);

            // >>> NÃO invoque evento dentro do lock:
            // SendCompleted?.Invoke(seq, outcome);

            // Invoca fora do lock usando Task (não bloqueia e evita reentrância no lock)
            if (sendCompleted != null)
                Task.Run(() => sendCompleted(seq, outcome));
        }


        private void SendTransportAck(byte seq)
        {
            var stream = BuildStreamFrame(_cfg.CmdAck, 0x00, seq, new byte[0]);
            _write(stream);
        }

        private byte NextTxSeq()
        {
            unchecked { _txSeq++; }
            return _txSeq;
        }

        private byte[] BuildStreamFrame(byte cmd, byte flags, byte seq, byte[] payload)
        {
            int n = 3 + payload.Length + 1;
            var raw = new byte[n];
            raw[0] = cmd;
            raw[1] = flags;
            raw[2] = seq;

            if (payload.Length > 0)
                Buffer.BlockCopy(payload, 0, raw, 3, payload.Length);

            raw[n - 1] = Crc8Atm(raw, 0, n - 1);

            var enc = CobsEncode(raw);
            var stream = new byte[enc.Length + 1];
            Buffer.BlockCopy(enc, 0, stream, 0, enc.Length);
            stream[stream.Length - 1] = 0x00;
            return stream;
        }

        private static byte[] Slice(byte[] src, int offset, int len)
        {
            var dst = new byte[len];
            Buffer.BlockCopy(src, offset, dst, 0, len);
            return dst;
        }

        public static byte Crc8Atm(byte[] data, int offset, int len)
        {
            byte crc = 0x00;
            for (int i = 0; i < len; i++)
            {
                crc ^= data[offset + i];
                for (int b = 0; b < 8; b++)
                {
                    bool msb = (crc & 0x80) != 0;
                    crc <<= 1;
                    if (msb) crc ^= 0x07;
                }
            }
            return crc;
        }

        public static byte[] CobsEncode(byte[] input)
        {
            if (input == null) return new byte[0];

            var output = new List<byte>(input.Length + 2);
            int codeIndex = 0;
            byte code = 1;

            output.Add(0);

            for (int i = 0; i < input.Length; i++)
            {
                byte b = input[i];
                if (b == 0)
                {
                    output[codeIndex] = code;
                    codeIndex = output.Count;
                    output.Add(0);
                    code = 1;
                }
                else
                {
                    output.Add(b);
                    code++;
                    if (code == 0xFF)
                    {
                        output[codeIndex] = code;
                        codeIndex = output.Count;
                        output.Add(0);
                        code = 1;
                    }
                }
            }

            output[codeIndex] = code;
            return output.ToArray();
        }

        public static byte[] CobsDecode(byte[] input)
        {
            if (input == null || input.Length == 0) return new byte[0];

            var output = new List<byte>(input.Length);
            int i = 0;

            while (i < input.Length)
            {
                byte code = input[i];
                if (code == 0) throw new InvalidOperationException("COBS inválido (code=0).");
                i++;

                int copyLen = code - 1;
                if (i + copyLen > input.Length) throw new InvalidOperationException("COBS inválido (overflow).");

                for (int j = 0; j < copyLen; j++)
                    output.Add(input[i++]);

                if (code != 0xFF && i < input.Length)
                    output.Add(0x00);
            }

            return output.ToArray();
        }
    }
}
