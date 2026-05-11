using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.Services.CAN
{
    /// <summary>
    /// API-side facade for CAN hardware control operations routed through SDH/UCE dispatcher.
    /// </summary>
    public sealed class CanControlApiService
    {
        private readonly IUceDispatcher _uceDispatcher;

        public CanControlApiService(IUceDispatcher uceDispatcher)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return _uceDispatcher.SetCanConfigAsync(controller, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode, UceCanRxMode rxMode)
        {
            return _uceDispatcher.SetCanConfigAsync(controller, bitrateKbps, mode, rxMode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled)
        {
            return _uceDispatcher.SetCanEnabledAsync(controller, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller)
        {
            return _uceDispatcher.GetCanStatusAsync(controller);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller)
        {
            return _uceDispatcher.ResetCanAsync(controller);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller)
        {
            return _uceDispatcher.PollCanDriverLogAsync(controller);
        }
    }
}
