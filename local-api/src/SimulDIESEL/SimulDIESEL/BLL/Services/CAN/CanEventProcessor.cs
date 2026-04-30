using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    public sealed class CanEventProcessor
    {
        private readonly CanRxMirrorManager _mirrorManager;

        public CanEventProcessor(CanRxMirrorManager mirrorManager)
        {
            _mirrorManager = mirrorManager;
        }

        public void ProcessCanRxEvent(UceCanRxEvent canRxEvent)
        {
        }

        public void ProcessCreate(CanCreateDto create)
        {
            _mirrorManager.ApplyCreate(create);
        }

        public void ProcessEdit(CanEditDto edit)
        {
            _mirrorManager.ApplyEdit(edit);
        }

        public void ProcessDelete(CanDeleteDto delete)
        {
            _mirrorManager.ApplyDelete(delete);
        }

        public void ProcessRow(CanRowDto row)
        {
        }
    }
}
