using SimulDIESEL.DAL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public sealed class UceOperationResult<T>
        where T : class
    {
        private UceOperationResult(bool success, T response, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
        {
            Success = success;
            Response = response;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public T Response { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static UceOperationResult<T> Succeeded(T response, SdGwLinkEngine.SendOutcome sendOutcome, string message)
        {
            return new UceOperationResult<T>(true, response, message, sendOutcome);
        }

        public static UceOperationResult<T> Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            return new UceOperationResult<T>(false, null, message, sendOutcome);
        }
    }
}
