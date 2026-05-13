using System;
using System.Collections.Generic;
using SimulDIESEL.DAL.Repositories;
using SimulDIESEL.DTL.Protocols.J1939.Catalogs;

namespace SimulDIESEL.BLL.Services.Database
{
    public sealed class J1939ReferenceCatalogService
    {
        private const string UnknownName = "Desconhecido";

        private readonly IJ1939ReferenceCatalogRepository _repository;

        public J1939ReferenceCatalogService(IJ1939ReferenceCatalogRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public J1939CatalogEntryDto ResolveIndustryGroup(int code)
        {
            ValidateRange(code, 0, 7, nameof(code));
            return _repository.GetIndustryGroupByCode(code) ?? CreateUnknownEntry(code);
        }

        public J1939ManufacturerCatalogDto ResolveManufacturer(int code)
        {
            ValidateMinimum(code, 0, nameof(code));
            return _repository.GetManufacturerByCode(code) ?? new J1939ManufacturerCatalogDto
            {
                Code = code,
                Name = UnknownName,
                IsKnown = false
            };
        }

        public J1939CatalogEntryDto ResolveFunction(int code)
        {
            ValidateRange(code, 0, 255, nameof(code));
            return _repository.GetFunctionByCode(code) ?? CreateUnknownEntry(code);
        }

        public J1939CatalogEntryDto ResolveVehicleSystem(int code, int? industryGroupCode)
        {
            ValidateRange(code, 0, 127, nameof(code));
            ValidateOptionalRange(industryGroupCode, 0, 7, nameof(industryGroupCode));
            return _repository.GetVehicleSystemByCode(code, industryGroupCode) ?? CreateUnknownEntry(code);
        }

        public J1939PreferredAddressCatalogDto ResolvePreferredAddress(int address, int? industryGroupCode)
        {
            ValidateRange(address, 0, 255, nameof(address));
            ValidateOptionalRange(industryGroupCode, 0, 7, nameof(industryGroupCode));
            return _repository.GetPreferredAddressByAddress(address, industryGroupCode) ?? new J1939PreferredAddressCatalogDto
            {
                Address = address,
                Name = UnknownName,
                IndustryGroupCode = industryGroupCode,
                IsKnown = false
            };
        }

        public J1939NameFieldDefinitionCatalogDto ResolveNameFieldDefinition(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException("Nome do campo NAME J1939/81 e obrigatorio.", nameof(fieldName));
            }

            string normalized = fieldName.Trim();
            return _repository.GetNameFieldDefinitionByFieldName(normalized) ?? new J1939NameFieldDefinitionCatalogDto
            {
                FieldName = normalized,
                IsKnown = false
            };
        }

        public IReadOnlyList<J1939CatalogEntryDto> ListIndustryGroups()
        {
            return _repository.ListIndustryGroups();
        }

        public IReadOnlyList<J1939ManufacturerCatalogDto> ListManufacturers()
        {
            return _repository.ListManufacturers();
        }

        public IReadOnlyList<J1939CatalogEntryDto> ListFunctions()
        {
            return _repository.ListFunctions();
        }

        public IReadOnlyList<J1939PreferredAddressCatalogDto> ListPreferredAddresses()
        {
            return _repository.ListPreferredAddresses();
        }

        public IReadOnlyList<J1939NameFieldDefinitionCatalogDto> ListNameFieldDefinitions()
        {
            return _repository.ListNameFieldDefinitions();
        }

        private static J1939CatalogEntryDto CreateUnknownEntry(int code)
        {
            return new J1939CatalogEntryDto
            {
                Code = code,
                Name = UnknownName,
                IsKnown = false
            };
        }

        private static void ValidateOptionalRange(int? value, int minimum, int maximum, string parameterName)
        {
            if (value.HasValue)
            {
                ValidateRange(value.Value, minimum, maximum, parameterName);
            }
        }

        private static void ValidateRange(int value, int minimum, int maximum, string parameterName)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Codigo fora da faixa permitida para J1939/81.");
            }
        }

        private static void ValidateMinimum(int value, int minimum, string parameterName)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Codigo fora da faixa permitida para J1939/81.");
            }
        }
    }
}
