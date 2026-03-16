using System;

namespace SimulDIESEL.DTL
{
    public sealed class SdhTarget
    {
        public string Board { get; set; }
        public string Resource { get; set; }
        public string Subresource { get; set; }

        public static SdhTarget Parse(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target SDH é obrigatório.", nameof(target));

            string[] parts = target.Trim().Split('.');
            if (parts.Length < 2 || parts.Length > 3)
                throw new ArgumentException("Target SDH inválido. Use Board.resource ou Board.resource.subresource.", nameof(target));

            if (string.IsNullOrWhiteSpace(parts[0]))
                throw new ArgumentException("Target SDH inválido. O segmento Board é obrigatório.", nameof(target));

            if (string.IsNullOrWhiteSpace(parts[1]))
                throw new ArgumentException("Target SDH inválido. O segmento Resource é obrigatório.", nameof(target));

            if (parts.Length == 3 && string.IsNullOrWhiteSpace(parts[2]))
                throw new ArgumentException("Target SDH inválido. O segmento Subresource não pode ser vazio.", nameof(target));

            return new SdhTarget
            {
                Board = parts[0],
                Resource = parts[1],
                Subresource = parts.Length > 2 ? parts[2] : null
            };
        }
    }
}
