namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanDeleteDto
    {
        public int Index { get; set; }
        public byte Reason { get; set; }
        public uint MessageOrder { get; set; }
    }
}
