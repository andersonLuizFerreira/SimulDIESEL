using System;
using SimulDIESEL.DTL;

namespace SimulDIESEL.BLL.SDH
{
    public sealed class SdhToSggwMapper
    {
        public sealed class MappedSggwCommand
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

        public MappedSggwCommand Map(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.Equals(command.Target, BpmGatewayTarget, StringComparison.OrdinalIgnoreCase))
            {
                return MapBpmGateway(command);
            }

            if (string.Equals(command.Target, LedTarget, StringComparison.OrdinalIgnoreCase))
            {
                return MapGsaLed(command);
            }

            throw new NotSupportedException("Mapeamento SDH->SGGW ainda não suporta target: " + command.Target + ".");
        }

        private static MappedSggwCommand MapBpmGateway(SdhCommand command)
        {
            if (!string.Equals(command.Op, PingOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Mapeamento SDH->SGGW ainda não suporta op: " + command.Op + ".");

            return new MappedSggwCommand
            {
                Cmd = GwProtocol.MakeCompactCommand(GwProtocol.BpmAddress, GwProtocol.BpmPingOp),
                Payload = Array.Empty<byte>(),
                RequireAck = true,
                TimeoutMs = 150,
                Retries = 1
            };
        }

        private static MappedSggwCommand MapGsaLed(SdhCommand command)
        {
            if (!string.Equals(command.Op, SetOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Mapeamento SDH->SGGW ainda não suporta op: " + command.Op + ".");

            string state;
            if (!command.Args.TryGetValue(StateArg, out state))
                throw new InvalidOperationException("Mapeamento SDH->SGGW requer o argumento state.");

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
                throw new InvalidOperationException("State inválido para mapeamento SDH->SGGW: " + state + ".");
            }

            // O primeiro fluxo oficial priorizado nesta etapa eh o comando local
            // da BPM via byte compacto. O GSA agora segue o mesmo caminho oficial:
            // CMD compacto no host e payload interno TLV+CRC da baby board.
            return new MappedSggwCommand
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
            payload[3] = SdGwLinkEngine.Crc8Atm(payload, 0, 3);
            return payload;
        }
    }
}
