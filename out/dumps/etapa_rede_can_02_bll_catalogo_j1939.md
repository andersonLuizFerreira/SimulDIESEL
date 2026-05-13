# ETAPA interna 2 - Service BLL J1939ReferenceCatalogService

## Objetivo

Criar a porta BLL read-only para consumo dos catalogos J1939/81, mantendo SQL e provider confinados na DAL.

## Branch utilizada

- `feature/j1939-reference-catalogs`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/J1939ReferenceCatalogService.cs`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabaseRuntimeFactory.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## Metodos BLL criados

- `ResolveIndustryGroup(int code)`
- `ResolveManufacturer(int code)`
- `ResolveFunction(int code)`
- `ResolveVehicleSystem(int code, int? industryGroupCode)`
- `ResolvePreferredAddress(int address, int? industryGroupCode)`
- `ResolveNameFieldDefinition(string fieldName)`
- `ListIndustryGroups()`
- `ListManufacturers()`
- `ListFunctions()`
- `ListPreferredAddresses()`
- `ListNameFieldDefinitions()`

## Factories ampliadas

- `ModuleDatabaseRuntimeFactory.CreateJ1939ReferenceCatalogRepository()`
- `LocalDatabaseService.CreateDefaultJ1939ReferenceCatalogRepository()`
- `LocalDatabaseService.CreateDefaultJ1939ReferenceCatalogService()`

## Tratamento para desconhecido

Codigo valido mas ausente do catalogo nao gera excecao.

Padrao aplicado:

- `Name = "Desconhecido"`
- `IsKnown = false`
- codigo bruto preservado no DTO

Codigo fora da faixa esperada gera `ArgumentOutOfRangeException`, mantendo falhas de uso indevido explicitas na BLL.

Faixas validadas:

- Industry Group: `0..7`
- Function: `0..255`
- Vehicle System: `0..127`
- Preferred Address: `0..255`
- Manufacturer: `>= 0`

## Validacoes executadas

### Build completo

Comando:

```powershell
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-rede-can-02/
```

Resultado:

- 0 erros
- 0 avisos

Observacao: uma primeira tentativa de build encontrou bloqueio de sandbox em `AppData/Local/Microsoft SDKs`; o build foi repetido com permissao elevada e validou a solucao corretamente.

### Smoke test em banco temporario

Banco temporario:

- `out/validation-rede-can-02/catalog-bll-smoke.db`

Resultados observados:

- `ResolveManufacturer(94)` retornou `CNH Industrial N.V.`, `Known=True`.
- `ResolveManufacturer(9999)` retornou `Desconhecido`, `Known=False`, `Code=9999`.
- `ResolveFunction(0)` retornou `Engine`, `Known=True`.
- `ResolveIndustryGroup(2)` retornou `Agricultural and Forestry`, `Known=True`.
- `ResolvePreferredAddress(249, null)` retornou `Off-board Diagnostic-Service Tool #1`, `Known=True`.
- `ResolvePreferredAddress(255, null)` retornou `Global Address`, `Known=True`.

### Separacao de camadas

Validado por busca textual:

- `J1939ReferenceCatalogService` nao contem SQL.
- `J1939ReferenceCatalogService` nao referencia `System.Data`.
- `J1939ReferenceCatalogService` nao referencia `SQLite` ou `Sqlite`.
- `J1939ReferenceCatalogService` nao referencia `IBdServiceProvider`.
- `J1939ReferenceCatalogService` nao referencia UI ou WinForms.

## Confirmacao arquitetural

- A BLL depende de `IJ1939ReferenceCatalogRepository`.
- A BLL nao chama `SqliteBdServiceProvider`.
- A BLL nao abre conexao SQLite.
- A UI ainda nao foi alterada nesta ETAPA interna.
- Firmware, SDGW, SDCTP e fluxo CAN RX nao foram alterados.

## Limitacoes

- O service resolve catalogos individualmente, mas ainda nao monta identidade enriquecida de no J1939/81.
- O consumo pela UI ainda nao existe nesta ETAPA interna.

## Proxima ETAPA interna

Criar `J1939NodeIdentityService` para receber entradas ja decodificadas do registry J1939/81 e produzir DTO enriquecido por catalogo para consumo da janela Rede CAN.
