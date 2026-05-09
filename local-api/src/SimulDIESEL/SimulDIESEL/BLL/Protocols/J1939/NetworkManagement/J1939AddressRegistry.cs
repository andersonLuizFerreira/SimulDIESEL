using System;
using System.Collections.Generic;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939AddressRegistry
    {
        private readonly Dictionary<byte, J1939AddressRegistryEntryDto> _entries =
            new Dictionary<byte, J1939AddressRegistryEntryDto>();

        public J1939NetworkEventDto Apply(J1939AddressClaimDto claim)
        {
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (claim.IsCannotClaimAddress)
                return BuildCannotClaimEvent(claim);

            if (!claim.IsAddressClaimed || claim.Name == null)
                return BuildEvent("AddressClaimIgnored", claim, null, claim.Status, "Address Claim invalido ou sem NAME.");

            J1939AddressRegistryEntryDto existing;
            if (!_entries.TryGetValue(claim.SourceAddress, out existing))
            {
                J1939AddressRegistryEntryDto created = CreateEntry(claim);
                _entries[claim.SourceAddress] = created;
                return BuildEvent("AddressClaimed", claim, created, "AddressClaimed", "Novo Source Address registrado.");
            }

            if (existing.ParsedName != null && existing.ParsedName.RawNameUInt64 == claim.Name.RawNameUInt64)
            {
                existing.LastSeenTimestamp = claim.Timestamp;
                existing.ClaimStatus = "AddressClaimed";
                existing.Notes = "Mesmo NAME observado novamente; LastSeen atualizado.";
                return BuildEvent("AddressClaimRefreshed", claim, existing, "AddressClaimRefreshed", existing.Notes);
            }

            string previousName = existing.NameHex;
            ulong existingValue = existing.ParsedName != null ? existing.ParsedName.RawNameUInt64 : ulong.MaxValue;
            bool incomingWins = claim.Name.RawNameUInt64 < existingValue;
            existing.ConflictDetected = true;
            existing.PreviousNameHex = previousName;
            existing.WinningNameHex = incomingWins ? claim.Name.NameHex : existing.NameHex;
            existing.LastSeenTimestamp = claim.Timestamp;
            existing.ClaimStatus = "AddressConflictDetected";
            existing.Notes = incomingWins
                ? "Conflito: NAME recebido possui maior prioridade numerica e vence a disputa."
                : "Conflito: NAME existente possui maior prioridade numerica e permanece vencedor.";

            if (incomingWins)
            {
                existing.NameHex = claim.Name.NameHex;
                existing.ParsedName = claim.Name;
            }

            return BuildEvent("AddressConflictDetected", claim, existing, "AddressConflictDetected", existing.Notes);
        }

        public IReadOnlyList<J1939AddressRegistryEntryDto> GetSnapshot()
        {
            return new List<J1939AddressRegistryEntryDto>(_entries.Values);
        }

        private static J1939AddressRegistryEntryDto CreateEntry(J1939AddressClaimDto claim)
        {
            return new J1939AddressRegistryEntryDto
            {
                SourceAddress = claim.SourceAddress,
                NameHex = claim.Name.NameHex,
                ParsedName = claim.Name,
                FirstSeenTimestamp = claim.Timestamp,
                LastSeenTimestamp = claim.Timestamp,
                ClaimStatus = "AddressClaimed",
                IsCannotClaim = false,
                ConflictDetected = false,
                WinningNameHex = claim.Name.NameHex,
                Notes = "Address Claimed recebido."
            };
        }

        private static J1939NetworkEventDto BuildCannotClaimEvent(J1939AddressClaimDto claim)
        {
            return BuildEvent("CannotClaimAddress", claim, null, "CannotClaimAddress", "CA informou Cannot Claim Address com SA 0xFE.");
        }

        private static J1939NetworkEventDto BuildEvent(string type, J1939AddressClaimDto claim, J1939AddressRegistryEntryDto entry, string status, string notes)
        {
            return new J1939NetworkEventDto
            {
                EventType = type,
                SourceAddress = claim.SourceAddress,
                AddressClaim = claim,
                RegistryEntry = entry,
                Status = status,
                Notes = notes,
                Timestamp = claim.Timestamp
            };
        }
    }
}
