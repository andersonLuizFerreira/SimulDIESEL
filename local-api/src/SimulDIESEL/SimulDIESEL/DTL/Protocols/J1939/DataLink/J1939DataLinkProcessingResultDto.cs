using SimulDIESEL.DTL.Protocols.J1939.Common;

namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public sealed class J1939DataLinkProcessingResultDto
    {
        public bool IsJ1939 { get; set; }
        public J1939ProcessingStatusDto Status { get; set; }
        public uint RawCanId { get; set; }
        public J1939IdFieldsDto IdFields { get; set; }
        public uint Pgn { get; set; }
        public string FormattedPgn { get; set; }
        public J1939MessageTypeDto MessageType { get; set; }
        public bool IsSingleFrame { get; set; }
        public bool IsTransportProtocol { get; set; }
        public bool IsTransportSessionComplete { get; set; }
        public J1939DataLinkMessageDto SingleFrameMessage { get; set; }
        public J1939TransportControlMessageDto TransportControlMessage { get; set; }
        public J1939TransportDataPacketDto TransportDataPacket { get; set; }
        public J1939TransportSessionDto TransportSession { get; set; }
        public J1939ReassembledMessageDto ReassembledMessage { get; set; }
        public string DiagnosticText { get; set; }
    }
}
