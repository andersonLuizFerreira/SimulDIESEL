using System.Collections.Generic;
using SimulDIESEL.DTL.Protocols.J1939.Catalogs;

namespace SimulDIESEL.DAL.Repositories
{
    public interface IJ1939ReferenceCatalogRepository
    {
        J1939CatalogEntryDto GetIndustryGroupByCode(int code);

        J1939ManufacturerCatalogDto GetManufacturerByCode(int code);

        J1939CatalogEntryDto GetFunctionByCode(int code);

        J1939CatalogEntryDto GetVehicleSystemByCode(int code, int? industryGroupCode);

        J1939PreferredAddressCatalogDto GetPreferredAddressByAddress(int address, int? industryGroupCode);

        J1939NameFieldDefinitionCatalogDto GetNameFieldDefinitionByFieldName(string fieldName);

        IReadOnlyList<J1939CatalogEntryDto> ListIndustryGroups();

        IReadOnlyList<J1939ManufacturerCatalogDto> ListManufacturers();

        IReadOnlyList<J1939CatalogEntryDto> ListFunctions();

        IReadOnlyList<J1939PreferredAddressCatalogDto> ListPreferredAddresses();

        IReadOnlyList<J1939NameFieldDefinitionCatalogDto> ListNameFieldDefinitions();
    }
}
