using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace SimulDIESEL.DAL.Database
{
    public sealed class SqliteConnectionFactory : IDatabaseConnectionFactory
    {
        public SqliteConnectionFactory()
            : this(ModuleDatabasePaths.ResolveDatabasePath())
        {
        }

        public SqliteConnectionFactory(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public string DatabasePath { get; }

        public DbConnection CreateOpenConnection()
        {
            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                ForeignKeys = true
            }.ToString();

            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
