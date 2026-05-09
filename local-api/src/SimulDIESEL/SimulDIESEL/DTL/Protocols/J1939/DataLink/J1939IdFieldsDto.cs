namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939IdFieldsDto
    {
        public uint CanId { get; set; }
        public byte Priority { get; set; }
        public bool ExtendedDataPage { get; set; }
        public bool DataPage { get; set; }
        public byte PduFormat { get; set; }
        public byte PduSpecific { get; set; }
        public byte SourceAddress { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public byte? DestinationAddress { get; set; }
        public byte? GroupExtension { get; set; }
        public bool IsPdu1 { get; set; }
        public bool IsPdu2 { get; set; }
        public bool IsGlobalDestination { get; set; }
        public bool IsIso15765Frame { get; set; }
        public bool IsReservedEdpDpCombination { get; set; }
    }
}
