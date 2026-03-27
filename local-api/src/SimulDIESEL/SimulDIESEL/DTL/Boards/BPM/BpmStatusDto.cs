using SimulDIESEL.DAL.Transport;

namespace SimulDIESEL.DTL.Boards.BPM
{
    public sealed class BpmStatusDto
    {
        public bool IsConnected { get; set; }
        public bool IsLinked { get; set; }
        public string InterfaceName { get; set; }
        public string LinkState { get; set; }
        public TransportKind TransportKind { get; set; }
        public string TransportDisplayName { get; set; }
    }
}
