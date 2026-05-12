namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939TransportControlMessageDto
    {
        public byte ControlByte { get; set; }
        public string ControlName { get; set; }
        public ushort TotalMessageSize { get; set; }
        public byte TotalPackets { get; set; }
        public byte MaxPacketsPerCts { get; set; }
        public byte CtsPacketCount { get; set; }
        public byte CtsNextPacketNumber { get; set; }
        public byte AbortReason { get; set; }
        public uint TransportedPgn { get; set; }
        public string FormattedTransportedPgn { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public bool IsBam { get; set; }
        public bool IsRts { get; set; }
        public bool IsCts { get; set; }
        public bool IsEndOfMessageAck { get; set; }
        public bool IsAbort { get; set; }
    }
}
