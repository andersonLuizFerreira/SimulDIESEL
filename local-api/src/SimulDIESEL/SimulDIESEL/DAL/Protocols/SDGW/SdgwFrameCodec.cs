using System;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public static class SdgwFrameCodec
    {
        public static byte Crc8Atm(byte[] data, int offset, int len)
        {
            return SdGwLinkEngine.Crc8Atm(data, offset, len);
        }

        public static byte[] CobsEncode(byte[] input)
        {
            return SdGwLinkEngine.CobsEncode(input);
        }

        public static byte[] CobsDecode(byte[] input)
        {
            return SdGwLinkEngine.CobsDecode(input);
        }
    }
}
