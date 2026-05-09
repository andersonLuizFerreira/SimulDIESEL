namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanRowDto
    {
        public int Index { get; set; }
        public bool Valid { get; set; }
        public byte Flags { get; set; }
        public uint CanId { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public ushort CycleTime { get; set; }
        public uint MessageOrder { get; set; }

        public bool IsExtended
        {
            get { return (Flags & 0x01) != 0; }
        }

        public bool IsRemoteRequest
        {
            get { return (Flags & 0x02) != 0; }
        }

        public CanRowDto Clone()
        {
            return new CanRowDto
            {
                Index = Index,
                Valid = Valid,
                Flags = Flags,
                CanId = CanId,
                Dlc = Dlc,
                Data = (byte[])Data.Clone(),
                CycleTime = CycleTime,
                MessageOrder = MessageOrder
            };
        }
    }
}
