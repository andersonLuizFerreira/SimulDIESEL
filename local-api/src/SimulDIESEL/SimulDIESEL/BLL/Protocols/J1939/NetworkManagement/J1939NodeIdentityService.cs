using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Services.Database;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NodeIdentityService
    {
        private const string UnknownName = "Desconhecido";

        private readonly J1939ReferenceCatalogService _catalogService;

        public J1939NodeIdentityService(J1939ReferenceCatalogService catalogService)
        {
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        }

        public J1939NodeIdentityDto Resolve(J1939AddressRegistryEntryDto entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var name = entry.ParsedName;
            var identity = new J1939NodeIdentityDto
            {
                SourceAddressDecimal = entry.SourceAddress,
                SourceAddressHex = FormatSourceAddress(entry.SourceAddress),
                NameHex = entry.NameHex ?? (name != null ? name.NameHex : null),
                LastSeenAt = entry.LastSeenTimestamp,
                Status = string.IsNullOrWhiteSpace(entry.ClaimStatus) ? "Desconhecido" : entry.ClaimStatus
            };

            if (name != null)
            {
                FillNameFields(identity, name, entry.SourceAddress);
            }
            else
            {
                FillUnknownNameFields(identity, entry.SourceAddress);
            }

            identity.Summary = BuildSummary(identity);
            return identity;
        }

        public IReadOnlyList<J1939NodeIdentityDto> ResolveAll(IEnumerable<J1939AddressRegistryEntryDto> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var result = new List<J1939NodeIdentityDto>();
            foreach (var entry in entries)
            {
                if (entry != null)
                {
                    result.Add(Resolve(entry));
                }
            }

            return result;
        }

        private void FillNameFields(J1939NodeIdentityDto identity, J1939NameDto name, byte sourceAddress)
        {
            var manufacturer = _catalogService.ResolveManufacturer(name.ManufacturerCode);
            var function = _catalogService.ResolveFunction(name.Function);
            var industryGroup = _catalogService.ResolveIndustryGroup(name.IndustryGroup);
            var vehicleSystem = _catalogService.ResolveVehicleSystem(name.VehicleSystem, name.IndustryGroup);
            var preferredAddress = _catalogService.ResolvePreferredAddress(sourceAddress, name.IndustryGroup);

            identity.ManufacturerCode = name.ManufacturerCode;
            identity.ManufacturerName = manufacturer.Name;
            identity.ManufacturerKnown = manufacturer.IsKnown;
            identity.FunctionCode = name.Function;
            identity.FunctionName = function.Name;
            identity.FunctionKnown = function.IsKnown;
            identity.IndustryGroupCode = name.IndustryGroup;
            identity.IndustryGroupName = industryGroup.Name;
            identity.IndustryGroupKnown = industryGroup.IsKnown;
            identity.VehicleSystemCode = name.VehicleSystem;
            identity.VehicleSystemName = vehicleSystem.Name;
            identity.VehicleSystemKnown = vehicleSystem.IsKnown;
            identity.PreferredAddressName = preferredAddress.Name;
            identity.PreferredAddressKnown = preferredAddress.IsKnown;
            identity.EcuInstance = name.EcuInstance;
            identity.FunctionInstance = name.FunctionInstance;
            identity.VehicleSystemInstance = name.VehicleSystemInstance;
            identity.IdentityNumber = name.IdentityNumber;
            identity.ArbitraryAddressCapable = name.IsArbitraryAddressCapable;
        }

        private void FillUnknownNameFields(J1939NodeIdentityDto identity, byte sourceAddress)
        {
            var preferredAddress = _catalogService.ResolvePreferredAddress(sourceAddress, null);

            identity.ManufacturerName = UnknownName;
            identity.FunctionName = UnknownName;
            identity.IndustryGroupName = UnknownName;
            identity.VehicleSystemName = UnknownName;
            identity.PreferredAddressName = preferredAddress.Name;
            identity.PreferredAddressKnown = preferredAddress.IsKnown;
        }

        private static string FormatSourceAddress(byte sourceAddress)
        {
            return "0x" + sourceAddress.ToString("X2");
        }

        private static string BuildSummary(J1939NodeIdentityDto identity)
        {
            return string.Format(
                "{0} - {1} - {2}",
                string.IsNullOrWhiteSpace(identity.SourceAddressHex) ? "SA ?" : identity.SourceAddressHex,
                string.IsNullOrWhiteSpace(identity.ManufacturerName) ? UnknownName : identity.ManufacturerName,
                string.IsNullOrWhiteSpace(identity.FunctionName) ? UnknownName : identity.FunctionName);
        }
    }
}
