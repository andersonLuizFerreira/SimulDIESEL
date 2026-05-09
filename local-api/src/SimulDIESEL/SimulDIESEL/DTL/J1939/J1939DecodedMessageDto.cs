using System;

namespace SimulDIESEL.DTL.J1939
{
    public sealed class J1939DecodedMessageDto
    {
        public J1939IdFieldsDto IdFields { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public DateTime Timestamp { get; set; }
        public bool IsValidJ1939 { get; set; }
        public string ValidationMessage { get; set; }
    }
}
