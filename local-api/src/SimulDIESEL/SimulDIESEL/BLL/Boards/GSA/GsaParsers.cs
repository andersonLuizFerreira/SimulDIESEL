using System;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public static class GsaParsers
    {
        public static bool TryReadBuiltinLedResponse(SggwFrame frame, out GsaLedResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaSetLedType, 0x01, "LED builtin", out data, out error))
                return false;

            response = new GsaLedResponse
            {
                AppliedState = data[0] != 0
            };

            return true;
        }

        public static bool TryReadChannelSetpointResponse(SggwFrame frame, out GsaChannelSetpointResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelSetpointType, 0x02, "setpoint de canal", out data, out error))
                return false;

            response = new GsaChannelSetpointResponse
            {
                Channel = data[0],
                AppliedValue = data[1]
            };

            return true;
        }

        public static bool TryReadChannelEnableResponse(SggwFrame frame, out GsaChannelEnableResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelEnableType, 0x02, "enable de canal", out data, out error))
                return false;

            response = new GsaChannelEnableResponse
            {
                Channel = data[0],
                AppliedState = data[1] != 0
            };

            return true;
        }

        public static bool TryReadChannelStatusResponse(SggwFrame frame, out GsaChannelStatusResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelStatusType, 0x06, "status de canal", out data, out error))
                return false;

            response = new GsaChannelStatusResponse
            {
                Channel = data[0],
                Setpoint = data[1],
                VoltageRead = data[2],
                CurrentRead = data[3],
                Enabled = data[4] != 0,
                Fault = data[5] != 0
            };

            return true;
        }

        public static bool TryReadChannelsStatusResponse(SggwFrame frame, out GsaChannelsStatusResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelsStatusType, 0x60, "status global da GSA", out data, out error))
                return false;

            var parsed = new GsaChannelsStatusResponse();
            for (int index = 0; index < data.Length; index += 6)
            {
                parsed.Channels.Add(new GsaChannelStatusResponse
                {
                    Channel = data[index],
                    Setpoint = data[index + 1],
                    VoltageRead = data[index + 2],
                    CurrentRead = data[index + 3],
                    Enabled = data[index + 4] != 0,
                    Fault = data[index + 5] != 0
                });
            }

            response = parsed;
            return true;
        }

        public static bool TryReadChannelsEnableResponse(SggwFrame frame, out GsaChannelsEnableResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelsEnableType, 0x02, "enable global da GSA", out data, out error))
                return false;

            response = new GsaChannelsEnableResponse
            {
                RequestedState = data[0] != 0,
                AffectedCount = data[1]
            };

            return true;
        }

        public static bool TryReadChannelFaultResetResponse(SggwFrame frame, out GsaChannelFaultResetResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelFaultResetType, 0x02, "reset de fault do canal", out data, out error))
                return false;

            response = new GsaChannelFaultResetResponse
            {
                Channel = data[0],
                FaultState = data[1] != 0
            };

            return true;
        }

        public static bool TryReadChannelOffsetResponse(SggwFrame frame, byte expectedType, out GsaChannelOffsetResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, expectedType, 0x04, "offset de canal", out data, out error))
                return false;

            response = new GsaChannelOffsetResponse
            {
                Channel = data[0],
                Kind = ReadOffsetKind(data[1]),
                Offset = BitConverter.ToInt16(new[] { data[2], data[3] }, 0)
            };

            return true;
        }

        public static bool TryReadChannelOffsetSaveResponse(SggwFrame frame, out GsaChannelOffsetSaveResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelOffsetSaveType, 0x01, "save de offset do canal", out data, out error))
                return false;

            response = new GsaChannelOffsetSaveResponse
            {
                Channel = data[0]
            };

            return true;
        }

        public static bool TryReadChannelOffsetResetResponse(SggwFrame frame, out GsaChannelOffsetResetResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelOffsetResetType, 0x01, "reset de offset do canal", out data, out error))
                return false;

            response = new GsaChannelOffsetResetResponse
            {
                Channel = data[0]
            };

            return true;
        }

        public static bool TryReadOffsetResetResponse(SggwFrame frame, out GsaOffsetResetResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaOffsetResetType, 0x01, "reset global de offsets", out data, out error))
                return false;

            response = new GsaOffsetResetResponse
            {
                AffectedChannels = data[0]
            };

            return true;
        }

        public static bool TryReadFunctionalError(SggwFrame frame, out GsaFunctionalErrorResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaErrorType, 0x03, "erro funcional da GSA", out data, out error))
                return false;

            GsaErrorCode errorCode = (GsaErrorCode)data[2];
            response = new GsaFunctionalErrorResponse
            {
                RequestType = data[0],
                Channel = data[1],
                ErrorCode = errorCode,
                Message = BuildFunctionalErrorMessage(data[0], data[1], errorCode)
            };

            return true;
        }

        public static bool TryReadChannelFaultEvent(SggwFrame frame, out GsaChannelFaultEvent faultEvent, out string error)
        {
            faultEvent = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelFaultEventType, 0x06, "evento de fault da GSA", out data, out error))
                return false;

            faultEvent = new GsaChannelFaultEvent
            {
                Channel = data[0],
                Setpoint = data[1],
                VoltageRead = data[2],
                CurrentRead = data[3],
                Enabled = data[4] != 0,
                Fault = data[5] != 0
            };

            return true;
        }

        public static bool TryReadBusEvent(SggwFrame frame, out GsaBusEvent busEvent, out string error)
        {
            busEvent = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GsaChannelFaultEventType, 0x03, "evento BUSY/IDLE da GSA", out data, out error))
                return false;

            GsaBusEventType eventType = (GsaBusEventType)data[0];
            if (eventType != GsaBusEventType.Busy && eventType != GsaBusEventType.Idle)
            {
                error = "Evento BUSY/IDLE da GSA com eventType inválido.";
                return false;
            }

            busEvent = new GsaBusEvent
            {
                EventType = eventType,
                Channel = data[1],
                State = data[2] == (byte)GsaRemoteState.Busy ? GsaRemoteState.Busy : GsaRemoteState.Idle
            };

            return true;
        }

        public static bool TryReadGatewayError(SggwFrame frame, out GsaGatewayErrorResponse gatewayError, out string error)
        {
            gatewayError = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.GatewayErrorType, 0x01, "erro de gateway da BPM para a GSA", out data, out error))
                return false;

            byte errorCode = data[0];
            gatewayError = new GsaGatewayErrorResponse
            {
                ErrorCode = errorCode,
                IsBusy = errorCode == GwProtocol.GatewayBusyError,
                Message = BuildGatewayErrorMessage(errorCode)
            };

            return true;
        }

        private static bool TryReadTlv(SggwFrame frame, byte expectedType, byte expectedLen, string operationName, out byte[] data, out string error)
        {
            data = null;
            error = null;

            if (frame?.Payload == null)
            {
                error = "Resposta da GSA sem payload para " + operationName + ".";
                return false;
            }

            int payloadLength = frame.Payload.Length;
            if (payloadLength != expectedLen + 2 && payloadLength != expectedLen + 3)
            {
                error = "Resposta da GSA com tamanho inválido para " + operationName + ".";
                return false;
            }

            if (frame.Payload[0] != expectedType || frame.Payload[1] != expectedLen)
            {
                error = "Resposta da GSA com TLV inesperado para " + operationName + ".";
                return false;
            }

            bool hasCrc = payloadLength == expectedLen + 3;
            if (hasCrc)
            {
                byte expectedCrc = SdgwFrameCodec.Crc8Atm(frame.Payload, 0, payloadLength - 1);
                if (frame.Payload[payloadLength - 1] != expectedCrc)
                {
                    error = "Resposta da GSA com CRC inválido para " + operationName + ".";
                    return false;
                }
            }

            data = new byte[expectedLen];
            if (expectedLen > 0)
                Buffer.BlockCopy(frame.Payload, 2, data, 0, expectedLen);

            return true;
        }

        private static GsaOffsetKind ReadOffsetKind(byte rawKind)
        {
            switch (rawKind)
            {
                case GwProtocol.GsaOffsetKindVout:
                    return GsaOffsetKind.Vout;
                case GwProtocol.GsaOffsetKindVread:
                    return GsaOffsetKind.Vread;
                case GwProtocol.GsaOffsetKindIread:
                    return GsaOffsetKind.Iread;
                default:
                    throw new InvalidOperationException("Kind de offset inválido retornado pela GSA: 0x" + rawKind.ToString("X2") + ".");
            }
        }

        private static string BuildFunctionalErrorMessage(byte requestType, int channel, GsaErrorCode errorCode)
        {
            string suffix = channel > 0
                ? " no canal " + channel.ToString() + " (TLV 0x" + requestType.ToString("X2") + ")."
                : " (TLV 0x" + requestType.ToString("X2") + ").";

            switch (errorCode)
            {
                case GsaErrorCode.InvalidChannel:
                    return "A GSA rejeitou a operação por canal inválido" + suffix;
                case GsaErrorCode.InvalidValue:
                    return "A GSA rejeitou a operação por valor inválido" + suffix;
                case GsaErrorCode.InvalidState:
                    return "A GSA rejeitou a operação por state inválido" + suffix;
                case GsaErrorCode.InvalidKind:
                    return "A GSA rejeitou a operação por kind inválido" + suffix;
                case GsaErrorCode.FaultLatched:
                    return "A GSA rejeitou a operação porque o canal está com fault latched" + suffix;
                case GsaErrorCode.EepromWriteFailed:
                    return "A GSA falhou ao gravar a EEPROM" + suffix;
                case GsaErrorCode.CommandNotSupported:
                    return "A GSA informou que o comando não é suportado" + suffix;
                case GsaErrorCode.InvalidPayload:
                    return "A GSA rejeitou a operação por payload inválido" + suffix;
                case GsaErrorCode.InvalidTlvCrc:
                    return "A GSA rejeitou a operação por CRC TLV inválido" + suffix;
                case GsaErrorCode.PhysicalFaultStillPresent:
                    return "A GSA informou que a condição física ainda está em fault" + suffix;
                case GsaErrorCode.OperationNotAllowedInCurrentState:
                    return "A GSA rejeitou a operação por não ser permitida no estado atual" + suffix;
                default:
                    return "A GSA retornou erro funcional desconhecido 0x" + ((byte)errorCode).ToString("X2") + suffix;
            }
        }

        private static string BuildGatewayErrorMessage(byte errorCode)
        {
            switch (errorCode)
            {
                case GwProtocol.GatewayBusyError:
                    return "A BPM informou que a GSA está BUSY e adiou o comando até o evento IDLE.";
                case 0xE1:
                    return "A BPM informou que o endereço da GSA não está mapeado.";
                case 0xE2:
                    return "A BPM informou indisponibilidade no barramento da GSA.";
                case 0xE3:
                    return "A BPM informou timeout ao falar com a GSA.";
                case 0xE4:
                    return "A BPM informou CRC inválido na resposta da GSA.";
                case 0xE5:
                    return "A BPM informou frame inválido retornado pela GSA.";
                default:
                    return "A BPM retornou erro de gateway desconhecido para a GSA: 0x" + errorCode.ToString("X2") + ".";
            }
        }
    }
}
