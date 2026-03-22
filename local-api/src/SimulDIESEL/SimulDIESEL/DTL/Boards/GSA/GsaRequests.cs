namespace SimulDIESEL.DTL.Boards.GSA
{
    public sealed class GsaChannelSetpointRequest
    {
        public int Channel { get; set; }
        public byte Value { get; set; }
    }

    public sealed class GsaChannelEnableRequest
    {
        public int Channel { get; set; }
        public bool State { get; set; }
    }

    public sealed class GsaChannelsEnableRequest
    {
        public bool State { get; set; }
    }

    public sealed class GsaChannelStatusRequest
    {
        public int Channel { get; set; }
    }

    public sealed class GsaChannelFaultResetRequest
    {
        public int Channel { get; set; }
    }

    public sealed class GsaChannelOffsetSetRequest
    {
        public int Channel { get; set; }
        public GsaOffsetKind Kind { get; set; }
        public short Value { get; set; }
    }

    public sealed class GsaChannelOffsetGetRequest
    {
        public int Channel { get; set; }
        public GsaOffsetKind Kind { get; set; }
    }

    public sealed class GsaChannelOffsetSaveRequest
    {
        public int Channel { get; set; }
    }

    public sealed class GsaChannelOffsetResetRequest
    {
        public int Channel { get; set; }
    }
}
