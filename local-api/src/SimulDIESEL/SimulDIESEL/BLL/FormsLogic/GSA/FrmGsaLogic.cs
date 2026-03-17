using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.GSA;

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

        public static FrmGsaLogic CreateFromLegacyAdapter()
        {
            // Adapter transitório: encapsula o uso do SerialLink para que o form
            // não navegue diretamente pelos serviços globais legados.
            return new FrmGsaLogic(SerialLink.Service.Gsa, () => SerialLink.IsLinked);
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
