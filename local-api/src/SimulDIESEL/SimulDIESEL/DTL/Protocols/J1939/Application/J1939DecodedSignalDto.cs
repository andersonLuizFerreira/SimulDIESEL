namespace SimulDIESEL.DTL.Protocols.J1939.Application
{
    public sealed class J1939DecodedSignalDto
    {
        public int Spn { get; set; }
        public string Name { get; set; }
        public int Pgn { get; set; }
        public string RawHex { get; set; }
        public ulong? RawValue { get; set; }
        public double? PhysicalValue { get; set; }
        public string DisplayValue { get; set; }
        public string Unit { get; set; }
        public J1939SignalStatusDto Status { get; set; }
        public string Notes { get; set; }
    }
}
