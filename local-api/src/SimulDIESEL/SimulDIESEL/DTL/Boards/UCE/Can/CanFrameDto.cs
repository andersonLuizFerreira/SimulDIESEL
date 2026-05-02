namespace SimulDIESEL.DTL.Boards.UCE.Can
{
    public enum CanFrameSource
    {
        Unknown = 0,
        Direct = 1,
        Reconstructed = 2,
        Replay = 3
    }

    public sealed class CanFrameDto
    {
        public uint CanId { get; set; }
        public bool IsExtended { get; set; }
        public bool IsRemoteRequest { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; } = new byte[8];
        public System.DateTime Timestamp { get; set; }
        public CanFrameSource Source { get; set; }

        public CanFrameDto Clone()
        {
            return new CanFrameDto
            {
                CanId = CanId,
                IsExtended = IsExtended,
                IsRemoteRequest = IsRemoteRequest,
                Dlc = Dlc,
                Data = Data != null ? (byte[])Data.Clone() : new byte[8],
                Timestamp = Timestamp,
                Source = Source
            };
        }
    }
}
