using System;
using System.IO;

namespace SimulDIESEL.DAL.Database
{
    public static class ModuleDatabasePaths
    {
        public const string DatabaseRelativePath = "Data\\Modules\\modules.db";
        public const string SchemaRelativePath = "Data\\Modules\\schema\\sqlite_schema_v1.sql";
        public const string MigrationsRelativePath = "Data\\Modules\\schema\\migrations";
        public const string J1939CatalogsRelativePath = "Data\\Protocols\\J1939\\catalogs";

        public static string ResolveDatabasePath()
        {
            return Path.Combine(FindRepositoryRoot(), DatabaseRelativePath);
        }

        public static string ResolveSchemaPath()
        {
            return Path.Combine(FindRepositoryRoot(), SchemaRelativePath);
        }

        public static string ResolveMigrationsPath()
        {
            return Path.Combine(FindRepositoryRoot(), MigrationsRelativePath);
        }

        public static string ResolveJ1939CatalogsPath()
        {
            return Path.Combine(FindRepositoryRoot(), J1939CatalogsRelativePath);
        }

        private static string FindRepositoryRoot()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (current != null)
            {
                var schemaPath = Path.Combine(current.FullName, SchemaRelativePath);
                var agentsPath = Path.Combine(current.FullName, "AGENTS.md");

                if (File.Exists(schemaPath) && File.Exists(agentsPath))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
