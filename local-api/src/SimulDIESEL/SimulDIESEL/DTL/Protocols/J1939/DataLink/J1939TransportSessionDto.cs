using System;

namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939TransportSessionDto
    {
        public string SessionKey { get; set; }
        public string TransportType { get; set; }
        public uint TransportedPgn { get; set; }
        public string FormattedTransportedPgn { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public ushort TotalMessageSize { get; set; }
        public byte TotalPackets { get; set; }
        public byte NextExpectedSequenceNumber { get; set; }
        public int ReceivedPacketCount { get; set; }
        public bool IsComplete { get; set; }
        public bool HasSequenceError { get; set; }
        public bool IsTimedOut { get; set; }
        public string DiagnosticText { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime LastActivityTimestamp { get; set; }
    }
}
