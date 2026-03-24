using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.DTL.Boards.GSA;

namespace SimulDIESEL.BLL.FormsLogic.GSA
{
    /// <summary>
    /// Orquestra o fluxo funcional do form da GSA.
    /// A UI conversa com esta camada e não com o ponto global de serial.
    /// </summary>
    public sealed class FrmGsaLogic : IDisposable
    {
        private readonly GsaClient _gsaClient;
        private readonly Func<bool> _isLinked;
        private bool _disposed;

        public FrmGsaLogic(GsaClient gsaClient, Func<bool> isLinked)
        {
            _gsaClient = gsaClient ?? throw new ArgumentNullException(nameof(gsaClient));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));

            _gsaClient.ChannelFaultEventReceived += OnChannelFaultEventReceived;
        }

        public event Action<GsaChannelFaultEvent> ChannelFaultEventReceived;

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmGsaLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmGsaLogic(service.Gsa, () => service.IsLinked);
        }

        public Task<GsaCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(GsaCommandResult.Fail("Link serial não está em estado Linked."));

            return _gsaClient.SetBuiltinLedAsync(ligado);
        }

        public Task<GsaOperationResult<GsaChannelSetpointResponse>> SetChannelSetpointAsync(int channel, byte value)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelSetpointResponse>();

            return _gsaClient.SetChannelSetpointAsync(new GsaChannelSetpointRequest
            {
                Channel = channel,
                Value = value
            });
        }

        public Task<GsaOperationResult<GsaChannelEnableResponse>> SetChannelEnableAsync(int channel, bool state)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelEnableResponse>();

            return _gsaClient.SetChannelEnableAsync(new GsaChannelEnableRequest
            {
                Channel = channel,
                State = state
            });
        }

        public Task<GsaOperationResult<GsaChannelsEnableResponse>> SetChannelsEnableAsync(bool state)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelsEnableResponse>();

            return _gsaClient.SetChannelsEnableAsync(new GsaChannelsEnableRequest
            {
                State = state
            });
        }

        public Task<GsaOperationResult<GsaChannelStatusResponse>> GetChannelStatusAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelStatusResponse>();

            return _gsaClient.GetChannelStatusAsync(new GsaChannelStatusRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelsStatusResponse>> GetChannelsStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelsStatusResponse>();

            return _gsaClient.GetChannelsStatusAsync();
        }

        public Task<GsaOperationResult<GsaChannelFaultResetResponse>> ResetChannelFaultAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelFaultResetResponse>();

            return _gsaClient.ResetChannelFaultAsync(new GsaChannelFaultResetRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> SetChannelOffsetAsync(int channel, GsaOffsetKind kind, short value)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetResponse>();

            return _gsaClient.SetChannelOffsetAsync(new GsaChannelOffsetSetRequest
            {
                Channel = channel,
                Kind = kind,
                Value = value
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> GetChannelOffsetAsync(int channel, GsaOffsetKind kind)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetResponse>();

            return _gsaClient.GetChannelOffsetAsync(new GsaChannelOffsetGetRequest
            {
                Channel = channel,
                Kind = kind
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetSaveResponse>> SaveChannelOffsetAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetSaveResponse>();

            return _gsaClient.SaveChannelOffsetAsync(new GsaChannelOffsetSaveRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetResetResponse>> ResetChannelOffsetAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetResetResponse>();

            return _gsaClient.ResetChannelOffsetAsync(new GsaChannelOffsetResetRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaOffsetResetResponse>> ResetOffsetsAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaOffsetResetResponse>();

            return _gsaClient.ResetOffsetsAsync();
        }

        private void OnChannelFaultEventReceived(GsaChannelFaultEvent faultEvent)
        {
            ChannelFaultEventReceived?.Invoke(faultEvent);
        }

        private static Task<GsaOperationResult<T>> FailWhenNotLinked<T>()
            where T : class
        {
            return Task.FromResult(GsaOperationResult<T>.Fail("Link serial não está em estado Linked."));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _gsaClient.ChannelFaultEventReceived -= OnChannelFaultEventReceived;
            _disposed = true;
        }
    }
}
