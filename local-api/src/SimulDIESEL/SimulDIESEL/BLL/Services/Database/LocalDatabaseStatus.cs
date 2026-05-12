using System.Collections.Generic;

namespace SimulDIESEL.BLL.Services.Database
{
    public sealed class LocalDatabaseStatus
    {
        public LocalDatabaseStatus(string databasePath, IReadOnlyList<string> appliedMigrations, int moduleProfileCount)
        {
            DatabasePath = databasePath;
            AppliedMigrations = appliedMigrations;
            ModuleProfileCount = moduleProfileCount;
        }

        public string DatabasePath { get; }

        public IReadOnlyList<string> AppliedMigrations { get; }

        public int ModuleProfileCount { get; }
    }
}
