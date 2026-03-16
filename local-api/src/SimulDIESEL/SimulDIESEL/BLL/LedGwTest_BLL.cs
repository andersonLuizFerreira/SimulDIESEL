using SimulDIESEL.BLL.Boards;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Serviço BLL para teste do LED do gateway.
    /// Usa SDH como camada semântica e preserva SGGW como transporte atual.
    /// </summary>
    public sealed class LedGwTest_BLL : IDisposable
    {
        private readonly GsaClient _gsa;
        private readonly Func<bool> _isLinked;

        private Timer _timer;
        private volatile bool _running;
        private volatile bool _desiredOn;

        public bool IsRunning => _running;

        public event Action<bool> LedStatusChanged;

        public LedGwTest_BLL(GsaClient gsa, SdGgwClient sggw, Func<bool> isLinked)
        {
            _gsa = gsa ?? throw new ArgumentNullException(nameof(gsa));
            if (sggw == null) throw new ArgumentNullException(nameof(sggw));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));

            // Cria o timer no "load" da BLL, mas não o inicia (pausado).
            // O timer será reiniciado por Start() e pausado por Stop().
            _timer = new Timer(
                callback: _ => Tick(),
                state: null,
                dueTime: Timeout.Infinite,
                period: 1000);
        }

        public void Start()
        {
            if (_running) return;

            _running = true;

            // Resume/ativa o timer criado no construtor.
            _timer?.Change(0, 1000);
        }

        public void Stop(bool forceOff = true)
        {
            if (!_running) return;

            _running = false;

            // Pausa o timer (não o descarta).
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);

            if (forceOff)
            {
                _desiredOn = false;
                _ = SendLedAsync(false);
            }
        }

        private void Tick()
        {
            if (!_running)
                return;

            _desiredOn = !_desiredOn;

            _ = SendLedAsync(_desiredOn);
        }

        private async Task SendLedAsync(bool on)
        {
            if (!_isLinked())
                return;

            GsaLedResult result = await _gsa.SetBuiltinLedAsync(on).ConfigureAwait(false);
            if (!result.Success)
                return;

            LedStatusChanged?.Invoke(result.AppliedState ?? on);
        }

        public void Dispose()
        {
            // Descarta o timer ao destruir a BLL
            try { _timer?.Dispose(); } catch { }
            _timer = null;
        }
    }
}
