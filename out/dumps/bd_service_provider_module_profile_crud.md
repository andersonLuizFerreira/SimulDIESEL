# Dump - BD SERVICE PROVIDER + CRUD piloto Module Profile

Data: 2026-05-12

## Objetivo da ETAPA

Consolidar a arquitetura oficial de acesso ao banco local da API por meio do `BD_Service_Provider` e implementar o primeiro CRUD piloto funcional usando apenas `module_profiles`.

Fluxo validado:

```text
UI -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

Para o piloto:

```text
ModuleProfile DTO -> ModuleProfileService -> IModuleProfileRepository -> IBdServiceProvider -> SQLite
```

## Estado real antes da implementacao

- Ja existia core SQLite local.
- Ja existia `IBdServiceProvider` / `SqliteBdServiceProvider`.
- Ja existia `IModuleProfileRepository` / `SqliteModuleProfileRepository`, mas apenas com contagem de perfis ativos.
- Ja existiam migrations `0001_sqlite_schema_v1` e `0002_sync_metadata`.
- `module_profiles` ja possuia campos suficientes para CRUD piloto:
  - `id`;
  - `name`;
  - `manufacturer`;
  - `model`;
  - `category`;
  - `application`;
  - `description`;
  - `status`;
  - `created_at`;
  - `updated_at`;
  - `sync_status`;
  - `cloud_id`;
  - `deleted_at`.

Nenhuma migration nova foi necessaria.

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileCreateRequest.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileUpdateRequest.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/ModuleProfileService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabaseRuntimeFactory.cs`
- `out/dumps/bd_service_provider_module_profile_crud.md`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IModuleProfileRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteModuleProfileRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `.codex/skills/module-database/SKILL.md`
- `docs/agents/skills/module-database-skill.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`

## Provider implementado/consolidado

O provider existente foi preservado:

- `IBdServiceProvider`
- `SqliteBdServiceProvider`

Responsabilidades validadas:

- execucao parametrizada;
- scalar;
- query;
- transacao basica;
- abertura de conexao via factory;
- ausencia de regra de negocio.

A composicao concreta SQLite foi movida para `ModuleDatabaseRuntimeFactory`, dentro do DAL, para que a BLL nao precise instanciar tipos `Sqlite*`.

## CRUD piloto implementado

Entidade piloto: `module_profiles`.

Operacoes:

- `Create`
- `GetById`
- `Update`
- `SoftDelete`
- `ListActive`
- `CountProfiles`

Comportamento adotado:

- `id` gerado como GUID/TEXT pela BLL.
- `created_at` e `updated_at` preenchidos em UTC com formato round-trip.
- `sync_status = 'local'` no create.
- `sync_status = 'pending'` no update.
- `sync_status = 'deleted'` no soft delete.
- `deleted_at` define soft delete.
- consultas e listagens ativas filtram `deleted_at IS NULL`.

## Responsabilidades por camada

### DTL

- Contem DTOs/requests simples de `module_profiles`.
- Nao contem IO, SQL ou regra de persistencia.

### BLL

- `ModuleProfileService` valida nome e status.
- Gera GUID e timestamps.
- Orquestra repository.
- Nao contem SQL.
- Nao instancia SQLite.

### DAL Repository

- `SqliteModuleProfileRepository` conhece SQL especifico de `module_profiles`.
- Usa exclusivamente `IBdServiceProvider`.
- Nao abre conexao diretamente.
- Nao instancia `SQLiteConnection`.

### BD_Service_Provider

- Executa o acesso real ao banco.
- Centraliza conexao, comandos, parametros, queries e transacoes.
- Nao conhece regra de negocio de modulos.

## Decisoes arquiteturais

- Nenhuma entidade alem de `module_profiles` foi criada.
- Nenhuma migration foi criada porque o schema atual ja continha os campos necessarios.
- O CRUD piloto fica como padrao de referencia, nao como permissao para criar CRUDs especulativos.
- A BLL deixou de montar concretamente `SqliteConnectionFactory` e `SqliteBdServiceProvider`; essa composicao passou para o DAL.
- O banco real `Data/Modules/modules.db` nao recebeu dados de teste do smoke funcional.

