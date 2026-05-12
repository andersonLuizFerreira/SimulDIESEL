using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace SimulDIESEL.DAL.Database
{
    public sealed class SqliteMigrationRunner : IMigrationRunner
    {
        private const string BaselineMigrationId = "0001_sqlite_schema_v1";
        private const string AddColumnDirective = "-- SIMULDIESEL_ADD_COLUMN_IF_MISSING ";

        private readonly string _schemaPath;
        private readonly string _migrationsPath;

        public SqliteMigrationRunner()
            : this(ModuleDatabasePaths.ResolveSchemaPath(), ModuleDatabasePaths.ResolveMigrationsPath())
        {
        }

        public SqliteMigrationRunner(string schemaPath, string migrationsPath)
        {
            _schemaPath = schemaPath;
            _migrationsPath = migrationsPath;
        }

        public IReadOnlyList<string> ApplyMigrations(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            EnsureMigrationsTable(connection);

            var applied = new List<string>();
            foreach (var migration in GetMigrationSources())
            {
                if (IsMigrationApplied(connection, migration.Id))
                {
                    continue;
                }

                ExecuteMigration(connection, migration);
                RecordMigration(connection, migration.Id);
                applied.Add(migration.Id);
            }

            return applied;
        }

        private IEnumerable<MigrationSource> GetMigrationSources()
        {
            if (!File.Exists(_schemaPath))
            {
                throw new FileNotFoundException("SQLite schema baseline not found.", _schemaPath);
            }

            yield return new MigrationSource(BaselineMigrationId, _schemaPath, File.ReadAllText(_schemaPath, Encoding.UTF8));

            if (!Directory.Exists(_migrationsPath))
            {
                yield break;
            }

            foreach (var filePath in Directory.GetFiles(_migrationsPath, "*.sql").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var id = Path.GetFileNameWithoutExtension(filePath);
                yield return new MigrationSource(id, filePath, File.ReadAllText(filePath, Encoding.UTF8));
            }
        }

        private static void EnsureMigrationsTable(DbConnection connection)
        {
            ExecuteNonQuery(connection, @"
CREATE TABLE IF NOT EXISTS schema_migrations (
    id TEXT PRIMARY KEY,
    applied_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);");
        }

        private static bool IsMigrationApplied(DbConnection connection, string migrationId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(1) FROM schema_migrations WHERE id = @id;";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = migrationId;
                command.Parameters.Add(parameter);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void ExecuteMigration(DbConnection connection, MigrationSource migration)
        {
            var sqlBuilder = new StringBuilder();

            foreach (var line in ReadLogicalLines(migration.Sql))
            {
                if (line.StartsWith(AddColumnDirective, StringComparison.Ordinal))
                {
                    ExecuteSqlBatch(connection, sqlBuilder.ToString());
                    sqlBuilder.Clear();
                    ExecuteAddColumnIfMissing(connection, line.Substring(AddColumnDirective.Length), migration.Path);
                    continue;
                }

                sqlBuilder.AppendLine(line);
            }

            ExecuteSqlBatch(connection, sqlBuilder.ToString());
        }

        private static IEnumerable<string> ReadLogicalLines(string sql)
        {
            using (var reader = new StringReader(sql))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static void ExecuteAddColumnIfMissing(DbConnection connection, string directive, string migrationPath)
        {
            var parts = directive.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                throw new InvalidOperationException("Invalid add-column migration directive in " + migrationPath);
            }

            var tableName = parts[0];
            var columnName = parts[1];
            var columnDefinition = parts[2];

            if (ColumnExists(connection, tableName, columnName))
            {
                return;
            }

            ExecuteNonQuery(connection, "ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + columnDefinition + ";");
        }

        private static bool ColumnExists(DbConnection connection, string tableName, string columnName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(" + tableName + ");";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void RecordMigration(DbConnection connection, string migrationId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO schema_migrations (id) VALUES (@id);";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = migrationId;
                command.Parameters.Add(parameter);
                command.ExecuteNonQuery();
            }
        }

        private static void ExecuteSqlBatch(DbConnection connection, string sql)
        {
            foreach (var statement in SplitStatements(sql))
            {
                ExecuteNonQuery(connection, statement);
            }
        }

        private static IEnumerable<string> SplitStatements(string sql)
        {
            foreach (var statement in sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = statement.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    yield return trimmed;
                }
            }
        }

        private static void ExecuteNonQuery(DbConnection connection, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private sealed class MigrationSource
        {
            public MigrationSource(string id, string path, string sql)
            {
                Id = id;
                Path = path;
                Sql = sql;
            }

            public string Id { get; }

            public string Path { get; }

            public string Sql { get; }
        }
    }
}
