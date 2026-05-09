using System.Globalization;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939IdParser
    {
        public J1939IdFieldsDto Parse(uint canId)
        {
            uint normalizedId = canId & J1939Constants.ExtendedCanIdMask;
            byte priority = (byte)((normalizedId >> 26) & 0x07);
            bool extendedDataPage = ((normalizedId >> 25) & 0x01) != 0;
            bool dataPage = ((normalizedId >> 24) & 0x01) != 0;
            byte pduFormat = (byte)((normalizedId >> 16) & 0xFF);
            byte pduSpecific = (byte)((normalizedId >> 8) & 0xFF);
            byte sourceAddress = (byte)(normalizedId & 0xFF);
            bool isPdu1 = pduFormat < J1939Constants.Pdu1Threshold;
            uint pageBits = (uint)(((extendedDataPage ? 1 : 0) << 17) | ((dataPage ? 1 : 0) << 16));
            uint pgn = isPdu1
                ? pageBits | (uint)(pduFormat << 8)
                : pageBits | (uint)(pduFormat << 8) | pduSpecific;

            return new J1939IdFieldsDto
            {
                CanId = normalizedId,
                Priority = priority,
                ExtendedDataPage = extendedDataPage,
                DataPage = dataPage,
                PduFormat = pduFormat,
                PduSpecific = pduSpecific,
                SourceAddress = sourceAddress,
                Pgn = pgn,
                FormattedPgn = FormatPgn(pgn),
                DestinationAddress = isPdu1 ? (byte?)pduSpecific : null,
                GroupExtension = isPdu1 ? null : (byte?)pduSpecific,
                IsPdu1 = isPdu1,
                IsPdu2 = !isPdu1,
                IsGlobalDestination = isPdu1 && pduSpecific == J1939Constants.GlobalDestinationAddress,
                IsIso15765Frame = extendedDataPage && dataPage,
                IsReservedEdpDpCombination = extendedDataPage && !dataPage
            };
        }

        public static string FormatPgn(uint pgn)
        {
            return pgn.ToString("X6", CultureInfo.InvariantCulture);
        }
    }
}
