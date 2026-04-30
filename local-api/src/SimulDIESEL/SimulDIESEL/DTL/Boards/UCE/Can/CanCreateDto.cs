namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanCreateDto
    {
        public int Index { get; set; }
        public bool Valid { get; set; }
        public byte Flags { get; set; }
        public uint CanId { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public ushort CycleTime { get; set; }
        public uint MessageOrder { get; set; }
    }
}
