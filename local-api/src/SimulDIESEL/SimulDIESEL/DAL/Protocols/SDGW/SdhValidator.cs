using System;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdhValidator
    {
        private const string SupportedVersion = "sdh/1";
        private const string GsaBoard = "GSA";
        private const string GsaResource = "led";
        private const string GsaSetOp = "set";
        private const string StateArg = "state";
        private const string BpmBoard = "BPM";
        private const string BpmResource = "gateway";
        private const string BpmPingOp = "ping";

        public void Validate(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command.Version))
                throw new InvalidOperationException("Version SDH é obrigatória.");

            if (!string.Equals(command.Version, SupportedVersion, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Version SDH inválida. Versão suportada nesta fase: " + SupportedVersion + ".");

            if (string.IsNullOrWhiteSpace(command.Target))
                throw new InvalidOperationException("Target SDH é obrigatório.");

            if (string.IsNullOrWhiteSpace(command.Op))
                throw new InvalidOperationException("Op SDH é obrigatória.");

            SdhTarget target = SdhTarget.Parse(command.Target);

            if (string.Equals(target.Board, GsaBoard, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(target.Resource, GsaResource, StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                ValidateGsaLed(command);
                return;
            }

            if (string.Equals(target.Board, BpmBoard, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(target.Resource, BpmResource, StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(target.Subresource))
            {
                ValidateBpmGateway(command);
                return;
            }

            throw new NotSupportedException("Target SDH não suportado nesta fase: " + command.Target + ".");
        }

        private static void ValidateGsaLed(SdhCommand command)
        {
            if (!string.Equals(command.Op, GsaSetOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");

            string state;
            if (!command.Args.TryGetValue(StateArg, out state) || string.IsNullOrWhiteSpace(state))
                throw new InvalidOperationException("Argumento obrigatório ausente para " + command.Target + ": state.");

            if (!string.Equals(state, "on", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(state, "off", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("State inválido para " + command.Target + ". Valores aceitos: on, off.");
            }
        }

        private static void ValidateBpmGateway(SdhCommand command)
        {
            if (!string.Equals(command.Op, BpmPingOp, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Op SDH não suportada para " + command.Target + ": " + command.Op + ".");

            if (command.Args.Count > 0)
                throw new InvalidOperationException("O comando " + command.Target + " " + command.Op + " não aceita argumentos nesta fase.");
        }
    }
}
