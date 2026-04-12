using System;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.UCE
{
    public static class UceParsers
    {
        public static bool TryReadBuiltinLedResponse(SdgwFrame frame, out UceLedResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceSetLedType, 0x01, "LED builtin da UCE", out data, out error))
                return false;

            response = new UceLedResponse
            {
                AcceptedState = data[0] != 0
            };

            return true;
        }

        public static bool TryReadGatewayError(SdgwFrame frame, out string message, out string error)
        {
            message = null;

            byte[] data;
            if (!TryReadVariableTlv(frame, GwProtocol.GatewayErrorType, "erro de gateway da BPM para a UCE", out data, out error))
                return false;

            if (data.Length < 1)
            {
                error = "Resposta da UCE sem código de erro do gateway.";
                return false;
            }

            UceGatewayDiagnostic diagnostic = UceGatewayDiagnosticLog.Create(data);
            if (data[0] == 0xE4)
                UceGatewayDiagnosticLog.Append(diagnostic);

            switch (data[0])
            {
                case 0xE1:
                    message = "A BPM informou que o endereço lógico da UCE não está mapeado.";
                    return true;
                case 0xE2:
                    message = "A BPM informou indisponibilidade no barramento SPI da UCE.";
                    return true;
                case 0xE3:
                    message = "A BPM informou timeout ao falar com a UCE via SPI.";
                    return true;
                case 0xE4:
                    message = UceGatewayDiagnosticLog.BuildCrcMessage(diagnostic);
                    return true;
                case 0xE5:
                    message = "A BPM informou frame inválido retornado pela UCE.";
                    return true;
                default:
                    message = "A BPM retornou erro de gateway desconhecido para a UCE: 0x" + data[0].ToString("X2") + ".";
                    return true;
            }
        }

        public static bool TryReadFunctionalError(SdgwFrame frame, out string message, out string error)
        {
            message = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceErrorType, 0x03, "erro funcional da UCE", out data, out error))
                return false;

            switch (data[2])
            {
                case 0x03:
                    message = "A UCE rejeitou o comando do LED por state inválido.";
                    return true;
                case 0x07:
                    message = "A UCE informou que o comando não é suportado.";
                    return true;
                case 0x08:
                    message = "A UCE rejeitou a operação por payload inválido.";
                    return true;
                case 0x09:
                    message = "A UCE rejeitou a operação por CRC TLV inválido.";
                    return true;
                default:
                    message = "A UCE retornou erro funcional desconhecido 0x" + data[2].ToString("X2") + ".";
                    return true;
            }
        }

        private static bool TryReadTlv(SdgwFrame frame, byte expectedType, byte expectedLen, string operationName, out byte[] data, out string error)
        {
            data = null;
            error = null;

            if (frame?.Payload == null)
            {
                error = "Resposta da UCE sem payload para " + operationName + ".";
                return false;
            }

            int payloadLength = frame.Payload.Length;
            if (payloadLength != expectedLen + 2 && payloadLength != expectedLen + 3)
            {
                error = "Resposta da UCE com tamanho inválido para " + operationName + ".";
                return false;
            }

            if (frame.Payload[0] != expectedType || frame.Payload[1] != expectedLen)
            {
                error = "Resposta da UCE com TLV inesperado para " + operationName + ".";
                return false;
            }

            bool hasCrc = payloadLength == expectedLen + 3;
            if (hasCrc)
            {
                byte expectedCrc = SdgwFrameCodec.Crc8Atm(frame.Payload, 0, payloadLength - 1);
                if (frame.Payload[payloadLength - 1] != expectedCrc)
                {
                    error = "Resposta da UCE com CRC inválido para " + operationName + ".";
                    return false;
                }
            }

            data = new byte[expectedLen];
            if (expectedLen > 0)
                Buffer.BlockCopy(frame.Payload, 2, data, 0, expectedLen);

            return true;
        }

        private static bool TryReadVariableTlv(SdgwFrame frame, byte expectedType, string operationName, out byte[] data, out string error)
        {
            data = null;
            error = null;

            if (frame?.Payload == null)
            {
                error = "Resposta da UCE sem payload para " + operationName + ".";
                return false;
            }

            int payloadLength = frame.Payload.Length;
            if (payloadLength < 3)
            {
                error = "Resposta da UCE com tamanho inválido para " + operationName + ".";
                return false;
            }

            if (frame.Payload[0] != expectedType)
            {
                error = "Resposta da UCE com TLV inesperado para " + operationName + ".";
                return false;
            }

            byte valueLength = frame.Payload[1];
            int minimumLength = valueLength + 2;
            bool hasCrc = payloadLength == minimumLength + 1;
            if (payloadLength != minimumLength && !hasCrc)
            {
                error = "Resposta da UCE com tamanho inválido para " + operationName + ".";
                return false;
            }

            if (hasCrc)
            {
                byte expectedCrc = SdgwFrameCodec.Crc8Atm(frame.Payload, 0, payloadLength - 1);
                if (frame.Payload[payloadLength - 1] != expectedCrc)
                {
                    error = "Resposta da UCE com CRC inválido para " + operationName + ".";
                    return false;
                }
            }

            data = new byte[valueLength];
            if (valueLength > 0)
                Buffer.BlockCopy(frame.Payload, 2, data, 0, valueLength);

            return true;
        }
    }
}
