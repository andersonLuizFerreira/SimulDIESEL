namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939ReassembledMessageDto
    {
        public uint TransportedPgn { get; set; }
        public string FormattedTransportedPgn { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public ushort TotalSize { get; set; }
        public byte[] Data { get; set; }
        public string TransportType { get; set; }
    }
}
