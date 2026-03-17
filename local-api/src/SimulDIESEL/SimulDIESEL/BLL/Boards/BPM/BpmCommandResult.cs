using SimulDIESEL.DAL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.BPM
{
    public sealed class BpmCommandResult
    {
        private BpmCommandResult(bool success, string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            Success = success;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static BpmCommandResult Succeeded(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            return new BpmCommandResult(true, message, sendOutcome);
        }

        public static BpmCommandResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            return new BpmCommandResult(false, message, sendOutcome);
        }
    }
}
