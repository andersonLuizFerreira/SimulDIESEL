# Banco local da API - runtime

Status: `PARCIALMENTE IMPLEMENTADO`

## Resumo

A API local do SimulDIESEL passa a possuir um core de banco SQLite para o Banco de Modulos. A inicializacao ocorre no startup da aplicacao e aplica migrations controladas sobre `Data/Modules/modules.db`.

## Componentes

- `SqliteConnectionFactory`: resolve o caminho de `Data/Modules/modules.db`, cria o diretorio quando necessario e abre conexao SQLite com foreign keys habilitadas.
- `SqliteDatabaseInitializer`: executa o runner de migrations.
- `SqliteMigrationRunner`: aplica o baseline `sqlite_schema_v1.sql`, executa migrations em `schema/migrations/` e registra versoes em `schema_migrations`.
- `IBdServiceProvider` / `SqliteBdServiceProvider`: execucao generica de comandos, scalar, queries e transacoes basicas.
- `IModuleProfileRepository` / `SqliteModuleProfileRepository`: CRUD piloto de perfis de modulo, usando exclusivamente o provider.
- `ModuleProfileService`: service BLL do CRUD piloto, com validacao, GUID, timestamps e soft delete.
- `LocalDatabaseService`: camada BLL de inicializacao/status sem SQL direto.

## Fluxo oficial

```text
DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

- A BLL nao contem SQL.
- A BLL nao conhece SQLite.
- UI/API/Controllers/Forms consomem services BLL, nao repositories.
- Repository nao abre conexao diretamente.
- Repository nao instancia `SQLiteConnection`.
- Repository e detalhe interno da persistencia.
- Provider nao conhece dominio de modulo, conector, CAN, J1939 ou SDH.
- Provider e detalhe interno do DAL.
- Provider nao substitui repositories especializados.

## Regra de consumo

Fluxo oficial para qualquer consumidor externo:

```text
UI/API/Controllers/Forms -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database
```

Exemplo correto:

```text
ModuleProfileService.Create(request)
ModuleProfileService.GetById(id)
ModuleProfileService.Update(request)
ModuleProfileService.SoftDelete(id)
ModuleProfileService.ListActive()
```

Exemplos proibidos para UI/API:

```text
new SqliteModuleProfileRepository(...)
new SqliteBdServiceProvider(...)
new SqliteConnectionFactory(...)
SQLiteConnection
```

A UI e a API REST futura so devem nascer depois que o CRUD BLL/DAL correspondente estiver funcional, validado e documentado. Isso evita que telas ou endpoints dependam de detalhes SQLite e preserva a migracao futura para PostgreSQL/Supabase.

## CRUD piloto `module_profiles`

Status: `IMPLEMENTADO`.

Operacoes disponiveis:

- create;
- read by id;
- update;
- soft delete;
- listagem ativa.

O service BLL valida nome e status, gera `id` como GUID/TEXT, preenche `created_at` e `updated_at` em UTC e controla `sync_status`. O repository mantem o SQL da entidade, mas toda execucao passa pelo `BD_Service_Provider`.

Listagens e consultas ativas usam `deleted_at IS NULL`. Soft delete preenche `deleted_at`, atualiza `updated_at` e marca `sync_status = 'deleted'`.

## Migration atual

- `0001_sqlite_schema_v1`: baseline derivado de `Data/Modules/schema/sqlite_schema_v1.sql`.
- `0002_sync_metadata.sql`: adiciona metadados de sincronizacao e soft delete quando colunas estao ausentes.

## Estado atual

- Banco local: `IMPLEMENTADO`.
- Inicializacao automatica no startup: `IMPLEMENTADO`.
- Runner de migrations: `IMPLEMENTADO`.
- `BD_Service_Provider`: `IMPLEMENTADO`.
- CRUD piloto `module_profiles`: `IMPLEMENTADO`.
- Repository Pattern para demais entidades: `PLANEJADO`.
- PostgreSQL/Supabase runtime: `PLANEJADO`.
- Sincronizacao cloud: `PLANEJADO`.
- UI de manutencao do banco: `PLANEJADO`.

## Limites

O banco local nao executa comandos SDH, nao acessa SDGW/SDCTP e nao altera firmware. A compatibilidade com PostgreSQL/Supabase nesta ETAPA e estrutural, nao operacional.

## Regra para futuras entidades

Nova entidade/repository/service so deve ser criado quando uma ETAPA funcional exigir salvar, consultar, validar, expor dados ou migrar schema daquela entidade.

Cada novo dominio deve seguir:

```text
DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

Depois da validacao desse fluxo, UI/API podem consumir somente o BLL Service correspondente.
