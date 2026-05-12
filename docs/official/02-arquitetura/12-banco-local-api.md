# Banco local da API

Status: `PARCIALMENTE IMPLEMENTADO`

## Objetivo

O banco local da API prepara o SimulDIESEL para persistir o Banco de Modulos em SQLite no host local, mantendo a arquitetura `UI -> BLL -> DAL -> DATABASE` e preservando caminho futuro para PostgreSQL/Supabase.

## Estrutura implementada

- `Data/Modules/modules.db`: arquivo SQLite local.
- `Data/Modules/schema/sqlite_schema_v1.sql`: baseline estrutural SQLite do Banco de Modulos.
- `Data/Modules/schema/postgres_schema_v1.sql`: referencia futura para PostgreSQL/Supabase.
- `Data/Modules/schema/migrations/`: migrations incrementais do banco local.
- `DAL/Database/`: factories, initializer, runner de migrations e `BD_Service_Provider`.
- `DAL/Repositories/`: contratos e implementacoes de repository.
- `BLL/Services/Database/`: servico de inicializacao e status consumivel por camadas superiores.
- `DTL/Modules/`: DTOs do CRUD piloto de perfis de modulo.

## Fluxo de inicializacao

No startup da aplicacao, `LocalDatabaseService` solicita a inicializacao ao DAL. O DAL resolve o caminho do repositorio, abre o provider SQLite, cria `Data/Modules/modules.db` quando necessario, aplica o baseline `sqlite_schema_v1.sql` e executa migrations pendentes registradas em `schema_migrations`.

```text
Program -> BLL/Services/Database -> DAL/Database -> SQLite modules.db
```

Fluxo oficial de persistencia por dominio:

