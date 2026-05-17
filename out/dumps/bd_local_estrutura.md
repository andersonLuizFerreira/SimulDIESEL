# Dump - BD LOCAL ESTRUTURA

Data: 2026-05-12

## Tema e objetivo

ETAPA: BD LOCAL ESTRUTURA.

Objetivo: criar o core do banco local da API do SimulDIESEL usando SQLite, preservando `UI -> BLL -> DAL -> DATABASE` e preparando compatibilidade futura com PostgreSQL/Supabase.

## Analise do estado real

- Ja existia `Data/Modules/modules.db`.
- Ja existiam schemas de referencia:
  - `Data/Modules/schema/sqlite_schema_v1.sql`
  - `Data/Modules/schema/postgres_schema_v1.sql`
- Ja existiam documentos do Banco de Modulos:
  - `Data/Modules/docs/module_database_model_v1.md`
  - `Data/Modules/docs/module_database_sdh_relation.md`
- Nao havia DAL/BLL de banco local implementado na API.
- Nao havia pacote SQLite referenciado no projeto C#.
- Nao havia mecanismo de migrations na API.
- Ja existia skill adequada para Banco de Modulos: `.agents/skills/module-database/SKILL.md`; ela foi atualizada, nao duplicada.

## Arquivos criados

### DAL Database

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/DatabaseInitializationResult.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseConnectionFactory.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseInitializer.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IMigrationRunner.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabasePaths.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteConnectionFactory.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteDatabaseInitializer.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteMigrationRunner.cs`

### DAL Repositories

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IModuleProfileRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteModuleProfileRepository.cs`

### BLL Database

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseStatus.cs`

### Schema/migrations

- `Data/Modules/schema/migrations/0002_sync_metadata.sql`

### Documentacao

- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`
- `out/dumps/bd_local_estrutura.md`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/Program.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `Data/Modules/modules.db`
- `.agents/skills/module-database/SKILL.md`
- `.agents/skills/module-database/SKILL.md`

## Arquitetura adotada

```text
Program -> BLL/Services/Database -> DAL/Database + DAL/Repositories -> SQLite modules.db
```

- `Program` dispara a inicializacao no startup.
- `LocalDatabaseService` e a fachada BLL; ela nao contem SQL nem tipos SQLite.
- `SqliteDatabaseInitializer` abre conexao e executa migrations.
- `SqliteMigrationRunner` aplica baseline e migrations incrementais.
- `SqliteModuleProfileRepository` concentra SQL de leitura inicial no DAL.

## Decisoes tecnicas

- Provider inicial: SQLite via `System.Data.SQLite.Core` `1.0.119`.
- O baseline de schema reaproveita `Data/Modules/schema/sqlite_schema_v1.sql`.
- A tabela `schema_migrations` controla migrations aplicadas.
- A migration incremental `0002_sync_metadata.sql` adiciona metadados de sync/soft delete com diretivas idempotentes processadas pelo runner.
- IDs continuam portaveis: `TEXT` no SQLite, com caminho futuro para `UUID` no PostgreSQL.
- JSON continua portavel: `TEXT` validado no SQLite, `JSONB` no PostgreSQL.
- `modules.db` existente foi migrado; nenhum dado real foi inserido.

## Schema utilizado

- Baseline: `0001_sqlite_schema_v1`, derivado de `Data/Modules/schema/sqlite_schema_v1.sql`.
- Migration incremental: `0002_sync_metadata`.

Tabelas principais preservadas:

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

## Migrations criadas

- `0002_sync_metadata.sql`: adiciona `sync_status`, `cloud_id`, `deleted_at` e campos temporais ausentes quando aplicavel.

Migrations aplicadas no banco real:

- `0001_sqlite_schema_v1`
- `0002_sync_metadata`

## Preparacao PostgreSQL/Supabase

- Metadados `sync_status`, `cloud_id` e `deleted_at` foram adicionados ao modelo local.
- O acesso ao banco ficou atras de factory/initializer/repository.
- A BLL nao depende diretamente de SQLite.
- O runtime Supabase, autenticacao e sincronizacao cloud permanecem `PLANEJADO` e fora de escopo.

## Skills criadas/alteradas

