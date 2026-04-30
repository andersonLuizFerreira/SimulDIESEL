using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    public sealed class CanTxManager
    {
        private readonly IUceDispatcher _uceDispatcher;

        public CanTxManager(IUceDispatcher uceDispatcher)
        {
            _uceDispatcher = uceDispatcher;
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame)
        {
            return SendFrameAsync(controller, frame, 0);
        }

        public Task<UceOperationResult<UceCanTxResponse>> StartCyclicTxAsync(string controller, CanFrameDto frame, ushort periodMs)
        {
            return SendFrameAsync(controller, frame, periodMs);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopTxAsync(string controller)
        {
            return _uceDispatcher.StopCanTxAsync(controller);
        }

        public void CreateMessage(CanCreateDto create)
        {
        }

        public void EditMessage(CanEditDto edit)
        {
        }

        public void RemoveMessage(CanDeleteDto delete)
        {
        }

        private Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame, ushort periodMs)
        {
            byte[] data = frame == null ? new byte[8] : frame.Data ?? new byte[8];
            return _uceDispatcher.SendCanAsync(
                controller,
                frame != null && frame.IsExtended,
                frame == null ? 0U : frame.CanId,
                frame == null ? (byte)0 : frame.Dlc,
                data,
                periodMs);
        }
    }
}
