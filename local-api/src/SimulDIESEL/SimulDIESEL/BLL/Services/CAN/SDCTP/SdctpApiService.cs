using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// API-side entrypoint for SDCTP, the official SimulDIESEL CAN Transport Protocol.
    /// Wraps the validated ApiCanService without changing RX/TX behavior.
    /// </summary>
    public sealed class SdctpApiService : IDisposable
    {
        private const string DefaultCanController = "can0";

        private readonly IUceDispatcher _uceDispatcher;
        private readonly ApiCanService _inner;
        private readonly bool _ownsInner;

        public SdctpApiService(IUceDispatcher uceDispatcher)
            : this(uceDispatcher, new ApiCanService(uceDispatcher), true)
        {
        }

        public SdctpApiService(ApiCanService inner)
            : this(null, inner, true)
        {
        }

        public SdctpApiService(ApiCanService inner, bool ownsInner)
            : this(null, inner, ownsInner)
        {
        }

        private SdctpApiService(IUceDispatcher uceDispatcher, ApiCanService inner, bool ownsInner)
        {
            _uceDispatcher = uceDispatcher;
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _ownsInner = ownsInner;
        }

        public ApiCanService InnerApiCanService
        {
            get { return _inner; }
        }

        public event EventHandler CanRxTableChanged
        {
            add { _inner.CanRxTableChanged += value; }
            remove { _inner.CanRxTableChanged -= value; }
        }

        public event EventHandler CanDiagnosticStateChanged
        {
            add { _inner.CanDiagnosticStateChanged += value; }
            remove { _inner.CanDiagnosticStateChanged -= value; }
        }

        public event EventHandler CanRxFrameAvailable
        {
            add { _inner.CanRxFrameAvailable += value; }
            remove { _inner.CanRxFrameAvailable -= value; }
        }

        public bool IsMirrorOutOfSync { get { return _inner.IsMirrorOutOfSync; } }
        public bool IsSyncingReadAll { get { return _inner.IsSyncingReadAll; } }
        public bool HasDispatcherFifoOverflow { get { return _inner.HasDispatcherFifoOverflow; } }
        public uint DispatcherFifoOverflowCount { get { return _inner.DispatcherFifoOverflowCount; } }
        public DateTime? LastDiagnosticAt { get { return _inner.LastDiagnosticAt; } }
        public string LastDiagnosticText { get { return _inner.LastDiagnosticText; } }
        public int OutputBufferCount { get { return _inner.OutputBufferCount; } }
        public uint OutputBufferOverflowCount { get { return _inner.OutputBufferOverflowCount; } }
        public uint DirectFrameCount { get { return _inner.DirectFrameCount; } }
        public uint ReconstructedFrameCount { get { return _inner.ReconstructedFrameCount; } }

        public IReadOnlyList<CanRowDto> GetSnapshot()
        {
            return _inner.GetSnapshot();
        }

        public IReadOnlyList<CanRowDto> GetRxSnapshot()
        {
            return _inner.GetSnapshot();
        }

        public bool TryReadRxFrame(out CanFrameDto frame)
        {
            return _inner.TryReadRxFrame(out frame);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return RequireDispatcher().SetCanConfigAsync(controller, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled)
        {
            return RequireDispatcher().SetCanEnabledAsync(controller, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller)
        {
            return RequireDispatcher().GetCanStatusAsync(controller);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller)
        {
            return RequireDispatcher().ResetCanAsync(controller);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller)
        {
            return RequireDispatcher().PollCanDriverLogAsync(controller);
        }

        public Task<UceOperationResult<UceCanReadAllResponse>> RequestReadAllAsync(string controller)
        {
            return _inner.RequestReadAllAsync(controller);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendDirectAsync(CanFrameDto frame)
        {
            return SendDirectAsync(DefaultCanController, frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendDirectAsync(string controller, CanFrameDto frame)
        {
            return _inner.SendFrameAsync(controller, frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame)
        {
            return SendDirectAsync(controller, frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> StartTxAsync(string controller, CanFrameDto frame, ushort periodMs)
        {
            return _inner.StartTxAsync(controller, frame, periodMs);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopTxAsync(string controller)
        {
            return _inner.StopTxAsync(controller);
        }

        public Task<UceOperationResult<UceCanTxResponse>> CreateTxRowAsync(string controller, int index, CanFrameDto frame, ushort periodMs, bool enabled)
        {
            return _inner.CreateTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> EditTxRowAsync(string controller, int index, CanFrameDto frame, ushort? periodMs, bool? enabled)
        {
            return _inner.EditTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> DeleteTxRowAsync(string controller, int index, byte reason)
        {
            return _inner.DeleteTxRowAsync(controller, index, reason);
        }

        public IReadOnlyList<CanTxRowDto> GetTxSnapshot()
        {
            return _inner.GetTxSnapshot();
        }

        public void Dispose()
        {
            if (_ownsInner)
                _inner.Dispose();
        }

        private IUceDispatcher RequireDispatcher()
        {
            if (_uceDispatcher == null)
                throw new InvalidOperationException("SdctpApiService foi criado sem IUceDispatcher para operacoes CAN de controle.");

            return _uceDispatcher;
        }
    }
}
