namespace SimulDIESEL.DTL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DtcDto
    {
        public uint Spn { get; set; }
        public string SpnName { get; set; }
        public byte Fmi { get; set; }
        public string FmiDescription { get; set; }
        public byte OccurrenceCount { get; set; }
        public byte ConversionMethod { get; set; }
        public bool IsLegacyConversionMethod { get; set; }
        public string Status { get; set; }
        public byte[] RawBytes { get; set; }
    }
}
