# ETAPA - Importacao inicial dos catalogos publicos J1939/81

Data: 2026-05-13

## Objetivo da ETAPA

Popular as tabelas de referencia J1939/81 do banco local SQLite a partir de arquivos JSON locais e versionados, mantendo rastreabilidade das fontes publicas usadas para montar o dataset inicial.

Esta ETAPA nao implementa UI, firmware, SDGW, SDCTP, fluxo CAN RX, parsers, scraping online em runtime ou sincronizacao automatica web.

## Branch utilizada

- Branch: `feature/j1939-reference-catalogs`
- `git fetch origin`: executado com sucesso.
- A branch local nao possui upstream remoto configurado.
- Base local da branch: `599b27f40096b3b5b0fbfb83f2e92b356595196c`, igual a `origin/main` apos fetch.

## Arquivos JSON criados

- `Data/Protocols/J1939/catalogs/j1939_industry_groups.json`
- `Data/Protocols/J1939/catalogs/j1939_manufacturers.json`
- `Data/Protocols/J1939/catalogs/j1939_functions.json`
- `Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json`
- `Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json`

## Arquivos de codigo e projeto alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/J1939CatalogSeedImporter.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteDatabaseInitializer.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabasePaths.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## Arquivos de schema e documentacao alterados

- `Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql`
- `Data/Modules/schema/postgres_schema_v1.sql`
- `Data/Modules/docs/module_database_model_v1.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`

## Quantidade de registros importados por tabela

Validacao em banco SQLite temporario:

- `j1939_industry_groups`: 8 registros de seed.
- `j1939_manufacturers`: 26 registros de seed.
- `j1939_functions`: 25 registros de seed.
- `j1939_preferred_addresses`: 28 registros de seed.
- `j1939_name_field_definitions`: 9 registros de seed.

Observacao: a contagem total de `j1939_manufacturers` na validacao ficou 27 porque foi inserido um registro manual de controle (`code = 9999`) para provar que a reexecucao do importador nao apaga dados manuais futuros.

## Estrategia de importacao utilizada

- Os dados ficam em JSONs UTF-8 locais em `Data/Protocols/J1939/catalogs/`.
- Cada registro possui campo `source`.
- O importador `J1939CatalogSeedImporter` roda no DAL apos as migrations de `SqliteDatabaseInitializer`.
- A BLL continua sem SQL e sem dependencia direta de SQLite.
- A importacao usa transacao unica por inicializacao.
- A importacao usa `INSERT ... ON CONFLICT ... DO UPDATE`:
  - `j1939_industry_groups`: conflito por `code`.
  - `j1939_manufacturers`: conflito por `code`.
  - `j1939_functions`: conflito por `code`.
  - `j1939_preferred_addresses`: conflito por `address, industry_group_code`.
  - `j1939_name_field_definitions`: conflito por `field_name`.
- Nao ha `DELETE` geral.
- Nao ha recriacao de tabelas.
- A reexecucao atualiza registros conhecidos do seed e preserva registros manuais fora do seed.
- A constraint de `j1939_preferred_addresses.address` foi ajustada de `0..253` para `0..255` para aceitar os enderecos especiais exigidos nesta ETAPA: `254 -> Null Address` e `255 -> Global Address`.

## Fontes publicas rastreadas

- `https://www.isobus.net/isobus/manufacturerCode`
- `https://www.isobus.net/isobus/nameFunction`
- `https://www.isobus.net/isobus/sourceAddress`
- `https://kvaser.com/about-can/higher-layer-protocols/j1939-introduction/`
- `https://delgrossoengineering.com/isobus-docs/classisobus_1_1NAME`
- `https://github.com/krone-landmaschinen/isobus-name-resolver-ts`

## Resultado da validacao

Validacao executada com `SqliteDatabaseInitializer`, `SqliteMigrationRunner` e `J1939CatalogSeedImporter` reais do projeto sobre banco controlado:

`out/validation-bd-local/j1939_catalog_seed_20260513_094549/modules.db`

Resultado:

- Primeira inicializacao aplicou migrations: `0001_sqlite_schema_v1`, `0002_sync_metadata`, `0003_j1939_reference_catalogs`.
- `schema_migrations`: `0001_sqlite_schema_v1`, `0002_sync_metadata`, `0003_j1939_reference_catalogs`.
- Tabelas antigas verificadas sem ausencias:
  - `module_profiles`
  - `module_profile_versions`
  - `module_connectors`
  - `module_pins`
  - `module_power_requirements`
  - `module_can_networks`
  - `module_j1939_pgns`
  - `module_signal_channels`
  - `module_sdh_commands`
  - `module_test_sequences`
  - `module_test_steps`
  - `module_capture_sessions`
  - `module_capture_events`
- Linha de validacao em `module_profiles` permaneceu apos reinicializacoes: `1`.
- Registro manual em `j1939_manufacturers` permaneceu apos reinicializacoes: `1`.
- Enderecos especiais `254` e `255` foram importados: `2`.
- `Data/Modules/modules.db` nao foi alterado durante a validacao.

## Resultado da reexecucao sem duplicacoes

Foram executadas tres inicializacoes no mesmo banco de validacao.

- Segunda inicializacao aplicou migrations: nenhuma.
- Terceira inicializacao aplicou migrations: nenhuma.
- Duplicacoes por chave:
  - `industry_group_code`: 0
  - `manufacturer_code`: 0
  - `function_code`: 0
  - `preferred_address_group`: 0
  - `name_field`: 0

Contagens finais:

- `j1939_industry_groups`: 8
- `j1939_manufacturers`: 27 total, sendo 26 do seed e 1 manual de controle.
- `j1939_functions`: 25
- `j1939_preferred_addresses`: 28
- `j1939_name_field_definitions`: 9

## Resultado do build

Build pre-implementacao da branch:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

Build apos implementacao:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

## Limitacoes

- Dataset inicial parcial, conforme permitido pela ETAPA.
- Nao ha importador web.
- Nao ha scraping online em runtime.
- Nao ha sincronizacao automatica web.
- Nao ha CRUD dedicado para catalogos J1939/81.
- Nao ha UI.
- Nao houve alteracao de firmware, SDGW, SDCTP, fluxo CAN RX ou parsers J1939.
- Algumas nomenclaturas de funcoes e enderecos preferenciais foram mantidas conforme lista de aceite da ETAPA; divergencias finas entre listas publicas permanecem `pendente de confirmacao` para uma ETAPA futura de curadoria completa.

## Proximos passos sugeridos

- Criar ETAPA de curadoria ampliada dos catalogos com revisao item a item.
- Criar ETAPA de consulta BLL/DAL para resolver codigos J1939/81 a partir do banco.
- Criar ETAPA de integracao com visualizacao de NAME e Source Address sem falhar quando codigos estiverem ausentes.
- Criar ETAPA de documentacao de politica de atualizacao dos catalogos publicos.

## Rollback

Rollback preservado:

- A implementacao esta isolada na branch `feature/j1939-reference-catalogs`.
- O seed e local e versionado em JSON.
- Nao houve alteracao de dados reais em `Data/Modules/modules.db`.
- Nao houve commit, tag ou push.
- A remocao do importador e dos JSONs reverte a popularizacao automatica sem afetar migrations antigas.
