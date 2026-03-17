using System;
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
        private const string LedTarget = "GSA.led";
        private const string SetOp = "set";
        private const string StateArg = "state";

        public MappedSdgwCommand Map(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.Equals(command.Target, BpmGatewayTarget, StringComparison.OrdinalIgnoreCase))
                return MapBpmGateway(command);

            if (string.Equals(command.Target, LedTarget, StringComparison.OrdinalIgnoreCase))
                return MapGsaLed(command);

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

        private static MappedSdgwCommand MapGsaLed(SdhCommand command)
        {
            if (!string.Equals(command.Op, SetOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Mapeamento SDH->SDGW ainda não suporta op: " + command.Op + ".");

            string state;
            if (!command.Args.TryGetValue(StateArg, out state))
                throw new InvalidOperationException("Mapeamento SDH->SDGW requer o argumento state.");

            bool isOn;
            if (string.Equals(state, "on", StringComparison.OrdinalIgnoreCase))
            {
                isOn = true;
            }
            else if (string.Equals(state, "off", StringComparison.OrdinalIgnoreCase))
            {
                isOn = false;
            }
            else
            {
                throw new InvalidOperationException("State inválido para mapeamento SDH->SDGW: " + state + ".");
            }

            return new MappedSdgwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp),
                Payload = BuildGsaLedPayload(isOn),
                RequireAck = true,
                TimeoutMs = 150,
                Retries = 1
            };
        }

        private static byte[] BuildGsaLedPayload(bool isOn)
        {
            var payload = new byte[4];
            payload[0] = GwProtocol.GsaSetLedType;
            payload[1] = 0x01;
            payload[2] = isOn ? (byte)0x01 : (byte)0x00;
            payload[3] = SdgwFrameCodec.Crc8Atm(payload, 0, 3);
            return payload;
        }
    }
}
