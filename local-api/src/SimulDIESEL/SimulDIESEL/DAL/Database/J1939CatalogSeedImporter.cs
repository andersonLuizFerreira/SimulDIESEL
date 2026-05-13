using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Web.Script.Serialization;

namespace SimulDIESEL.DAL.Database
{
    public sealed class J1939CatalogSeedImporter
    {
        private readonly string _catalogsPath;

        public J1939CatalogSeedImporter()
            : this(ModuleDatabasePaths.ResolveJ1939CatalogsPath())
        {
        }

        public J1939CatalogSeedImporter(string catalogsPath)
        {
            _catalogsPath = catalogsPath;
        }

        public void Import(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(_catalogsPath) || !Directory.Exists(_catalogsPath))
            {
                return;
            }

            using (var transaction = connection.BeginTransaction())
            {
                ImportIndustryGroups(connection, transaction);
                ImportManufacturers(connection, transaction);
                ImportFunctions(connection, transaction);
                ImportPreferredAddresses(connection, transaction);
                ImportNameFieldDefinitions(connection, transaction);
                transaction.Commit();
            }
        }

        private void ImportIndustryGroups(DbConnection connection, DbTransaction transaction)
        {
            foreach (var item in Load<IndustryGroupSeed>("j1939_industry_groups.json"))
            {
                ExecuteNonQuery(connection, transaction,
                    @"INSERT INTO j1939_industry_groups
                        (id, code, name, description, source, notes)
                      VALUES
                        (@id, @code, @name, @description, @source, @notes)
                      ON CONFLICT(code) DO UPDATE SET
                        name = excluded.name,
                        description = excluded.description,
                        source = excluded.source,
                        notes = excluded.notes,
                        updated_at = CURRENT_TIMESTAMP;",
                    new Dictionary<string, object>
                    {
                        { "@id", item.id },
                        { "@code", item.code },
                        { "@name", item.name },
                        { "@description", item.description },
                        { "@source", item.source },
                        { "@notes", item.notes }
                    });
            }
        }

        private void ImportManufacturers(DbConnection connection, DbTransaction transaction)
        {
            foreach (var item in Load<ManufacturerSeed>("j1939_manufacturers.json"))
            {
                ExecuteNonQuery(connection, transaction,
                    @"INSERT INTO j1939_manufacturers
                        (id, code, name, country, source, notes)
                      VALUES
                        (@id, @code, @name, @country, @source, @notes)
                      ON CONFLICT(code) DO UPDATE SET
                        name = excluded.name,
                        country = excluded.country,
                        source = excluded.source,
                        notes = excluded.notes,
                        updated_at = CURRENT_TIMESTAMP;",
                    new Dictionary<string, object>
                    {
                        { "@id", item.id },
                        { "@code", item.code },
                        { "@name", item.name },
                        { "@country", item.country },
                        { "@source", item.source },
                        { "@notes", item.notes }
                    });
            }
        }

        private void ImportFunctions(DbConnection connection, DbTransaction transaction)
        {
            foreach (var item in Load<FunctionSeed>("j1939_functions.json"))
            {
                ExecuteNonQuery(connection, transaction,
                    @"INSERT INTO j1939_functions
                        (id, code, name, description, source, notes)
                      VALUES
                        (@id, @code, @name, @description, @source, @notes)
                      ON CONFLICT(code) DO UPDATE SET
                        name = excluded.name,
                        description = excluded.description,
                        source = excluded.source,
                        notes = excluded.notes,
                        updated_at = CURRENT_TIMESTAMP;",
                    new Dictionary<string, object>
                    {
                        { "@id", item.id },
                        { "@code", item.code },
                        { "@name", item.name },
                        { "@description", item.description },
                        { "@source", item.source },
                        { "@notes", item.notes }
                    });
            }
        }

