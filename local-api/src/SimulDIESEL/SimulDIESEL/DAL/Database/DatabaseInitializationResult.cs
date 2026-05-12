using System.Collections.Generic;

namespace SimulDIESEL.DAL.Database
{
    public sealed class DatabaseInitializationResult
    {
        public DatabaseInitializationResult(string databasePath, IReadOnlyList<string> appliedMigrations)
        {
            DatabasePath = databasePath;
            AppliedMigrations = appliedMigrations;
        }

        public string DatabasePath { get; }

        public IReadOnlyList<string> AppliedMigrations { get; }
    }
}
