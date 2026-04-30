namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanEditDto
    {
        public int Index { get; set; }
        public uint Mask { get; set; }
        public byte[] UpdatedBytes { get; set; } = new byte[0];
        public int CycleTime { get; set; }
        public int MessageOrder { get; set; }
    }
}
