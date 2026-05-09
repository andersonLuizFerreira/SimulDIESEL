using System;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939AddressClaimDecoder
    {
        public const uint AddressClaimedPgn = 60928;
        public const string AddressClaimedPgnHex = "00EE00";

        private readonly J1939NameParser _nameParser;

        public J1939AddressClaimDecoder()
            : this(new J1939NameParser())
        {
        }

        public J1939AddressClaimDecoder(J1939NameParser nameParser)
        {
            _nameParser = nameParser ?? throw new ArgumentNullException(nameof(nameParser));
        }

        public bool CanDecode(J1939DataLinkMessageDto message)
        {
            return message != null && message.Pgn == AddressClaimedPgn;
        }

        public J1939AddressClaimDto Decode(J1939DataLinkMessageDto message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            byte sourceAddress = message.IdFields != null ? message.IdFields.SourceAddress : (byte)0;
            J1939AddressClaimDto dto = new J1939AddressClaimDto
            {
                Pgn = AddressClaimedPgn,
                PgnHex = AddressClaimedPgnHex,
                SourceAddress = sourceAddress,
                Timestamp = message.Timestamp == default(DateTime) ? DateTime.Now : message.Timestamp,
                IsCannotClaimAddress = sourceAddress == J1939ToolAddressConfig.NullAddress,
                IsAddressClaimed = sourceAddress <= 0xFD,
                IsInvalidSourceAddress = sourceAddress == J1939ToolAddressConfig.GlobalAddress
            };

            if (message.Data == null || message.Dlc < 8)
            {
                dto.Status = "InvalidAddressClaimPayload";
                return dto;
            }

            dto.Name = _nameParser.Parse(message.Data);
            dto.Status = dto.IsInvalidSourceAddress
                ? "InvalidSourceAddress"
                : (dto.IsCannotClaimAddress ? "CannotClaimAddress" : "AddressClaimed");
            return dto;
        }
    }
}
