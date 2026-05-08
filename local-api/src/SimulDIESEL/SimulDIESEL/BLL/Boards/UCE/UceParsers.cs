using System;
using System.Collections.Generic;
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
            if (!TryReadTlv(frame, GwProtocol.UceSetLedType, GwProtocol.UceLedPayloadLength, "LED builtin da UCE", out data, out error))
                return false;

            response = new UceLedResponse
            {
                AcceptedState = data[0] != 0
            };

            return true;
        }

        public static bool TryReadLedEvent(SdgwFrame frame, out UceLedEvent ledEvent, out string error)
        {
            ledEvent = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceLedEventType, GwProtocol.UceLedEventPayloadLength, "evento LED da UCE", out data, out error))
                return false;

            if (data[0] > 1)
            {
                error = "Evento LED da UCE com estado inválido.";
                return false;
            }

            ushort counter = (ushort)(data[2] | (data[3] << 8));
            ledEvent = new UceLedEvent
            {
                LedState = data[0] != 0,
                EventCode = data[1],
                Counter = counter
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
            if (diagnostic.HasExtendedData)
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
                    message = diagnostic.HasExtendedData
                        ? UceGatewayDiagnosticLog.BuildTimeoutMessage(diagnostic)
                        : "A BPM informou timeout ao falar com a UCE via SPI.";
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

            string requestName = DescribeRequestType(data[0]);
            switch (data[2])
            {
                case 0x03:
                    message = "A UCE rejeitou " + requestName + " por valor inválido.";
                    return true;
                case 0x07:
                    message = "A UCE informou que " + requestName + " não é suportado.";
                    return true;
                case 0x08:
                    message = "A UCE rejeitou " + requestName + " por payload inválido.";
                    return true;
                case 0x09:
                    message = "A UCE rejeitou " + requestName + " por CRC TLV inválido.";
                    return true;
                default:
                    message = "A UCE retornou erro funcional desconhecido 0x" + data[2].ToString("X2") + ".";
                    return true;
            }
        }

        public static bool TryReadCanConfigResponse(SdgwFrame frame, out UceCanConfigResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanConfigType, GwProtocol.UceCanConfigPayloadLength, "configuração CAN da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta da UCE com controller CAN inválido.";
                return false;
            }

            int bitrateKbps;
            if (!UceCanProtocol.TryDecodeBitrate(data[1], out bitrateKbps))
            {
                error = "Resposta da UCE com bitrate CAN inválido.";
                return false;
            }

            UceCanMode mode;
            if (!UceCanProtocol.TryDecodeMode(data[2], out mode))
            {
                error = "Resposta da UCE com modo CAN inválido.";
                return false;
            }

            response = new UceCanConfigResponse
            {
                Controller = controller,
                AcceptedBitrateKbps = bitrateKbps,
                AcceptedMode = mode
            };

            return true;
        }

        public static bool TryReadCanEnableResponse(SdgwFrame frame, out UceCanEnableResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanEnableType, GwProtocol.UceCanEnablePayloadLength, "habilitação CAN da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta da UCE com controller CAN inválido.";
                return false;
            }

            if (data[1] != GwProtocol.UceCanStateOff && data[1] != GwProtocol.UceCanStateOn)
            {
                error = "Resposta da UCE com estado CAN inválido.";
                return false;
            }

            response = new UceCanEnableResponse
            {
                Controller = controller,
                EffectiveEnabled = data[1] == GwProtocol.UceCanStateOn
            };

            return true;
        }

        public static bool TryReadCanStatusResponse(SdgwFrame frame, out UceCanStatusResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanStatusType, GwProtocol.UceCanStatusResponsePayloadLength, "status CAN da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta da UCE com controller CAN inválido.";
                return false;
            }

            UceCanInterfaceState state;
            if (!UceCanProtocol.TryDecodeState(data[1], out state))
            {
                error = "Resposta da UCE com estado de interface CAN inválido.";
                return false;
            }

            int bitrateKbps;
            if (!UceCanProtocol.TryDecodeBitrate(data[2], out bitrateKbps))
            {
                error = "Resposta da UCE com bitrate CAN inválido.";
                return false;
            }

            UceCanMode mode;
            if (!UceCanProtocol.TryDecodeMode(data[3], out mode))
            {
                error = "Resposta da UCE com modo CAN inválido.";
                return false;
            }

            response = new UceCanStatusResponse
            {
                Controller = controller,
                State = state,
                BitrateKbps = bitrateKbps,
                Mode = mode
            };

            return true;
        }

        public static bool TryReadCanResetResponse(SdgwFrame frame, out UceCanResetResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanResetType, GwProtocol.UceCanResetResponsePayloadLength, "reset CAN da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta da UCE com controller CAN inválido.";
                return false;
            }

            if (data[1] != GwProtocol.UceCanResetFailed && data[1] != GwProtocol.UceCanResetSucceeded)
            {
                error = "Resposta da UCE com status de reset CAN inválido.";
                return false;
            }

            response = new UceCanResetResponse
            {
                Controller = controller,
                ResetSucceeded = data[1] == GwProtocol.UceCanResetSucceeded
            };

            return true;
        }

        public static bool TryReadCanRxPollResponse(SdgwFrame frame, out UceCanRxPollResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadVariableTlv(frame, GwProtocol.UceCanRxPollType, "poll CAN_RX da UCE", out data, out error))
                return false;

            if (data.Length < 2)
            {
                error = "Resposta CAN_RX da UCE sem controller/count.";
                return false;
            }

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta CAN_RX da UCE com controller CAN inválido.";
                return false;
            }

            byte count = data[1];
            if (count > GwProtocol.UceCanRxMaxFramesPerResponse)
            {
                error = "Resposta CAN_RX da UCE excedeu o limite de frames.";
                return false;
            }

            int expectedLength = 2 + (count * GwProtocol.UceCanRxFrameLength);
            if (data.Length != expectedLength)
            {
                error = "Resposta CAN_RX da UCE com tamanho incompatível com a quantidade de frames.";
                return false;
            }

            var frames = new List<UceCanFrame>(count);
            int offset = 2;
            for (int i = 0; i < count; ++i)
            {
                uint rawId = ((uint)data[offset] << 24) |
                             ((uint)data[offset + 1] << 16) |
                             ((uint)data[offset + 2] << 8) |
                             data[offset + 3];
                byte flags = data[offset + 4];
                byte dlc = data[offset + 5];
                if ((flags & 0xFC) != 0)
                {
                    error = "Resposta CAN_RX da UCE com flags reservados preenchidos.";
                    return false;
                }

                if (dlc > 8)
                {
                    error = "Resposta CAN_RX da UCE com DLC inválido.";
                    return false;
                }

                bool extended = (flags & 0x01) != 0;
                uint id = rawId & (extended ? 0x1FFFFFFFU : 0x7FFU);
                byte[] payload = new byte[8];
                Buffer.BlockCopy(data, offset + 6, payload, 0, 8);
                frames.Add(new UceCanFrame
                {
                    Id = id,
                    Extended = extended,
                    RemoteRequest = (flags & 0x02) != 0,
                    Dlc = dlc,
                    Data = payload
                });

                offset += GwProtocol.UceCanRxFrameLength;
            }

            response = new UceCanRxPollResponse
            {
                Controller = controller,
                Frames = frames
            };

            return true;
        }

        public static bool TryReadCanRxEvent(SdgwFrame frame, out UceCanRxEvent rxEvent, out string error)
        {
            rxEvent = null;

            byte[] data;
            if (!TryReadVariableTlv(frame, GwProtocol.UceCanRxEventType, "evento CAN_RX da UCE", out data, out error))
                return false;

            if (data.Length < GwProtocol.UceCanRxEventHeaderLength)
            {
                error = "Evento CAN_RX da UCE sem controller/count.";
                return false;
            }

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Evento CAN_RX da UCE com controller CAN inválido.";
                return false;
            }

            byte count = data[1];
            if (count > GwProtocol.UceCanRxEventMaxFrames)
            {
                error = "Evento CAN_RX da UCE excedeu o limite de frames.";
                return false;
            }

            int expectedLength = GwProtocol.UceCanRxEventHeaderLength + (count * GwProtocol.UceCanRxFrameLength);
            if (data.Length != expectedLength)
            {
                error = "Evento CAN_RX da UCE com tamanho incompatível com a quantidade de frames.";
                return false;
            }

            var frames = new List<UceCanFrame>(count);
            int offset = GwProtocol.UceCanRxEventHeaderLength;
            for (int i = 0; i < count; ++i)
            {
                uint rawId = data[offset] |
                             ((uint)data[offset + 1] << 8) |
                             ((uint)data[offset + 2] << 16) |
                             ((uint)data[offset + 3] << 24);
                byte flags = data[offset + 4];
                byte dlc = data[offset + 5];
                if ((flags & 0xFC) != 0)
                {
                    error = "Evento CAN_RX da UCE com flags reservados preenchidos.";
                    return false;
                }

                if (dlc > 8)
                {
                    error = "Evento CAN_RX da UCE com DLC inválido.";
                    return false;
                }

                bool extended = (flags & 0x01) != 0;
                uint id = rawId & (extended ? 0x1FFFFFFFU : 0x7FFU);
                byte[] payload = new byte[8];
                Buffer.BlockCopy(data, offset + 6, payload, 0, 8);
                frames.Add(new UceCanFrame
                {
                    Id = id,
                    Extended = extended,
                    RemoteRequest = (flags & 0x02) != 0,
                    Dlc = dlc,
                    Data = payload
                });

                offset += GwProtocol.UceCanRxFrameLength;
            }

            rxEvent = new UceCanRxEvent
            {
                Controller = controller,
                Frames = frames
            };

            return true;
        }

        public static bool TryReadCanCrudEvent(SdgwFrame frame, out byte eventType, out byte[] payload, out string error)
        {
            eventType = 0;
            payload = null;
            error = null;

            if (frame?.Payload == null || frame.Payload.Length < 2)
                return false;

            switch (frame.Payload[0])
            {
                case GwProtocol.UceCanCreateType:
                case GwProtocol.UceCanEditType:
                case GwProtocol.UceCanDeleteType:
                case GwProtocol.UceCanRowType:
                case GwProtocol.UceCanReadAllDoneType:
                case GwProtocol.UceCanTicType:
                    eventType = frame.Payload[0];
                    return TryReadVariableTlv(frame, eventType, "evento CAN CRUD da UCE", out payload, out error);
                default:
                    return false;
            }
        }

        public static bool TryReadTransportDiagnosticEvent(SdgwFrame frame, out UceDispatcherOverflowDiagnostic diagnostic, out string error)
        {
            diagnostic = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceTransportDiagType, GwProtocol.UceTransportDiagDispatcherFifoOverflowPayloadLength, "diagnostico de transporte da UCE", out data, out error))
                return false;

            if (data[0] != GwProtocol.UceTransportDiagDispatcherFifoOverflow)
            {
                error = "Diagnostico de transporte da UCE com tipo desconhecido.";
                return false;
            }

            uint overflowCount = (uint)data[1] |
                ((uint)data[2] << 8) |
                ((uint)data[3] << 16) |
                ((uint)data[4] << 24);

            diagnostic = new UceDispatcherOverflowDiagnostic
            {
                OverflowCount = overflowCount,
                QueueSize = data[5],
                MaxEventSize = data[6]
            };

            return true;
        }

        public static bool TryReadCanDriverLogPollResponse(SdgwFrame frame, out UceCanDriverLogPollResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadVariableTlv(frame, GwProtocol.UceCanDriverLogPollType, "poll de diagnóstico do driver CAN da UCE", out data, out error))
                return false;

            if (data.Length < 2)
            {
                error = "Resposta de diagnóstico CAN sem controller/count.";
                return false;
            }

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta de diagnóstico CAN com controller inválido.";
                return false;
            }

            byte count = data[1];
            if (count > GwProtocol.UceCanDriverLogMaxEntriesPerResponse)
            {
                error = "Resposta de diagnóstico CAN excedeu o limite de entradas.";
                return false;
            }

            int expectedLength = 2 + (count * GwProtocol.UceCanDriverLogEntryLength);
            if (data.Length != expectedLength)
            {
                error = "Resposta de diagnóstico CAN com tamanho incompatível com a quantidade de entradas.";
                return false;
            }

            var entries = new List<UceCanDriverLogEntry>(count);
            int offset = 2;
            for (int i = 0; i < count; ++i)
            {
                UceCanInterfaceState state;
                if (!UceCanProtocol.TryDecodeState(data[offset + 2], out state))
                {
                    error = "Resposta de diagnóstico CAN com interfaceState inválido.";
                    return false;
                }

                int bitrateKbps;
                if (!UceCanProtocol.TryDecodeBitrate(data[offset + 3], out bitrateKbps))
                {
                    error = "Resposta de diagnóstico CAN com bitrate inválido.";
                    return false;
                }

                UceCanMode mode;
                if (!UceCanProtocol.TryDecodeMode(data[offset + 4], out mode))
                {
                    error = "Resposta de diagnóstico CAN com modo inválido.";
                    return false;
                }

                entries.Add(new UceCanDriverLogEntry
                {
                    TimestampLow = data[offset],
                    EventCode = data[offset + 1],
                    InterfaceState = state,
                    BitrateKbps = bitrateKbps,
                    Mode = mode,
                    Detail0 = data[offset + 5],
                    Detail1 = data[offset + 6],
                    Detail2 = data[offset + 7]
                });

                offset += GwProtocol.UceCanDriverLogEntryLength;
            }

            response = new UceCanDriverLogPollResponse
            {
                Controller = controller,
                Entries = entries
            };

            return true;
        }

        public static bool TryReadCanTxResponse(SdgwFrame frame, out UceCanTxResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanTxType, GwProtocol.UceCanTxResponsePayloadLength, "envio CAN_TX da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta CAN_TX da UCE com controller inválido.";
                return false;
            }

            response = new UceCanTxResponse
            {
                Controller = controller,
                TxStatus = data[1],
                SequenceOrSlot = data[2]
            };

            return true;
        }

        public static bool TryReadCanTxStopResponse(SdgwFrame frame, out UceCanTxStopResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanTxStopType, GwProtocol.UceCanTxStopResponsePayloadLength, "parada CAN_TX periódico da UCE", out data, out error))
                return false;

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Resposta CAN_TX_STOP da UCE com controller inválido.";
                return false;
            }

            response = new UceCanTxStopResponse
            {
                Controller = controller,
                TxStatus = data[1]
            };

            return true;
        }

        public static bool TryReadCanReadAllResponse(SdgwFrame frame, out UceCanReadAllResponse response, out string error)
        {
            response = null;

            byte[] data;
            if (!TryReadTlv(frame, GwProtocol.UceCanReadAllType, GwProtocol.UceCanReadAllPayloadLength, "solicitação CAN_READ_ALL da UCE", out data, out error))
                return false;

            response = new UceCanReadAllResponse
            {
                Accepted = true
            };

            return true;
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

        private static string DescribeRequestType(byte requestType)
        {
            switch (requestType)
            {
                case GwProtocol.UceSetLedType:
                    return "o comando do LED builtin da UCE";
                case GwProtocol.UceCanConfigType:
                    return "a configuração CAN da UCE";
                case GwProtocol.UceCanEnableType:
                    return "a habilitação CAN da UCE";
                case GwProtocol.UceCanStatusType:
                    return "a leitura de status CAN da UCE";
                case GwProtocol.UceCanResetType:
                    return "o reset CAN da UCE";
                case GwProtocol.UceCanRxPollType:
                    return "o poll CAN_RX da UCE";
                case GwProtocol.UceCanDriverLogPollType:
                    return "o poll de diagnóstico do driver CAN da UCE";
                case GwProtocol.UceCanTxType:
                    return "o envio CAN_TX da UCE";
                case GwProtocol.UceCanTxStopType:
                    return "a parada CAN_TX periódico da UCE";
                case GwProtocol.UceCanReadAllType:
                    return "a leitura completa da tabela RX da UCE";
                default:
                    return "a operação solicitada para a UCE";
            }
        }
    }
}
