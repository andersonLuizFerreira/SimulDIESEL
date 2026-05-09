namespace SimulDIESEL.DTL.Protocols.J1939.Application
{
    public sealed class J1939SpnDefinitionDto
    {
        public int Spn { get; set; }
        public string Name { get; set; }
        public int Pgn { get; set; }
        public int? StartByte { get; set; }
        public int? StartBit { get; set; }
        public int BitLength { get; set; }
        public string DataType { get; set; }
        public string ValueType { get; set; }
        public double Resolution { get; set; }
        public double Offset { get; set; }
        public string Unit { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public bool IsAscii { get; set; }
        public bool IsDiscrete { get; set; }
        public bool PositionPending { get; set; }
        public string Notes { get; set; }
    }
}
