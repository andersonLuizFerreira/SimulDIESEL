using System.Collections.Generic;
using SimulDIESEL.DTL.Protocols.J1939.Application;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939PgnCatalog
    {
        private readonly Dictionary<int, J1939PgnDefinitionDto> _definitions;

        public J1939PgnCatalog()
        {
            _definitions = BuildMiniCatalog();
        }

        public bool TryGetDefinition(int pgn, out J1939PgnDefinitionDto definition)
        {
            return _definitions.TryGetValue(pgn, out definition);
        }

        public IReadOnlyCollection<J1939PgnDefinitionDto> GetAll()
        {
            return _definitions.Values;
        }

        private static Dictionary<int, J1939PgnDefinitionDto> BuildMiniCatalog()
        {
            Dictionary<int, J1939PgnDefinitionDto> catalog = new Dictionary<int, J1939PgnDefinitionDto>();

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 61444,
                Hex = "00F004",
                Name = "Electronic Engine Controller 1",
                Acronym = "EEC1",
                ExpectedLengthBytes = 8,
                NominalRepetition = "engine speed dependent",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(190, "Engine Speed", 61444, 4, null, 16, "Unsigned", "Measured", 0.125, 0, "rpm")
                }
            });

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 61443,
                Hex = "00F003",
                Name = "Electronic Engine Controller 2",
                Acronym = "EEC2",
                ExpectedLengthBytes = 8,
                NominalRepetition = "50 ms",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(91, "Accelerator Pedal Position 1", 61443, 2, null, 8, "Unsigned", "Measured", 0.4, 0, "%"),
                    Spn(92, "Percent Load At Current Speed", 61443, 3, null, 8, "Unsigned", "Status", 1.0, 0, "%")
                }
            });

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 65262,
                Hex = "00FEEE",
                Name = "Engine Temperature 1",
                Acronym = "ET1",
                ExpectedLengthBytes = 8,
                NominalRepetition = "1 s",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(110, "Engine Coolant Temperature", 65262, 1, null, 8, "Unsigned", "Measured", 1.0, -40, "deg C"),
                    Spn(174, "Fuel Temperature", 65262, 2, null, 8, "Unsigned", "Measured", 1.0, -40, "deg C"),
                    Spn(175, "Engine Oil Temperature 1", 65262, 3, null, 16, "Unsigned", "Measured", 0.03125, -273, "deg C")
                }
            });

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 65263,
                Hex = "00FEEF",
                Name = "Engine Fluid Level/Pressure 1",
                Acronym = "EFL/P1",
                ExpectedLengthBytes = 8,
                NominalRepetition = "0.5 s",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(94, "Fuel Delivery Pressure", 65263, 1, null, 8, "Unsigned", "Measured", 4.0, 0, "kPa"),
                    Spn(98, "Engine Oil Level", 65263, 3, null, 8, "Unsigned", "Measured", 0.4, 0, "%"),
                    Spn(100, "Engine Oil Pressure", 65263, 4, null, 8, "Unsigned", "Measured", 4.0, 0, "kPa"),
                    Spn(111, "Coolant Level", 65263, 8, null, 8, "Unsigned", "Measured", 0.4, 0, "%")
                }
            });

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 65265,
                Hex = "00FEF1",
                Name = "Cruise Control/Vehicle Speed",
                Acronym = "CCVS",
                ExpectedLengthBytes = 8,
                NominalRepetition = "100 ms",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(84, "Wheel-Based Vehicle Speed", 65265, 2, null, 16, "Unsigned", "Measured", 1.0 / 256.0, 0, "km/h"),
                    DiscreteSpn(70, "Parking Brake Switch", 65265, 1, 3, 2, "Measured")
                }
            });

            Add(catalog, new J1939PgnDefinitionDto
            {
                Pgn = 65271,
                Hex = "00FEF7",
                Name = "Vehicle Electrical Power",
                Acronym = "VEP",
                ExpectedLengthBytes = 8,
                NominalRepetition = "1 s",
                Notes = "Positions confirmed in J1939-71 Parameter Group Definitions.",
                Spns = new List<J1939SpnDefinitionDto>
                {
                    Spn(168, "Electrical Potential (Voltage)", 65271, 5, null, 16, "Unsigned", "Measured", 0.05, 0, "V"),
                    Spn(158, "Battery Potential (Voltage), Switched", 65271, 7, null, 16, "Unsigned", "Measured", 0.05, 0, "V")
                }
            });

            return catalog;
        }

        private static void Add(Dictionary<int, J1939PgnDefinitionDto> catalog, J1939PgnDefinitionDto definition)
        {
            catalog[definition.Pgn] = definition;
        }

        private static J1939SpnDefinitionDto Spn(
            int spn,
            string name,
            int pgn,
            int startByte,
            int? startBit,
            int bitLength,
            string dataType,
            string valueType,
            double resolution,
            double offset,
            string unit)
        {
            return new J1939SpnDefinitionDto
            {
                Spn = spn,
                Name = name,
                Pgn = pgn,
                StartByte = startByte,
                StartBit = startBit,
                BitLength = bitLength,
                DataType = dataType,
                ValueType = valueType,
                Resolution = resolution,
                Offset = offset,
                Unit = unit,
                PositionPending = false
            };
        }

        private static J1939SpnDefinitionDto DiscreteSpn(
            int spn,
            string name,
            int pgn,
            int startByte,
            int startBit,
            int bitLength,
            string valueType)
        {
            J1939SpnDefinitionDto definition = Spn(spn, name, pgn, startByte, startBit, bitLength, "Discrete", valueType, 1.0, 0, string.Empty);
            definition.IsDiscrete = true;
            return definition;
        }
    }
}
