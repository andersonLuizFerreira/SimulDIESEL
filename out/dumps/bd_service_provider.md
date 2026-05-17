# Dump - BD SERVICE PROVIDER + regra de escalabilidade dos repositories

Data: 2026-05-12

## Objetivo da ETAPA

Consolidar a regra arquitetural oficial de persistencia do banco local da API:

```text
DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

O foco foi centralizar a execucao real de banco no provider, adaptar o repository existente e documentar o padrao de crescimento futuro sem criar entidades especulativas.

## Arquitetura antes/depois

### Antes

```text
BLL Service -> Entity Repository -> Connection Factory -> SQLite
```

O `SqliteModuleProfileRepository` ainda abria conexao por meio da factory.

### Depois

```text
BLL Service -> Entity Repository -> BD_Service_Provider -> Connection Factory -> SQLite
```

O repository mantem o SQL de dominio, mas a execucao real passa pelo provider.

## Regra oficial aprovada

- BLL nunca contem SQL.
- BLL nunca depende de SQLite.
- UI nunca acessa repository diretamente.
- Repository nunca abre conexao diretamente.
- Repository nunca instancia `SQLiteConnection`.
- Repository acessa banco exclusivamente via `BD_Service_Provider`.
- Provider nao contem regra de negocio.
- Provider nao substitui repositories de dominio.
- SQL especifico de entidade pode permanecer no repository, mas a execucao deve passar pelo provider.
- Alteracao de schema permanece via migration.
- Toda nova entidade deve atualizar documentacao.
- Toda nova entidade deve preservar compatibilidade futura SQLite/PostgreSQL/Supabase.
- Nao criar CRUDs de entidades sem uso real.

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/BdCommandParameter.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IBdServiceProvider.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteBdServiceProvider.cs`
- `out/dumps/bd_service_provider.md`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteModuleProfileRepository.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `.agents/skills/module-database/SKILL.md`
- `.agents/skills/module-database/SKILL.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`

## Provider criado

### `IBdServiceProvider`

Contrato generico de persistencia com:

- `ExecuteNonQuery`
- `ExecuteScalar`
- `ExecuteScalar<T>`
- `Query<T>`
- `InTransaction<T>`
- `InTransaction(Action<...>)`

### `SqliteBdServiceProvider`

Implementacao SQLite atual:

- abre conexoes via `IDatabaseConnectionFactory`;
- cria comandos parametrizados com `BdCommandParameter`;
- concentra execucao comum de comandos;
- oferece transacao basica;
- permanece sem regra de negocio de modulo, pino, CAN, J1939 ou SDH.

## Repository adaptado

`SqliteModuleProfileRepository` deixou de usar a connection factory diretamente e passou a depender de `IBdServiceProvider`.

Comportamento preservado:

- `CountProfiles()` continua retornando a contagem de perfis ativos.

## Documentacao atualizada

- `docs/official/02-arquitetura/12-banco-local-api.md`
  - fluxo oficial com provider;
  - papeis de BLL, repository, provider e database;
  - regra de escalabilidade;
  - provider como ponto de troca futura.
- `Data/Modules/docs/local_api_database_runtime.md`
  - provider no runtime local;
  - repository usando provider;
  - regra para futuras entidades.
- Skills estruturada e humana de Banco de Modulos
  - regra de provider;
  - proibicao de repository abrir conexao;
  - criterio para criar novas entidades somente por demanda funcional real.

## Skill atualizada

- `.agents/skills/module-database/SKILL.md`
- `.agents/skills/module-database/SKILL.md`

Regras novas:

- fluxo `DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database`;
- UI nao acessa repository diretamente;
- repository usa provider exclusivamente;
- provider nao vira super repository;
- entidades novas so surgem quando uma ETAPA funcional precisar delas.

## Validacoes executadas

### Build

Comando:

```text
MSBuild SimulDIESEL.sln /t:Restore,Build /p:Configuration=Debug /p:OutDir=out/build-bd-provider/
```

Resultado:

```text
0 Aviso(s)
0 Erro(s)
```

### Inicializacao e repository

Validacao via Windows PowerShell/.NET Framework carregando o assembly compilado:

```text
DatabasePath=G:\PROJETOS\SIMULADORES\SimulDIESEL\Data\Modules\modules.db
AppliedMigrations=
ModuleProfileCount=0
ProviderCountProfiles=0
```

### Provider

Smoke adicional:

```text
SchemaMigrationCountViaProvider=2
TransactionSmoke=OK
```

O smoke de transacao usou tabela temporaria SQLite, sem alterar o schema persistente do banco real.

### Arquitetura

- Busca confirmou que `SqliteModuleProfileRepository` nao usa `SQLiteConnection` nem `CreateOpenConnection()`.
- Busca confirmou ausencia de SQL em `BLL/Services/Database`.
- Uso direto de SQLite ficou restrito a `SqliteConnectionFactory` e infraestrutura DAL de database/migration/provider.
- `SimulDIESEL.csproj` foi atualizado para incluir os tres arquivos C# novos.
- Documentacao oficial e skill de Banco de Modulos foram atualizadas.

## Observacoes de worktree

- O `.csproj` ja possuia alteracoes abertas da ETAPA anterior `BD LOCAL ESTRUTURA`; nesta ETAPA ele foi adicionalmente sincronizado com `BdCommandParameter.cs`, `IBdServiceProvider.cs` e `SqliteBdServiceProvider.cs`.
- O worktree continua contendo alteracoes documentais e funcionais de ETAPAS recentes ainda sem commit; nenhuma delas foi revertida.

## Itens fora do escopo preservados

- UI.
- Supabase runtime.
- Sincronizacao cloud.
- Autenticacao.
- APIs REST.
- Execucao SDH.
- Novos CRUDs de entidades ainda sem uso.
- Dashboards.
- Firmware.
- UCE.
- BPM.
- GSA.
- SDGW.
- SDCTP.

## Proximos passos planejados

- Criar repositories especializados adicionais somente quando ETAPA funcional exigir.
- Evoluir provider para implementacao PostgreSQL/Supabase quando o runtime cloud entrar em escopo.
- Avaliar uma convencao comum para DTOs/entities de persistencia quando surgirem entidades novas.

## Confirmacao de rollback e escopo

- Nenhum commit, branch ou tag foi criado.
- Nao houve alteracao em UI, firmware, SDGW, SDCTP, UCE, BPM ou GSA.
- A arquitetura `UI -> BLL -> DAL -> DATABASE` foi preservada.
