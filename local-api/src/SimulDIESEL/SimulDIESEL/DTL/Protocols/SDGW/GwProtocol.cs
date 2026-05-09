namespace SimulDIESEL.DTL.Protocols.SDGW
{
    /// <summary>
    /// Contrato compacto oficial compartilhado entre SDH host e BPM.
    /// </summary>
    public static class GwProtocol
    {
        public const byte BpmAddress = 0x0;
        public const byte GsaAddress = 0x1;
        public const byte UceAddress = 0x2;
        public const byte BroadcastAddress = 0xF;

        public const byte BpmPingOp = 0x0;
        public const byte GsaTlvTransactOp = 0x0;
        public const byte UceTlvTransactOp = 0x0;

        // O LED builtin permanece em 0x12 por contrato.
        // O status por canal foi migrado para 0x1B para remover a
        // ambiguidade histórica do contrato TLV da GSA.
        public const byte GsaSetLedType = 0x12;
        public const byte GsaChannelSetpointType = 0x10;
        public const byte GsaChannelEnableType = 0x11;
        public const byte GsaChannelStatusType = 0x1B;
        public const byte GsaChannelsStatusType = 0x13;
        public const byte GsaChannelsEnableType = 0x14;
        public const byte GsaChannelFaultResetType = 0x15;
        public const byte GsaChannelOffsetSetType = 0x16;
        public const byte GsaChannelOffsetGetType = 0x17;
        public const byte GsaChannelOffsetSaveType = 0x18;
        public const byte GsaChannelOffsetResetType = 0x19;
        public const byte GsaOffsetResetType = 0x1A;
        public const byte GsaChannelFaultEventType = 0x30;
        public const byte GsaPhysicalOperationEventType = 0x31;
        public const byte GsaErrorType = 0x7F;
        public const byte UceSetLedType = 0x12;
        public const byte UceLedEventType = 0x13;
        public const byte UceCanConfigType = 0x20;
        public const byte UceCanEnableType = 0x21;
        public const byte UceCanStatusType = 0x22;
        public const byte UceCanResetType = 0x23;
        public const byte UceCanRxPollType = 0x24;
        public const byte UceCanDriverLogPollType = 0x25;
        public const byte UceCanTxType = 0x26;
        public const byte UceCanTxStopType = 0x27;
        public const byte UceCanRxEventType = 0x28;
        public const byte UceCanTxDirectType = 0x50;
        public const byte UceCanTxCreateType = 0x51;
        public const byte UceCanTxEditType = 0x52;
        public const byte UceCanTxDeleteType = 0x53;
        public const byte UceCanCreateType = 0x40;
        public const byte UceCanEditType = 0x41;
        public const byte UceCanDeleteType = 0x42;
        public const byte UceCanReadAllType = 0x43;
        public const byte UceCanRowType = 0x44;
        public const byte UceCanReadAllDoneType = 0x45;
        public const byte UceCanTicType = 0x46;
        public const byte UceTransportDiagType = 0x7E;
        public const byte UceErrorType = 0x7F;
        public const byte GatewayErrorType = 0xFE;

        public const byte UceLedPayloadLength = 0x01;
        public const byte UceLedEventPayloadLength = 0x04;
        public const byte UceCanConfigPayloadLength = 0x03;
        public const byte UceCanConfigWithRxModePayloadLength = 0x04;
        public const byte UceCanEnablePayloadLength = 0x02;
        public const byte UceCanStatusRequestPayloadLength = 0x01;
        public const byte UceCanStatusResponsePayloadLength = 0x04;
        public const byte UceCanResetRequestPayloadLength = 0x01;
        public const byte UceCanResetResponsePayloadLength = 0x02;
        public const byte UceCanRxPollRequestPayloadLength = 0x01;
        // CAN_RX frame payload: id[31:24], id[23:16], id[15:8], id[7:0], flags, dlc, data[8].
        // flags bit0=EXT, bit1=RTR. STD masks ID to 11 bits; EXT masks ID to 29 bits.
        public const byte UceCanRxFrameLength = 0x0E;
        public const byte UceCanRxMaxFramesPerResponse = 0x03;
        public const byte UceCanRxEventHeaderLength = 0x02;
        public const byte UceCanRxEventMaxFrames = 0x01;
        public const byte UceCanCreatePayloadLength = 0x15;
        public const byte UceCanEditPayloadMinLength = 0x06;
        public const byte UceCanEditPayloadMaxLength = 0x17;
        public const byte UceCanDeletePayloadLength = 0x06;
        public const byte UceCanTicPayloadLength = 0x01;
        public const byte UceCanReadAllPayloadLength = 0x00;
        public const byte UceCanRowPayloadLength = 0x15;
        public const byte UceCanReadAllDonePayloadLength = 0x05;
        public const byte UceCanRxMirrorCapacity = 0x64;
        public const byte UceTransportDiagDispatcherFifoOverflowPayloadLength = 0x07;
        public const byte UceCanDriverLogPollRequestPayloadLength = 0x01;
        public const byte UceCanDriverLogEntryLength = 0x08;
        public const byte UceCanDriverLogMaxEntriesPerResponse = 0x06;
        public const byte UceCanTxRequestPayloadLength = 0x11;
        public const byte UceCanTxResponsePayloadLength = 0x03;
        public const byte UceCanTxStopRequestPayloadLength = 0x02;
        public const byte UceCanTxStopResponsePayloadLength = 0x02;
        public const byte UceCanTxDirectPayloadLength = 0x0E;
        public const byte UceCanTxCreatePayloadLength = 0x12;
        public const byte UceCanTxEditPayloadMinLength = 0x02;
        public const byte UceCanTxEditPayloadMaxLength = 0x15;
        public const byte UceCanTxDeletePayloadLength = 0x02;
        public const byte UceCanTxStopAllSlots = 0xFF;

        public const byte UceCanControllerCan0 = 0x00;
        public const byte UceCanControllerCan1 = 0x01;
        public const byte UceCanBitrate5Code = 0x00;
        public const byte UceCanBitrate10Code = 0x01;
        public const byte UceCanBitrate25Code = 0x02;
        public const byte UceCanBitrate50Code = 0x03;
        public const byte UceCanBitrate125Code = 0x04;
        public const byte UceCanBitrate250Code = 0x05;
        public const byte UceCanBitrate500Code = 0x06;
        public const byte UceCanBitrate800Code = 0x07;
        public const byte UceCanBitrate1000Code = 0x08;
        public const byte UceCanModeNormal = 0x00;
        public const byte UceCanModeListen = 0x01;
        public const byte UceCanModeLoopback = 0x02;
        public const byte UceCanRxModeAuto = 0x00;
        public const byte UceCanRxModeDirectOnly = 0x01;
        public const byte UceCanStateOff = 0x00;
        public const byte UceCanStateOn = 0x01;
        public const byte UceCanInterfaceDisabled = 0x00;
        public const byte UceCanInterfaceConfigured = 0x01;
        public const byte UceCanInterfaceOpen = 0x02;
        public const byte UceCanInterfaceFault = 0x03;
        public const byte UceCanResetFailed = 0x00;
        public const byte UceCanResetSucceeded = 0x01;
        public const byte UceCanDriverEventBegin = 0x01;
        public const byte UceCanDriverEventConfigRequested = 0x02;
        public const byte UceCanDriverEventConfigOk = 0x03;
        public const byte UceCanDriverEventConfigFault = 0x04;
        public const byte UceCanDriverEventOpenRequested = 0x05;
        public const byte UceCanDriverEventOpenOk = 0x06;
        public const byte UceCanDriverEventOpenFault = 0x07;
        public const byte UceCanDriverEventCloseRequested = 0x08;
        public const byte UceCanDriverEventCloseOk = 0x09;
        public const byte UceCanDriverEventResetRequested = 0x0A;
        public const byte UceCanDriverEventResetOk = 0x0B;
        public const byte UceCanDriverEventStatusSnapshot = 0x0C;
        public const byte UceCanDriverEventRxPoll = 0x0D;
        public const byte UceCanDriverEventRxFrameRead = 0x0E;
        public const byte UceCanDriverEventUnsupportedController = 0x0F;
        public const byte UceCanDriverEventInvalidBitrate = 0x10;
        public const byte UceCanDriverEventInvalidMode = 0x11;
        public const byte UceCanDriverEventCanPhysicalError = 0x12;
        public const byte UceCanDriverEventLoopbackDropped = 0x16;
        public const byte UceCanTxStatusAcceptedSent = 0x00;
        public const byte UceCanTxStatusInvalidPayload = 0x01;
        public const byte UceCanTxStatusControllerDisabled = 0x02;
        public const byte UceCanTxStatusFailed = 0x03;
        public const byte UceCanTxStatusPeriodicStarted = 0x04;
        public const byte UceCanTxStatusPeriodicStopped = 0x05;
        public const byte UceCanTxStatusNoFreePeriodicSlot = 0x06;
        public const byte UceCanTxStatusLineMissing = 0x07;
        public const byte UceCanTxEditMaskFlags = 0x01;
        public const byte UceCanTxEditMaskCanId = 0x02;
        public const byte UceCanTxEditMaskDlc = 0x04;
        public const byte UceCanTxEditMaskData = 0x08;
        public const byte UceCanTxEditMaskPeriodMs = 0x10;
        public const byte UceCanTxEditMaskEnabled = 0x20;
        public const byte UceCanTxDeleteReasonUserDelete = 0x01;
        public const byte UceCanTxDeleteReasonTableClear = 0x02;
        public const byte UceCanTxDeleteReasonReset = 0x03;
        public const byte UceCanTxDeleteReasonDisable = 0x04;
        public const byte UceCanCrudEditMaskFlags = 0x01;
        public const byte UceCanCrudEditMaskCanId = 0x02;
        public const byte UceCanCrudEditMaskDlc = 0x04;
        public const byte UceCanCrudEditMaskData = 0x08;
        public const byte UceCanCrudEditMaskCycleTime = 0x10;
        public const byte UceCanDeleteReasonTimeout = 0x01;
        public const byte UceCanDeleteReasonReset = 0x02;
        public const byte UceCanDeleteReasonTableClear = 0x03;
        public const byte UceCanDeleteReasonManualDelete = 0x04;
        public const byte UceTransportDiagDispatcherFifoOverflow = 0x01;

        public const byte GsaOffsetKindVout = 0x01;
        public const byte GsaOffsetKindVread = 0x02;
        public const byte GsaOffsetKindIread = 0x03;

        public static byte MakeCompactCommand(byte address, byte op)
        {
            return (byte)(((address & 0x0F) << 4) | (op & 0x0F));
        }
    }
}
