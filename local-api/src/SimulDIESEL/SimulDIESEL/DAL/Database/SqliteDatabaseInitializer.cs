using System;
using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IMigrationRunner _migrationRunner;

        public SqliteDatabaseInitializer()
            : this(new SqliteConnectionFactory(), new SqliteMigrationRunner())
        {
        }

        public SqliteDatabaseInitializer(IDatabaseConnectionFactory connectionFactory, IMigrationRunner migrationRunner)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _migrationRunner = migrationRunner ?? throw new ArgumentNullException(nameof(migrationRunner));
        }

        public DatabaseInitializationResult Initialize()
        {
            using (var connection = _connectionFactory.CreateOpenConnection())
            {
                EnableForeignKeys(connection);
                var appliedMigrations = _migrationRunner.ApplyMigrations(connection);
                return new DatabaseInitializationResult(_connectionFactory.DatabasePath, appliedMigrations);
            }
        }

        private static void EnableForeignKeys(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }
        }
    }
}
