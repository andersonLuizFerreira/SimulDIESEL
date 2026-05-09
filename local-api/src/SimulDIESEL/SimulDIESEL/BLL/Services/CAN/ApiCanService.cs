using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN
{
    /// <summary>
    /// Validated API-side SDCTP implementation. SdctpApiService is the official
    /// protocol facade; this class remains in place to preserve current consumers.
    /// </summary>
    public sealed class ApiCanService : IDisposable
    {
        private const string DefaultCanController = "can0";

        private readonly object _sync = new object();
        private readonly IUceDispatcher _uceDispatcher;
        private readonly CanRxMirrorManager _rxMirrorManager;
        private readonly CanRxOutputBuffer _rxOutputBuffer;
        private readonly CanTxManager _txManager;
        private readonly CanEventProcessor _eventProcessor;
        private bool _isMirrorOutOfSync;
        private bool _isSyncingReadAll;
        private bool _hasDispatcherFifoOverflow;
        private uint _dispatcherFifoOverflowCount;
        private uint _directFrameCount;
        private uint _reconstructedFrameCount;
        private DateTime? _lastDiagnosticAt;
        private string _lastDiagnosticText = "-";
        private bool _disposed;

        public ApiCanService(IUceDispatcher uceDispatcher)
            : this(
                uceDispatcher,
                new CanRxMirrorManager())
        {
        }

        public ApiCanService(IUceDispatcher uceDispatcher, CanRxMirrorManager rxMirrorManager)
        {
            _uceDispatcher = uceDispatcher;
            _rxMirrorManager = rxMirrorManager;
            _rxOutputBuffer = new CanRxOutputBuffer();
            _txManager = new CanTxManager(uceDispatcher);
            _eventProcessor = new CanEventProcessor(rxMirrorManager);

            _uceDispatcher.CanRxEventReceived += OnCanRxEventReceived;
            _uceDispatcher.CanCrudEventReceived += OnCanCrudEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived += OnDispatcherOverflowDiagnosticReceived;
            _rxMirrorManager.MirrorOutOfSyncDetected += OnMirrorOutOfSyncDetected;
        }

        public event EventHandler CanRxTableChanged;
        public event EventHandler CanDiagnosticStateChanged;
        public event EventHandler CanRxFrameAvailable;

        public bool IsMirrorOutOfSync
        {
            get
            {
                lock (_sync)
                {
                    return _isMirrorOutOfSync;
                }
            }
        }

        public bool IsSyncingReadAll
        {
            get
            {
                lock (_sync)
                {
                    return _isSyncingReadAll;
                }
            }
        }

        public bool HasDispatcherFifoOverflow
        {
            get
            {
                lock (_sync)
                {
                    return _hasDispatcherFifoOverflow;
                }
            }
        }

        public uint DispatcherFifoOverflowCount
        {
            get
            {
                lock (_sync)
                {
                    return _dispatcherFifoOverflowCount;
                }
            }
        }

        public DateTime? LastDiagnosticAt
        {
            get
            {
                lock (_sync)
                {
                    return _lastDiagnosticAt;
                }
            }
        }

        public string LastDiagnosticText
        {
            get
            {
                lock (_sync)
                {
                    return _lastDiagnosticText;
                }
            }
        }

        public int OutputBufferCount
        {
            get { return _rxOutputBuffer.Count; }
        }

        public uint OutputBufferOverflowCount
        {
            get { return _rxOutputBuffer.OverflowCount; }
        }

        public uint DirectFrameCount
        {
            get
            {
                lock (_sync)
                {
                    return _directFrameCount;
                }
            }
        }

        public uint ReconstructedFrameCount
        {
            get
            {
                lock (_sync)
                {
                    return _reconstructedFrameCount;
                }
            }
        }

        public IReadOnlyList<CanRowDto> GetAll()
        {
            return GetSnapshot();
        }

        public IReadOnlyList<CanRowDto> GetSnapshot()
        {
            return _rxMirrorManager.GetSnapshot();
        }

        public CanRowDto GetById(int index)
        {
            return _rxMirrorManager.GetById(index);
        }

        public bool TryGetById(int index, out CanRowDto row)
        {
            return _rxMirrorManager.TryGetById(index, out row);
        }

        public bool TryReadRxFrame(out CanFrameDto frame)
        {
            return _rxOutputBuffer.TryDequeue(out frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame)
        {
            return _txManager.SendFrameAsync(controller, frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> StartTxAsync(string controller, CanFrameDto frame, ushort periodMs)
        {
            return _txManager.StartCyclicTxAsync(controller, frame, periodMs);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopTxAsync(string controller)
        {
            return _txManager.StopTxAsync(controller);
        }

        public Task<UceOperationResult<UceCanTxResponse>> CreateTxRowAsync(string controller, int index, CanFrameDto frame, ushort periodMs, bool enabled)
        {
            return _txManager.CreateTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> EditTxRowAsync(string controller, int index, CanFrameDto frame, ushort? periodMs, bool? enabled)
        {
            return _txManager.EditTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> DeleteTxRowAsync(string controller, int index, byte reason)
        {
            return _txManager.DeleteTxRowAsync(controller, index, reason);
        }

        public IReadOnlyList<CanTxRowDto> GetTxSnapshot()
        {
            return _txManager.GetTxSnapshot();
        }

        public async Task<UceOperationResult<UceCanReadAllResponse>> RequestReadAllAsync(string controller)
        {
            Debug.WriteLine("ApiCanService: API enviou CAN_READ_ALL (0x43) para controller " + controller + ".");
            _rxMirrorManager.StartReadAll();
            SetSyncingReadAll(true);

            UceOperationResult<UceCanReadAllResponse> result = await _uceDispatcher
                .RequestCanReadAllAsync(controller)
                .ConfigureAwait(false);

            if (!result.Success)
            {
                Debug.WriteLine("ApiCanService: CAN_READ_ALL falhou antes da conclusão do snapshot. " + result.Message);
                _rxMirrorManager.CancelReadAll();
                SetSyncingReadAll(false);
            }
            else
            {
                Debug.WriteLine("ApiCanService: UCE confirmou a solicitação síncrona de CAN_READ_ALL.");
            }

            return result;
        }

        private void OnCanRxEventReceived(UceCanRxEvent canRxEvent)
        {
            _eventProcessor.ProcessCanRxEvent(canRxEvent);
            if (canRxEvent == null || canRxEvent.Frames == null)
                return;

            foreach (UceCanFrame frame in canRxEvent.Frames)
                EnqueueDirectFrame(frame);
        }

        private void OnCanCrudEventReceived(byte type, byte[] payload)
        {
            if (_eventProcessor.ProcessEvent(type, payload))
            {
                if (type == GwProtocol.UceCanReadAllDoneType)
                    FinishReadAllSync();
                else if (type == GwProtocol.UceCanCreateType || type == GwProtocol.UceCanEditType || type == GwProtocol.UceCanTicType)
                    EnqueueReconstructedFrame(GetIndexFromPayload(payload));

                CanRxTableChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnMirrorOutOfSyncDetected(int index, string reason)
        {
            _ = HandleMirrorOutOfSyncAsync(reason, index);
        }

        private async Task HandleMirrorOutOfSyncAsync(string reason, int index)
        {
            lock (_sync)
            {
                if (_disposed || _isSyncingReadAll || _isMirrorOutOfSync)
                    return;

                _isMirrorOutOfSync = true;
                _lastDiagnosticText = "MIRROR_OUT_OF_SYNC";
                _lastDiagnosticAt = DateTime.Now;
            }

            Debug.WriteLine("ApiCanService: MIRROR_OUT_OF_SYNC detectado no index " + index.ToString() + ".");
            UceGatewayDiagnosticLog.AppendCanMirrorOutOfSync(reason, index);
            OnCanDiagnosticStateChanged();

            _rxMirrorManager.ClearAll();
            CanRxTableChanged?.Invoke(this, EventArgs.Empty);

            SetSyncingReadAll(true);
            _rxMirrorManager.StartReadAll(false);

            try
            {
                Debug.WriteLine("ApiCanService: solicitando CAN_READ_ALL automático para recuperar MIRROR_OUT_OF_SYNC.");
                UceOperationResult<UceCanReadAllResponse> result = await _uceDispatcher
                    .RequestCanReadAllAsync(DefaultCanController)
                    .ConfigureAwait(false);

                if (!result.Success)
                {
                    Debug.WriteLine("ApiCanService: CAN_READ_ALL automático falhou. " + result.Message);
                    _rxMirrorManager.CancelReadAll();
                    SetSyncingReadAll(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ApiCanService: exceção ao solicitar CAN_READ_ALL automático. " + ex.Message);
                _rxMirrorManager.CancelReadAll();
                SetSyncingReadAll(false);
            }
        }

        private void FinishReadAllSync()
        {
            lock (_sync)
            {
                _isSyncingReadAll = false;
                _isMirrorOutOfSync = false;
            }

            Debug.WriteLine("ApiCanService: MIRROR_OUT_OF_SYNC recuperado após CAN_READ_ALL_DONE.");
            OnCanDiagnosticStateChanged();
        }

        private void EnqueueDirectFrame(UceCanFrame frame)
        {
            if (frame == null)
                return;

            CanFrameDto dto = new CanFrameDto
            {
                CanId = frame.Id,
                IsExtended = frame.Extended,
                IsRemoteRequest = frame.RemoteRequest,
                Dlc = frame.Dlc,
                Data = frame.Data != null ? (byte[])frame.Data.Clone() : new byte[8],
                Timestamp = DateTime.Now,
                Source = CanFrameSource.Direct
            };

            if (_rxOutputBuffer.Enqueue(dto))
            {
                lock (_sync)
                {
                    ++_directFrameCount;
                }

                OnCanRxFrameAvailable();
            }
        }

        private void EnqueueReconstructedFrame(int index)
        {
            if (!_rxMirrorManager.TryGetById(index, out CanRowDto row) || row == null || !row.Valid)
                return;

            CanFrameDto frame = ToFrame(row, CanFrameSource.Reconstructed);
            if (_rxOutputBuffer.Enqueue(frame))
            {
                IncrementReconstructedFrameCount();
                OnCanRxFrameAvailable();
            }
        }

        private void IncrementReconstructedFrameCount()
        {
            lock (_sync)
            {
                ++_reconstructedFrameCount;
            }
        }

        private void OnCanRxFrameAvailable()
        {
            CanRxFrameAvailable?.Invoke(this, EventArgs.Empty);
        }

        private static int GetIndexFromPayload(byte[] payload)
        {
            return payload != null && payload.Length > 0 ? payload[0] : -1;
        }

        private static CanFrameDto ToFrame(CanRowDto row, CanFrameSource source)
        {
            return new CanFrameDto
            {
                CanId = row.CanId,
                IsExtended = row.IsExtended,
                IsRemoteRequest = row.IsRemoteRequest,
                Dlc = row.Dlc,
                Data = row.Data != null ? (byte[])row.Data.Clone() : new byte[8],
                Timestamp = DateTime.Now,
                Source = source
            };
        }

        private void OnDispatcherOverflowDiagnosticReceived(UceDispatcherOverflowDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return;

            lock (_sync)
            {
                _hasDispatcherFifoOverflow = true;
                _dispatcherFifoOverflowCount = diagnostic.OverflowCount;
                _lastDiagnosticText = "DISPATCHER_FIFO_OVERFLOW";
                _lastDiagnosticAt = DateTime.Now;
            }

            OnCanDiagnosticStateChanged();
        }

        private void SetSyncingReadAll(bool syncing)
        {
            bool changed;
            lock (_sync)
            {
                if (_disposed && syncing)
                    return;

                changed = _isSyncingReadAll != syncing;
                _isSyncingReadAll = syncing;
            }

            if (changed)
                OnCanDiagnosticStateChanged();
        }

        private void OnCanDiagnosticStateChanged()
        {
            CanDiagnosticStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _uceDispatcher.CanRxEventReceived -= OnCanRxEventReceived;
            _uceDispatcher.CanCrudEventReceived -= OnCanCrudEventReceived;
            _uceDispatcher.DispatcherOverflowDiagnosticReceived -= OnDispatcherOverflowDiagnosticReceived;
            _rxMirrorManager.MirrorOutOfSyncDetected -= OnMirrorOutOfSyncDetected;
            _disposed = true;
        }
    }
}
