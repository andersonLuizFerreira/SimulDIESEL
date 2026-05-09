using System.Collections.Generic;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939FmiCatalog
    {
        private readonly Dictionary<byte, J1939FmiDefinitionDto> _definitions;

        public J1939FmiCatalog()
        {
            _definitions = BuildCatalog();
        }

        public J1939FmiDefinitionDto Get(byte fmi)
        {
            J1939FmiDefinitionDto definition;
            if (_definitions.TryGetValue(fmi, out definition))
                return definition;

            return new J1939FmiDefinitionDto
            {
                Fmi = fmi,
                Description = "FMI nao cadastrado"
            };
        }

        private static Dictionary<byte, J1939FmiDefinitionDto> BuildCatalog()
        {
            Dictionary<byte, J1939FmiDefinitionDto> catalog = new Dictionary<byte, J1939FmiDefinitionDto>();
            Add(catalog, 0, "Data valid but above normal operational range - most severe");
            Add(catalog, 1, "Data valid but below normal operational range - most severe");
            Add(catalog, 2, "Data erratic, intermittent or incorrect");
            Add(catalog, 3, "Voltage above normal or shorted high");
            Add(catalog, 4, "Voltage below normal or shorted low");
            Add(catalog, 5, "Current below normal or open circuit");
            Add(catalog, 6, "Current above normal or grounded circuit");
            Add(catalog, 7, "Mechanical system not responding properly");
            Add(catalog, 8, "Abnormal frequency, pulse width, or period");
            Add(catalog, 9, "Abnormal update rate");
            Add(catalog, 10, "Abnormal rate of change");
            Add(catalog, 11, "Root cause not known");
            Add(catalog, 12, "Bad intelligent device or component");
            Add(catalog, 13, "Out of calibration");
            Add(catalog, 14, "Special instructions");
            Add(catalog, 15, "Data valid but above normal operational range - least severe");
            Add(catalog, 16, "Data valid but above normal operational range - moderately severe");
            Add(catalog, 17, "Data valid but below normal operational range - least severe");
            Add(catalog, 18, "Data valid but below normal operational range - moderately severe");
            Add(catalog, 19, "Received network data in error");
            Add(catalog, 20, "Data drifted high");
            Add(catalog, 21, "Data drifted low");
            Add(catalog, 31, "Condition exists");
            return catalog;
        }

        private static void Add(Dictionary<byte, J1939FmiDefinitionDto> catalog, byte fmi, string description)
        {
            catalog[fmi] = new J1939FmiDefinitionDto
            {
                Fmi = fmi,
                Description = description
            };
        }
    }
}
