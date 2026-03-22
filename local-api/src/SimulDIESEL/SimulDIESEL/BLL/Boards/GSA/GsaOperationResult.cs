using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.GSA;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public sealed class GsaOperationResult<T>
        where T : class
    {
        private GsaOperationResult(bool success, T response, string message, SdGwLinkEngine.SendOutcome? sendOutcome, GsaFunctionalErrorResponse functionalError)
        {
            Success = success;
            Response = response;
            Message = message ?? string.Empty;
            SendOutcome = sendOutcome;
            FunctionalError = functionalError;
        }

        public bool Success { get; }
        public T Response { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }
        public GsaFunctionalErrorResponse FunctionalError { get; }
        public bool HasFunctionalError => FunctionalError != null;

        public static GsaOperationResult<T> Succeeded(T response, SdGwLinkEngine.SendOutcome sendOutcome, string message)
        {
            return new GsaOperationResult<T>(true, response, message, sendOutcome, null);
        }

        public static GsaOperationResult<T> Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            return new GsaOperationResult<T>(false, null, message, sendOutcome, null);
        }

        public static GsaOperationResult<T> FunctionalFail(GsaFunctionalErrorResponse functionalError, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            string message = functionalError != null ? functionalError.Message : "Falha funcional da GSA.";
            return new GsaOperationResult<T>(false, null, message, sendOutcome, functionalError);
        }
    }
}
