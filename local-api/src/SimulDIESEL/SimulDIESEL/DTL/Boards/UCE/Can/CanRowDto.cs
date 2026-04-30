namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanRowDto
    {
        public int Index { get; set; }
        public CanFrameDto Frame { get; set; } = new CanFrameDto();
        public int CycleTime { get; set; }
        public int MessageOrder { get; set; }
    }
}
