using System;
using SimulDIESEL.DTL.Protocols.J1939.Application;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939PgnDecoder
    {
        private readonly J1939PgnCatalog _catalog;
        private readonly J1939SpnDecoder _spnDecoder;

        public J1939PgnDecoder()
            : this(new J1939PgnCatalog(), new J1939SpnDecoder())
        {
        }

        public J1939PgnDecoder(J1939PgnCatalog catalog, J1939SpnDecoder spnDecoder)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _spnDecoder = spnDecoder ?? throw new ArgumentNullException(nameof(spnDecoder));
        }

        public J1939ApplicationMessageDto Decode(
            int pgn,
            string pgnHex,
            byte sourceAddress,
            byte? destinationAddress,
            DateTime timestamp,
            byte[] payload)
        {
            J1939PgnDefinitionDto definition;
            if (!_catalog.TryGetDefinition(pgn, out definition))
            {
                return new J1939ApplicationMessageDto
                {
                    Pgn = pgn,
                    PgnHex = pgnHex,
                    SourceAddress = sourceAddress,
                    DestinationAddress = destinationAddress,
                    Timestamp = timestamp,
                    IsDecoded = false,
                    Status = "PgnNotSupportedYet",
                    RawPayload = NormalizePayload(payload)
                };
            }

            J1939ApplicationMessageDto message = new J1939ApplicationMessageDto
            {
                Pgn = definition.Pgn,
                PgnHex = definition.Hex,
                PgnName = definition.Name,
                Acronym = definition.Acronym,
                SourceAddress = sourceAddress,
                DestinationAddress = destinationAddress,
                Timestamp = timestamp,
                IsDecoded = true,
                Status = "Decoded",
                RawPayload = NormalizePayload(payload)
            };

            foreach (J1939SpnDefinitionDto spn in definition.Spns)
            {
                message.Signals.Add(_spnDecoder.Decode(spn, message.RawPayload));
            }

            return message;
        }

        private static byte[] NormalizePayload(byte[] payload)
        {
            if (payload == null)
                return new byte[0];

            byte[] copy = new byte[payload.Length];
            Array.Copy(payload, copy, payload.Length);
            return copy;
        }
    }
}
