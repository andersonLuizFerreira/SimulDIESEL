using System.Collections.Generic;

namespace SimulDIESEL.DTL.Protocols.J1939.Application
{
    public sealed class J1939PgnDefinitionDto
    {
        public int Pgn { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public int? ExpectedLengthBytes { get; set; }
        public string NominalRepetition { get; set; }
        public string Notes { get; set; }
        public List<J1939SpnDefinitionDto> Spns { get; set; } = new List<J1939SpnDefinitionDto>();
    }
}
