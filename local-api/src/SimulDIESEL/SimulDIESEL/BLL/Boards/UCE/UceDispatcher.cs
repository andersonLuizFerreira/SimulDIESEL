using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDCTP;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public interface IUceDispatcher
    {
        event Action<UceLedEvent> LedEventReceived;
        event Action<SdctpRawEventDto> SdctpRawEventReceived;
        event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;

        Task<UceCommandResult> SetBuiltinLedAsync(bool on);
        Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode);
        Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode, UceCanRxMode rxMode);
        Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(string controller, bool enabled);
        Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync(string controller);
        Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync(string controller);
        Task<UceOperationResult<UceCanRxPollResponse>> PollCanRxAsync(string controller);
        [Obsolete("CAN_READ_ALL e legado. Use fluxo SDCTP por GetRxSnapshot/TryReadRxFrame.")]
        Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller);
        Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller);
        [Obsolete("Use SdctpApiService.SendDirectAsync / CAN_TX_DIRECT 0x50, or SDCTP TX table methods.")]
        Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs);
        Task<UceOperationResult<UceCanTxResponse>> SendCanDirectAsync(string controller, bool extended, bool rtr, uint id, byte dlc, byte[] data);
        Task<UceOperationResult<UceCanTxResponse>> CreateCanTxRowAsync(string controller, byte index, bool extended, bool rtr, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled);
        Task<UceOperationResult<UceCanTxResponse>> EditCanTxRowAsync(string controller, byte index, byte mask, byte flags, uint id, byte dlc, byte dataMask, byte[] data, ushort periodMs, bool enabled);
        Task<UceOperationResult<UceCanTxResponse>> DeleteCanTxRowAsync(string controller, byte index, byte reason);
        Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller);
    }

    public sealed class UceDispatcher : IUceDispatcher
    {
        private readonly UceClient _client;

        public UceDispatcher(UceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _client.LedEventReceived += OnLedEventReceived;
            _client.SdctpRawEventReceived += OnSdctpRawEventReceived;
            _client.DispatcherOverflowDiagnosticReceived += OnDispatcherOverflowDiagnosticReceived;
        }

        public event Action<UceLedEvent> LedEventReceived;
        public event Action<SdctpRawEventDto> SdctpRawEventReceived;
        public event Action<UceDispatcherOverflowDiagnostic> DispatcherOverflowDiagnosticReceived;

        public Task<UceCommandResult> SetBuiltinLedAsync(bool on)
        {
            return _client.SetBuiltinLedAsync(on);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode)
        {
            return _client.SetCanConfigAsync(controller, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(string controller, int bitrateKbps, string mode, UceCanRxMode rxMode)
        {
            return _client.SetCanConfigAsync(controller, bitrateKbps, mode, rxMode);
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

        [Obsolete("CAN_READ_ALL e legado. Use fluxo SDCTP por GetRxSnapshot/TryReadRxFrame.")]
        public Task<UceOperationResult<UceCanReadAllResponse>> RequestCanReadAllAsync(string controller)
        {
            return _client.RequestCanReadAllAsync(controller);
        }

        public Task<UceOperationResult<UceCanDriverLogPollResponse>> PollCanDriverLogAsync(string controller)
        {
            return _client.PollCanDriverLogAsync(controller);
        }

        [Obsolete("Use SdctpApiService.SendDirectAsync / CAN_TX_DIRECT 0x50, or SDCTP TX table methods.")]
        public Task<UceOperationResult<UceCanTxResponse>> SendCanAsync(string controller, bool extended, uint id, byte dlc, byte[] data, ushort periodMs)
        {
            return _client.SendCanAsync(controller, extended, id, dlc, data, periodMs);
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendCanDirectAsync(string controller, bool extended, bool rtr, uint id, byte dlc, byte[] data)
        {
            return _client.SendCanDirectAsync(controller, extended, rtr, id, dlc, data);
        }

        public Task<UceOperationResult<UceCanTxResponse>> CreateCanTxRowAsync(string controller, byte index, bool extended, bool rtr, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled)
        {
            return _client.CreateCanTxRowAsync(controller, index, extended, rtr, id, dlc, data, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> EditCanTxRowAsync(string controller, byte index, byte mask, byte flags, uint id, byte dlc, byte dataMask, byte[] data, ushort periodMs, bool enabled)
        {
            return _client.EditCanTxRowAsync(controller, index, mask, flags, id, dlc, dataMask, data, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> DeleteCanTxRowAsync(string controller, byte index, byte reason)
        {
            return _client.DeleteCanTxRowAsync(controller, index, reason);
        }

        public Task<UceOperationResult<UceCanTxStopResponse>> StopCanTxAsync(string controller)
        {
            return _client.StopCanTxAsync(controller);
        }

        private void OnLedEventReceived(UceLedEvent ledEvent)
        {
            LedEventReceived?.Invoke(ledEvent);
        }

        private void OnSdctpRawEventReceived(SdctpRawEventDto rawEvent)
        {
            SdctpRawEventReceived?.Invoke(rawEvent);
        }

        private void OnDispatcherOverflowDiagnosticReceived(UceDispatcherOverflowDiagnostic diagnostic)
        {
            DispatcherOverflowDiagnosticReceived?.Invoke(diagnostic);
        }
    }
}
