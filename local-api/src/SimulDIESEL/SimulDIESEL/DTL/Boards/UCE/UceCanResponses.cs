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

    public sealed class UceCanRxPollResponse
    {
        public UceCanController Controller { get; set; }
        public System.Collections.Generic.IReadOnlyList<UceCanFrame> Frames { get; set; }
    }

    public sealed class UceCanRxEvent
    {
        public UceCanController Controller { get; set; }
        public System.Collections.Generic.IReadOnlyList<UceCanFrame> Frames { get; set; }
    }

    public sealed class UceCanFrame
    {
        public uint Id { get; set; }
        public bool Extended { get; set; }
        public bool RemoteRequest { get; set; }
        public byte Dlc { get; set; }
        public byte[] Data { get; set; }
    }

    public sealed class UceCanDriverLogPollResponse
    {
        public UceCanController Controller { get; set; }
        public System.Collections.Generic.IReadOnlyList<UceCanDriverLogEntry> Entries { get; set; }
    }

    public sealed class UceCanDriverLogEntry
    {
        public byte TimestampLow { get; set; }
        public byte EventCode { get; set; }
        public UceCanInterfaceState InterfaceState { get; set; }
        public int BitrateKbps { get; set; }
        public UceCanMode Mode { get; set; }
        public byte Detail0 { get; set; }
        public byte Detail1 { get; set; }
        public byte Detail2 { get; set; }
    }

    public sealed class UceCanTxResponse
    {
        public UceCanController Controller { get; set; }
        public byte TxStatus { get; set; }
        public byte SequenceOrSlot { get; set; }
    }

    public sealed class UceCanTxStopResponse
    {
        public UceCanController Controller { get; set; }
        public byte TxStatus { get; set; }
    }

    public sealed class UceCanReadAllResponse
    {
        public bool Accepted { get; set; }
    }

    public sealed class UceDispatcherOverflowDiagnostic
    {
        public uint OverflowCount { get; set; }
        public byte QueueSize { get; set; }
        public byte MaxEventSize { get; set; }
    }
}
