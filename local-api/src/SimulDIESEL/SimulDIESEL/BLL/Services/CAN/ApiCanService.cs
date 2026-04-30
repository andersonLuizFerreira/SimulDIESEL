using System.Collections.Generic;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    public sealed class ApiCanService
    {
        private readonly IUceDispatcher _uceDispatcher;
        private readonly CanRxMirrorManager _rxMirrorManager;
        private readonly CanTxManager _txManager;
        private readonly CanEventProcessor _eventProcessor;

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
        }

        public IReadOnlyCollection<CanRowDto> GetAll()
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
    }
}
