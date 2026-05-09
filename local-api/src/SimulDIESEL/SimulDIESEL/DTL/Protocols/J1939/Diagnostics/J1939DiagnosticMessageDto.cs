using System;
using System.Collections.Generic;

namespace SimulDIESEL.DTL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DiagnosticMessageDto
    {
        public string Type { get; set; }
        public uint Pgn { get; set; }
        public string PgnHex { get; set; }
        public byte SourceAddress { get; set; }
        public byte? DestinationAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public J1939LampStatusDto LampStatus { get; set; }
        public List<J1939DtcDto> Dtcs { get; set; } = new List<J1939DtcDto>();
        public bool HasDtcs { get; set; }
        public bool IsReassembled { get; set; }
        public string Status { get; set; }
        public byte[] RawPayload { get; set; }
    }
}
