namespace SimulDIESEL.DTL.Protocols.J1939.Common
{
    public sealed class J1939PgnDefinitionDto
    {
        public int Pgn { get; set; }
        public int? PgnEnd { get; set; }
        public string Hex { get; set; }
        public string Acronym { get; set; }
        public string Label { get; set; }
        public string SaeDocument { get; set; }
        public string Section { get; set; }
        public bool MultiPacket { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }

        public bool Contains(int pgn)
        {
            return pgn >= Pgn && pgn <= (PgnEnd ?? Pgn);
        }
    }
}
