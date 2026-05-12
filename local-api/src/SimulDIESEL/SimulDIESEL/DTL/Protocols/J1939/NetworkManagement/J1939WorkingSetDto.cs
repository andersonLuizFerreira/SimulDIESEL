namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939WorkingSetDto
    {
        public byte SourceAddress { get; set; }
        public byte MemberCount { get; set; }
        public string Status { get; set; }
    }
}
