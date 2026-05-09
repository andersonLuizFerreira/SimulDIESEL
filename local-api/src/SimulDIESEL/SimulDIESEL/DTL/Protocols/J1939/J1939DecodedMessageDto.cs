using System;

namespace SimulDIESEL.DTL.Protocols.J1939
{
    public sealed class J1939DecodedMessageDto
    {
        public uint CanId { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public DateTime Timestamp { get; set; }
        public J1939IdFieldsDto IdFields { get; set; }
        public string FormattedPgn { get; set; }
        public bool IsStructurallyDecoded { get; set; }
        public string StatusText { get; set; }
    }
}
