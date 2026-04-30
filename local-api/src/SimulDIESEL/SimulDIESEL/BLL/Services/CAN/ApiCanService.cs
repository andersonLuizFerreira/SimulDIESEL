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
    public sealed class ApiCanService : IDisposable
    {
        private readonly IUceDispatcher _uceDispatcher;
        private readonly CanRxMirrorManager _rxMirrorManager;
        private readonly CanTxManager _txManager;
        private readonly CanEventProcessor _eventProcessor;
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
            _txManager = new CanTxManager(uceDispatcher);
            _eventProcessor = new CanEventProcessor(rxMirrorManager);

            _uceDispatcher.CanRxEventReceived += OnCanRxEventReceived;
            _uceDispatcher.CanCrudEventReceived += OnCanCrudEventReceived;
        }

        public event EventHandler CanRxTableChanged;

        public IReadOnlyList<CanRowDto> GetAll()
        {
            return _rxMirrorManager.GetAll();
        }

        public CanRowDto GetById(int index)
        {
            return _rxMirrorManager.GetById(index);
        }

        public bool TryGetById(int index, out CanRowDto row)
        {
            return _rxMirrorManager.TryGetById(index, out row);
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

        public Task<UceOperationResult<UceCanRxPollResponse>> RequestFullSyncAsync(string controller)
        {
            return _uceDispatcher.PollCanRxAsync(controller);
        }

        public async Task<UceOperationResult<UceCanReadAllResponse>> RequestReadAllAsync(string controller)
        {
            Debug.WriteLine("ApiCanService: API enviou CAN_READ_ALL (0x43) para controller " + controller + ".");
            _rxMirrorManager.StartReadAll();

            UceOperationResult<UceCanReadAllResponse> result = await _uceDispatcher
                .RequestCanReadAllAsync(controller)
                .ConfigureAwait(false);

            if (!result.Success)
            {
                Debug.WriteLine("ApiCanService: CAN_READ_ALL falhou antes da conclusão do snapshot. " + result.Message);
                _rxMirrorManager.CancelReadAll();
            }
            else
            {
                Debug.WriteLine("ApiCanService: UCE confirmou a solicitação síncrona de CAN_READ_ALL.");
            }

            return result;
        }

        public void ReceiveCreate(CanCreateDto create)
        {
            _eventProcessor.ProcessCreate(create);
        }

        public void ReceiveEdit(CanEditDto edit)
        {
            _eventProcessor.ProcessEdit(edit);
        }

        public void ReceiveDelete(CanDeleteDto delete)
        {
            _eventProcessor.ProcessDelete(delete);
        }

        private void OnCanRxEventReceived(UceCanRxEvent canRxEvent)
        {
            _eventProcessor.ProcessCanRxEvent(canRxEvent);
        }

        private void OnCanCrudEventReceived(byte type, byte[] payload)
        {
            if (type == GwProtocol.UceCanRowType)
                Debug.WriteLine("ApiCanService: API recebeu CAN_ROW (0x44).");
            else if (type == GwProtocol.UceCanReadAllDoneType)
                Debug.WriteLine("ApiCanService: API recebeu CAN_READ_ALL_DONE (0x45).");

            if (_eventProcessor.ProcessEvent(type, payload))
                CanRxTableChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _uceDispatcher.CanRxEventReceived -= OnCanRxEventReceived;
            _uceDispatcher.CanCrudEventReceived -= OnCanCrudEventReceived;
            _disposed = true;
        }
    }
}
