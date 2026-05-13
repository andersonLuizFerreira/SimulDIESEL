# ETAPA - Consumo dos Catalogos J1939/81 e criacao da UI Rede CAN

## Objetivo

Criar a cadeia de consumo dos catalogos J1939/81 para resolver identidade de modulos detectados na rede CAN e exibir os resultados em uma janela MDI-child `FrmRedeCan`.

## Branch utilizada

- `feature/j1939-reference-catalogs`

## Resumo das ETAPAS internas

### ETAPA interna 1 - Repository DAL

Criados DTOs DTL de catalogo, `IJ1939ReferenceCatalogRepository` e `SqliteJ1939ReferenceCatalogRepository`.

Validacoes:

- Build: 0 erros, 0 avisos.
- Banco temporario com migrations + seed.
- Consultas reais para Manufacturer `94`, Function `0`, Industry Group `2`, Preferred Address `0`, `254` e `255`.
- Codigo inexistente retornou `null` sem excecao.

Dump:

- `out/dumps/etapa_rede_can_01_repository_catalogo_j1939.md`

### ETAPA interna 2 - Service BLL de catalogo

Criado `J1939ReferenceCatalogService` como porta BLL read-only. Ampliadas factories para criar repository e service padrao.

Validacoes:

- Build: 0 erros, 0 avisos.
- `ResolveManufacturer(94)` retornou `CNH Industrial N.V.`.
- `ResolveManufacturer(9999)` retornou `Desconhecido`, `Known=False`.
- `ResolveFunction(0)`, `ResolveIndustryGroup(2)`, `ResolvePreferredAddress(249, null)` e `ResolvePreferredAddress(255, null)` passaram.
- BLL sem SQL e sem referencia a SQLite.

Dump:

- `out/dumps/etapa_rede_can_02_bll_catalogo_j1939.md`

### ETAPA interna 3 - Service BLL de identidade do no

Criados `J1939NodeIdentityService` e `J1939NodeIdentityDto`.

Validacoes:

- Build: 0 erros, 0 avisos.
- Snapshot simulado com SA `0`, Manufacturer `94`, Function `0`, Industry Group `2` retornou `CNH Industrial N.V.`, `Engine`, `Agricultural and Forestry` e `Engine #1`.
- Snapshot simulado com SA `249`, Function `129` retornou `Off-board Diagnostic Tool` e `Off-board Diagnostic-Service Tool #1`.
- Manufacturer `9999` retornou `Desconhecido`, `Known=False`.
- `J1939NameParser.cs` e `J1939AddressClaimDecoder.cs` nao foram alterados.

Dump:

- `out/dumps/etapa_rede_can_03_node_identity_service.md`

### ETAPA interna 4 - FrmRedeCan

Criada a janela MDI-child `FrmRedeCan`, conectado `btnRedeCan` no `DashBoard` e exposto snapshot read-only do registry via `frmUCE_UI`.

Validacoes:

- Build: 0 erros, 0 avisos.
- `FrmRedeCan` instanciou em contexto STA.
- Snapshot simulado foi aceito e exibido no modelo interno da tela.
- `NodesAfterRefresh=1`.
- Limpar visualizacao deixou `NodesAfterClear=0`, sem apagar registry ou banco.
- `FrmRedeCan.cs` nao referencia DAL, repository, provider, SQLite, `System.Data`, SQL ou `IBdServiceProvider`.

Dump:

- `out/dumps/etapa_rede_can_04_frm_rede_can.md`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IJ1939ReferenceCatalogRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteJ1939ReferenceCatalogRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/J1939ReferenceCatalogService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NodeIdentityService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939CatalogEntryDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939ManufacturerCatalogDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939PreferredAddressCatalogDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939NameFieldDefinitionCatalogDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NodeIdentityDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.cs`
- `out/dumps/etapa_rede_can_01_repository_catalogo_j1939.md`
- `out/dumps/etapa_rede_can_02_bll_catalogo_j1939.md`
- `out/dumps/etapa_rede_can_03_node_identity_service.md`
- `out/dumps/etapa_rede_can_04_frm_rede_can.md`
- `out/dumps/etapa_rede_can_consumo_j1939_catalogos_consolidado.md`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabaseRuntimeFactory.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `docs/official/02-arquitetura/12-banco-local-api.md`

## Cadeia final implementada

```text
FrmRedeCan
  -> J1939NodeIdentityService
  -> J1939ReferenceCatalogService
  -> IJ1939ReferenceCatalogRepository
  -> SqliteJ1939ReferenceCatalogRepository
  -> IBdServiceProvider
  -> SQLite
```

## Confirmacoes

- A UI nao acessa banco diretamente.
- A UI nao acessa repository diretamente.
- A UI nao acessa provider diretamente.
- A UI nao contem SQL.
- O parser J1939/81 nao foi recriado.
- `J1939NameParser` nao foi alterado.
- `J1939AddressClaimDecoder` nao foi alterado.
- `J1939AddressRegistry` nao foi alterado.
- Firmware nao foi alterado.
- SDGW nao foi alterado.
- SDCTP nao foi alterado.
- Fluxo CAN RX consolidado nao foi alterado.
- Nao foi criado CRUD de catalogos.
- Nao foi criada edicao de catalogos.
- Nao foi criado scraping web.
- Nao ha dependencia de internet em runtime.

## Resultado final do build

Comando final:

```powershell
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-rede-can-04/
```

Resultado:

- 0 erros
- 0 avisos

Observacao: alguns builds precisaram de permissao elevada porque o sandbox bloqueou acesso ao SDK local em `AppData/Local/Microsoft SDKs`. Os builds validos foram executados com o MSBuild do Visual Studio.

## Limitacoes

- `FrmRedeCan` e somente leitura.
- O dataset dos catalogos J1939/81 continua parcial.
- A abertura MDI foi validada por build e inspecao do handler; nao foi feita uma sessao interativa manual de desktop.
- A tela consome o snapshot atual do registry; ela nao altera o ciclo de vida de deteccao ou limpeza do registry.

## Proximos passos sugeridos

- Reaproveitar `J1939NodeIdentityService` para substituir os textos `Desconhecido` da aba J1939 Identification da tela UCE, em ETAPA separada.
- Expandir os JSONs versionados com mais fabricantes, funcoes e enderecos preferenciais.
- Criar testes automatizados dedicados para catalogos e identidade J1939/81.
- Avaliar uma fachada BLL compartilhada para snapshots de rede CAN caso outras telas passem a consumir a mesma fonte.
