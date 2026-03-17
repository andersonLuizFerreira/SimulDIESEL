using System;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdgwFrameReader
    {
        public bool TryDecode(byte[] encodedFrame, out SggwFrame frame, out string error)
        {
            frame = null;
            error = null;

            if (encodedFrame == null || encodedFrame.Length == 0)
            {
                error = "Frame vazio.";
                return false;
            }

            try
            {
                byte[] decoded = SdgwFrameCodec.CobsDecode(encodedFrame);
                if (decoded.Length < 4)
                {
                    error = "Frame curto (descartado).";
                    return false;
                }

                byte crcRx = decoded[decoded.Length - 1];
                byte crcCalc = SdgwFrameCodec.Crc8Atm(decoded, 0, decoded.Length - 1);
                if (crcRx != crcCalc)
                {
                    error = "CRC inválido (descartado).";
                    return false;
                }

                int payloadLen = decoded.Length - 4;
                byte[] payload = payloadLen > 0 ? Slice(decoded, 3, payloadLen) : Array.Empty<byte>();
                frame = new SggwFrame(decoded[0], decoded[2], decoded[1], payload);
                return true;
            }
            catch (Exception ex)
            {
                error = "COBS decode falhou: " + ex.Message;
                return false;
            }
        }

        private static byte[] Slice(byte[] src, int offset, int len)
        {
            var dst = new byte[len];
            Buffer.BlockCopy(src, offset, dst, 0, len);
            return dst;
        }
    }
}
