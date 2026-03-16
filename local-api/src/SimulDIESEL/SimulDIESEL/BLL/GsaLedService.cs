using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards;

namespace SimulDIESEL.BLL
{
    public sealed class GsaLedService : IDisposable
    {
        private readonly GsaClient _gsa;
        private readonly Func<bool> _isLinked;

        public GsaLedService(GsaClient gsa, Func<bool> isLinked)
        {
            _gsa = gsa ?? throw new ArgumentNullException(nameof(gsa));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
        }

        public async Task<GsaLedCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
            {
                return GsaLedCommandResult.Fail("Link serial não está em estado Linked.");
            }

            GsaLedResult result = await _gsa.SetBuiltinLedAsync(ligado).ConfigureAwait(false);
            if (!result.Success)
            {
                return GsaLedCommandResult.Fail(result.Message, result.SendOutcome, result.AppliedState);
            }

            return GsaLedCommandResult.Succeeded(
                result.AppliedState ?? ligado,
                result.SendOutcome,
                result.Message);
        }

        public void Dispose()
        {
        }
    }

    public sealed class GsaLedCommandResult
    {
        private GsaLedCommandResult(bool success, bool? appliedState, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
        {
            Success = success;
            AppliedState = appliedState;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public bool? AppliedState { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static GsaLedCommandResult Succeeded(bool appliedState, SdGwLinkEngine.SendOutcome? sendOutcome, string message)
        {
            return new GsaLedCommandResult(true, appliedState, message, sendOutcome);
        }

        public static GsaLedCommandResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null, bool? appliedState = null)
        {
            return new GsaLedCommandResult(false, appliedState, message, sendOutcome);
        }
    }
}
