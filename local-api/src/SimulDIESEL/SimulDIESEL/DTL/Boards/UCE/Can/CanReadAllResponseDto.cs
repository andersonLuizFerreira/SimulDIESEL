using System.Collections.Generic;

namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanReadAllResponseDto
    {
        public List<CanRowDto> Rows { get; set; } = new List<CanRowDto>();
    }
}
