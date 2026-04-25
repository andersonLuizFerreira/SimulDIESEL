using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Boards.GSA;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public interface IGsaDispatcher
    {
        event Action<GsaChannelFaultEvent> ChannelFaultEventReceived;
        event Action<GsaPhysicalOperationEvent> PhysicalOperationEventReceived;

        Task<GsaCommandResult> SetBuiltinLedAsync(bool on);
        Task<GsaOperationResult<GsaChannelSetpointResponse>> SetChannelSetpointAsync(GsaChannelSetpointRequest request);
        Task<GsaOperationResult<GsaChannelEnableResponse>> SetChannelEnableAsync(GsaChannelEnableRequest request);
        Task<GsaOperationResult<GsaChannelsEnableResponse>> SetChannelsEnableAsync(GsaChannelsEnableRequest request);
        Task<GsaOperationResult<GsaChannelStatusResponse>> GetChannelStatusAsync(GsaChannelStatusRequest request);
        Task<GsaOperationResult<GsaChannelsStatusResponse>> GetChannelsStatusAsync();
        Task<GsaOperationResult<GsaChannelFaultResetResponse>> ResetChannelFaultAsync(GsaChannelFaultResetRequest request);
        Task<GsaOperationResult<GsaChannelOffsetResponse>> SetChannelOffsetAsync(GsaChannelOffsetSetRequest request);
        Task<GsaOperationResult<GsaChannelOffsetResponse>> GetChannelOffsetAsync(GsaChannelOffsetGetRequest request);
        Task<GsaOperationResult<GsaChannelOffsetSaveResponse>> SaveChannelOffsetAsync(GsaChannelOffsetSaveRequest request);
        Task<GsaOperationResult<GsaChannelOffsetResetResponse>> ResetChannelOffsetAsync(GsaChannelOffsetResetRequest request);
        Task<GsaOperationResult<GsaOffsetResetResponse>> ResetOffsetsAsync();
    }

    public sealed class GsaDispatcher : IGsaDispatcher, IDisposable
    {
        private readonly GsaClient _client;
        private bool _disposed;

        public GsaDispatcher(GsaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _client.ChannelFaultEventReceived += OnChannelFaultEventReceived;
            _client.PhysicalOperationEventReceived += OnPhysicalOperationEventReceived;
        }

        public event Action<GsaChannelFaultEvent> ChannelFaultEventReceived;
        public event Action<GsaPhysicalOperationEvent> PhysicalOperationEventReceived;

        public Task<GsaCommandResult> SetBuiltinLedAsync(bool on)
        {
            return _client.SetBuiltinLedAsync(on);
        }

        public Task<GsaOperationResult<GsaChannelSetpointResponse>> SetChannelSetpointAsync(GsaChannelSetpointRequest request)
        {
            return _client.SetChannelSetpointAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelEnableResponse>> SetChannelEnableAsync(GsaChannelEnableRequest request)
        {
            return _client.SetChannelEnableAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelsEnableResponse>> SetChannelsEnableAsync(GsaChannelsEnableRequest request)
        {
            return _client.SetChannelsEnableAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelStatusResponse>> GetChannelStatusAsync(GsaChannelStatusRequest request)
        {
            return _client.GetChannelStatusAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelsStatusResponse>> GetChannelsStatusAsync()
        {
            return _client.GetChannelsStatusAsync();
        }

        public Task<GsaOperationResult<GsaChannelFaultResetResponse>> ResetChannelFaultAsync(GsaChannelFaultResetRequest request)
        {
            return _client.ResetChannelFaultAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> SetChannelOffsetAsync(GsaChannelOffsetSetRequest request)
        {
            return _client.SetChannelOffsetAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> GetChannelOffsetAsync(GsaChannelOffsetGetRequest request)
        {
            return _client.GetChannelOffsetAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelOffsetSaveResponse>> SaveChannelOffsetAsync(GsaChannelOffsetSaveRequest request)
        {
            return _client.SaveChannelOffsetAsync(request);
        }

        public Task<GsaOperationResult<GsaChannelOffsetResetResponse>> ResetChannelOffsetAsync(GsaChannelOffsetResetRequest request)
        {
            return _client.ResetChannelOffsetAsync(request);
        }

        public Task<GsaOperationResult<GsaOffsetResetResponse>> ResetOffsetsAsync()
        {
            return _client.ResetOffsetsAsync();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _client.ChannelFaultEventReceived -= OnChannelFaultEventReceived;
            _client.PhysicalOperationEventReceived -= OnPhysicalOperationEventReceived;
            _disposed = true;
        }

        private void OnChannelFaultEventReceived(GsaChannelFaultEvent faultEvent)
        {
            ChannelFaultEventReceived?.Invoke(faultEvent);
        }

        private void OnPhysicalOperationEventReceived(GsaPhysicalOperationEvent physicalEvent)
        {
            PhysicalOperationEventReceived?.Invoke(physicalEvent);
        }
    }
}
