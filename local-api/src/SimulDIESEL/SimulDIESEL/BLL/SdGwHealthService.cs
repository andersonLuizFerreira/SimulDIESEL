using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL
{
    public sealed class SdGwHealthService : IDisposable
    {
        public sealed class Config
        {
            public int PingIntervalMs { get; set; } = 1000;
            public int PingTimeoutMs { get; set; } = 150;
            public int PingRetries { get; set; } = 2;

            // CMD de ping (aplicação)
            public byte PingCmd { get; set; } = 0x55;
        }

        private readonly Config _cfg;
        private readonly SdGwLinkEngine _engine;

        private Timer _timer;
        private volatile bool _enabled;

        // evita Tick concorrente (Timer + async)
        private int _tickBusy;

        private int _consecutiveFails;

        public bool IsAlive { get; private set; }

        public event Action<bool> LinkHealthChanged;
        public event Action<string> HealthLog;

        public SdGwHealthService(SdGwLinkEngine engine, Config cfg)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            _engine = engine;

            _cfg = cfg ?? new Config();
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;

            if (!enabled)
            {
                // Para o timer, mas NÃO notifica DEAD (desligado != falha)
                if (_timer != null)
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);

                IsAlive = false;
                _consecutiveFails = 0;
                return;
            }

            // Liga timer (compatível com .NET antigo)
            if (_timer == null)
                _timer = new Timer(Tick, null, 0, _cfg.PingIntervalMs);
            else
                _timer.Change(0, _cfg.PingIntervalMs);
        }

        private async void Tick(object _)
        {
            if (!_enabled) return;

            // Protege contra reentrância
            if (Interlocked.Exchange(ref _tickBusy, 1) == 1)
                return;

            try
            {
                var opt = new SdGwLinkEngine.SendOptions
                {
                    RequireAck = true,
                    TimeoutMs = _cfg.PingTimeoutMs,
                    MaxRetries = _cfg.PingRetries,
                    IsEvent = false
                };

                // .NET antigo: não usar Array.Empty<byte>()
                var outcome = await _engine.SendAsync(_cfg.PingCmd, new byte[0], opt);

                if (!_enabled) return; // pode ter sido desabilitado enquanto aguardava

                if (outcome == SdGwLinkEngine.SendOutcome.Acked)
                {
                    _consecutiveFails = 0;
                    SetAlive(true);
                }
                else
                {
                    _consecutiveFails++;
                    HealthLog?.Invoke("Ping falhou: " + outcome);

                    // política: 1 falha já marca como perdido
                    SetAlive(false);
                }
            }
            catch (Exception ex)
            {
                if (!_enabled) return;

                _consecutiveFails++;
                HealthLog?.Invoke("Exceção no ping: " + ex.Message);
                SetAlive(false);
            }
            finally
            {
                Interlocked.Exchange(ref _tickBusy, 0);
            }
        }

        private void SetAlive(bool alive)
        {
            if (IsAlive == alive) return;
            IsAlive = alive;

            // Boa prática: não duplicar handlers aqui é responsabilidade do assinante,
            // mas o evento é disparado uma única vez por mudança.
            LinkHealthChanged?.Invoke(alive);
        }

        public void Dispose()
        {
            _enabled = false;

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
