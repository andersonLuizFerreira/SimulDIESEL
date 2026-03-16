using System;
using System.Linq;
using SimulDIESEL.DTL;

namespace SimulDIESEL.BLL.SDH
{
    public sealed class SdhTextSerializer
    {
        public string Serialize(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command.Version))
                throw new InvalidOperationException("Version SDH é obrigatória para serialização.");

            if (string.IsNullOrWhiteSpace(command.Target))
                throw new InvalidOperationException("Target SDH é obrigatório para serialização.");

            if (string.IsNullOrWhiteSpace(command.Op))
                throw new InvalidOperationException("Op SDH é obrigatória para serialização.");

            var parts = new[] { command.Version.Trim(), command.Target.Trim(), command.Op.Trim() }
                .Concat(command.Args
                    .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .Select(kvp => kvp.Key + "=" + kvp.Value));

            return string.Join(" ", parts);
        }
    }
}
