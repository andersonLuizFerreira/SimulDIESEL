using SimulDIESEL.DTL.Protocols.J1939;

namespace SimulDIESEL.BLL.Protocols.J1939
{
    public sealed class J1939IdParser
    {
        private const uint ExtendedCanIdMask = 0x1FFFFFFF;
        private const byte Pdu1Threshold = 240;
        private const byte GlobalDestinationAddress = 0xFF;

        public J1939IdFieldsDto Parse(uint canId)
        {
            uint normalizedId = canId & ExtendedCanIdMask;
            byte priority = (byte)((normalizedId >> 26) & 0x07);
            bool reserved = ((normalizedId >> 25) & 0x01) != 0;
            bool dataPage = ((normalizedId >> 24) & 0x01) != 0;
            byte pduFormat = (byte)((normalizedId >> 16) & 0xFF);
            byte pduSpecific = (byte)((normalizedId >> 8) & 0xFF);
            byte sourceAddress = (byte)(normalizedId & 0xFF);
            bool isPdu1 = pduFormat < Pdu1Threshold;
            uint pageBits = (uint)(((reserved ? 1 : 0) << 17) | ((dataPage ? 1 : 0) << 16));
            uint pgn = isPdu1
                ? pageBits | (uint)(pduFormat << 8)
                : pageBits | (uint)(pduFormat << 8) | pduSpecific;

            return new J1939IdFieldsDto
            {
                CanId = normalizedId,
                Priority = priority,
                Reserved = reserved,
                DataPage = dataPage,
                PduFormat = pduFormat,
                PduSpecific = pduSpecific,
                SourceAddress = sourceAddress,
                Pgn = pgn,
                DestinationAddress = isPdu1 ? (byte?)pduSpecific : null,
                IsPdu1 = isPdu1,
                IsPdu2 = !isPdu1,
                IsGlobalDestination = isPdu1 && pduSpecific == GlobalDestinationAddress
            };
        }
    }
}
