using System;

namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939AddressClaimDto
    {
        public uint Pgn { get; set; }
        public string PgnHex { get; set; }
        public string PgnAcronym { get; set; }
        public string PgnLabel { get; set; }
        public byte SourceAddress { get; set; }
        public bool IsAddressClaimed { get; set; }
        public bool IsCannotClaimAddress { get; set; }
        public bool IsInvalidSourceAddress { get; set; }
        public J1939NameDto Name { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
    }
}
