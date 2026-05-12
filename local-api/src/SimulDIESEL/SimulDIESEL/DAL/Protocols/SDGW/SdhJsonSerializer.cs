using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdhJsonSerializer
    {
        public string Serialize(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command.Version))
                throw new InvalidOperationException("Version SDH é obrigatória para serialização JSON.");

            if (string.IsNullOrWhiteSpace(command.Target))
                throw new InvalidOperationException("Target SDH é obrigatório para serialização JSON.");

            if (string.IsNullOrWhiteSpace(command.Op))
                throw new InvalidOperationException("Op SDH é obrigatória para serialização JSON.");

            var document = new Dictionary<string, object>
            {
                { "version", command.Version },
                { "target", command.Target },
                { "op", command.Op },
                { "args", command.Args ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) },
                { "meta", command.Meta ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) }
            };

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(document);
        }
    }
}
