using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;

namespace SimulDIESEL.BLL.FormsLogic.UCE
{
    public sealed class FrmUceLogic
    {
        private readonly UceClient _uceClient;
        private readonly Func<bool> _isLinked;

        public FrmUceLogic(UceClient uceClient, Func<bool> isLinked)
        {
            _uceClient = uceClient ?? throw new ArgumentNullException(nameof(uceClient));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
        }

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmUceLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmUceLogic(service.Uce, () => service.IsLinked);
        }

        public Task<UceCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(UceCommandResult.Fail("Link serial não está em estado Linked."));

            return _uceClient.SetBuiltinLedAsync(ligado);
        }
    }
}
