using System;

namespace SimulDIESEL.DTL.Protocols.J1939.Capture
{
    public sealed class J1939TemporalTickDto
    {
        public DateTime FirstTimestamp { get; set; }
        public DateTime LastTimestamp { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public bool IsGlobalDestination { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public string DataHex { get; set; }
        public int RepeatCount { get; set; }
        public long IntervalMs { get; set; }
    }
}
