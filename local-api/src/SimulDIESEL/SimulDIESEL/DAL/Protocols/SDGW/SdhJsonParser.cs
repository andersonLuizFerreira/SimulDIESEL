using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public sealed class SdhJsonParser
    {
        public SdhCommand Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON SDH é obrigatório.", nameof(json));

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> document = serializer.Deserialize<Dictionary<string, object>>(json);
            if (document == null)
                throw new InvalidOperationException("JSON SDH inválido.");

            var command = new SdhCommand
            {
                Version = RequireString(document, "version"),
                Target = RequireString(document, "target"),
                Op = RequireString(document, "op")
            };

            command.Args = ReadStringDictionary(document, "args");
            command.Meta = ReadStringDictionary(document, "meta");
            return command;
        }

        private static string RequireString(Dictionary<string, object> document, string key)
        {
            object value;
            if (!document.TryGetValue(key, out value) || value == null || string.IsNullOrWhiteSpace(ConvertToString(value)))
                throw new InvalidOperationException("Campo obrigatório ausente no JSON SDH: " + key + ".");

            return ConvertToString(value);
        }

        private static Dictionary<string, string> ReadStringDictionary(Dictionary<string, object> document, string key)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            object raw;
            if (!document.TryGetValue(key, out raw) || raw == null)
                return values;

            Dictionary<string, object> rawDictionary = raw as Dictionary<string, object>;
            if (rawDictionary == null)
                throw new InvalidOperationException("Campo JSON SDH inválido: " + key + " deve ser objeto.");

            foreach (KeyValuePair<string, object> item in rawDictionary)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    throw new InvalidOperationException("Campo JSON SDH inválido: " + key + " contém chave vazia.");

                values[item.Key] = ConvertToString(item.Value);
            }

            return values;
        }

        private static string ConvertToString(object value)
        {
            if (value == null)
                return string.Empty;

            IFormattable formattable = value as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return value.ToString();
        }
    }
}
