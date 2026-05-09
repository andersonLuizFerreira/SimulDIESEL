using System;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939Dm2Decoder : J1939Dm1Decoder
    {
        public new const uint Pgn = 65227;
        public new const string PgnHex = "00FECB";

        public J1939Dm2Decoder()
        {
        }

        public J1939Dm2Decoder(J1939LampStatusDecoder lampStatusDecoder, J1939DtcParser dtcParser)
            : base(lampStatusDecoder, dtcParser)
        {
        }

        public new J1939DiagnosticMessageDto Decode(byte sourceAddress, byte? destinationAddress, DateTime timestamp, byte[] payload, bool isReassembled)
        {
            return DecodeCommon("DM2", Pgn, PgnHex, sourceAddress, destinationAddress, timestamp, payload, isReassembled);
        }
    }
}
