using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdGwLinkSupervisor : IDisposable
    {
        private readonly Func<Task<SdGwLinkEngine.SendOutcome>> _sendPingAsync;

        public sealed class Config
        {
            public byte PingCmd { get; set; } = 0x55;
            public int IdleBeforePingMs { get; set; } = 500;
            public int LinkTimeoutMs { get; set; } = 1000;
            public int PingTimeoutMs { get; set; } = 150;
            public int PingRetries { get; set; } = 2;
            public int TickPeriodMs { get; set; } = 50;
        }

        private readonly Config _cfg;

        private Timer _timer;
        private volatile bool _enabled;
        private bool _disposed;
        private bool _alive;
        private int _inTick;
        private bool _awaitingPingReply;
        private DateTime _lastValidRxUtc;

        public event Action<bool> LinkHealthChanged;
        public event Action<string> HealthLog;

        public SdGwLinkSupervisor(Config cfg, Func<Task<SdGwLinkEngine.SendOutcome>> sendPingAsync)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _sendPingAsync = sendPingAsync ?? throw new ArgumentNullException(nameof(sendPingAsync));
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed)
                return;

            _enabled = enabled;

            if (!enabled)
            {
                StopTimer();
                _awaitingPingReply = false;
                Interlocked.Exchange(ref _inTick, 0);
                SetAlive(false);
                return;
            }

            _lastValidRxUtc = DateTime.UtcNow;
            _awaitingPingReply = false;
            StartTimer();
        }

        public void OnValidFrameReceived()
        {
            if (_disposed)
                return;

            _lastValidRxUtc = DateTime.UtcNow;
            _awaitingPingReply = false;
            SetAlive(true);
        }

        private void StartTimer()
        {
            if (_timer == null)
                _timer = new Timer(Tick, null, _cfg.TickPeriodMs, _cfg.TickPeriodMs);
            else
                _timer.Change(_cfg.TickPeriodMs, _cfg.TickPeriodMs);
        }

        private void StopTimer()
        {
            if (_timer != null)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetAlive(bool value)
        {
            if (_alive == value)
                return;

            _alive = value;
            LinkHealthChanged?.Invoke(_alive);
        }

        private async void Tick(object _)
        {
            if (!_enabled || _disposed)
                return;

            if (Interlocked.Exchange(ref _inTick, 1) == 1)
                return;

            try
            {
                DateTime now = DateTime.UtcNow;
                double silenceMs = (now - _lastValidRxUtc).TotalMilliseconds;

                if (silenceMs < _cfg.IdleBeforePingMs)
                {
                    SetAlive(true);
                    return;
                }

                if (silenceMs >= _cfg.LinkTimeoutMs)
                {
                    _awaitingPingReply = false;
                    SetAlive(false);
                    return;
                }

                if (_awaitingPingReply)
                    return;

                _awaitingPingReply = true;

                SdGwLinkEngine.SendOutcome outcome = await _sendPingAsync().ConfigureAwait(false);
                _awaitingPingReply = false;

                if (outcome == SdGwLinkEngine.SendOutcome.Timeout ||
                    outcome == SdGwLinkEngine.SendOutcome.Nacked ||
                    outcome == SdGwLinkEngine.SendOutcome.TransportDown)
                {
                    EvaluateSilenceAfterPing();
                }
            }
            catch (Exception ex)
            {
                _awaitingPingReply = false;
                HealthLog?.Invoke("SdGwLinkSupervisor: excecao no watchdog do link: " + ex.Message);
                EvaluateSilenceAfterPing();
            }
            finally
            {
                Interlocked.Exchange(ref _inTick, 0);
            }
        }

        private void EvaluateSilenceAfterPing()
        {
            if (_disposed || !_enabled)
                return;

            double silenceMs = (DateTime.UtcNow - _lastValidRxUtc).TotalMilliseconds;
            if (silenceMs >= _cfg.LinkTimeoutMs)
                SetAlive(false);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _enabled = false;
            _awaitingPingReply = false;

            try { _timer?.Dispose(); } catch { }
            _timer = null;
        }
    }
}
