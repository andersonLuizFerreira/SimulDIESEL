using System;

namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NetworkEventDto
    {
        public string EventType { get; set; }
        public byte SourceAddress { get; set; }
        public J1939AddressClaimDto AddressClaim { get; set; }
        public J1939AddressRegistryEntryDto RegistryEntry { get; set; }
        public J1939WorkingSetDto WorkingSet { get; set; }
        public J1939WorkingSetMemberDto WorkingSetMember { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
