using System;
using System.Collections.Generic;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDCTP;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    public static class SdctpEventParser
    {
        public static bool IsSdctpEventType(byte type)
        {
            switch (type)
            {
                case GwProtocol.UceCanRxEventType:
                case GwProtocol.UceCanCreateType:
                case GwProtocol.UceCanEditType:
                case GwProtocol.UceCanDeleteType:
                case GwProtocol.UceCanRowType:
                case GwProtocol.UceCanReadAllDoneType:
                case GwProtocol.UceCanTicType:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryReadRawEvent(SdgwFrame frame, out SdctpRawEventDto rawEvent, out string error)
        {
            rawEvent = null;
            error = null;

            if (frame?.Payload == null || frame.Payload.Length < 3)
                return false;

            byte type = frame.Payload[0];
            if (!IsSdctpEventType(type))
                return false;

            byte valueLen = frame.Payload[1];
            int expectedLength = valueLen + 3;
            if (frame.Payload.Length != expectedLength)
            {
                error = "Evento SDCTP com tamanho TLV incompatível.";
                return false;
            }

            byte crc = SdgwFrameCodec.Crc8Atm(frame.Payload, 0, frame.Payload.Length - 1);
            if (crc != frame.Payload[frame.Payload.Length - 1])
            {
                error = "Evento SDCTP com CRC TLV inválido.";
                return false;
            }

            byte[] payload = new byte[valueLen];
            if (valueLen > 0)
                Buffer.BlockCopy(frame.Payload, 2, payload, 0, valueLen);

            rawEvent = new SdctpRawEventDto
            {
                Type = type,
                Payload = payload,
                TimestampUtc = DateTime.UtcNow
            };
            return true;
        }

        public static bool TryReadCanRxEvent(SdctpRawEventDto rawEvent, out UceCanRxEvent rxEvent, out string error)
        {
            rxEvent = null;
            error = null;

            if (rawEvent == null || rawEvent.Type != GwProtocol.UceCanRxEventType)
                return false;

            byte[] data = rawEvent.Payload;
            if (data == null || data.Length < GwProtocol.UceCanRxEventHeaderLength)
            {
                error = "Evento CAN_RX SDCTP sem controller/count.";
                return false;
            }

            UceCanController controller;
            if (!UceCanProtocol.TryDecodeController(data[0], out controller))
            {
                error = "Evento CAN_RX SDCTP com controller CAN inválido.";
                return false;
            }

            byte count = data[1];
            if (count > GwProtocol.UceCanRxEventMaxFrames)
            {
                error = "Evento CAN_RX SDCTP excedeu o limite de frames.";
                return false;
            }

            int expectedLength = GwProtocol.UceCanRxEventHeaderLength + (count * GwProtocol.UceCanRxFrameLength);
            if (data.Length != expectedLength)
            {
                error = "Evento CAN_RX SDCTP com tamanho incompatível com a quantidade de frames.";
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
                    error = "Evento CAN_RX SDCTP com flags reservados preenchidos.";
                    return false;
                }

                if (dlc > 8)
                {
                    error = "Evento CAN_RX SDCTP com DLC inválido.";
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
    }
}