        private void ImportPreferredAddresses(DbConnection connection, DbTransaction transaction)
        {
            foreach (var item in Load<PreferredAddressSeed>("j1939_preferred_addresses.json"))
            {
                ExecuteNonQuery(connection, transaction,
                    @"INSERT INTO j1939_preferred_addresses
                        (id, address, name, description, function_code, industry_group_code, source, notes)
                      VALUES
                        (@id, @address, @name, @description, @function_code, @industry_group_code, @source, @notes)
                      ON CONFLICT(address, industry_group_code) DO UPDATE SET
                        name = excluded.name,
                        description = excluded.description,
                        function_code = excluded.function_code,
                        source = excluded.source,
                        notes = excluded.notes,
                        updated_at = CURRENT_TIMESTAMP;",
                    new Dictionary<string, object>
                    {
                        { "@id", item.id },
                        { "@address", item.address },
                        { "@name", item.name },
                        { "@description", item.description },
                        { "@function_code", item.function_code },
                        { "@industry_group_code", item.industry_group_code },
                        { "@source", item.source },
                        { "@notes", item.notes }
                    });
            }
        }

        private void ImportNameFieldDefinitions(DbConnection connection, DbTransaction transaction)
        {
            foreach (var item in Load<NameFieldDefinitionSeed>("j1939_name_field_definitions.json"))
            {
                ExecuteNonQuery(connection, transaction,
                    @"INSERT INTO j1939_name_field_definitions
                        (id, field_name, bit_start, bit_length, description, source, notes)
                      VALUES
                        (@id, @field_name, @bit_start, @bit_length, @description, @source, @notes)
                      ON CONFLICT(field_name) DO UPDATE SET
                        bit_start = excluded.bit_start,
                        bit_length = excluded.bit_length,
                        description = excluded.description,
                        source = excluded.source,
                        notes = excluded.notes,
                        updated_at = CURRENT_TIMESTAMP;",
                    new Dictionary<string, object>
                    {
                        { "@id", item.id },
                        { "@field_name", item.field_name },
                        { "@bit_start", item.bit_start },
                        { "@bit_length", item.bit_length },
                        { "@description", item.description },
                        { "@source", item.source },
                        { "@notes", item.notes }
                    });
            }
        }

        private List<T> Load<T>(string fileName)
        {
            var path = Path.Combine(_catalogsPath, fileName);
            if (!File.Exists(path))
            {
                return new List<T>();
            }

            var serializer = new JavaScriptSerializer();
            var records = serializer.Deserialize<List<T>>(File.ReadAllText(path));
            return records ?? new List<T>();
        }

        private static void ExecuteNonQuery(DbConnection connection, DbTransaction transaction, string sql, IDictionary<string, object> parameters)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql;

                foreach (var parameter in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = parameter.Key;
                    dbParameter.Value = parameter.Value ?? DBNull.Value;
                    command.Parameters.Add(dbParameter);
                }

                command.ExecuteNonQuery();
            }
        }

        private sealed class IndustryGroupSeed
        {
            public string id { get; set; }
            public int code { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string source { get; set; }
            public string notes { get; set; }
        }

        private sealed class ManufacturerSeed
        {
            public string id { get; set; }
            public int code { get; set; }
            public string name { get; set; }
            public string country { get; set; }
            public string source { get; set; }
            public string notes { get; set; }
        }

        private sealed class FunctionSeed
        {
            public string id { get; set; }
            public int code { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string source { get; set; }
            public string notes { get; set; }
        }

        private sealed class PreferredAddressSeed
        {
            public string id { get; set; }
            public int address { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int? function_code { get; set; }
            public int? industry_group_code { get; set; }
            public string source { get; set; }
            public string notes { get; set; }
        }

        private sealed class NameFieldDefinitionSeed
        {
            public string id { get; set; }
            public string field_name { get; set; }
            public int? bit_start { get; set; }
            public int? bit_length { get; set; }
            public string description { get; set; }
            public string source { get; set; }
            public string notes { get; set; }
        }
    }
}
