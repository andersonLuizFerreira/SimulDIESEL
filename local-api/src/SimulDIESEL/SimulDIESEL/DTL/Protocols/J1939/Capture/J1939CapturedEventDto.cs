using System;

namespace SimulDIESEL.DTL.Protocols.J1939.Capture
{
    public sealed class J1939CapturedEventDto
    {
        public DateTime Timestamp { get; set; }
        public long DeltaMs { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public bool IsGlobalDestination { get; set; }
        public uint? RawCanId { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public string DataHex { get; set; }
        public string NameHex { get; set; }
        public byte? ClaimedSourceAddress { get; set; }
        public int RepeatCount { get; set; }
        public string EventType { get; set; }
        public long? IntervalMs { get; set; }
        public string Notes { get; set; }
    }
}
