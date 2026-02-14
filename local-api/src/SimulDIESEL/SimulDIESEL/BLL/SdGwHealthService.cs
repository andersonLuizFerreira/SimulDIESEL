using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL
{
    public sealed class SdGwHealthService : IDisposable
    {
        public sealed class Config
        {
            public byte PingCmd { get; set; } = 0x55;
            public int PingIntervalMs { get; set; } = 1000;
            public int PingTimeoutMs { get; set; } = 150;
            public int PingRetries { get; set; } = 2;
        }

        private readonly SdGwLinkEngine _engine;
        private readonly Config _cfg;

        private Timer _timer;
        private volatile bool _enabled;
        private bool _disposed;

        private bool _alive;
        private int _inTick; // 0 = livre, 1 = em execução
        private bool _transportDownLatched;

        public event Action<bool> LinkHealthChanged;
        public event Action<string> HealthLog;

        // >>> FIX: avisa 1x que o transporte caiu
        public event Action TransportDownDetected;

        public SdGwHealthService(SdGwLinkEngine engine, Config cfg)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed) return;

            _enabled = enabled;

            if (!enabled)
            {
                _transportDownLatched = false;

                // Se você aplicou o patch do _inTick como int:
                Interlocked.Exchange(ref _inTick, 0);

                StopTimer();
                SetAlive(false);
                return;
            }

            StartTimer();
        }


        private void StartTimer()
        {
            if (_timer == null)
                _timer = new Timer(Tick, null, 0, _cfg.PingIntervalMs);
            else
                _timer.Change(0, _cfg.PingIntervalMs);
        }

        private void StopTimer()
        {
            if (_timer != null)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetAlive(bool value)
        {
            if (_alive == value) return;
            _alive = value;
            LinkHealthChanged?.Invoke(_alive);
        }

        private async void Tick(object _)
        {
            if (!_enabled || _disposed) return;

            // evita reentrância se o ping atrasar (atômico)
            if (Interlocked.Exchange(ref _inTick, 1) == 1) return;


            try
            {
                var opt = new SdGwLinkEngine.SendOptions
                {
                    RequireAck = true,
                    TimeoutMs = _cfg.PingTimeoutMs,
                    MaxRetries = _cfg.PingRetries,
                    IsEvent = false
                };

                var outcome = await _engine.SendAsync(_cfg.PingCmd, Array.Empty<byte>(), opt);

                if (outcome == SdGwLinkEngine.SendOutcome.Acked)
                {
                    _transportDownLatched = false;
                    SetAlive(true);
                    return;
                }

                // >>> FIX: se o problema é transporte, derruba 1x e para o timer
                if (outcome == SdGwLinkEngine.SendOutcome.TransportDown)
                {
                    HealthLog?.Invoke("Health: TransportDown detectado no ping.");

                    SetAlive(false);

                    if (!_transportDownLatched)
                    {
                        _transportDownLatched = true;

                        // para de spamar tentativas
                        StopTimer();
                        _enabled = false;

                        TransportDownDetected?.Invoke();
                    }

                    return;
                }

                // outras falhas: timeout/nack/busy etc -> marca dead
                HealthLog?.Invoke("Ping falhou: " + outcome);
                SetAlive(false);
            }
            catch (Exception ex)
            {
                HealthLog?.Invoke("Exceção no ping: " + ex.Message);
                SetAlive(false);
            }
            finally
            {
                Interlocked.Exchange(ref _inTick, 0);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _timer?.Dispose(); } catch { }
            _timer = null;
        }
    }
}
