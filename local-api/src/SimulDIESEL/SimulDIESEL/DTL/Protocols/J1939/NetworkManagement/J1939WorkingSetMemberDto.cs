namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939WorkingSetMemberDto
    {
        public byte SourceAddress { get; set; }
        public J1939NameDto Name { get; set; }
        public string Status { get; set; }
    }
}