## Validacoes executadas

### Build

Primeira tentativa no sandbox:

```text
MSBuild SimulDIESEL.sln /t:Restore,Build /p:Configuration=Debug /p:OutDir=out/build-bd-module-profile-crud/
```

Resultado: falhou por acesso negado ao SDK local em `C:\Users\Escritório\AppData\Local\Microsoft SDKs`.

Build repetido com permissao elevada:

```text
MSBuild SimulDIESEL.sln /t:Restore,Build /p:Configuration=Debug /p:OutDir=out/build-bd-module-profile-crud/
```

Resultado:

```text
0 Aviso(s)
0 Erro(s)
```

Build final apos ajuste de composicao DAL:

```text
0 Aviso(s)
0 Erro(s)
```

### Smoke funcional CRUD

Executado em banco temporario sob `out/build-bd-module-profile-crud/validation/`, sem inserir dados de teste em `Data/Modules/modules.db`.

Resultado observado:

```text
AppliedMigrations=0001_sqlite_schema_v1,0002_sync_metadata
ReinitAppliedMigrations=
CreatedId=491a04d3-47e8-4d91-9749-683377076de0
ReadName=CRUD Piloto
Updated=True
UpdatedName=CRUD Piloto Atualizado
UpdatedStatus=active
BeforeDeleteCount=1
Deleted=True
AfterDeleteIsNull=True
AfterDeleteCount=0
CreatedAtPresent=True
UpdatedAtPresent=True
```

Validado:

- criacao do banco temporario;
- aplicacao de migrations;
- reinicializacao sem reaplicar migrations;
- create;
- read by id;
- update;
- soft delete;
- listagem ativa ignorando soft deleted;
- timestamps presentes.

Observacao de ambiente: duas tentativas de smoke com carregamento do assembly via PowerShell moderno nao foram consideradas validas; a validacao aceita foi feita no Windows PowerShell classico, compativel com .NET Framework.

### Arquitetura

Comandos de busca confirmaram:

- BLL de banco sem SQL, `SQLiteConnection`, `CreateOpenConnection` ou `PRAGMA`;
- repositories sem `SQLiteConnection`, `CreateOpenConnection` ou `System.Data.SQLite`;
- uso direto de SQLite restrito a `DAL/Database` e ao `PackageReference`;
- arquivos C# novos incluidos no `.csproj`;
- nenhum C# orfao nas pastas alteradas.

## Documentacao atualizada

- `.codex/skills/module-database/SKILL.md`
- `docs/agents/skills/module-database-skill.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`

## Itens fora do escopo preservados

- UI.
- APIs REST.
- Supabase runtime.
- Sincronizacao cloud.
- Autenticacao.
- Capturas persistidas.
- Logs persistidos.
- Catalogo J1939 persistido.
- CRUD de pins.
- CRUD de connectors.
- CRUD de CAN/J1939.
- CRUD de SDH.
- CRUD de sequencias de teste.
- Dashboards.
- Firmware.
- UCE.
- BPM.
- GSA.
- SDGW.
- SDCTP.

## Proximos passos recomendados

- Criar CRUDs de outras entidades somente por ETAPA propria e necessidade funcional real.
- Quando surgir uma nova entidade, replicar o padrao validado:

```text
DTO -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database
```

- Definir politica de UI/importacao antes de expor manutencao de perfis ao operador.
- Evoluir provider futuro PostgreSQL/Supabase sem alterar a BLL.

## Confirmacao de rollback e escopo

- Nenhum commit, branch ou tag foi criado.
- Nenhum contrato SDH, SDGW, SDCTP, UCE, BPM ou GSA foi alterado.
- Nenhum CRUD especulativo foi criado.
- A arquitetura `UI -> BLL -> DAL -> DATABASE` foi preservada.
