namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanFrameDto
    {
        public uint CanId { get; set; }
        public bool IsExtended { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
    }
}
