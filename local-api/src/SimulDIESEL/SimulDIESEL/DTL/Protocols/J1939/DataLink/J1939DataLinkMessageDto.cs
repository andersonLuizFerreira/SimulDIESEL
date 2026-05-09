using System;
using SimulDIESEL.DTL.Protocols.J1939.Common;

namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939DataLinkMessageDto
    {
        public uint CanId { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public DateTime Timestamp { get; set; }
        public J1939IdFieldsDto IdFields { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public J1939MessageTypeDto MessageType { get; set; }
        public bool IsSingleFrame { get; set; }
        public J1939ProcessingStatusDto Status { get; set; }
    }
}
