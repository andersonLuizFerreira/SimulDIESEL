using System;
using System.Collections.Generic;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.J1939;

namespace SimulDIESEL.BLL.Services.J1939
{
    public sealed class J1939DecodeService
    {
        private readonly J1939IdParser _idParser;

        public J1939DecodeService()
            : this(new J1939IdParser())
        {
        }

        public J1939DecodeService(J1939IdParser idParser)
        {
            _idParser = idParser ?? throw new ArgumentNullException(nameof(idParser));
        }

        public J1939DecodedMessageDto Decode(CanFrameDto frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            return DecodeFrame(
                frame.CanId,
                frame.IsExtended,
                frame.Dlc,
                frame.Data,
                frame.Timestamp);
        }

        public J1939DecodedMessageDto Decode(CanRowDto row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            return DecodeFrame(
                row.CanId,
                row.IsExtended,
                row.Dlc,
                row.Data,
                DateTime.MinValue);
        }

        public IReadOnlyList<J1939DecodedMessageDto> DecodeSnapshot(IEnumerable<CanRowDto> rows)
        {
            List<J1939DecodedMessageDto> decoded = new List<J1939DecodedMessageDto>();
            if (rows == null)
                return decoded;

            foreach (CanRowDto row in rows)
            {
                if (row == null || !row.Valid)
                    continue;

                decoded.Add(Decode(row));
            }

            return decoded;
        }

        private J1939DecodedMessageDto DecodeFrame(uint canId, bool isExtended, byte dlc, byte[] data, DateTime timestamp)
        {
            J1939DecodedMessageDto decoded = new J1939DecodedMessageDto
            {
                Dlc = dlc > 8 ? (byte)8 : dlc,
                Data = NormalizeData(data),
                Timestamp = timestamp,
                IsValidJ1939 = isExtended,
                ValidationMessage = isExtended ? "OK" : "J1939 requer CAN ID estendido de 29 bits."
            };

            if (isExtended)
                decoded.IdFields = _idParser.Parse(canId);

            return decoded;
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
