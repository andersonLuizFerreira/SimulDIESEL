# ETAPA interna 1 - Repository DAL read-only dos catalogos J1939/81

## Objetivo

Criar a camada DAL read-only para consulta das tabelas de referencia J1939/81 no banco local SQLite, mantendo a UI e a BLL sem acesso direto a SQL, provider ou conexao SQLite.

## Branch utilizada

- `feature/j1939-reference-catalogs`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IJ1939ReferenceCatalogRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteJ1939ReferenceCatalogRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939CatalogEntryDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939ManufacturerCatalogDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939PreferredAddressCatalogDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939NameFieldDefinitionCatalogDto.cs`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## Metodos criados

- `GetIndustryGroupByCode(int code)`
- `GetManufacturerByCode(int code)`
- `GetFunctionByCode(int code)`
- `GetVehicleSystemByCode(int code, int? industryGroupCode)`
- `GetPreferredAddressByAddress(int address, int? industryGroupCode)`
- `GetNameFieldDefinitionByFieldName(string fieldName)`
- `ListIndustryGroups()`
- `ListManufacturers()`
- `ListFunctions()`
- `ListPreferredAddresses()`
- `ListNameFieldDefinitions()`

## SQL usado

As consultas sao `SELECT` read-only sobre:

- `j1939_industry_groups`
- `j1939_manufacturers`
- `j1939_functions`
- `j1939_vehicle_systems`
- `j1939_preferred_addresses`
- `j1939_name_field_definitions`

Nao foram criados metodos `INSERT`, `UPDATE` ou `DELETE` no repository.

## Validacoes executadas

### Build completo

Comando:

```powershell
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-rede-can-01/
```

Resultado:

- 0 erros
- 0 avisos

### Smoke test em banco temporario

Banco temporario:

- `out/validation-rede-can-01/catalog-smoke.db`

Fluxo executado:

1. Criacao do banco temporario.
2. Execucao do `SqliteDatabaseInitializer`.
3. Aplicacao das migrations existentes.
4. Execucao do seed local dos catalogos J1939/81.
5. Criacao do `SqliteBdServiceProvider`.
6. Criacao do `SqliteJ1939ReferenceCatalogRepository`.
7. Consultas read-only pelos novos metodos.
8. Reexecucao do initializer para validar idempotencia.

Resultados observados:

- Primeira inicializacao: `Applied=3`, `Pending=0`.
- Segunda inicializacao: `Applied=0`, `Pending=0`.
- `GetManufacturerByCode(94)` retornou `CNH Industrial N.V.`.
- `GetFunctionByCode(0)` retornou `Engine`.
- `GetIndustryGroupByCode(2)` retornou `Agricultural and Forestry`.
- `GetPreferredAddressByAddress(0, null)` retornou `Engine #1`.
- `GetPreferredAddressByAddress(0, 2)` retornou `Engine #1`, usando fallback global.
- `GetPreferredAddressByAddress(254, null)` retornou `Null Address`.
- `GetPreferredAddressByAddress(255, null)` retornou `Global Address`.
- `GetManufacturerByCode(999999)` retornou `null`, sem excecao.

### Separacao de camadas

Validado por busca textual:

- `SqliteJ1939ReferenceCatalogRepository` nao referencia `System.Data.SQLite`.
- `SqliteJ1939ReferenceCatalogRepository` nao referencia `SQLiteConnection`.
- `SqliteJ1939ReferenceCatalogRepository` nao referencia `System.Windows.Forms`.
- `SqliteJ1939ReferenceCatalogRepository` usa `IBdServiceProvider`.

## Confirmacao arquitetural

- A ETAPA interna 1 adicionou apenas DAL read-only e DTOs DTL.
- A BLL ainda nao foi alterada nesta ETAPA interna.
- A UI nao foi alterada nesta ETAPA interna.
- Nao houve acesso direto a SQLite fora do provider ja existente.
- Firmware, SDGW, SDCTP e fluxo CAN RX nao foram alterados.

## Limitacoes

- O repository apenas consulta catalogos; nao resolve regras de negocio nem monta identidade de no J1939/81.
- Tratamento padronizado de codigos desconhecidos sera feito na BLL na ETAPA interna 2.

## Proxima ETAPA interna

Criar `J1939ReferenceCatalogService` na BLL, dependente de `IJ1939ReferenceCatalogRepository`, sem SQL e sem referencia a SQLite.
