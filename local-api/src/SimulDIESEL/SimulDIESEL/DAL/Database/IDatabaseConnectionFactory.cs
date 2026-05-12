using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public interface IDatabaseConnectionFactory
    {
        string DatabasePath { get; }

        DbConnection CreateOpenConnection();
    }
}
