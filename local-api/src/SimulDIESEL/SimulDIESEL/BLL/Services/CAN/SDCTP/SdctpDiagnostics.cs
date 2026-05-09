using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    public static class SdctpDiagnostics
    {
        public const string DispatcherFifoOverflow = "DISPATCHER_FIFO_OVERFLOW";
        public const string MirrorOutOfSync = "MIRROR_OUT_OF_SYNC";
        public const string CanTicInvalidPayload = "CAN_TIC_INVALID_PAYLOAD";
        public const string CanEditTruncated = "CAN_EDIT_TRUNCATED";
        public const string CanEditInvalidDataMask = "CAN_EDIT_INVALID_DATA_MASK";
        public const string OutputBufferOverflow = "OUTPUT_BUFFER_OVERFLOW";
        public const byte DispatcherFifoOverflowCode = GwProtocol.UceTransportDiagDispatcherFifoOverflow;
    }
}
