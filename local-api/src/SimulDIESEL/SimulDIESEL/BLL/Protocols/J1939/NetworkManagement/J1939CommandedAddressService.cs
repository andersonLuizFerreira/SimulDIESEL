using System;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939CommandedAddressService
    {
        public const uint CommandedAddressPgn = 65240;
        public const string CommandedAddressPgnHex = "00FED8";

        public byte[] BuildCommandedAddressPayload(byte[] targetNameBytes, byte newSourceAddress)
        {
            if (targetNameBytes == null || targetNameBytes.Length < 8)
                throw new ArgumentException("Commanded Address requer NAME alvo com 8 bytes.", nameof(targetNameBytes));

            byte[] payload = new byte[9];
            Array.Copy(targetNameBytes, payload, 8);
            payload[8] = newSourceAddress;
            return payload;
        }
    }
}
