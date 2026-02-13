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
            // >>> PREENCHA com a sua spec quando quiser (para não chutar)
            public byte CmdAck { get; set; } = 0x06;     // EXEMPLO (não obrigatório)
            public byte CmdErr { get; set; } = 0x15;     // EXEMPLO (não obrigatório)

            public byte FlagAckReq { get; set; } = 0x01; // EXEMPLO (ACK_REQ bit)
            public byte FlagIsEvt { get; set; } = 0x02; // EXEMPLO (IS_EVT bit)

            public int MaxRawFrameLen { get; set; } = 250; // MTU do frame cru
            public bool DeliverAckErrToApp { get; set; } = false; // diagnóstico
        }

        public readonly struct AppFrame
        {
            public readonly byte Cmd;
            public readonly byte Flags;
            public readonly byte Seq;
            public readonly byte[] Payload;

            public AppFrame(byte cmd, byte flags, byte seq, byte[] payload)
            {
                Cmd = cmd; Flags = flags; Seq = seq; Payload = payload ?? Array.Empty<byte>();
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

        // Inferior (DAL): caller fornece Write()
        private readonly Func<byte[], bool> _write;

        // RX framing
        private readonly List<byte> _rx = new List<byte>(512);

        // SEQ por direção (TX)
        private byte _txSeq;

        // Dedupe RX (ACK_REQ)
        private bool _hasLastRxSeq;
        private byte _lastRxSeq;

        // Stop-and-wait
        private readonly object _swSync = new object();
        private bool _waitAck;
        private byte _waitSeq;
        private byte[] _lastTxFrame; // frame no stream (COBS + 0x00) para retransmitir
        private int _retriesLeft;
        private int _timeoutMs;
        private Timer _ackTimer;
        private TaskCompletionSource<SendOutcome> _tcs;

        public event Action<AppFrame> AppFrameReceived;
        public event Action<string> ProtocolError;
        public event Action<byte, SendOutcome> SendCompleted;

        public SdGwLinkEngine(Config cfg, Func<byte[], bool> write)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _write = write ?? throw new ArgumentNullException(nameof(write));
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

                    // proteção simples (evita buffer infinito se stream vier lixo sem 0x00)
                    if (_rx.Count > _cfg.MaxRawFrameLen + 16)
                    {
                        _rx.Clear();
                        ProtocolError?.Invoke("RX overflow: frame sem delimitador 0x00 (descartado).");
                    }
                    continue;
                }

                // Delimitador encontrado => decodifica frame
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

                if (decoded.Length < 4) // cmd, flags, seq, crc
                {
                    ProtocolError?.Invoke("Frame curto (descartado).");
                    continue;
                }

                // CRC: último byte
                byte crcRx = decoded[decoded.Length - 1];
                byte crcCalc = Crc8Atm(decoded, 0, decoded.Length - 1);

                if (crcRx != crcCalc)
                {
                    // Por spec: descarta e conta (aqui só notifica opcionalmente)
                    ProtocolError?.Invoke("CRC inválido (descartado).");
                    continue;
                }

                byte cmd = decoded[0];
                byte flags = decoded[1];
                byte seq = decoded[2];
                int payloadLen = decoded.Length - 4;
                byte[] payload = payloadLen > 0 ? Slice(decoded, 3, payloadLen) : Array.Empty<byte>();

                // Trata ACK/T_ERR internamente (por padrão)
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

                // Se ACK_REQ=1: envia ACK automaticamente
                if (ackReq)
                {
                    // Dedupe: se receber novamente o mesmo SEQ, só re-ACK e não sobe para app
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
            if (opt == null)
                opt = new SendOptions();

            if (payload == null)
                payload = new byte[0];


            // Monta flags
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

                bool ok = _write(stream);
                if (!ok)
                {
                    CompleteWait(seq, SendOutcome.TransportDown);
                    return _tcs.Task;
                }

                // start timer
                _ackTimer?.Dispose();
                _ackTimer = new Timer(AckTimeoutTick, null, _timeoutMs, Timeout.Infinite);

                return _tcs.Task;
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

                CompleteWait(seq, SendOutcome.Acked);
            }
        }

        private void HandleErr(byte seq)
        {
            lock (_swSync)
            {
                if (!_waitAck) return;
                if (seq != _waitSeq) return;

                CompleteWait(seq, SendOutcome.Nacked);
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
                        CompleteWait(_waitSeq, SendOutcome.TransportDown);
                        return;
                    }

                    _ackTimer?.Change(_timeoutMs, Timeout.Infinite);
                    return;
                }

                CompleteWait(_waitSeq, SendOutcome.Timeout);
            }
        }

        private void CompleteWait(byte seq, SendOutcome outcome)
        {
            _ackTimer?.Dispose();
            _ackTimer = null;

            _waitAck = false;

            _tcs?.TrySetResult(outcome);
            _tcs = null;

            SendCompleted?.Invoke(seq, outcome);
        }

        private void SendTransportAck(byte seq)
        {
            // ACK de transporte: cmd=CmdAck, flags=0, seq=mesmo, payload vazio
            var stream = BuildStreamFrame(_cfg.CmdAck, 0x00, seq, Array.Empty<byte>());
            _write(stream);
        }

        private byte NextTxSeq()
        {
            unchecked { _txSeq++; }
            return _txSeq;
        }

        // =========================
        // Montagem/Encode
        // =========================
        private byte[] BuildStreamFrame(byte cmd, byte flags, byte seq, byte[] payload)
        {
            // raw: CMD|FLAGS|SEQ|PAYLOAD|CRC
            int n = 3 + payload.Length + 1;
            var raw = new byte[n];
            raw[0] = cmd;
            raw[1] = flags;
            raw[2] = seq;

            if (payload.Length > 0)
                Buffer.BlockCopy(payload, 0, raw, 3, payload.Length);

            raw[n - 1] = Crc8Atm(raw, 0, n - 1);

            // COBS + 0x00
            var enc = CobsEncode(raw);
            var stream = new byte[enc.Length + 1];
            Buffer.BlockCopy(enc, 0, stream, 0, enc.Length);
            stream[stream.Length - 1] = 0x00;
            return stream;
        }

        // =========================
        // Utils
        // =========================
        private static byte[] Slice(byte[] src, int offset, int len)
        {
            var dst = new byte[len];
            Buffer.BlockCopy(src, offset, dst, 0, len);
            return dst;
        }

        // CRC-8/ATM: poly 0x07, init 0x00, refin/out false, xorout 0x00
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

        // COBS (Consistent Overhead Byte Stuffing)
        public static byte[] CobsEncode(byte[] input)
        {
            if (input == null) return Array.Empty<byte>();

            var output = new List<byte>(input.Length + 2);
            int codeIndex = 0;
            byte code = 1;

            output.Add(0); // placeholder para code

            for (int i = 0; i < input.Length; i++)
            {
                byte b = input[i];
                if (b == 0)
                {
                    output[codeIndex] = code;
                    codeIndex = output.Count;
                    output.Add(0); // novo placeholder
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
            if (input == null || input.Length == 0) return Array.Empty<byte>();

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
