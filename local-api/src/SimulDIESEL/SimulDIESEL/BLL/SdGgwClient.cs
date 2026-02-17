using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Cliente de alto nível do protocolo SGGW.
    /// Encapsula SdGwLinkEngine e expõe API segura e tipada.
    /// </summary>
    public sealed class SdGgwClient : IDisposable
    {
        private readonly SdGwLinkEngine _engine;
        private bool _disposed;

        /// <summary>
        /// Disparado quando qualquer frame SGGW é recebido.
        /// </summary>
        public event Action<SggwFrame> FrameReceived;

        /// <summary>
        /// Disparado quando um evento (FlagIsEvt) é recebido.
        /// </summary>
        public event Action<SggwFrame> EventReceived;

        public SdGgwClient(SdGwLinkEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _engine.AppFrameReceived += OnAppFrameReceived;
        }

        private void OnAppFrameReceived(SdGwLinkEngine.AppFrame f)
        {
            var frame = new SggwFrame(
                cmd: f.Cmd,
                seq: f.Seq,
                flags: f.Flags,
                payload: f.Payload);

            FrameReceived?.Invoke(frame);

            bool isEvent = (f.Flags & 0x02) != 0;
            if (isEvent)
                EventReceived?.Invoke(frame);
        }

        /// <summary>
        /// Envia comando SGGW com payload arbitrário.
        /// </summary>
        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            SggwCmd cmd,
            byte[] payload,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2)
        {
            ThrowIfDisposed();

            var opt = new SdGwLinkEngine.SendOptions
            {
                RequireAck = requireAck,
                TimeoutMs = timeoutMs,
                MaxRetries = retries,
                IsEvent = false
            };

            return _engine.SendAsync(
                cmd: (byte)cmd,
                payload: payload ?? Array.Empty<byte>(),
                opt: opt);
        }

        /// <summary>
        /// Envia comando sem payload.
        /// </summary>
        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            SggwCmd cmd,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2)
        {
            return SendAsync(cmd, Array.Empty<byte>(), requireAck, timeoutMs, retries);
        }

        /// <summary>
        /// Envia comando e retorna o SEQ usado + Task que completa quando ACK/ERR/Timeout ocorrer.
        /// </summary>
        public SdGwLinkEngine.SendTicket SendWithSeq(
            SggwCmd cmd,
            byte[] payload,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 1)
        {
            ThrowIfDisposed();

            var opt = new SdGwLinkEngine.SendOptions
            {
                RequireAck = requireAck,
                TimeoutMs = timeoutMs,
                MaxRetries = retries,
                IsEvent = false
            };

            return _engine.SendWithSeq(
                cmd: (byte)cmd,
                payload: payload ?? Array.Empty<byte>(),
                opt: opt);
        }

        /// <summary>
        /// Envia comando sem payload e retorna o SEQ usado + Task que completa quando ACK/ERR/Timeout ocorrer.
        /// </summary>
        public SdGwLinkEngine.SendTicket SendWithSeq(
            SggwCmd cmd,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 1)
        {
            return SendWithSeq(cmd, Array.Empty<byte>(), requireAck, timeoutMs, retries);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SdGgwClient));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _engine.AppFrameReceived -= OnAppFrameReceived;
            _disposed = true;
        }
    }
}
