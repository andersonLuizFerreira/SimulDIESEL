using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.FormsLogic.UCE
{
    public sealed class CanDiagnosticStatusDto
    {
        public string MirrorStatusText { get; set; }
        public string SyncStatusText { get; set; }
        public string DispatcherStatusText { get; set; }
        public uint DispatcherOverflowCount { get; set; }
        public string TableStatusText { get; set; }
        public string CanStatusText { get; set; }
        public string LastErrorText { get; set; }
        public DateTime? LastDiagnosticAt { get; set; }
        public int RxOutputBufferCount { get; set; }
        public uint RxOutputBufferOverflowCount { get; set; }
    }

    public sealed class FrmUceLogic : IDisposable
    {
        private const string DefaultCanController = "can0";

        private readonly IUceDispatcher _uceDispatcher;
        private readonly Func<bool> _isLinked;
        private readonly ApiCanService _apiCanService;
        private readonly bool _ownsApiCanService;
        private bool _disposed;

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked)
            : this(uceDispatcher, isLinked, new ApiCanService(uceDispatcher), true)
        {
        }

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, ApiCanService apiCanService)
            : this(uceDispatcher, isLinked, apiCanService, false)
        {
        }

        private FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, ApiCanService apiCanService, bool ownsApiCanService)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
            _apiCanService = apiCanService ?? throw new ArgumentNullException(nameof(apiCanService));
            _ownsApiCanService = ownsApiCanService;
            _uceDispatcher.LedEventReceived += OnLedEventReceived;
            _uceDispatcher.CanRxEventReceived += OnCanRxEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived += OnDispatcherOverflowDiagnosticReceived;
            _apiCanService.CanRxTableChanged += OnCanRxTableChanged;
            _apiCanService.CanDiagnosticStateChanged += OnCanDiagnosticStateChanged;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<UceCanRxEvent> CanRxEventReceived;
        public event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;
        public event EventHandler CanRxTableChanged;
        public event EventHandler CanDiagnosticStateChanged;

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmUceLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmUceLogic(service.BoardDispatcher.Uce, () => service.IsLinked, service.ApiCan);
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
            return _apiCanService.GetSnapshot();
        }

        public CanDiagnosticStatusDto GetCanDiagnosticStatus(UceCanStatusResponse canStatus)
        {
            bool hasFifoOverflow = _apiCanService.HasDispatcherFifoOverflow;
            uint fifoCount = _apiCanService.DispatcherFifoOverflowCount;
            return new CanDiagnosticStatusDto
            {
                MirrorStatusText = _apiCanService.IsMirrorOutOfSync ? "OUT_OF_SYNC" : "OK",
                SyncStatusText = _apiCanService.IsSyncingReadAll ? "SYNCING_READ_ALL" : "Estável",
                DispatcherStatusText = hasFifoOverflow ? "FIFO_OVERFLOW (count=" + fifoCount.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")" : "OK",
                DispatcherOverflowCount = fifoCount,
                TableStatusText = "Não informado",
                CanStatusText = GetCanStatusText(canStatus),
                LastErrorText = string.IsNullOrWhiteSpace(_apiCanService.LastDiagnosticText) ? "-" : _apiCanService.LastDiagnosticText,
                LastDiagnosticAt = _apiCanService.LastDiagnosticAt,
                RxOutputBufferCount = _apiCanService.OutputBufferCount,
                RxOutputBufferOverflowCount = _apiCanService.OutputBufferOverflowCount
            };
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

        private void OnCanDiagnosticStateChanged(object sender, EventArgs e)
        {
            CanDiagnosticStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDispatcherOverflowDiagnosticReceived(UceDispatcherOverflowDiagnostic diagnostic)
        {
            DispatcherOverflowDiagnosticReceived?.Invoke(diagnostic);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _uceDispatcher.LedEventReceived -= OnLedEventReceived;
            _uceDispatcher.CanRxEventReceived -= OnCanRxEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived -= OnDispatcherOverflowDiagnosticReceived;
            _apiCanService.CanRxTableChanged -= OnCanRxTableChanged;
            _apiCanService.CanDiagnosticStateChanged -= OnCanDiagnosticStateChanged;
            if (_ownsApiCanService)
                _apiCanService.Dispose();
            _disposed = true;
        }

        private static string GetCanStatusText(UceCanStatusResponse canStatus)
        {
            if (canStatus == null)
                return "Não informado";

            switch (canStatus.State)
            {
                case UceCanInterfaceState.Open:
                    return "ABERTA";
                case UceCanInterfaceState.Fault:
                    return "ERRO";
                case UceCanInterfaceState.Configured:
                case UceCanInterfaceState.Disabled:
                    return "FECHADA";
                default:
                    return "Não informado";
            }
        }
    }
}
