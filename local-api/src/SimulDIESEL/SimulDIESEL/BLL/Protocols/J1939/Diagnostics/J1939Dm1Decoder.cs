using System;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public class J1939Dm1Decoder
    {
        public const uint Pgn = 65226;
        public const string PgnHex = "00FECA";

        private readonly J1939LampStatusDecoder _lampStatusDecoder;
        private readonly J1939DtcParser _dtcParser;

        public J1939Dm1Decoder()
            : this(new J1939LampStatusDecoder(), new J1939DtcParser())
        {
        }

        public J1939Dm1Decoder(J1939LampStatusDecoder lampStatusDecoder, J1939DtcParser dtcParser)
        {
            _lampStatusDecoder = lampStatusDecoder ?? throw new ArgumentNullException(nameof(lampStatusDecoder));
            _dtcParser = dtcParser ?? throw new ArgumentNullException(nameof(dtcParser));
        }

        public J1939DiagnosticMessageDto Decode(byte sourceAddress, byte? destinationAddress, DateTime timestamp, byte[] payload, bool isReassembled)
        {
            return DecodeCommon("DM1", Pgn, PgnHex, sourceAddress, destinationAddress, timestamp, payload, isReassembled);
        }

        protected J1939DiagnosticMessageDto DecodeCommon(string type, uint pgn, string pgnHex, byte sourceAddress, byte? destinationAddress, DateTime timestamp, byte[] payload, bool isReassembled)
        {
            byte[] data = payload ?? new byte[0];
            J1939DiagnosticMessageDto message = new J1939DiagnosticMessageDto
            {
                Type = type,
                Pgn = pgn,
                PgnHex = pgnHex,
                SourceAddress = sourceAddress,
                DestinationAddress = destinationAddress,
                Timestamp = timestamp == default(DateTime) ? DateTime.Now : timestamp,
                IsReassembled = isReassembled,
                RawPayload = data,
                Status = "Ok"
            };

            if (data.Length < 2)
            {
                message.Status = "InvalidDiagnosticPayload";
                message.LampStatus = _lampStatusDecoder.Decode(0xFF, 0xFF);
                return message;
            }

            message.LampStatus = _lampStatusDecoder.Decode(data[0], data[1]);
            for (int offset = 2; offset + 3 < data.Length; offset += 4)
            {
                byte b1 = data[offset];
                byte b2 = data[offset + 1];
                byte b3 = data[offset + 2];
                byte b4 = data[offset + 3];
                if (_dtcParser.IsNoDtc(b1, b2, b3, b4))
                    continue;

                message.Dtcs.Add(_dtcParser.Parse(b1, b2, b3, b4));
            }

            message.HasDtcs = message.Dtcs.Count > 0;
            message.Status = message.HasDtcs ? "DtcDecoded" : "Sem DTC";
            return message;
        }
    }
}
