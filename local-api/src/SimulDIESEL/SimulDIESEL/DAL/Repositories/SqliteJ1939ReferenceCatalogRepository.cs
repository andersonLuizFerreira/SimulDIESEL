using System;
using System.Collections.Generic;
using System.Data.Common;
using SimulDIESEL.DAL.Database;
using SimulDIESEL.DTL.Protocols.J1939.Catalogs;

namespace SimulDIESEL.DAL.Repositories
{
    public sealed class SqliteJ1939ReferenceCatalogRepository : IJ1939ReferenceCatalogRepository
    {
        private readonly IBdServiceProvider _bdServiceProvider;

        public SqliteJ1939ReferenceCatalogRepository(IBdServiceProvider bdServiceProvider)
        {
            _bdServiceProvider = bdServiceProvider ?? throw new ArgumentNullException(nameof(bdServiceProvider));
        }

        public J1939CatalogEntryDto GetIndustryGroupByCode(int code)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, code, name, description, source, notes
                  FROM j1939_industry_groups
                  WHERE code = @code;",
                MapCatalogEntry,
                new[] { new BdCommandParameter("@code", code) }));
        }

        public J1939ManufacturerCatalogDto GetManufacturerByCode(int code)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, code, name, country, source, notes
                  FROM j1939_manufacturers
                  WHERE code = @code;",
                MapManufacturer,
                new[] { new BdCommandParameter("@code", code) }));
        }

        public J1939CatalogEntryDto GetFunctionByCode(int code)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, code, name, description, source, notes
                  FROM j1939_functions
                  WHERE code = @code;",
                MapCatalogEntry,
                new[] { new BdCommandParameter("@code", code) }));
        }

        public J1939CatalogEntryDto GetVehicleSystemByCode(int code, int? industryGroupCode)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, code, name, description, source, notes
                  FROM j1939_vehicle_systems
                  WHERE code = @code
                    AND (@industry_group_code IS NULL OR industry_group_code = @industry_group_code OR industry_group_code IS NULL)
                  ORDER BY
                    CASE
                      WHEN industry_group_code = @industry_group_code THEN 0
                      WHEN industry_group_code IS NULL THEN 1
                      ELSE 2
                    END
                  LIMIT 1;",
                MapCatalogEntry,
                new[]
                {
                    new BdCommandParameter("@code", code),
                    new BdCommandParameter("@industry_group_code", industryGroupCode)
                }));
        }

        public J1939PreferredAddressCatalogDto GetPreferredAddressByAddress(int address, int? industryGroupCode)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, address, name, description, function_code, industry_group_code, source, notes
                  FROM j1939_preferred_addresses
                  WHERE address = @address
                    AND (@industry_group_code IS NULL OR industry_group_code = @industry_group_code OR industry_group_code IS NULL OR industry_group_code = 0)
                  ORDER BY
                    CASE
                      WHEN industry_group_code = @industry_group_code THEN 0
                      WHEN industry_group_code IS NULL THEN 1
                      WHEN industry_group_code = 0 THEN 2
                      ELSE 3
                    END
                  LIMIT 1;",
                MapPreferredAddress,
                new[]
                {
                    new BdCommandParameter("@address", address),
                    new BdCommandParameter("@industry_group_code", industryGroupCode)
                }));
        }

        public J1939NameFieldDefinitionCatalogDto GetNameFieldDefinitionByFieldName(string fieldName)
        {
            return FirstOrNull(_bdServiceProvider.Query(
                @"SELECT id, field_name, bit_start, bit_length, description, source, notes
                  FROM j1939_name_field_definitions
                  WHERE field_name = @field_name;",
                MapNameFieldDefinition,
                new[] { new BdCommandParameter("@field_name", fieldName) }));
        }

        public IReadOnlyList<J1939CatalogEntryDto> ListIndustryGroups()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, code, name, description, source, notes
                  FROM j1939_industry_groups
                  ORDER BY code;",
                MapCatalogEntry);
        }

        public IReadOnlyList<J1939ManufacturerCatalogDto> ListManufacturers()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, code, name, country, source, notes
                  FROM j1939_manufacturers
                  ORDER BY code;",
                MapManufacturer);
        }

        public IReadOnlyList<J1939CatalogEntryDto> ListFunctions()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, code, name, description, source, notes
                  FROM j1939_functions
                  ORDER BY code;",
                MapCatalogEntry);
        }

        public IReadOnlyList<J1939PreferredAddressCatalogDto> ListPreferredAddresses()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, address, name, description, function_code, industry_group_code, source, notes
                  FROM j1939_preferred_addresses
                  ORDER BY address, industry_group_code;",
                MapPreferredAddress);
        }

        public IReadOnlyList<J1939NameFieldDefinitionCatalogDto> ListNameFieldDefinitions()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, field_name, bit_start, bit_length, description, source, notes
                  FROM j1939_name_field_definitions
                  ORDER BY bit_start, field_name;",
                MapNameFieldDefinition);
        }

        private static T FirstOrNull<T>(IReadOnlyList<T> results)
            where T : class
        {
            return results.Count == 0 ? null : results[0];
        }

        private static J1939CatalogEntryDto MapCatalogEntry(DbDataReader reader)
        {
            return new J1939CatalogEntryDto
            {
                Id = ReadString(reader, "id"),
                Code = ReadInt32(reader, "code").GetValueOrDefault(),
                Name = ReadString(reader, "name"),
                Description = ReadString(reader, "description"),
                Source = ReadString(reader, "source"),
                Notes = ReadString(reader, "notes"),
                IsKnown = true
            };
        }

        private static J1939ManufacturerCatalogDto MapManufacturer(DbDataReader reader)
        {
            return new J1939ManufacturerCatalogDto
            {
                Id = ReadString(reader, "id"),
                Code = ReadInt32(reader, "code").GetValueOrDefault(),
                Name = ReadString(reader, "name"),
                Country = ReadString(reader, "country"),
                Source = ReadString(reader, "source"),
                Notes = ReadString(reader, "notes"),
                IsKnown = true
            };
        }

        private static J1939PreferredAddressCatalogDto MapPreferredAddress(DbDataReader reader)
        {
            return new J1939PreferredAddressCatalogDto
            {
                Id = ReadString(reader, "id"),
                Address = ReadInt32(reader, "address").GetValueOrDefault(),
                Name = ReadString(reader, "name"),
                Description = ReadString(reader, "description"),
                FunctionCode = ReadInt32(reader, "function_code"),
                IndustryGroupCode = ReadInt32(reader, "industry_group_code"),
                Source = ReadString(reader, "source"),
                Notes = ReadString(reader, "notes"),
                IsKnown = true
            };
        }

        private static J1939NameFieldDefinitionCatalogDto MapNameFieldDefinition(DbDataReader reader)
        {
            return new J1939NameFieldDefinitionCatalogDto
            {
                Id = ReadString(reader, "id"),
                FieldName = ReadString(reader, "field_name"),
                BitStart = ReadInt32(reader, "bit_start"),
                BitLength = ReadInt32(reader, "bit_length"),
                Description = ReadString(reader, "description"),
                Source = ReadString(reader, "source"),
                Notes = ReadString(reader, "notes"),
                IsKnown = true
            };
        }

        private static string ReadString(DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static int? ReadInt32(DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? (int?)null : Convert.ToInt32(reader.GetValue(ordinal));
        }
    }
}
