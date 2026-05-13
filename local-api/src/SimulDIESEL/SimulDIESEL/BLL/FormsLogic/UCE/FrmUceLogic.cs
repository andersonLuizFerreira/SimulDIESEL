using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Protocols.J1939;
using SimulDIESEL.BLL.Protocols.J1939.Capture;
using SimulDIESEL.BLL.Protocols.J1939.Diagnostics;
using SimulDIESEL.BLL.Protocols.J1939.NetworkManagement;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.BLL.Services.CAN.SDCTP;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.Capture;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

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

    public sealed class J1939DataMonitorMessageDto
    {
        public uint? RawCanId { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public bool IsGlobalDestination { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public byte[] Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public sealed class FrmUceLogic : IDisposable
    {
        private const string DefaultCanController = "can0";

        private readonly IUceDispatcher _uceDispatcher;
        private readonly Func<bool> _isLinked;
        private readonly CanControlApiService _canControl;
        private readonly SdctpApiService _sdctp;
        private readonly J1939ProtocolService _j1939Protocol;
        private readonly J1939DiagnosticsService _j1939Diagnostics;
        private readonly J1939DiagnosticRequestService _j1939DiagnosticRequests;
        private readonly J1939NetworkManagementService _j1939NetworkManagement;
        private readonly J1939TemporalCaptureService _j1939TemporalCapture;
        private readonly J1939CaptureExportService _j1939CaptureExport;
        private readonly bool _ownsSdctp;
        private bool _disposed;

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked)
            : this(uceDispatcher, isLinked, new CanControlApiService(uceDispatcher), new SdctpApiService(uceDispatcher), true)
        {
        }

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, SdctpApiService sdctp)
            : this(uceDispatcher, isLinked, new CanControlApiService(uceDispatcher), sdctp, false)
        {
        }

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, CanControlApiService canControl, SdctpApiService sdctp)
            : this(uceDispatcher, isLinked, canControl, sdctp, false)
        {
        }

        [Obsolete("Use the constructor that receives SdctpApiService.")]
        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, ApiCanService apiCanService)
            : this(uceDispatcher, isLinked, new CanControlApiService(uceDispatcher), new SdctpApiService(apiCanService, false), false)
        {
        }

        private FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked, CanControlApiService canControl, SdctpApiService sdctp, bool ownsSdctp)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
            _canControl = canControl ?? throw new ArgumentNullException(nameof(canControl));
            _sdctp = sdctp ?? throw new ArgumentNullException(nameof(sdctp));
            _j1939Protocol = new J1939ProtocolService();
            _j1939Diagnostics = new J1939DiagnosticsService();
            _j1939DiagnosticRequests = new J1939DiagnosticRequestService();
            _j1939NetworkManagement = new J1939NetworkManagementService();
            _j1939TemporalCapture = new J1939TemporalCaptureService();
            _j1939CaptureExport = new J1939CaptureExportService();
            _ownsSdctp = ownsSdctp;
            _uceDispatcher.LedEventReceived += OnLedEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived += OnDispatcherOverflowDiagnosticReceived;
            _sdctp.CanRxFrameAvailable += OnCanRxFrameAvailable;
            _sdctp.CanRxTableChanged += OnCanRxTableChanged;
            _sdctp.CanDiagnosticStateChanged += OnCanDiagnosticStateChanged;
        }

        public event Action<UceLedEvent> LedEventReceived;
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
            return new FrmUceLogic(service.BoardDispatcher.Uce, () => service.IsLinked, service.CanControl, service.Sdctp);
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

            return _canControl.SetCanConfigAsync(DefaultCanController, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(bool enabled)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanEnableResponse>();

            return _canControl.SetCanEnabledAsync(DefaultCanController, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanStatusResponse>();

            return _canControl.GetCanStatusAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanResetResponse>();

            return _canControl.ResetCanAsync(DefaultCanController);
        }

        [Obsolete("CAN_READ_ALL e legado. Use GetCanRxMirrorRows/TryReadRxFrame.")]
        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanReadAllResponse>();

            // TODO ETAPA 04: legado mantido temporariamente. UI/BLL deve consumir GetCanRxMirrorRows/TryReadRxFrame.
            return _sdctp.RequestReadAllAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanDriverLogPollResponse>();

            return _canControl.PollCanDriverLogAsync(DefaultCanController);
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

        public bool TryDecodeJ1939Frame(
            CanFrameDto frame,
            out J1939DiagnosticMessageDto diagnosticMessage,
            out J1939DataMonitorMessageDto dataMessage)
        {
            diagnosticMessage = null;
            dataMessage = null;
            if (frame == null)
                return false;

            J1939DataLinkProcessingResultDto result = _j1939Protocol.ProcessCanFrame(frame);
            _j1939Diagnostics.TryDecode(result, out diagnosticMessage);
            dataMessage = BuildJ1939DataMonitorMessage(result);
            return diagnosticMessage != null || dataMessage != null;
        }

        public bool TryProcessJ1939NetworkFrame(CanFrameDto frame, out J1939NetworkEventDto networkEvent)
        {
            networkEvent = null;
            if (frame == null)
                return false;

            J1939DataLinkProcessingResultDto result = _j1939Protocol.ProcessCanFrame(frame);
            return _j1939NetworkManagement.TryProcess(result, out networkEvent);
        }

        public System.Collections.Generic.IReadOnlyList<J1939AddressRegistryEntryDto> GetJ1939AddressRegistrySnapshot()
        {
            return _j1939NetworkManagement.AddressRegistry.GetSnapshot();
        }

        public bool IsJ1939TemporalCaptureActive
        {
            get { return _j1939TemporalCapture.IsCapturing; }
        }

        public J1939CaptureSessionDto StartJ1939TemporalCapture()
        {
            return _j1939TemporalCapture.Start();
        }

        public J1939CaptureSessionDto StopJ1939TemporalCapture()
        {
            return _j1939TemporalCapture.Stop();
        }

        public void ClearJ1939TemporalCapture()
        {
            _j1939TemporalCapture.Clear();
        }

        public J1939CaptureSessionDto GetJ1939TemporalCaptureSnapshot()
        {
            return _j1939TemporalCapture.GetSnapshot();
        }

        public void RegisterJ1939TemporalCaptureMessage(J1939DataMonitorMessageDto message, string notes)
        {
            if (message == null)
                return;

            _j1939TemporalCapture.RegisterFrame(
                message.Timestamp,
                message.SourceAddress,
                message.DestinationAddress,
                message.IsGlobalDestination,
                message.RawCanId,
                message.Pgn,
                message.FormattedPgn,
                message.Data,
                notes);
        }

        public void ExportJ1939TemporalCapture(J1939CaptureSessionDto session, string path)
        {
            _j1939CaptureExport.ExportToFile(session, path);
        }

        public CanDiagnosticStatusDto GetCanDiagnosticStatus(UceCanStatusResponse canStatus)
        {
            bool hasFifoOverflow = _sdctp.HasDispatcherFifoOverflow;
            uint fifoCount = _sdctp.DispatcherFifoOverflowCount;
            return new CanDiagnosticStatusDto
            {
                MirrorStatusText = _sdctp.IsMirrorOutOfSync ? "OUT_OF_SYNC" : "OK",
                SyncStatusText = _sdctp.IsSyncingReadAll ? "SYNCING_SDCTP" : "Estável",
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

        private static J1939DataMonitorMessageDto BuildJ1939DataMonitorMessage(J1939DataLinkProcessingResultDto result)
        {
            if (result == null || !result.IsJ1939)
                return null;

            if (result.IsTransportSessionComplete && result.ReassembledMessage != null)
            {
                return new J1939DataMonitorMessageDto
                {
                    RawCanId = result.RawCanId == 0 ? (uint?)null : result.RawCanId,
                    SourceAddress = result.ReassembledMessage.SourceAddress,
                    DestinationAddress = result.ReassembledMessage.DestinationAddress,
                    IsGlobalDestination = !result.ReassembledMessage.DestinationAddress.HasValue ||
                        result.ReassembledMessage.DestinationAddress.Value == 0xFF,
                    Pgn = result.ReassembledMessage.TransportedPgn,
                    FormattedPgn = result.ReassembledMessage.FormattedTransportedPgn,
                    Data = result.ReassembledMessage.Data != null ? (byte[])result.ReassembledMessage.Data.Clone() : new byte[0],
                    Timestamp = DateTime.Now
                };
            }

            if (!result.IsSingleFrame || result.SingleFrameMessage == null || result.IdFields == null)
                return null;

            byte dlc = result.SingleFrameMessage.Dlc > 8 ? (byte)8 : result.SingleFrameMessage.Dlc;
            byte[] data = new byte[dlc];
            if (result.SingleFrameMessage.Data != null)
                Array.Copy(result.SingleFrameMessage.Data, data, Math.Min(dlc, result.SingleFrameMessage.Data.Length));

            return new J1939DataMonitorMessageDto
            {
                RawCanId = result.IdFields.CanId,
                SourceAddress = result.IdFields.SourceAddress,
                DestinationAddress = result.IdFields.DestinationAddress,
                IsGlobalDestination = !result.IdFields.DestinationAddress.HasValue || result.IdFields.IsGlobalDestination,
                Pgn = result.Pgn,
                FormattedPgn = result.FormattedPgn,
                Data = data,
                Timestamp = result.SingleFrameMessage.Timestamp == default(DateTime) ? DateTime.Now : result.SingleFrameMessage.Timestamp
            };
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
