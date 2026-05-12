using System.Collections.Generic;
using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public interface IMigrationRunner
    {
        IReadOnlyList<string> ApplyMigrations(DbConnection connection);
    }
}
