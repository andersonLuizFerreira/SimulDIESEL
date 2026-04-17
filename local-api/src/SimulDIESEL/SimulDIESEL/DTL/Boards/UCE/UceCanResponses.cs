namespace SimulDIESEL.DTL.Boards.UCE
{
    public sealed class UceCanConfigResponse
    {
        public UceCanController Controller { get; set; }
        public int AcceptedBitrateKbps { get; set; }
        public UceCanMode AcceptedMode { get; set; }
    }

    public sealed class UceCanEnableResponse
    {
        public UceCanController Controller { get; set; }
        public bool EffectiveEnabled { get; set; }
    }

    public sealed class UceCanStatusResponse
    {
        public UceCanController Controller { get; set; }
        public UceCanInterfaceState State { get; set; }
        public int BitrateKbps { get; set; }
        public UceCanMode Mode { get; set; }
    }

    public sealed class UceCanResetResponse
    {
        public UceCanController Controller { get; set; }
        public bool ResetSucceeded { get; set; }
    }
}
