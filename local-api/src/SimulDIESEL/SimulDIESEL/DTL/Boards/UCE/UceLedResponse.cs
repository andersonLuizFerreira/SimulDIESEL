namespace SimulDIESEL.DTL.Boards.UCE
{
    public sealed class UceLedResponse
    {
        public bool AcceptedState { get; set; }
    }

    public sealed class UceLedEvent
    {
        public bool LedState { get; set; }
        public byte EventCode { get; set; }
        public ushort Counter { get; set; }
    }
}
