using System;

namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939AddressRegistryEntryDto
    {
        public byte SourceAddress { get; set; }
        public string NameHex { get; set; }
        public J1939NameDto ParsedName { get; set; }
        public DateTime FirstSeenTimestamp { get; set; }
        public DateTime LastSeenTimestamp { get; set; }
        public string ClaimStatus { get; set; }
        public bool IsCannotClaim { get; set; }
        public bool ConflictDetected { get; set; }
        public string PreviousNameHex { get; set; }
        public string WinningNameHex { get; set; }
        public string Notes { get; set; }
    }
}
