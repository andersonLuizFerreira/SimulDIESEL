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
        private readonly J1939PgnStandardCatalog _standardCatalog;

        public J1939AddressClaimDecoder()
            : this(new J1939NameParser(), new J1939PgnStandardCatalog())
        {
        }

        public J1939AddressClaimDecoder(J1939NameParser nameParser)
            : this(nameParser, new J1939PgnStandardCatalog())
        {
        }

        public J1939AddressClaimDecoder(J1939NameParser nameParser, J1939PgnStandardCatalog standardCatalog)
        {
            _nameParser = nameParser ?? throw new ArgumentNullException(nameof(nameParser));
            _standardCatalog = standardCatalog ?? throw new ArgumentNullException(nameof(standardCatalog));
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
            SimulDIESEL.DTL.Protocols.J1939.Common.J1939PgnDefinitionDto definition = _standardCatalog.FindByPgn((int)AddressClaimedPgn);
            J1939AddressClaimDto dto = new J1939AddressClaimDto
            {
                Pgn = AddressClaimedPgn,
                PgnHex = AddressClaimedPgnHex,
                PgnAcronym = definition != null ? definition.Acronym : "AC",
                PgnLabel = definition != null ? definition.Label : "Address Claimed",
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
