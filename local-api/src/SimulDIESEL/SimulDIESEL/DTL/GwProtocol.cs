namespace SimulDIESEL.DTL
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

        public const byte GsaSetLedType = 0x12;

        public static byte MakeCompactCommand(byte address, byte op)
        {
            return (byte)(((address & 0x0F) << 4) | (op & 0x0F));
        }
    }
}
