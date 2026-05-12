using System;
using System.Collections.Generic;
using System.Globalization;
using SimulDIESEL.DAL.Repositories;
using SimulDIESEL.DTL.Modules;

namespace SimulDIESEL.BLL.Services.Database
{
    public sealed class ModuleProfileService
    {
        private const string DefaultStatus = "draft";
        private const string SyncStatusLocal = "local";
        private const string SyncStatusPending = "pending";
        private const string SyncStatusDeleted = "deleted";

        private readonly IModuleProfileRepository _moduleProfileRepository;

        public ModuleProfileService(IModuleProfileRepository moduleProfileRepository)
        {
            _moduleProfileRepository = moduleProfileRepository ?? throw new ArgumentNullException(nameof(moduleProfileRepository));
        }

        public ModuleProfileDto Create(ModuleProfileCreateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string status = NormalizeStatus(request.Status);
            ValidateName(request.Name);

            string now = CreateTimestamp();
            var profile = new ModuleProfileDto
            {
                Id = Guid.NewGuid().ToString("D"),
                Name = request.Name.Trim(),
                Manufacturer = NormalizeOptional(request.Manufacturer),
                Model = NormalizeOptional(request.Model),
                Category = NormalizeOptional(request.Category),
                Application = NormalizeOptional(request.Application),
                Description = NormalizeOptional(request.Description),
                Status = status,
                CreatedAt = now,
                UpdatedAt = now,
                SyncStatus = SyncStatusLocal
            };

            _moduleProfileRepository.Create(profile);
            return profile;
        }

        public ModuleProfileDto GetById(string id)
        {
            ValidateId(id);
            return _moduleProfileRepository.GetById(id.Trim());
        }

        public IReadOnlyList<ModuleProfileDto> ListActive()
        {
            return _moduleProfileRepository.ListActive();
        }

        public bool Update(ModuleProfileUpdateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateId(request.Id);
            ValidateName(request.Name);

            var existing = _moduleProfileRepository.GetById(request.Id.Trim());
            if (existing == null)
            {
                return false;
            }

            existing.Name = request.Name.Trim();
            existing.Manufacturer = NormalizeOptional(request.Manufacturer);
            existing.Model = NormalizeOptional(request.Model);
            existing.Category = NormalizeOptional(request.Category);
            existing.Application = NormalizeOptional(request.Application);
            existing.Description = NormalizeOptional(request.Description);
            existing.Status = NormalizeStatus(request.Status);
            existing.UpdatedAt = CreateTimestamp();
            existing.SyncStatus = SyncStatusPending;

            return _moduleProfileRepository.Update(existing);
        }

        public bool SoftDelete(string id)
        {
            ValidateId(id);
            return _moduleProfileRepository.SoftDelete(id.Trim(), CreateTimestamp(), SyncStatusDeleted);
        }

        private static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return DefaultStatus;
            }

            string normalized = status.Trim().ToLowerInvariant();
            if (normalized == "draft" || normalized == "active" || normalized == "archived")
            {
                return normalized;
            }

            throw new ArgumentException("Status de perfil de modulo invalido.", nameof(status));
        }

        private static void ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id do perfil de modulo e obrigatorio.", nameof(id));
            }
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nome do perfil de modulo e obrigatorio.", nameof(name));
            }
        }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string CreateTimestamp()
        {
            return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
