using System;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DiagnosticsService
    {
        private readonly J1939Dm1Decoder _dm1Decoder;
        private readonly J1939Dm2Decoder _dm2Decoder;

        public J1939DiagnosticsService()
            : this(new J1939Dm1Decoder(), new J1939Dm2Decoder())
        {
        }

        public J1939DiagnosticsService(J1939Dm1Decoder dm1Decoder, J1939Dm2Decoder dm2Decoder)
        {
            _dm1Decoder = dm1Decoder ?? throw new ArgumentNullException(nameof(dm1Decoder));
            _dm2Decoder = dm2Decoder ?? throw new ArgumentNullException(nameof(dm2Decoder));
        }

        public bool TryDecode(J1939DataLinkProcessingResultDto result, out J1939DiagnosticMessageDto message)
        {
            message = null;
            if (result == null)
                return false;

            if (result.ReassembledMessage != null)
                return TryDecode(result.ReassembledMessage, out message);

            if (result.SingleFrameMessage != null)
                return TryDecode(result.SingleFrameMessage, out message);

            return false;
        }

        public bool TryDecode(J1939DataLinkMessageDto dataLinkMessage, out J1939DiagnosticMessageDto message)
        {
            message = null;
            if (dataLinkMessage == null)
                return false;

            byte sourceAddress = dataLinkMessage.IdFields != null ? dataLinkMessage.IdFields.SourceAddress : (byte)0;
            byte? destinationAddress = dataLinkMessage.IdFields != null ? dataLinkMessage.IdFields.DestinationAddress : null;
            if (dataLinkMessage.Pgn == J1939Dm1Decoder.Pgn)
            {
                message = _dm1Decoder.Decode(sourceAddress, destinationAddress, dataLinkMessage.Timestamp, TrimPayload(dataLinkMessage.Data, dataLinkMessage.Dlc), false);
                return true;
            }

            if (dataLinkMessage.Pgn == J1939Dm2Decoder.Pgn)
            {
                message = _dm2Decoder.Decode(sourceAddress, destinationAddress, dataLinkMessage.Timestamp, TrimPayload(dataLinkMessage.Data, dataLinkMessage.Dlc), false);
                return true;
            }

            return false;
        }

        public bool TryDecode(J1939ReassembledMessageDto reassembledMessage, out J1939DiagnosticMessageDto message)
        {
            message = null;
            if (reassembledMessage == null)
                return false;

            if (reassembledMessage.TransportedPgn == J1939Dm1Decoder.Pgn)
            {
                message = _dm1Decoder.Decode(reassembledMessage.SourceAddress, reassembledMessage.DestinationAddress, DateTime.Now, reassembledMessage.Data, true);
                return true;
            }

            if (reassembledMessage.TransportedPgn == J1939Dm2Decoder.Pgn)
            {
                message = _dm2Decoder.Decode(reassembledMessage.SourceAddress, reassembledMessage.DestinationAddress, DateTime.Now, reassembledMessage.Data, true);
                return true;
            }

            return false;
        }

        private static byte[] TrimPayload(byte[] data, byte dlc)
        {
            int length = Math.Min(dlc, (byte)8);
            byte[] payload = new byte[length];
            if (data != null)
                Array.Copy(data, payload, Math.Min(length, data.Length));

            return payload;
        }
    }
}
