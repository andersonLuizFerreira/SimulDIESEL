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
        private readonly IGsaDispatcher _gsaDispatcher;
        private readonly Func<bool> _isLinked;
        private bool _disposed;

        public FrmGsaLogic(IGsaDispatcher gsaDispatcher, Func<bool> isLinked)
        {
            _gsaDispatcher = gsaDispatcher ?? throw new ArgumentNullException(nameof(gsaDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));

            _gsaDispatcher.ChannelFaultEventReceived += OnChannelFaultEventReceived;
            _gsaDispatcher.PhysicalOperationEventReceived += OnPhysicalOperationEventReceived;
        }

        public event Action<GsaChannelFaultEvent> ChannelFaultEventReceived;
        public event Action<GsaPhysicalOperationEvent> PhysicalOperationEventReceived;

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmGsaLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmGsaLogic(service.BoardDispatcher.Gsa, () => service.IsLinked);
        }

        public Task<GsaCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(GsaCommandResult.Fail("Link serial não está em estado Linked."));

            return _gsaDispatcher.SetBuiltinLedAsync(ligado);
        }

        public Task<GsaOperationResult<GsaChannelSetpointResponse>> SetChannelSetpointAsync(int channel, byte value)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelSetpointResponse>();

            return _gsaDispatcher.SetChannelSetpointAsync(new GsaChannelSetpointRequest
            {
                Channel = channel,
                Value = value
            });
        }

        public Task<GsaOperationResult<GsaChannelEnableResponse>> SetChannelEnableAsync(int channel, bool state)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelEnableResponse>();

            return _gsaDispatcher.SetChannelEnableAsync(new GsaChannelEnableRequest
            {
                Channel = channel,
                State = state
            });
        }

        public Task<GsaOperationResult<GsaChannelsEnableResponse>> SetChannelsEnableAsync(bool state)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelsEnableResponse>();

            return _gsaDispatcher.SetChannelsEnableAsync(new GsaChannelsEnableRequest
            {
                State = state
            });
        }

        public Task<GsaOperationResult<GsaChannelStatusResponse>> GetChannelStatusAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelStatusResponse>();

            return _gsaDispatcher.GetChannelStatusAsync(new GsaChannelStatusRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelsStatusResponse>> GetChannelsStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelsStatusResponse>();

            return _gsaDispatcher.GetChannelsStatusAsync();
        }

        public Task<GsaOperationResult<GsaChannelFaultResetResponse>> ResetChannelFaultAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelFaultResetResponse>();

            return _gsaDispatcher.ResetChannelFaultAsync(new GsaChannelFaultResetRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetResponse>> SetChannelOffsetAsync(int channel, GsaOffsetKind kind, short value)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetResponse>();

            return _gsaDispatcher.SetChannelOffsetAsync(new GsaChannelOffsetSetRequest
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

            return _gsaDispatcher.GetChannelOffsetAsync(new GsaChannelOffsetGetRequest
            {
                Channel = channel,
                Kind = kind
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetSaveResponse>> SaveChannelOffsetAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetSaveResponse>();

            return _gsaDispatcher.SaveChannelOffsetAsync(new GsaChannelOffsetSaveRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaChannelOffsetResetResponse>> ResetChannelOffsetAsync(int channel)
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaChannelOffsetResetResponse>();

            return _gsaDispatcher.ResetChannelOffsetAsync(new GsaChannelOffsetResetRequest
            {
                Channel = channel
            });
        }

        public Task<GsaOperationResult<GsaOffsetResetResponse>> ResetOffsetsAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<GsaOffsetResetResponse>();

            return _gsaDispatcher.ResetOffsetsAsync();
        }

        private void OnChannelFaultEventReceived(GsaChannelFaultEvent faultEvent)
        {
            ChannelFaultEventReceived?.Invoke(faultEvent);
        }

        private void OnPhysicalOperationEventReceived(GsaPhysicalOperationEvent physicalEvent)
        {
            PhysicalOperationEventReceived?.Invoke(physicalEvent);
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

            _gsaDispatcher.ChannelFaultEventReceived -= OnChannelFaultEventReceived;
            _gsaDispatcher.PhysicalOperationEventReceived -= OnPhysicalOperationEventReceived;
            _disposed = true;
        }
    }
}
