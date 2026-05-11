using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP command and payload contract facade over the validated TLV constants.
    /// </summary>
    public static class SdctpProtocol
    {
        public const byte CanRxEvent = GwProtocol.UceCanRxEventType;
        public const byte CanCreate = GwProtocol.UceCanCreateType;
        public const byte CanEdit = GwProtocol.UceCanEditType;
        public const byte CanDelete = GwProtocol.UceCanDeleteType;
        public const byte CanRow = GwProtocol.UceCanRowType;
        public const byte CanReadAllDone = GwProtocol.UceCanReadAllDoneType;
        public const byte CanTic = GwProtocol.UceCanTicType;
        public const byte CanTxDirect = GwProtocol.UceCanTxDirectType;
        public const byte CanTxCreate = GwProtocol.UceCanTxCreateType;
        public const byte CanTxEdit = GwProtocol.UceCanTxEditType;
        public const byte CanTxDelete = GwProtocol.UceCanTxDeleteType;
        public const byte TransportDiag = GwProtocol.UceTransportDiagType;

        public const byte RxModeAuto = GwProtocol.UceCanRxModeAuto;
        public const byte RxModeDirectOnly = GwProtocol.UceCanRxModeDirectOnly;
    }
}
