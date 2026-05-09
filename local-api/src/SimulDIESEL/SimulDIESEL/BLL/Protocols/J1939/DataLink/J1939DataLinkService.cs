using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939DataLinkService
    {
        private readonly J1939IdParser _idParser;
        private readonly J1939MessageTypeClassifier _messageTypeClassifier;
        private readonly J1939TransportProtocolService _transportProtocolService;

        public J1939DataLinkService()
            : this(new J1939IdParser(), new J1939MessageTypeClassifier(), new J1939TransportProtocolService())
        {
        }

        public J1939DataLinkService(
            J1939IdParser idParser,
            J1939MessageTypeClassifier messageTypeClassifier,
            J1939TransportProtocolService transportProtocolService)
        {
            _idParser = idParser ?? throw new ArgumentNullException(nameof(idParser));
            _messageTypeClassifier = messageTypeClassifier ?? throw new ArgumentNullException(nameof(messageTypeClassifier));
            _transportProtocolService = transportProtocolService ?? throw new ArgumentNullException(nameof(transportProtocolService));
        }

        public J1939DataLinkProcessingResultDto ProcessCanFrame(CanFrameDto frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            return ProcessFrame(frame.CanId, frame.IsExtended, frame.Dlc, frame.Data, frame.Timestamp);
        }

        public J1939DataLinkProcessingResultDto ProcessCanRow(CanRowDto row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            return ProcessFrame(row.CanId, row.IsExtended, row.Dlc, row.Data, DateTime.MinValue);
        }

        public IReadOnlyList<J1939DataLinkProcessingResultDto> ProcessSnapshot(IEnumerable<CanRowDto> rows)
        {
            List<J1939DataLinkProcessingResultDto> results = new List<J1939DataLinkProcessingResultDto>();
            if (rows == null)
                return results;

            foreach (CanRowDto row in rows)
            {
                if (row == null || !row.Valid)
                    continue;

                results.Add(ProcessCanRow(row));
            }

            return results;
        }

        public IReadOnlyList<J1939TransportSessionDto> CheckTimeouts(DateTime now)
        {
            return _transportProtocolService.SessionManager.CheckTimeouts(now);
        }

        private J1939DataLinkProcessingResultDto ProcessFrame(uint canId, bool isExtended, byte dlc, byte[] data, DateTime timestamp)
        {
            if (!isExtended)
                return BuildUnsupportedStandardFrame(canId, dlc, data, timestamp);

            J1939IdFieldsDto idFields = _idParser.Parse(canId);
            if (idFields.IsIso15765Frame)
                return BuildNonJ1939Result(canId, dlc, data, timestamp, idFields, J1939MessageTypeDto.Iso15765, J1939Constants.StatusIso15765Frame, "EDP=1 e DP=1: quadro ISO 15765-3.");

            if (idFields.IsReservedEdpDpCombination)
                return BuildNonJ1939Result(canId, dlc, data, timestamp, idFields, J1939MessageTypeDto.Reserved, J1939Constants.StatusReservedEdpDpCombination, "EDP=1 e DP=0: combinacao reservada.");

            J1939MessageTypeDto messageType = _messageTypeClassifier.Classify(idFields);
            J1939DataLinkMessageDto message = BuildMessage(canId, dlc, data, timestamp, idFields, messageType);
            if (messageType == J1939MessageTypeDto.TransportConnectionManagement)
                return _transportProtocolService.ProcessTransportConnectionManagement(message, ResolveTimestamp(timestamp));

            if (messageType == J1939MessageTypeDto.TransportDataTransfer)
                return _transportProtocolService.ProcessTransportDataTransfer(message, ResolveTimestamp(timestamp));

            return new J1939DataLinkProcessingResultDto
            {
                IsJ1939 = true,
                Status = Status(J1939Constants.StatusSingleFrameDecoded, "Mensagem J1939 single-frame decodificada estruturalmente.", false),
                RawCanId = canId,
                IdFields = idFields,
                Pgn = idFields.Pgn,
                FormattedPgn = idFields.FormattedPgn,
                MessageType = messageType,
                IsSingleFrame = true,
                IsTransportProtocol = false,
                SingleFrameMessage = message,
                DiagnosticText = "Mensagem J1939 single-frame decodificada estruturalmente."
            };
        }

        private static J1939DataLinkMessageDto BuildMessage(uint canId, byte dlc, byte[] data, DateTime timestamp, J1939IdFieldsDto idFields, J1939MessageTypeDto messageType)
        {
            return new J1939DataLinkMessageDto
            {
                CanId = canId,
                Dlc = dlc > 8 ? (byte)8 : dlc,
                Data = NormalizeData(data),
                Timestamp = timestamp,
                IdFields = idFields,
                Pgn = idFields.Pgn,
                FormattedPgn = idFields.FormattedPgn,
                MessageType = messageType,
                IsSingleFrame = messageType != J1939MessageTypeDto.TransportConnectionManagement &&
                    messageType != J1939MessageTypeDto.TransportDataTransfer,
                Status = Status(J1939Constants.StatusOk, "J1939-21 estrutural.", false)
            };
        }

        private static J1939DataLinkProcessingResultDto BuildUnsupportedStandardFrame(uint canId, byte dlc, byte[] data, DateTime timestamp)
        {
            return new J1939DataLinkProcessingResultDto
            {
                IsJ1939 = false,
                Status = Status(J1939Constants.StatusUnsupportedStandardFrame, "Frame CAN STD 11-bit nao segue J1939 padronizado.", false),
                RawCanId = canId,
                MessageType = J1939MessageTypeDto.NotJ1939,
                DiagnosticText = "Frame CAN STD 11-bit nao segue J1939 padronizado.",
                SingleFrameMessage = new J1939DataLinkMessageDto
                {
                    CanId = canId,
                    Dlc = dlc > 8 ? (byte)8 : dlc,
                    Data = NormalizeData(data),
                    Timestamp = timestamp,
                    MessageType = J1939MessageTypeDto.NotJ1939,
                    Status = Status(J1939Constants.StatusNotExtendedFrame, "Frame CAN STD 11-bit.", false)
                }
            };
        }

        private static J1939DataLinkProcessingResultDto BuildNonJ1939Result(
            uint canId,
            byte dlc,
            byte[] data,
            DateTime timestamp,
            J1939IdFieldsDto idFields,
            J1939MessageTypeDto messageType,
            string statusCode,
            string statusText)
        {
            return new J1939DataLinkProcessingResultDto
            {
                IsJ1939 = false,
                Status = Status(statusCode, statusText, false),
                RawCanId = canId,
                IdFields = idFields,
                Pgn = idFields.Pgn,
                FormattedPgn = idFields.FormattedPgn,
                MessageType = messageType,
                DiagnosticText = statusText,
                SingleFrameMessage = BuildMessage(canId, dlc, data, timestamp, idFields, messageType)
            };
        }

        private static J1939ProcessingStatusDto Status(string code, string text, bool isError)
        {
            return new J1939ProcessingStatusDto
            {
                Code = code,
                Text = text,
                IsError = isError
            };
        }

        private static byte[] NormalizeData(byte[] data)
        {
            byte[] normalized = new byte[8];
            if (data != null)
                Array.Copy(data, normalized, Math.Min(8, data.Length));

            return normalized;
        }

        private static DateTime ResolveTimestamp(DateTime timestamp)
        {
            return timestamp == default(DateTime) ? DateTime.Now : timestamp;
        }
    }
}
