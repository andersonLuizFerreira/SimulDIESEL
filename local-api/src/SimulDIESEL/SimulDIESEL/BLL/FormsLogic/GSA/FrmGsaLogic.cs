using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;

namespace SimulDIESEL.BLL.FormsLogic.GSA
{
    /// <summary>
    /// Orquestra o fluxo funcional do form da GSA.
    /// A UI conversa com esta camada e não com o ponto global de serial.
    /// </summary>
    public sealed class FrmGsaLogic : IDisposable
    {
        private readonly GsaClient _gsaClient;
        private readonly Func<bool> _isLinked;

        public FrmGsaLogic(GsaClient gsaClient, Func<bool> isLinked)
        {
            _gsaClient = gsaClient ?? throw new ArgumentNullException(nameof(gsaClient));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
        }

        public static FrmGsaLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmGsaLogic(service.Gsa, () => service.IsLinked);
        }

        public Task<GsaCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(GsaCommandResult.Fail("Link serial não está em estado Linked."));

            return _gsaClient.SetBuiltinLedAsync(ligado);
        }

        public void Dispose()
        {
        }
    }
}
