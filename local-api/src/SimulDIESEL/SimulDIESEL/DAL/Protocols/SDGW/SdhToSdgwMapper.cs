using System;
using System.Globalization;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    /// <summary>
    /// Traduz comandos SDH para payload/cmd SDGW.
    /// Mantido nesta área por segurança nesta fase, embora a fronteira semântica
    /// possa ser refinada em rodadas futuras.
    /// </summary>
    public sealed class SdhToSdgwMapper
    {
        public sealed class MappedSdgwCommand
        {
            public byte Cmd { get; set; }
            public byte[] Payload { get; set; }
            public bool RequireAck { get; set; }
            public int TimeoutMs { get; set; }
            public int Retries { get; set; }
        }

        private const string BpmGatewayTarget = "BPM.gateway";
        private const string PingOp = "ping";
        private const int DefaultBoardTimeoutMs = 400;
        private const int DefaultBoardRetries = 2;

        public MappedSdgwCommand Map(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.Equals(command.Target, BpmGatewayTarget, StringComparison.OrdinalIgnoreCase))
                return MapBpmGateway(command);

            if (command.Target.StartsWith("GSA.", StringComparison.OrdinalIgnoreCase))
                return MapGsa(command);

            if (command.Target.StartsWith("UCE.", StringComparison.OrdinalIgnoreCase))
                return MapUce(command);

            throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
        }

        private static MappedSdgwCommand MapBpmGateway(SdhCommand command)
        {
            if (!string.Equals(command.Op, PingOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta op: " + command.Op + ".");

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.BpmAddress, GwProtocol.BpmPingOp),
                Payload = Array.Empty<byte>(),
                RequireAck = true,
                TimeoutMs = 150,
                Retries = 1
            };
        }

        private static MappedSdgwCommand MapGsa(SdhCommand command)
        {
            byte[] payload;

            if (string.Equals(command.Target, "GSA.led", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaSetLedType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channel.setpoint", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelSetpointType,
                    ParseChannel(command),
                    ParseByte(command, "value"));
            }
            else if (string.Equals(command.Target, "GSA.channel.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelEnableType,
                    ParseChannel(command),
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channels.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelsEnableType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "GSA.channel.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelStatusType,
                    ParseChannel(command));
            }
            else if (string.Equals(command.Target, "GSA.channels.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(GwProtocol.GsaChannelsStatusType);
            }
            else if (string.Equals(command.Target, "GSA.channel.fault", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.GsaChannelFaultResetType,
                    ParseChannel(command));
            }
            else if (string.Equals(command.Target, "GSA.channel.offset", StringComparison.OrdinalIgnoreCase))
            {
                payload = MapGsaChannelOffset(command);
            }
            else if (string.Equals(command.Target, "GSA.offset", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(GwProtocol.GsaOffsetResetType);
            }
            else
            {
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
            }

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp),
                Payload = payload,
                RequireAck = true,
                TimeoutMs = DefaultBoardTimeoutMs,
                Retries = DefaultBoardRetries
            };
        }

        private static MappedSdgwCommand MapUce(SdhCommand command)
        {
            byte[] payload;

            if (string.Equals(command.Target, "UCE.led", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceSetLedType,
                    ParseState(command.Args["state"]) ? (byte)0x01 : (byte)0x00);
            }
            else if (string.Equals(command.Target, "UCE.can.config", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanConfigType,
                    ParseUceController(command),
                    ParseUceBitrateCode(command),
                    ParseUceMode(command));
            }
            else if (string.Equals(command.Target, "UCE.can.enable", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanEnableType,
                    ParseUceController(command),
                    ParseState(command.Args["state"]) ? GwProtocol.UceCanStateOn : GwProtocol.UceCanStateOff);
            }
            else if (string.Equals(command.Target, "UCE.can.status", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanStatusType,
                    ParseUceController(command));
            }
            else if (string.Equals(command.Target, "UCE.can.rx", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanRxPollType,
                    ParseUceController(command));
            }
            else if (string.Equals(command.Target, "UCE.can.driverLog", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanDriverLogPollType,
                    ParseUceController(command));
            }
            else if (string.Equals(command.Target, "UCE.can.tx", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(command.Op, "send", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(GwProtocol.UceCanTxType, BuildUceCanTxPayload(command));
            }
            else if (string.Equals(command.Target, "UCE.can.tx", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(command.Op, "stop", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanTxStopType,
                    ParseUceController(command),
                    ParseByte(command, "slot"));
            }
            else if (string.Equals(command.Target, "UCE.can", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(command.Op, "reset", StringComparison.OrdinalIgnoreCase))
            {
                payload = BuildTlvPayload(
                    GwProtocol.UceCanResetType,
                    ParseUceController(command));
            }
            else
            {
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta target: " + command.Target + ".");
            }

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.UceAddress, GwProtocol.UceTlvTransactOp),
                Payload = payload,
                RequireAck = true,
                TimeoutMs = DefaultBoardTimeoutMs,
                Retries = DefaultBoardRetries
            };
        }

        private static byte[] MapGsaChannelOffset(SdhCommand command)
        {
            byte channel = ParseChannel(command);

            if (string.Equals(command.Op, "set", StringComparison.OrdinalIgnoreCase))
            {
                short value = ParseInt16(command, "value");
                byte[] offsetBytes = BitConverter.GetBytes(value);
                return BuildTlvPayload(
                    GwProtocol.GsaChannelOffsetSetType,
                    channel,
                    ParseOffsetKind(command.Args["kind"]),
                    offsetBytes[0],
                    offsetBytes[1]);
            }

            if (string.Equals(command.Op, "get", StringComparison.OrdinalIgnoreCase))
            {
                return BuildTlvPayload(
                    GwProtocol.GsaChannelOffsetGetType,
                    channel,
                    ParseOffsetKind(command.Args["kind"]));
            }

            if (string.Equals(command.Op, "save", StringComparison.OrdinalIgnoreCase))
                return BuildTlvPayload(GwProtocol.GsaChannelOffsetSaveType, channel);

            if (string.Equals(command.Op, "reset", StringComparison.OrdinalIgnoreCase))
                return BuildTlvPayload(GwProtocol.GsaChannelOffsetResetType, channel);

            throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta op: " + command.Op + ".");
        }

        private static byte[] BuildTlvPayload(byte type, params byte[] data)
        {
            int payloadLength = data != null ? data.Length : 0;
            byte[] payload = new byte[payloadLength + 3];
            payload[0] = type;
            payload[1] = (byte)payloadLength;

            if (payloadLength > 0)
                Buffer.BlockCopy(data, 0, payload, 2, payloadLength);

            payload[payload.Length - 1] = SdgwFrameCodec.Crc8Atm(payload, 0, payload.Length - 1);
            return payload;
        }

        private static byte[] BuildUceCanTxPayload(SdhCommand command)
        {
            byte[] payload = new byte[GwProtocol.UceCanTxRequestPayloadLength];
            bool extended = ParseBool01(command, "extended");
            uint id = ParseUInt32(command, "id") & (extended ? 0x1FFFFFFFU : 0x7FFU);
            ushort period = checked((ushort)ParseInt(command, "period"));

            payload[0] = ParseUceController(command);
            payload[1] = extended ? (byte)0x01 : (byte)0x00;
            payload[2] = ParseByte(command, "dlc");
            payload[3] = (byte)(period & 0xFF);
            payload[4] = (byte)((period >> 8) & 0xFF);
            payload[5] = (byte)(id & 0xFF);
            payload[6] = (byte)((id >> 8) & 0xFF);
            payload[7] = (byte)((id >> 16) & 0xFF);
            payload[8] = (byte)((id >> 24) & 0xFF);
            for (int i = 0; i < 8; ++i)
                payload[9 + i] = ParseByte(command, "d" + i.ToString(CultureInfo.InvariantCulture));

            return payload;
        }

        private static byte ParseChannel(SdhCommand command)
        {
            int channel = ParseInt(command, "channel");
            return checked((byte)channel);
        }

        private static byte ParseByte(SdhCommand command, string argName)
        {
            int value = ParseInt(command, argName);
            return checked((byte)value);
        }

        private static short ParseInt16(SdhCommand command, string argName)
        {
            int value = ParseInt(command, argName);
            return checked((short)value);
        }

        private static int ParseInt(SdhCommand command, string argName)
        {
            string rawValue = command.Args[argName];
            return int.Parse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static uint ParseUInt32(SdhCommand command, string argName)
        {
            string rawValue = command.Args[argName];
            return uint.Parse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static bool ParseBool01(SdhCommand command, string argName)
        {
            string rawValue = command.Args[argName];
            if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawValue, "false", StringComparison.OrdinalIgnoreCase))
                return false;

            throw new InvalidOperationException("Argumento inválido para " + command.Target + ": " + argName + ".");
        }

        private static bool ParseState(string state)
        {
            return string.Equals(state, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static byte ParseOffsetKind(string kind)
        {
            if (string.Equals(kind, "vout", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindVout;

            if (string.Equals(kind, "vread", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindVread;

            if (string.Equals(kind, "iread", StringComparison.OrdinalIgnoreCase))
                return GwProtocol.GsaOffsetKindIread;

            throw new InvalidOperationException("Kind inválido para mapeamento SDH->SDGW: " + kind + ".");
        }

        private static byte ParseUceController(SdhCommand command)
        {
            UceCanController controller;
            if (!UceCanProtocol.TryParseController(command.Args["controller"], out controller) ||
                !UceCanProtocol.TryEncodeController(controller, out byte code))
            {
                throw new InvalidOperationException("Controller inválido para mapeamento SDH->SDGW: " + command.Args["controller"] + ".");
            }

            return code;
        }

        private static byte ParseUceBitrateCode(SdhCommand command)
        {
            int bitrate = ParseInt(command, "bitrate");
            if (!UceCanProtocol.TryEncodeBitrate(bitrate, out byte code))
                throw new InvalidOperationException("Bitrate inválido para mapeamento SDH->SDGW: " + bitrate.ToString(CultureInfo.InvariantCulture) + ".");

            return code;
        }

        private static byte ParseUceMode(SdhCommand command)
        {
            UceCanMode mode;
            if (!UceCanProtocol.TryParseMode(command.Args["mode"], out mode) ||
                !UceCanProtocol.TryEncodeMode(mode, out byte code))
            {
                throw new InvalidOperationException("Mode inválido para mapeamento SDH->SDGW: " + command.Args["mode"] + ".");
            }

            return code;
        }
    }
}
