using SimulDIESEL.DTL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Serviço BLL para teste do comando LED via SGGW.
    /// Envia toggle a cada 1s e recebe o estado real do ESP-32.
    /// </summary>
    public sealed class LedGwTest_BLL : IDisposable
    {
        private readonly SdGgwClient _sggw;
        private readonly Func<bool> _isLinked;

        private Timer _timer;
        private volatile bool _running;
        private volatile bool _desiredOn;

        public bool IsRunning => _running;

        public event Action<bool> LedStatusChanged;

        public LedGwTest_BLL(SdGgwClient sggw, Func<bool> isLinked)
        {
            _sggw = sggw ?? throw new ArgumentNullException(nameof(sggw));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));

            _sggw.FrameReceived += OnFrameReceived;

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

            byte[] payload = { (byte)(on ? 1 : 0) };

            await _sggw.SendAsync(
                cmd: SggwCmd.LED,
                payload: payload,
                requireAck: false,
                timeoutMs: 150,
                retries: 0
            ).ConfigureAwait(false);
        }

        private void OnFrameReceived(SggwFrame frame)
        {
            if (frame == null)
                return;

            if (frame.Cmd != (byte)SggwCmd.LED)
                return;

            if (frame.Payload == null || frame.Payload.Length < 1)
                return;

            bool isOn = frame.Payload[0] != 0;

            LedStatusChanged?.Invoke(isOn);
        }

        public void Dispose()
        {
            _sggw.FrameReceived -= OnFrameReceived;

            // Descarta o timer ao destruir a BLL
            try { _timer?.Dispose(); } catch { }
            _timer = null;
        }
    }
}
