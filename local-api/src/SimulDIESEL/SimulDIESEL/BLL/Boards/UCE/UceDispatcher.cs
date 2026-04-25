using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public interface IUceDispatcher
    {
        Task<UceCommandResult> SetBuiltinLedAsync(bool on);
        Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode);
        Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled);
        Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller);
        Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller);
    }

    public sealed class UceDispatcher : IUceDispatcher
    {
        private readonly UceClient _client;

        public UceDispatcher(UceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<UceCommandResult> SetBuiltinLedAsync(bool on)
        {
            return _client.SetBuiltinLedAsync(on);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return _client.SetCanConfigAsync(controller, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled)
        {
            return _client.SetCanEnabledAsync(controller, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller)
        {
            return _client.GetCanStatusAsync(controller);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller)
        {
            return _client.ResetCanAsync(controller);
        }
    }
}
