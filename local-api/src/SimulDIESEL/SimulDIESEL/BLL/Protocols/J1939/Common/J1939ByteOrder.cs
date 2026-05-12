namespace SimulDIESEL.BLL.Protocols.J1939.Common
{
    public static class J1939ByteOrder
    {
        public static ushort ReadUInt16LittleEndian(byte[] data, int offset)
        {
            if (data == null || offset < 0 || data.Length < offset + 2)
                return 0;

            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }

        public static uint ReadPgnLittleEndian(byte[] data, int offset)
        {
            if (data == null || offset < 0 || data.Length < offset + 3)
                return 0;

            return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16));
        }
    }
}