- Nenhuma skill duplicada foi criada.
- `.agents/skills/module-database/SKILL.md` foi atualizada para cobrir DAL de banco, repositories, migrations e BLL de inicializacao.
- `.agents/skills/module-database/SKILL.md` foi mantida coerente com a skill estruturada.

## Documentacao criada/atualizada

- `docs/official/02-arquitetura/12-banco-local-api.md`: documenta arquitetura oficial do banco local da API.
- `Data/Modules/docs/local_api_database_runtime.md`: documenta runtime local, migrations e limites atuais.
- Skill humana e skill CODEX do Banco de Modulos atualizadas.

## Validacoes executadas

### Build

Comando:

```text
MSBuild SimulDIESEL.sln /t:Restore,Build /p:Configuration=Debug /p:OutDir=out/build-bd-local/
```

Resultado: sucesso, `0 Aviso(s)`, `0 Erro(s)`.

Observacao: a primeira tentativa de build no sandbox falhou por acesso negado ao SDK local em `C:\Users\Escritório\AppData\Local\Microsoft SDKs`; a validacao foi repetida com permissao elevada e passou.

### Inicializacao do banco real

Comando de validacao via Windows PowerShell/.NET Framework carregando o assembly compilado e instanciando `LocalDatabaseService` com caminhos explicitos.

Resultado:

```text
DatabasePath=G:\PROJETOS\SIMULADORES\SimulDIESEL\Data\Modules\modules.db
AppliedMigrations=0001_sqlite_schema_v1,0002_sync_metadata
ModuleProfileCount=0
```

### Reinicializacao segura

Segunda execucao do initializer no mesmo banco:

```text
AppliedMigrations=
ModuleProfileCount=0
```

Resultado: nenhuma migration reaplicada; banco nao foi corrompido.

### Verificacao de schema

Resultados observados:

```text
MigrationCount=2
ExpectedTablesFound=3
ForeignKeyCheckRows=0
module_profiles.id type=TEXT
module_profiles.created_at type=TEXT
module_profiles.updated_at type=TEXT
module_profiles.sync_status type=TEXT
module_profiles.cloud_id type=TEXT
module_profiles.deleted_at type=TEXT
```

### Criacao automatica

Validado em banco temporario `out/validation-bd-local/modules.db`:

```text
TempDbExists=True
AppliedMigrations=0001_sqlite_schema_v1,0002_sync_metadata
```

O banco temporario foi removido apos a validacao.

### Organizacao arquitetural

- Busca textual confirmou que `System.Data.SQLite`, `SQLiteConnection`, `PRAGMA`, `CREATE TABLE`, `ALTER TABLE` e `SELECT` aparecem apenas no DAL.
- Busca em `BLL/Services/Database` nao encontrou SQL nem tipos SQLite.
- `Program.cs` apenas dispara a fachada BLL.
- Cabeçalhos obrigatorios da skill `.agents/skills/module-database/SKILL.md`: OK.
- `git status --short` executado.

## Warnings e observacoes de validacao

- `git diff --name-only` emitiu aviso de normalizacao de linha: `Program.cs` e `SimulDIESEL.csproj` estao com LF no worktree e podem voltar para CRLF quando o Git tocar nesses arquivos.
- O worktree contem alteracoes documentais anteriores de ETAPAS recentes; elas foram preservadas e nao foram revertidas.

## Itens fora do escopo preservados

- UI.
- Sincronizacao cloud.
- Supabase runtime.
- Autenticacao.
- Execucao SDH.
- APIs REST.
- Dashboards.
- Firmware.
- SDGW.
- SDCTP.
- UCE/BPM/GSA.

## Pontos pendentes

- Repositories completos por entidade permanecem `PLANEJADO`.
- Validadores/importadores de dados reais permanecem `PLANEJADO`.
- Sincronizacao PostgreSQL/Supabase permanece `PLANEJADO`.
- O runner atual usa diretivas simples para migrations condicionais de coluna; se surgirem migrations complexas, sera necessario evoluir o DSL ou introduzir um migrator mais formal.

## Confirmacao funcional

- Codigo funcional da API foi alterado dentro do escopo de banco local.
- Firmware, UI, banco de comandos SDH runtime, SDGW, SDCTP e contratos de protocolo nao foram alterados.
- Nenhum commit, branch ou tag foi criado.
