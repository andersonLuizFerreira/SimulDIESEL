using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.FormsLogic.UCE
{
    public sealed class FrmUceLogic : IDisposable
    {
        private const string DefaultCanController = "can0";

        private readonly IUceDispatcher _uceDispatcher;
        private readonly Func<bool> _isLinked;
        private readonly ApiCanService _apiCanService;
        private bool _disposed;

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
            _apiCanService = new ApiCanService(_uceDispatcher);
            _uceDispatcher.LedEventReceived += OnLedEventReceived;
            _uceDispatcher.CanRxEventReceived += OnCanRxEventReceived;
            _apiCanService.CanRxTableChanged += OnCanRxTableChanged;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<UceCanRxEvent> CanRxEventReceived;
        public event EventHandler CanRxTableChanged;

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmUceLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmUceLogic(service.BoardDispatcher.Uce, () => service.IsLinked);
        }

        public Task<UceCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(UceCommandResult.Fail("Link serial não está em estado Linked."));

            return _uceDispatcher.SetBuiltinLedAsync(ligado);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(int bitrateKbps, string mode)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanConfigResponse>();

            return _uceDispatcher.SetCanConfigAsync(DefaultCanController, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(bool enabled)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanEnableResponse>();

            return _uceDispatcher.SetCanEnabledAsync(DefaultCanController, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanStatusResponse>();

            return _uceDispatcher.GetCanStatusAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanResetResponse>();

            return _uceDispatcher.ResetCanAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanRxPollResponse>();

            return _uceDispatcher.PollCanRxAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanReadAllResponse>();

            return _apiCanService.RequestReadAllAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanDriverLogPollResponse>();

            return _uceDispatcher.PollCanDriverLogAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanTxResponse>();

            return _uceDispatcher.SendCanAsync(DefaultCanController, extended, id, dlc, data, periodMs);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanTxStopResponse>();

            return _uceDispatcher.StopCanTxAsync(DefaultCanController);
        }

        public System.Collections.Generic.IReadOnlyList<CanRowDto> GetCanRxMirrorRows()
        {
            return _apiCanService.GetAll();
        }

        private static Task<UceOperationResult<T>> FailWhenNotLinked<T>()
            where T : class
        {
            return Task.FromResult(UceOperationResult<T>.Fail("Link serial não está em estado Linked."));
        }

        private void OnLedEventReceived(UceLedEvent ledEvent)
        {
            LedEventReceived?.Invoke(ledEvent);
        }

        private void OnCanRxEventReceived(UceCanRxEvent canRxEvent)
        {
            CanRxEventReceived?.Invoke(canRxEvent);
        }

        private void OnCanRxTableChanged(object sender, EventArgs e)
        {
            CanRxTableChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _uceDispatcher.LedEventReceived -= OnLedEventReceived;
            _uceDispatcher.CanRxEventReceived -= OnCanRxEventReceived;
            _apiCanService.CanRxTableChanged -= OnCanRxTableChanged;
            _apiCanService.Dispose();
            _disposed = true;
        }
    }
}
