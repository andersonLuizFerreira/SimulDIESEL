namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanEditDto
    {
        public int Index { get; set; }
        public byte Mask { get; set; }
        public uint MessageOrder { get; set; }
        public byte Flags { get; set; }
        public uint CanId { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public byte DataMask { get; set; }
        public bool UsesDataMask { get; set; }
        public ushort CycleTime { get; set; }
    }
}
