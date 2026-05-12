using System;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939RawValueReader
    {
        public ulong ReadUnsigned(byte[] payload, int startByte, int? startBit, int bitLength)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (startByte < 1 || bitLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(startByte));

            int bitOffset = (startByte - 1) * 8 + ((startBit ?? 1) - 1);
            ulong value = 0;
            for (int bitIndex = 0; bitIndex < bitLength; ++bitIndex)
            {
                int absoluteBit = bitOffset + bitIndex;
                int byteIndex = absoluteBit / 8;
                int bitInByte = absoluteBit % 8;
                if (byteIndex >= payload.Length)
                    break;

                if (((payload[byteIndex] >> bitInByte) & 0x01) != 0)
                    value |= 1UL << bitIndex;
            }

            return value;
        }

        public string ReadAscii(byte[] payload, int startByte, int byteLength)
        {
            if (payload == null || startByte < 1 || byteLength <= 0)
                return string.Empty;

            int offset = startByte - 1;
            int length = Math.Min(byteLength, Math.Max(0, payload.Length - offset));
            char[] chars = new char[length];
            for (int i = 0; i < length; ++i)
            {
                byte value = payload[offset + i];
                chars[i] = value == 0xFF ? ' ' : (char)value;
            }

            return new string(chars).Trim();
        }

        public static string ToRawHex(ulong value, int bitLength)
        {
            int digits = Math.Max(2, (bitLength + 3) / 4);
            return "0x" + value.ToString("X" + digits.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
