using SimulDIESEL.DAL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public sealed class UceCommandResult
    {
        private UceCommandResult(bool success, bool? acceptedState, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
        {
            Success = success;
            AcceptedState = acceptedState;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public bool? AcceptedState { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static UceCommandResult Succeeded(bool acceptedState, SdGwLinkEngine.SendOutcome sendOutcome, string message)
        {
            return new UceCommandResult(true, acceptedState, message, sendOutcome);
        }

        public static UceCommandResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null, bool? acceptedState = null)
        {
            return new UceCommandResult(false, acceptedState, message, sendOutcome);
        }
    }
}
