namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939TransportDataPacketDto
    {
        public byte SequenceNumber { get; set; }
        public byte[] Payload { get; set; } = new byte[7];
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
    }
}
