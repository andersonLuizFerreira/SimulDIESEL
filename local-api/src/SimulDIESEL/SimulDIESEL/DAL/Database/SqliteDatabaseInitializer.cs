using System;
using System.Data.Common;

namespace SimulDIESEL.DAL.Database
{
    public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IMigrationRunner _migrationRunner;
        private readonly J1939CatalogSeedImporter _j1939CatalogSeedImporter;

        public SqliteDatabaseInitializer()
            : this(new SqliteConnectionFactory(), new SqliteMigrationRunner(), new J1939CatalogSeedImporter())
        {
        }

        public SqliteDatabaseInitializer(IDatabaseConnectionFactory connectionFactory, IMigrationRunner migrationRunner)
            : this(connectionFactory, migrationRunner, new J1939CatalogSeedImporter())
        {
        }

        public SqliteDatabaseInitializer(IDatabaseConnectionFactory connectionFactory, IMigrationRunner migrationRunner, J1939CatalogSeedImporter j1939CatalogSeedImporter)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _migrationRunner = migrationRunner ?? throw new ArgumentNullException(nameof(migrationRunner));
            _j1939CatalogSeedImporter = j1939CatalogSeedImporter ?? throw new ArgumentNullException(nameof(j1939CatalogSeedImporter));
        }

        public DatabaseInitializationResult Initialize()
        {
            using (var connection = _connectionFactory.CreateOpenConnection())
            {
                EnableForeignKeys(connection);
                var appliedMigrations = _migrationRunner.ApplyMigrations(connection);
                _j1939CatalogSeedImporter.Import(connection);
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
