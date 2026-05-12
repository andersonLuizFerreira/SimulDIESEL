using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using SimulDIESEL.DTL.Protocols.J1939.Common;

namespace SimulDIESEL.BLL.Protocols.J1939.Common
{
    public sealed class J1939PgnStandardCatalog
    {
        private const string RelativeCatalogPath = "Data\\Protocols\\J1939\\j1939-pgn-standard-catalog.json";

        private readonly List<J1939PgnDefinitionDto> _definitions;

        public J1939PgnStandardCatalog()
            : this(ResolveDefaultCatalogPath())
        {
        }

        public J1939PgnStandardCatalog(string catalogPath)
        {
            _definitions = Load(catalogPath);
        }

        public J1939PgnDefinitionDto FindByPgn(int pgn)
        {
            foreach (J1939PgnDefinitionDto definition in _definitions)
            {
                if (definition.Contains(pgn))
                    return definition;
            }

            return null;
        }

        public IReadOnlyList<J1939PgnDefinitionDto> GetAll()
        {
            return _definitions.AsReadOnly();
        }

        public bool Contains(int pgn)
        {
            return FindByPgn(pgn) != null;
        }

        public string GetDisplayName(int pgn)
        {
            J1939PgnDefinitionDto definition = FindByPgn(pgn);
            return definition != null ? definition.Label : "Unknown PGN";
        }

        public string GetAcronym(int pgn)
        {
            J1939PgnDefinitionDto definition = FindByPgn(pgn);
            return definition != null ? definition.Acronym : "Unknown";
        }

        public string GetCategory(int pgn)
        {
            J1939PgnDefinitionDto definition = FindByPgn(pgn);
            return definition != null ? definition.Category : "Unknown";
        }

        private static List<J1939PgnDefinitionDto> Load(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath) || !File.Exists(catalogPath))
                return new List<J1939PgnDefinitionDto>();

            string json = File.ReadAllText(catalogPath);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<J1939PgnDefinitionDto> definitions = serializer.Deserialize<List<J1939PgnDefinitionDto>>(json);
            return definitions ?? new List<J1939PgnDefinitionDto>();
        }

        private static string ResolveDefaultCatalogPath()
        {
            string currentDirectory = Environment.CurrentDirectory;
            string found = FindCatalogFrom(currentDirectory);
            if (!string.IsNullOrEmpty(found))
                return found;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            found = FindCatalogFrom(baseDirectory);
            if (!string.IsNullOrEmpty(found))
                return found;

            return Path.Combine(currentDirectory, RelativeCatalogPath);
        }

        private static string FindCatalogFrom(string startDirectory)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
                return null;

            DirectoryInfo directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, RelativeCatalogPath);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return null;
        }
    }
}
