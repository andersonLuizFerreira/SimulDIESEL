using System.Collections.Generic;

namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanReadAllResponseDto
    {
        public bool IsSyncing { get; set; }
        public int Count { get; set; }
        public uint MessageOrder { get; set; }
        public List<CanRowDto> Rows { get; set; } = new List<CanRowDto>();
    }
}
