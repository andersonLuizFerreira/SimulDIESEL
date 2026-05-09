using System;
using SimulDIESEL.DTL.Protocols.J1939.Application;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939ApplicationLayerService
    {
        private readonly J1939PgnDecoder _pgnDecoder;

        public J1939ApplicationLayerService()
            : this(new J1939PgnDecoder())
        {
        }

        public J1939ApplicationLayerService(J1939PgnDecoder pgnDecoder)
        {
            _pgnDecoder = pgnDecoder ?? throw new ArgumentNullException(nameof(pgnDecoder));
        }

        public J1939ApplicationMessageDto Decode(J1939DataLinkMessageDto message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            byte sourceAddress = message.IdFields != null ? message.IdFields.SourceAddress : (byte)0;
            byte? destinationAddress = message.IdFields != null ? message.IdFields.DestinationAddress : null;
            string pgnHex = message.FormattedPgn ?? message.Pgn.ToString("X6", System.Globalization.CultureInfo.InvariantCulture);
            return _pgnDecoder.Decode((int)message.Pgn, pgnHex, sourceAddress, destinationAddress, message.Timestamp, message.Data);
        }

        public J1939ApplicationMessageDto Decode(J1939ReassembledMessageDto message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            string pgnHex = message.FormattedTransportedPgn ?? message.TransportedPgn.ToString("X6", System.Globalization.CultureInfo.InvariantCulture);
            return _pgnDecoder.Decode((int)message.TransportedPgn, pgnHex, message.SourceAddress, message.DestinationAddress, DateTime.MinValue, message.Data);
        }
    }
}
