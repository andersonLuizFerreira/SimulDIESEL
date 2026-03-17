using System;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdhTextParser
    {
        public SdhCommand Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Texto SDH é obrigatório.", nameof(text));

            string[] tokens = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
                throw new InvalidOperationException("Comando SDH inválido. Esperado: version target op [chave=valor...]");

            var command = new SdhCommand
            {
                Version = tokens[0],
                Target = tokens[1],
                Op = tokens[2]
            };

            for (int i = 3; i < tokens.Length; i++)
            {
                string token = tokens[i];
                int separatorIndex = token.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex == token.Length - 1)
                    throw new InvalidOperationException("Argumento SDH inválido: '" + token + "'. Use chave=valor.");

                string key = token.Substring(0, separatorIndex);
                string value = token.Substring(separatorIndex + 1);

                command.Args[key] = value;
            }

            return command;
        }
    }
}
