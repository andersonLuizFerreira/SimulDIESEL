using System.Collections.Generic;

namespace SimulDIESEL.DTL.Boards.GSA
{
    public sealed class GsaChannelSetpointResponse
    {
        public int Channel { get; set; }
        public byte AppliedValue { get; set; }
    }

    public sealed class GsaChannelEnableResponse
    {
        public int Channel { get; set; }
        public bool AppliedState { get; set; }
    }

    public sealed class GsaChannelStatusResponse : GsaChannelSnapshot
    {
    }

    public sealed class GsaChannelsStatusResponse
    {
        public GsaChannelsStatusResponse()
        {
            Channels = new List<GsaChannelStatusResponse>();
        }

        public List<GsaChannelStatusResponse> Channels { get; private set; }
    }

    public sealed class GsaChannelsEnableResponse
    {
        public bool RequestedState { get; set; }
        public byte AffectedCount { get; set; }
    }

    public sealed class GsaChannelFaultResetResponse
    {
        public int Channel { get; set; }
        public bool FaultState { get; set; }
    }

    public sealed class GsaChannelOffsetResponse
    {
        public int Channel { get; set; }
        public GsaOffsetKind Kind { get; set; }
        public short Offset { get; set; }
    }

    public sealed class GsaChannelOffsetSaveResponse
    {
        public int Channel { get; set; }
    }

    public sealed class GsaChannelOffsetResetResponse
    {
        public int Channel { get; set; }
    }

    public sealed class GsaOffsetResetResponse
    {
        public byte AffectedChannels { get; set; }
    }
}
