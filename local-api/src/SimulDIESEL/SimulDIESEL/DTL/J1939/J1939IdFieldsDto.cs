namespace SimulDIESEL.DTL.J1939
{
    public sealed class J1939IdFieldsDto
    {
        public uint CanId { get; set; }
        public byte Priority { get; set; }
        public bool Reserved { get; set; }
        public bool DataPage { get; set; }
        public byte PduFormat { get; set; }
        public byte PduSpecific { get; set; }
        public byte SourceAddress { get; set; }
        public uint Pgn { get; set; }
        public byte? DestinationAddress { get; set; }
        public bool IsPdu1 { get; set; }
        public bool IsPdu2 { get; set; }
    }
}
