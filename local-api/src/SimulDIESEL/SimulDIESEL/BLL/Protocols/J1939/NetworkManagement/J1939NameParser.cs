using System;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NameParser
    {
        public J1939NameDto Parse(byte[] payload)
        {
            if (payload == null || payload.Length < 8)
                throw new ArgumentException("NAME J1939-81 deve possuir 8 bytes.", nameof(payload));

            byte[] raw = new byte[8];
            Array.Copy(payload, raw, 8);

            ulong value = 0;
            for (int i = 0; i < 8; ++i)
                value |= ((ulong)raw[i]) << (8 * i);

            return new J1939NameDto
            {
                IdentityNumber = (uint)(value & 0x1FFFFFUL),
                ManufacturerCode = (ushort)((value >> 21) & 0x7FFUL),
                EcuInstance = (byte)((value >> 32) & 0x07UL),
                FunctionInstance = (byte)((value >> 35) & 0x1FUL),
                Function = (byte)((value >> 40) & 0xFFUL),
                Reserved = (byte)((value >> 48) & 0x01UL),
                VehicleSystem = (byte)((value >> 49) & 0x7FUL),
                VehicleSystemInstance = (byte)((value >> 56) & 0x0FUL),
                IndustryGroup = (byte)((value >> 60) & 0x07UL),
                IsArbitraryAddressCapable = ((value >> 63) & 0x01UL) != 0,
                RawNameBytes = raw,
                RawNameUInt64 = value,
                NameHex = value.ToString("X16", System.Globalization.CultureInfo.InvariantCulture)
            };
        }
    }
}
