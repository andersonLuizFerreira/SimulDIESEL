using System;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939TransportProtocolService
    {
        private readonly J1939TransportSessionManager _sessionManager;

        public J1939TransportProtocolService()
            : this(new J1939TransportSessionManager())
        {
        }

        public J1939TransportProtocolService(J1939TransportSessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        public J1939TransportSessionManager SessionManager
        {
            get { return _sessionManager; }
        }

        public J1939DataLinkProcessingResultDto ProcessTransportConnectionManagement(J1939DataLinkMessageDto message, DateTime now)
        {
            J1939TransportControlMessageDto control = ParseControlMessage(message);
            J1939DataLinkProcessingResultDto result = CreateTransportResult(message, control, null);

            if (message.Dlc != 8)
                return WithStatus(result, J1939Constants.StatusInvalidDlc, "TP.CM exige DLC 8.", true);

            if (control.IsBam || control.IsRts)
            {
                if (!IsValidTransportSize(control.TotalMessageSize, control.TotalPackets))
                    return WithStatus(result, J1939Constants.StatusInvalidTransportSize, "Tamanho ou quantidade de pacotes TP invalidos.", true);

                result.TransportSession = _sessionManager.StartSession(control, now);
                string status = control.IsBam ? J1939Constants.StatusTransportBamStarted : J1939Constants.StatusTransportRtsStarted;
                return WithStatus(result, status, status, false);
            }

            if (control.IsAbort)
            {
                result.TransportSession = _sessionManager.RegisterControl(control, now);
                return WithStatus(result, J1939Constants.StatusTransportAbortReceived, "TP.Conn_Abort recebido.", false);
            }

            if (control.IsCts || control.IsEndOfMessageAck)
            {
                result.TransportSession = _sessionManager.RegisterControl(control, now);
                return WithStatus(result, J1939Constants.StatusOk, control.ControlName + " recebido.", false);
            }

            return WithStatus(result, J1939Constants.StatusOk, "TP.CM reconhecido.", false);
        }

        public J1939DataLinkProcessingResultDto ProcessTransportDataTransfer(J1939DataLinkMessageDto message, DateTime now)
        {
            J1939TransportDataPacketDto packet = ParseDataPacket(message);
            J1939DataLinkProcessingResultDto result = CreateTransportResult(message, null, packet);

            if (message.Dlc != 8)
                return WithStatus(result, J1939Constants.StatusInvalidDlc, "TP.DT exige DLC 8.", true);

            uint transportedPgn;
            string transportType;
            J1939TransportSessionDto session = _sessionManager.FindSessionForPacket(packet, out transportedPgn, out transportType);
            if (session == null)
                return WithStatus(result, J1939Constants.StatusTransportOrphanPacket, "TP.DT sem sessao conhecida.", false);

            J1939ReassembledMessageDto reassembled;
            result.TransportSession = _sessionManager.AddDataPacket(packet, transportedPgn, transportType, now, out reassembled);
            result.ReassembledMessage = reassembled;
            result.IsTransportSessionComplete = reassembled != null;

            if (result.TransportSession != null && result.TransportSession.HasSequenceError)
                return WithStatus(result, J1939Constants.StatusTransportSequenceError, "Sequencia TP.DT fora de ordem.", true);

            return WithStatus(
                result,
                reassembled != null ? J1939Constants.StatusTransportCompleted : J1939Constants.StatusTransportInProgress,
                reassembled != null ? "Mensagem multipacote remontada." : "Mensagem multipacote em andamento.",
                false);
        }

        public J1939TransportControlMessageDto ParseControlMessage(J1939DataLinkMessageDto message)
        {
            byte[] data = message.Data ?? new byte[8];
            byte controlByte = data[0];
            uint transportedPgn = J1939ByteOrder.ReadPgnLittleEndian(data, 5);
            J1939TransportControlMessageDto control = new J1939TransportControlMessageDto
            {
                ControlByte = controlByte,
                ControlName = GetControlName(controlByte),
                TransportedPgn = transportedPgn,
                FormattedTransportedPgn = J1939IdParser.FormatPgn(transportedPgn),
                SourceAddress = message.IdFields.SourceAddress,
                DestinationAddress = message.IdFields.DestinationAddress,
                IsBam = controlByte == J1939Constants.TpCmBam,
                IsRts = controlByte == J1939Constants.TpCmRts,
                IsCts = controlByte == J1939Constants.TpCmCts,
                IsEndOfMessageAck = controlByte == J1939Constants.TpCmEndOfMsgAck,
                IsAbort = controlByte == J1939Constants.TpCmConnectionAbort
            };

            if (control.IsBam || control.IsRts || control.IsEndOfMessageAck)
            {
                control.TotalMessageSize = J1939ByteOrder.ReadUInt16LittleEndian(data, 1);
                control.TotalPackets = data[3];
            }

            if (control.IsRts || control.IsBam)
                control.MaxPacketsPerCts = data[4];

            if (control.IsCts)
            {
                control.CtsPacketCount = data[1];
                control.CtsNextPacketNumber = data[2];
            }

            if (control.IsAbort)
                control.AbortReason = data[1];

            return control;
        }

        public J1939TransportDataPacketDto ParseDataPacket(J1939DataLinkMessageDto message)
        {
            byte[] payload = new byte[7];
            byte[] data = message.Data ?? new byte[8];
            for (int i = 0; i < payload.Length; ++i)
                payload[i] = data.Length > i + 1 ? data[i + 1] : (byte)0xFF;

            return new J1939TransportDataPacketDto
            {
                SequenceNumber = data.Length > 0 ? data[0] : (byte)0,
                Payload = payload,
                SourceAddress = message.IdFields.SourceAddress,
                DestinationAddress = message.IdFields.DestinationAddress
            };
        }

        private static bool IsValidTransportSize(ushort totalSize, byte totalPackets)
        {
            return totalSize >= J1939Constants.TransportMinMessageSize &&
                totalSize <= J1939Constants.TransportMaxMessageSize &&
                totalPackets >= J1939Constants.TransportMinPackets &&
                totalPackets <= J1939Constants.TransportMaxPackets;
        }

        private static J1939DataLinkProcessingResultDto CreateTransportResult(
            J1939DataLinkMessageDto message,
            J1939TransportControlMessageDto control,
            J1939TransportDataPacketDto packet)
        {
            return new J1939DataLinkProcessingResultDto
            {
                IsJ1939 = true,
                RawCanId = message.CanId,
                IdFields = message.IdFields,
                Pgn = message.Pgn,
                FormattedPgn = message.FormattedPgn,
                MessageType = message.MessageType,
                IsSingleFrame = false,
                IsTransportProtocol = true,
                TransportControlMessage = control,
                TransportDataPacket = packet,
                DiagnosticText = message.Status != null ? message.Status.Text : null
            };
        }

        private static J1939DataLinkProcessingResultDto WithStatus(J1939DataLinkProcessingResultDto result, string code, string text, bool isError)
        {
            result.Status = new SimulDIESEL.DTL.Protocols.J1939.Common.J1939ProcessingStatusDto
            {
                Code = code,
                Text = text,
                IsError = isError
            };
            result.DiagnosticText = text;
            return result;
        }

        private static string GetControlName(byte controlByte)
        {
            switch (controlByte)
            {
                case J1939Constants.TpCmRts:
                    return "RTS";
                case J1939Constants.TpCmCts:
                    return "CTS";
                case J1939Constants.TpCmEndOfMsgAck:
                    return "EndOfMsgACK";
                case J1939Constants.TpCmBam:
                    return "BAM";
                case J1939Constants.TpCmConnectionAbort:
                    return "Conn_Abort";
                default:
                    return "Unknown";
            }
        }
    }
}
