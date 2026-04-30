using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public interface IUceDispatcher
    {
        event Action<UceLedEvent> LedEventReceived;
        event Action<UceCanRxEvent> CanRxEventReceived;
        event Action<byte, byte[]> CanCrudEventReceived;

        Task<UceCommandResult> SetBuiltinLedAsync(bool on);
        Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode);
        Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled);
        Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller);
        Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller);
        Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync(string controller);
        Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller);
        Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller);
        Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs);
        Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller);
    }

    public sealed class UceDispatcher : IUceDispatcher
    {
        private readonly UceClient _client;

        public UceDispatcher(UceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _client.LedEventReceived += OnLedEventReceived;
            _client.CanRxEventReceived += OnCanRxEventReceived;
            _client.CanCrudEventReceived += OnCanCrudEventReceived;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<UceCanRxEvent> CanRxEventReceived;
        public event Action<byte, byte[]> CanCrudEventReceived;

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

        public Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync(string controller)
        {
            return _client.PollCanRxAsync(controller);
        }

        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller)
        {
            return _client.RequestCanReadAllAsync(controller);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller)
        {
            return _client.PollCanDriverLogAsync(controller);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            return _client.SendCanAsync(controller, extended, id, dlc, data, periodMs);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller)
        {
            return _client.StopCanTxAsync(controller);
        }

        private void OnLedEventReceived(UceLedEvent ledEvent)
        {
            LedEventReceived?.Invoke(ledEvent);
        }

        private void OnCanRxEventReceived(UceCanRxEvent canRxEvent)
        {
            CanRxEventReceived?.Invoke(canRxEvent);
        }

        private void OnCanCrudEventReceived(byte type, byte[] payload)
        {
            CanCrudEventReceived?.Invoke(type, payload);
        }
    }
}
