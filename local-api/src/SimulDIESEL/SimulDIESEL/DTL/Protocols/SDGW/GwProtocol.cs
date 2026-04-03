namespace SimulDIESEL.DTL.Protocols.SDGW
{
    /// <summary>
    /// Contrato compacto oficial compartilhado entre SDH host e BPM.
    /// </summary>
    public static class GwProtocol
    {
        public const byte BpmAddress = 0x0;
        public const byte GsaAddress = 0x1;
        public const byte BroadcastAddress = 0xF;

        public const byte BpmPingOp = 0x0;
        public const byte GsaTlvTransactOp = 0x0;

        // O LED builtin legado permanece em 0x12.
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
        public const byte GatewayErrorType = 0xFE;

        public const byte GsaOffsetKindVout = 0x01;
        public const byte GsaOffsetKindVread = 0x02;
        public const byte GsaOffsetKindIread = 0x03;

        public static byte MakeCompactCommand(byte address, byte op)
        {
            return (byte)(((address & 0x0F) << 4) | (op & 0x0F));
        }
    }
}
