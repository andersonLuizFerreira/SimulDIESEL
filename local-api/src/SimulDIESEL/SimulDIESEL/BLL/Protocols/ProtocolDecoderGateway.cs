using System;
using SimulDIESEL.BLL.Protocols.J1939;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939;

namespace SimulDIESEL.BLL.Protocols
{
    public sealed class ProtocolDecoderGateway
    {
        private readonly J1939ProtocolService _j1939ProtocolService;

        public ProtocolDecoderGateway()
            : this(new J1939ProtocolService())
        {
        }

        public ProtocolDecoderGateway(J1939ProtocolService j1939ProtocolService)
        {
            _j1939ProtocolService = j1939ProtocolService ?? throw new ArgumentNullException(nameof(j1939ProtocolService));
        }

        public J1939DecodedMessageDto DecodeCanFrame(CanFrameDto frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            if (!frame.IsExtended)
                return BuildUnsupported(frame.CanId, frame.Dlc, frame.Data, frame.Timestamp);

            return _j1939ProtocolService.Decode(frame);
        }

        public J1939DecodedMessageDto DecodeCanRow(CanRowDto row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            if (!row.IsExtended)
                return BuildUnsupported(row.CanId, row.Dlc, row.Data, DateTime.MinValue);

            return _j1939ProtocolService.Decode(row);
        }

        private static J1939DecodedMessageDto BuildUnsupported(uint canId, byte dlc, byte[] data, DateTime timestamp)
        {
            return new J1939DecodedMessageDto
            {
                CanId = canId,
                Dlc = dlc > 8 ? (byte)8 : dlc,
                Data = NormalizeData(data),
                Timestamp = timestamp,
                IsStructurallyDecoded = false,
                StatusText = "Mensagem CAN STD 11-bit nao suportada pelo gateway de protocolos nesta etapa."
            };
        }

        private static byte[] NormalizeData(byte[] data)
        {
            byte[] normalized = new byte[8];
            if (data != null)
                Array.Copy(data, normalized, Math.Min(8, data.Length));

            return normalized;
        }
    }
}