```text
DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

## Provider e desacoplamento

O provider inicial e SQLite por meio de `SqliteConnectionFactory` e `SqliteBdServiceProvider`. A BLL consome interfaces de initializer e repository, sem SQL e sem referencia direta a tipos SQLite. A troca futura para PostgreSQL/Supabase deve preservar os contratos de DAL e adicionar implementacoes especificas de provider.

### Papel por camada

- `BLL Service`: regra de negocio, validacao de dominio e orquestracao.
- `Entity Repository`: SQL especifico da entidade, sem abrir conexao diretamente.
- `BD_Service_Provider`: conexao, execucao segura de comandos, scalar, query e transacao basica.
- `Database`: SQLite local atual; PostgreSQL/Supabase futuramente.

### Regras

- UI nunca acessa repository diretamente.
- UI nunca acessa `BD_Service_Provider` diretamente.
- API REST futura, controllers e forms nunca acessam repository ou provider diretamente.
- API REST futura deve consumir BLL Service.
- BLL nunca contem SQL e nunca depende de SQLite.
- BLL Service e a porta oficial de entrada para operacoes de dominio.
- Repository nunca abre conexao diretamente e nunca instancia `SQLiteConnection`.
- Repository acessa banco exclusivamente via `BD_Service_Provider`.
- Repository e detalhe interno da persistencia.
- Provider nao contem regra de negocio e nao substitui repositories de dominio.
- `BD_Service_Provider` e detalhe interno do DAL.
- SQL especifico de entidade pode permanecer no repository, mas sua execucao passa pelo provider.
- DTOs publicos nao carregam tipos SQLite, PostgreSQL ou Supabase.

## Regra de consumo dos CRUDs

Toda camada externa ao dominio de persistencia deve consumir CRUDs pelo service BLL correspondente:

```text
UI/API/Controllers/Forms -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database
```

Exemplo correto para o piloto validado:

```text
UI/API -> ModuleProfileService -> IModuleProfileRepository -> IBdServiceProvider -> SQLite
```

Exemplos proibidos:

```text
UI/API -> SqliteModuleProfileRepository
UI/API -> SqliteBdServiceProvider
UI/API -> SqliteConnectionFactory
UI/API -> SQLiteConnection
```

Sequencia correta para futuras entidades:

1. criar DTOs publicos sem tipos de banco;
2. criar BLL Service com validacoes e regras de dominio;
3. criar repository especializado;
4. executar banco somente via `BD_Service_Provider`;
5. validar CRUD BLL/DAL com build e smoke funcional;
6. liberar UI/API apenas depois do CRUD estar funcional e documentado.

Essa regra preserva a troca futura para PostgreSQL/Supabase: UI e API continuam falando com services BLL, enquanto a tecnologia de persistencia permanece isolada no DAL.

## CRUD piloto de `module_profiles`

Status: `IMPLEMENTADO`.

O primeiro CRUD funcional do banco local usa `module_profiles` como entidade piloto para validar o padrao oficial de crescimento:

```text
ModuleProfile DTO -> ModuleProfileService -> IModuleProfileRepository -> IBdServiceProvider -> SQLite
```

Operacoes implementadas:

- criar perfil;
- consultar por `id`;
- atualizar campos principais;
- soft delete usando `deleted_at` e `sync_status = 'deleted'`;
- listar perfis ativos ignorando registros com `deleted_at`.

Responsabilidades:

- `DTL/Modules`: contratos simples de entrada e saida do perfil;
- `ModuleProfileService`: validacao de nome/status, geracao de GUID e timestamps UTC;
- `SqliteModuleProfileRepository`: SQL especifico de `module_profiles`;
- `SqliteBdServiceProvider`: execucao real, parametros e conexao.

O CRUD piloto nao cria entidades adicionais e nao implementa pins, connectors, CAN, J1939, SDH, capturas, logs ou testes.

## Migrations

O runner aplica:

1. baseline `0001_sqlite_schema_v1`, a partir de `Data/Modules/schema/sqlite_schema_v1.sql`;
2. arquivos `.sql` em `Data/Modules/schema/migrations/`, em ordem lexicografica.

A tabela `schema_migrations` registra migrations aplicadas. A migration `0002_sync_metadata.sql` adiciona metadados portaveis para sincronizacao futura:

- `sync_status`;
- `cloud_id`;
- `deleted_at`;
- `created_at` e `updated_at` em tabelas que ainda nao possuam esses campos.

## Compatibilidade PostgreSQL/Supabase

As decisoes atuais preservam compatibilidade futura:

- IDs seguem como `TEXT` no SQLite e podem migrar para `UUID` no PostgreSQL.
- JSON segue como `TEXT` validado no SQLite e `JSONB` no PostgreSQL.
- Metadados `sync_status`, `cloud_id` e `deleted_at` preparam sincronizacao sem ativar runtime cloud.
- SQL fica localizado no DAL e nas migrations.
- O contrato do provider permite futuras implementacoes PostgreSQL/Supabase sem alterar a BLL.

## Regra de escalabilidade

Nenhuma nova entidade, repository ou service nasce por especulacao. A criacao ocorre somente quando uma ETAPA funcional precisar:

1. salvar dados daquela entidade;
2. consultar dados daquela entidade;
3. validar dados daquela entidade;
4. expor dados daquela entidade para outra camada;
5. migrar schema relacionado aquela entidade.

Quando uma entidade surgir, o fluxo obrigatorio e:

```text
DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database
```

UI/API para essa entidade so entram apos validacao do BLL Service e do repository. Criar tela, endpoint ou controller antes do CRUD BLL/DAL validado nao e entrega valida.

## Limites atuais

- `PARCIALMENTE IMPLEMENTADO`: core local, inicializacao, migrations, provider e CRUD piloto de `module_profiles` existem, mas ainda nao ha UI, importadores, editores completos ou sincronizacao cloud.
- Nao ha autenticacao.
- Nao ha Supabase runtime.
- O banco nao executa comandos SDH.
- `module_profiles` possui CRUD piloto funcional; repositorios completos das demais entidades permanecem `PLANEJADO`.
- `SqliteModuleProfileRepository` usa o `BD_Service_Provider`; ele permanece base estrutural e nao virou repository generico gigante.

## Regras

- SQL nao deve ser espalhado na BLL.
- Camadas superiores nao devem depender diretamente do SQLite.
- Dumps nao substituem esta documentacao oficial.
- Mudancas de schema devem atualizar migrations, docs e dump da ETAPA.
