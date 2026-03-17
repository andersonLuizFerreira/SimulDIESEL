using SimulDIESEL.DAL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public sealed class GsaCommandResult
    {
        private GsaCommandResult(bool success, bool? appliedState, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
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

        public static GsaCommandResult Succeeded(bool appliedState, SdGwLinkEngine.SendOutcome sendOutcome, string message)
        {
            return new GsaCommandResult(true, appliedState, message, sendOutcome);
        }

        public static GsaCommandResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null, bool? appliedState = null)
        {
            return new GsaCommandResult(false, appliedState, message, sendOutcome);
        }
    }
}
