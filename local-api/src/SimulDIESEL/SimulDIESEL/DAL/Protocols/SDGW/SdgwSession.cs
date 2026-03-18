using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    /// <summary>
    /// Sessão de alto nível do protocolo SGGW/SDGW.
    /// Encapsula o link engine e expõe API segura e tipada.
    /// </summary>
    public sealed class SdgwSession : IDisposable
    {
        private readonly SdGwLinkEngine _engine;
        private readonly SdGwTxScheduler _txScheduler;
        private bool _disposed;

        public event Action<SggwFrame> FrameReceived;
        public event Action<SggwFrame> EventReceived;

        public SdgwSession(SdGwLinkEngine engine, SdGwTxScheduler txScheduler)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _txScheduler = txScheduler ?? throw new ArgumentNullException(nameof(txScheduler));
            _engine.AppFrameReceived += OnAppFrameReceived;
        }

        private void OnAppFrameReceived(SdGwLinkEngine.AppFrame frame)
        {
            var logicalFrame = new SggwFrame(
                cmd: frame.Cmd,
                seq: frame.Seq,
                flags: frame.Flags,
                payload: frame.Payload);

            FrameReceived?.Invoke(logicalFrame);

            bool isEvent = (frame.Flags & 0x02) != 0;
            if (isEvent)
                EventReceived?.Invoke(logicalFrame);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            byte cmd,
            byte[] payload,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2,
            SdGwTxPriority priority = SdGwTxPriority.Normal,
            string origin = null)
        {
            ThrowIfDisposed();

            return _txScheduler.EnqueueAsync(
                cmd: cmd,
                payload: payload ?? Array.Empty<byte>(),
                options: new SdGwLinkEngine.SendOptions
                {
                    RequireAck = requireAck,
                    TimeoutMs = timeoutMs,
                    MaxRetries = retries,
                    IsEvent = false
                },
                priority: priority,
                origin: origin);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            SggwCmd cmd,
            byte[] payload,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2,
            SdGwTxPriority priority = SdGwTxPriority.Normal,
            string origin = null)
        {
            return SendAsync((byte)cmd, payload, requireAck, timeoutMs, retries, priority, origin);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            byte cmd,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2,
            SdGwTxPriority priority = SdGwTxPriority.Normal,
            string origin = null)
        {
            return SendAsync(cmd, Array.Empty<byte>(), requireAck, timeoutMs, retries, priority, origin);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(
            SggwCmd cmd,
            bool requireAck = true,
            int timeoutMs = 150,
            int retries = 2,
            SdGwTxPriority priority = SdGwTxPriority.Normal,
            string origin = null)
        {
            return SendAsync(cmd, Array.Empty<byte>(), requireAck, timeoutMs, retries, priority, origin);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SdgwSession));
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
