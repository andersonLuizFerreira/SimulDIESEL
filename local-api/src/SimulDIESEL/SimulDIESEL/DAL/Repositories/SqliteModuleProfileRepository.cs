using System.Collections.Generic;
using System.Data.Common;
using SimulDIESEL.DAL.Database;
using SimulDIESEL.DTL.Modules;

namespace SimulDIESEL.DAL.Repositories
{
    public sealed class SqliteModuleProfileRepository : IModuleProfileRepository
    {
        private readonly IBdServiceProvider _bdServiceProvider;

        public SqliteModuleProfileRepository(IBdServiceProvider bdServiceProvider)
        {
            _bdServiceProvider = bdServiceProvider;
        }

        public void Create(ModuleProfileDto profile)
        {
            _bdServiceProvider.ExecuteNonQuery(
                @"INSERT INTO module_profiles
                    (id, name, manufacturer, model, category, application, description, status, created_at, updated_at, sync_status, cloud_id, deleted_at)
                  VALUES
                    (@id, @name, @manufacturer, @model, @category, @application, @description, @status, @created_at, @updated_at, @sync_status, @cloud_id, @deleted_at);",
                CreateParameters(profile));
        }

        public ModuleProfileDto GetById(string id)
        {
            var results = _bdServiceProvider.Query(
                @"SELECT id, name, manufacturer, model, category, application, description, status, created_at, updated_at, sync_status, cloud_id, deleted_at
                  FROM module_profiles
                  WHERE id = @id AND deleted_at IS NULL;",
                MapProfile,
                new[]
                {
                    new BdCommandParameter("@id", id)
                });

            return results.Count == 0 ? null : results[0];
        }

        public bool Update(ModuleProfileDto profile)
        {
            int affected = _bdServiceProvider.ExecuteNonQuery(
                @"UPDATE module_profiles
                  SET name = @name,
                      manufacturer = @manufacturer,
                      model = @model,
                      category = @category,
                      application = @application,
                      description = @description,
                      status = @status,
                      updated_at = @updated_at,
                      sync_status = @sync_status,
                      cloud_id = @cloud_id
                  WHERE id = @id AND deleted_at IS NULL;",
                CreateParameters(profile));

            return affected > 0;
        }

        public bool SoftDelete(string id, string deletedAt, string syncStatus)
        {
            int affected = _bdServiceProvider.ExecuteNonQuery(
                @"UPDATE module_profiles
                  SET deleted_at = @deleted_at,
                      updated_at = @updated_at,
                      sync_status = @sync_status
                  WHERE id = @id AND deleted_at IS NULL;",
                new[]
                {
                    new BdCommandParameter("@id", id),
                    new BdCommandParameter("@deleted_at", deletedAt),
                    new BdCommandParameter("@updated_at", deletedAt),
                    new BdCommandParameter("@sync_status", syncStatus)
                });

            return affected > 0;
        }

        public IReadOnlyList<ModuleProfileDto> ListActive()
        {
            return _bdServiceProvider.Query(
                @"SELECT id, name, manufacturer, model, category, application, description, status, created_at, updated_at, sync_status, cloud_id, deleted_at
                  FROM module_profiles
                  WHERE deleted_at IS NULL
                  ORDER BY name COLLATE NOCASE, created_at;",
                MapProfile);
        }

        public int CountProfiles()
        {
            return _bdServiceProvider.ExecuteScalar<int>("SELECT COUNT(1) FROM module_profiles WHERE deleted_at IS NULL;");
        }

        private static IEnumerable<BdCommandParameter> CreateParameters(ModuleProfileDto profile)
        {
            return new[]
            {
                new BdCommandParameter("@id", profile.Id),
                new BdCommandParameter("@name", profile.Name),
                new BdCommandParameter("@manufacturer", profile.Manufacturer),
                new BdCommandParameter("@model", profile.Model),
                new BdCommandParameter("@category", profile.Category),
                new BdCommandParameter("@application", profile.Application),
                new BdCommandParameter("@description", profile.Description),
                new BdCommandParameter("@status", profile.Status),
                new BdCommandParameter("@created_at", profile.CreatedAt),
                new BdCommandParameter("@updated_at", profile.UpdatedAt),
                new BdCommandParameter("@sync_status", profile.SyncStatus),
                new BdCommandParameter("@cloud_id", profile.CloudId),
                new BdCommandParameter("@deleted_at", profile.DeletedAt)
            };
        }

        private static ModuleProfileDto MapProfile(DbDataReader reader)
        {
            return new ModuleProfileDto
            {
                Id = ReadString(reader, "id"),
                Name = ReadString(reader, "name"),
                Manufacturer = ReadString(reader, "manufacturer"),
                Model = ReadString(reader, "model"),
                Category = ReadString(reader, "category"),
                Application = ReadString(reader, "application"),
                Description = ReadString(reader, "description"),
                Status = ReadString(reader, "status"),
                CreatedAt = ReadString(reader, "created_at"),
                UpdatedAt = ReadString(reader, "updated_at"),
                SyncStatus = ReadString(reader, "sync_status"),
                CloudId = ReadString(reader, "cloud_id"),
                DeletedAt = ReadString(reader, "deleted_at")
            };
        }

        private static string ReadString(DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
    }
}
