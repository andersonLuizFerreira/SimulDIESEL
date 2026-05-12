using System;
using System.Collections.Generic;

namespace SimulDIESEL.DTL.Protocols.J1939.Application
{
    public sealed class J1939ApplicationMessageDto
    {
        public int Pgn { get; set; }
        public string PgnHex { get; set; }
        public string PgnName { get; set; }
        public string Acronym { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsDecoded { get; set; }
        public string Status { get; set; }
        public List<J1939DecodedSignalDto> Signals { get; set; } = new List<J1939DecodedSignalDto>();
        public byte[] RawPayload { get; set; } = new byte[0];
    }
}
