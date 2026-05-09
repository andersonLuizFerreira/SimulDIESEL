namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public sealed class CanTxRowDto
    {
        public int Index { get; set; }
        public bool Valid { get; set; }
        public bool Enabled { get; set; }
        public uint CanId { get; set; }
        public bool IsExtended { get; set; }
        public bool IsRemoteRequest { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public ushort PeriodMs { get; set; }

        public CanTxRowDto Clone()
        {
            return new CanTxRowDto
            {
                Index = Index,
                Valid = Valid,
                Enabled = Enabled,
                CanId = CanId,
                IsExtended = IsExtended,
                IsRemoteRequest = IsRemoteRequest,
                Dlc = Dlc,
                Data = Data != null ? (byte[])Data.Clone() : new byte[8],
                PeriodMs = PeriodMs
            };
        }
    }
}
