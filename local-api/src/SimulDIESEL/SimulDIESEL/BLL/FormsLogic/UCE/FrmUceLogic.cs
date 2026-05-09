using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Protocols.J1939;
using SimulDIESEL.BLL.Protocols.J1939.Diagnostics;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.BLL.Services.CAN.SDCTP;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

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
        private readonly SdctpApiService _sdctp;
        private readonly J1939ProtocolService _j1939Protocol;
        private readonly J1939DiagnosticsService _j1939Diagnostics;
        private readonly J1939DiagnosticRequestService _j1939DiagnosticRequests;
        private readonly bool _ownsSdctp;
        private bool _disposed;

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked)
            : this(uceDispatcher, isLinked, new SdctpApiService(uceDispatcher), true)
        {
        }

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, SdctpApiService sdctp)
            : this(uceDispatcher, isLinked, sdctp, false)
        {
        }

        [Obsolete("Use the constructor that receives SdctpApiService.")]
        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, ApiCanService apiCanService)
            : this(uceDispatcher, isLinked, new SdctpApiService(apiCanService, false), false)
        {
        }

        private FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, SdctpApiService sdctp, bool ownsSdctp)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
            _sdctp = sdctp ?? throw new ArgumentNullException(nameof(sdctp));
            _j1939Protocol = new J1939ProtocolService();
            _j1939Diagnostics = new J1939DiagnosticsService();
            _j1939DiagnosticRequests = new J1939DiagnosticRequestService();
            _ownsSdctp = ownsSdctp;
            _uceDispatcher.LedEventReceived += OnLedEventReceived;
            _uceDispatcher.CanRxEventReceived += OnCanRxEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived += OnDispatcherOverflowDiagnosticReceived;
            _sdctp.CanRxFrameAvailable += OnCanRxFrameAvailable;
            _sdctp.CanRxTableChanged += OnCanRxTableChanged;
            _sdctp.CanDiagnosticStateChanged += OnCanDiagnosticStateChanged;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<UceCanRxEvent> CanRxEventReceived;
        public event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;
        public event EventHandler CanRxFrameAvailable;
        public event EventHandler CanRxTableChanged;
        public event EventHandler CanDiagnosticStateChanged;

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmUceLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmUceLogic(service.BoardDispatcher.Uce, () => service.IsLinked, service.Sdctp);
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

            return _sdctp.SetCanConfigAsync(DefaultCanController, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(bool enabled)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanEnableResponse>();

            return _sdctp.SetCanEnabledAsync(DefaultCanController, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanStatusResponse>();

            return _sdctp.GetCanStatusAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanResetResponse>();

            return _sdctp.ResetCanAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanReadAllResponse>();

            return _sdctp.RequestReadAllAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanDriverLogPollResponse>();

            return _sdctp.PollCanDriverLogAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanTxResponse>();

            CanFrameDto frame = new CanFrameDto
            {
                CanId = id,
                IsExtended = extended,
                IsRemoteRequest = false,
                Dlc = dlc,
                Data = NormalizeCanData(data),
                Timestamp = DateTime.Now,
                Source = CanFrameSource.Unknown
            };

            if (periodMs > 0)
                return _sdctp.StartTxAsync(DefaultCanController, frame, periodMs);

            return _sdctp.SendDirectAsync(DefaultCanController, frame);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanTxStopResponse>();

            return _sdctp.StopTxAsync(DefaultCanController);
        }

        public System.Collections.Generic.IReadOnlyList<CanRowDto> GetCanRxMirrorRows()
        {
            return _sdctp.GetRxSnapshot();
        }

        public bool TryReadRxFrame(out CanFrameDto frame)
        {
            return _sdctp.TryReadRxFrame(out frame);
        }

        public async Task<UceOperationResult<J1939DiagnosticReadResultDto>> RequestJ1939DiagnosticCodesAsync()
        {
            if (!_isLinked())
                return UceOperationResult<J1939DiagnosticReadResultDto>.Fail("Link serial não está em estado Linked.");

            UceOperationResult<UceCanTxResponse> dm1 = await _sdctp
                .SendDirectAsync(DefaultCanController, _j1939DiagnosticRequests.BuildDm1Request())
                .ConfigureAwait(false);
            if (!dm1.Success)
                return UceOperationResult<J1939DiagnosticReadResultDto>.Fail("Falha ao solicitar DM1: " + dm1.Message, dm1.SendOutcome);

            UceOperationResult<UceCanTxResponse> dm2 = await _sdctp
                .SendDirectAsync(DefaultCanController, _j1939DiagnosticRequests.BuildDm2Request())
                .ConfigureAwait(false);
            if (!dm2.Success)
                return UceOperationResult<J1939DiagnosticReadResultDto>.Fail("Falha ao solicitar DM2: " + dm2.Message, dm2.SendOutcome);

            return UceOperationResult<J1939DiagnosticReadResultDto>.Succeeded(
                new J1939DiagnosticReadResultDto
                {
                    Dm1RequestSent = true,
                    Dm2RequestSent = true,
                    Status = "Requests DM1/DM2 enviados via Request PGN 59904."
                },
                dm2.SendOutcome.GetValueOrDefault(),
                "Requests DM1/DM2 enviados.");
        }

        public bool TryDecodeJ1939DiagnosticFrame(CanFrameDto frame, out J1939DiagnosticMessageDto diagnosticMessage)
        {
            diagnosticMessage = null;
            if (frame == null)
                return false;

            J1939DataLinkProcessingResultDto result = _j1939Protocol.ProcessCanFrame(frame);
            return _j1939Diagnostics.TryDecode(result, out diagnosticMessage);
        }

        public CanDiagnosticStatusDto GetCanDiagnosticStatus(UceCanStatusResponse canStatus)
        {
            bool hasFifoOverflow = _sdctp.HasDispatcherFifoOverflow;
            uint fifoCount = _sdctp.DispatcherFifoOverflowCount;
            return new CanDiagnosticStatusDto
            {
                MirrorStatusText = _sdctp.IsMirrorOutOfSync ? "OUT_OF_SYNC" : "OK",
                SyncStatusText = _sdctp.IsSyncingReadAll ? "SYNCING_READ_ALL" : "Estável",
                DispatcherStatusText = hasFifoOverflow ? "FIFO_OVERFLOW (count=" + fifoCount.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")" : "OK",
                DispatcherOverflowCount = fifoCount,
                TableStatusText = "Não informado",
                CanStatusText = GetCanStatusText(canStatus),
                LastErrorText = string.IsNullOrWhiteSpace(_sdctp.LastDiagnosticText) ? "-" : _sdctp.LastDiagnosticText,
                LastDiagnosticAt = _sdctp.LastDiagnosticAt,
                RxOutputBufferCount = _sdctp.OutputBufferCount,
                RxOutputBufferOverflowCount = _sdctp.OutputBufferOverflowCount
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

        private void OnCanRxFrameAvailable(object sender, EventArgs e)
        {
            CanRxFrameAvailable?.Invoke(this, EventArgs.Empty);
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
            _sdctp.CanRxFrameAvailable -= OnCanRxFrameAvailable;
            _sdctp.CanRxTableChanged -= OnCanRxTableChanged;
            _sdctp.CanDiagnosticStateChanged -= OnCanDiagnosticStateChanged;
            if (_ownsSdctp)
                _sdctp.Dispose();
            _disposed = true;
        }

        private static byte[] NormalizeCanData(byte[] data)
        {
            byte[] normalized = new byte[8];
            if (data != null)
                Array.Copy(data, normalized, Math.Min(8, data.Length));

            return normalized;
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
