# ETAPA interna 3 - J1939NodeIdentityService

## Objetivo

Criar o service BLL responsavel por receber dados J1939/81 ja decodificados pelo fluxo existente e devolver uma identidade de no enriquecida pelos catalogos de referencia J1939/81.

## Branch utilizada

- `feature/j1939-reference-catalogs`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NodeIdentityService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NodeIdentityDto.cs`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## DTO de saida

Criado `J1939NodeIdentityDto` com os campos:

- `SourceAddressDecimal`
- `SourceAddressHex`
- `NameHex`
- `ManufacturerCode`
- `ManufacturerName`
- `ManufacturerKnown`
- `FunctionCode`
- `FunctionName`
- `FunctionKnown`
- `IndustryGroupCode`
- `IndustryGroupName`
- `IndustryGroupKnown`
- `VehicleSystemCode`
- `VehicleSystemName`
- `VehicleSystemKnown`
- `PreferredAddressName`
- `PreferredAddressKnown`
- `EcuInstance`
- `FunctionInstance`
- `VehicleSystemInstance`
- `IdentityNumber`
- `ArbitraryAddressCapable`
- `LastSeenAt`
- `Status`
- `Summary`

## Fluxo de resolucao

Entrada:

- `J1939AddressRegistryEntryDto`
- `J1939NameDto` ja existente em `ParsedName`

Fluxo:

```text
J1939AddressRegistryEntryDto
  -> J1939NodeIdentityService
  -> J1939ReferenceCatalogService
  -> DTO enriquecido J1939NodeIdentityDto
```

Campos resolvidos por catalogo:

- `ManufacturerCode`
- `Function`
- `IndustryGroup`
- `VehicleSystem`
- `SourceAddress` / endereco preferencial

## Exemplos testados

Smoke test executado em banco temporario:

- `out/validation-rede-can-03/node-identity-smoke.db`

Resultados:

- SA `0`, Manufacturer `94`, Function `0`, Industry Group `2`:
  - Fabricante: `CNH Industrial N.V.`
  - Funcao: `Engine`
  - Grupo: `Agricultural and Forestry`
  - Endereco preferencial: `Engine #1`
  - SA Hex: `0x00`
- SA `249`, Function `129`:
  - Funcao: `Off-board Diagnostic Tool`
  - Endereco preferencial: `Off-board Diagnostic-Service Tool #1`
  - Known: `True`
- Manufacturer `9999`:
  - Fabricante: `Desconhecido`
  - Known: `False`
  - Codigo bruto preservado: `9999`

## Validacoes executadas

### Build completo

Comando:

```powershell
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-rede-can-03/
```

Resultado:

- 0 erros
- 0 avisos

Observacao: a primeira tentativa de build encontrou bloqueio de sandbox em `AppData/Local/Microsoft SDKs`; o build foi repetido com permissao elevada e validou a solucao corretamente.

### Separacao de camadas

Validado por busca textual:

- `J1939NodeIdentityService` nao referencia `Sqlite` ou `SQLite`.
- `J1939NodeIdentityService` nao referencia `System.Data`.
- `J1939NodeIdentityService` nao referencia `IJ1939ReferenceCatalogRepository`.
- `J1939NodeIdentityService` nao referencia `DAL.Repositories`.
- `J1939NodeIdentityService` nao contem SQL.
- `J1939NodeIdentityService` nao referencia WinForms/UI.

### Reuso obrigatorio

Confirmado por diff:

- `J1939NameParser.cs` nao foi alterado.
- `J1939AddressClaimDecoder.cs` nao foi alterado.

## Confirmacao arquitetural

- O service usa `J1939ReferenceCatalogService`.
- O service nao decodifica NAME novamente.
- O service nao recria parser.
- O service nao acessa SQLite.
- O service nao acessa repository diretamente.
- A UI ainda nao foi criada nesta ETAPA interna.
- Firmware, SDGW, SDCTP e fluxo CAN RX nao foram alterados.

## Limitacoes

- A origem dos snapshots ainda sera conectada na ETAPA interna 4 pela janela `FrmRedeCan`.
- O service trabalha com dados ja existentes no registry; ele nao altera o ciclo de vida do registry.

## Proxima ETAPA interna

Criar `FrmRedeCan` como MDI-child read-only, consumindo DTOs enriquecidos pela BLL e conectando `btnRedeCan` no form pai.
