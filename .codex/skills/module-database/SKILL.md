# Nome

Module Database

## Objetivo

Orientar ETAPAS do Banco de Modulos, schemas, perfis, pinagem, sinais, comandos SDH, sequencias de teste e capturas. Orientar tambem a camada local de banco da API quando o escopo envolver SQLite, migrations, repositories e preparacao futura para PostgreSQL/Supabase.

## Quando usar

Use para `Data/Modules/`, schemas SQLite/PostgreSQL, relacao com SDH, dumps, validadores do modelo, DAL de banco, repositories e BLL de inicializacao do banco local.

## Quando nao usar

Nao use para implementar UI, executor de comandos, firmware ou cloud/autenticacao sem pedido.

## Escopo permitido

- `Data/Modules/`
- `Data/Modules/schema/`
- `Data/Modules/docs/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/`
- `tools/dumps/module_database_model/`
- dumps do modelo.

## Escopo proibido

- Alterar `modules.db` com dados reais sem autorizacao.
- Fazer o banco executar comandos SDH.
- Mudar firmware/protocolo para caber no banco.

## Arquivos/pastas provaveis

- `Data/Modules/modules.db`
- `sqlite_schema_v1.sql`
- `postgres_schema_v1.sql`
- `module_database_model_v1.md`
- `module_database_sdh_relation.md`

## Padroes do projeto

- Tabelas em `snake_case`.
- SQLite usa JSON como `TEXT` com `json_valid`.
- PostgreSQL futuro usa `JSONB`.
- Comandos SDH devem ser validaveis por `SdhValidator`.
- A arquitetura do banco local da API deve preservar `UI -> BLL -> DAL -> DATABASE`.
- Fluxo oficial de persistencia: `DTO/Entity -> BLL Service -> Entity Repository -> BD_Service_Provider -> Database`.
- Fluxo oficial de consumo externo: `UI/API/Controllers/Forms -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database`.
- BLL nao deve conhecer SQL ou tipos concretos do provider SQLite.
- UI nunca acessa repository diretamente.
- UI, API REST futura, controllers e forms nunca acessam repository ou `BD_Service_Provider` diretamente.
- API REST futura deve consumir BLL Service, nunca DAL diretamente.
- Services BLL sao a porta oficial de entrada para operacoes de dominio.
- Repository representa o dominio/tabela e acessa banco exclusivamente via `BD_Service_Provider`.
- Repository e detalhe interno da persistencia.
- Repository nao abre conexao diretamente e nao instancia `SQLiteConnection`.
- `BD_Service_Provider` centraliza abertura de conexao, execucao SQL, scalar, query e transacao basica.
- `BD_Service_Provider` e detalhe interno do DAL.
- `BD_Service_Provider` nao contem regra de negocio e nao substitui repositories de dominio.
- SQL especifico de entidade pode permanecer no repository, mas a execucao deve passar pelo provider.
- O CRUD piloto validado para `module_profiles` e o padrao de referencia para futuras entidades: DTO em `DTL`, service BLL com validacoes, repository especializado no DAL e execucao via `BD_Service_Provider`.
- Exemplo correto: UI/API chama `ModuleProfileService`, que chama `IModuleProfileRepository`, que chama `IBdServiceProvider`.
- Exemplo proibido: UI/API instanciar `SqliteModuleProfileRepository`, `SqliteBdServiceProvider`, `SqliteConnectionFactory` ou qualquer tipo SQLite.
- CRUDs devem implementar create, read by id, update, soft delete e listagem ativa apenas quando houver ETAPA funcional autorizada.
- Qualquer novo CRUD deve nascer primeiro como BLL Service + repository validado; UI/API so podem ser criadas depois do CRUD BLL/DAL estar funcional e validado.
- DAL deve expor factories, initializers, migrations, provider e repositories por interfaces quando houver consumo por camadas superiores.
- DTOs publicos nao devem carregar tipos SQLite, PostgreSQL ou Supabase.
- IDs devem permanecer portaveis para PostgreSQL/Supabase: `TEXT` no SQLite e `UUID` no PostgreSQL.
- Migrations devem ser reexecutaveis de forma segura por controle de versao.
- Nenhuma nova entidade/repository/service deve nascer por especulacao; somente quando uma ETAPA funcional precisar salvar, consultar, validar, expor dados ou migrar schema daquela entidade.

## Checklist de validacao

- [ ] Validar schema com scripts existentes.
- [ ] Gerar dump de schema quando alterado.
- [ ] Confirmar que banco nao executa comandos.
- [ ] Separar exemplo de dado real.
- [ ] Confirmar que inicializacao cria `Data/Modules/modules.db` quando ausente.
- [ ] Confirmar que migrations nao corrompem reinicializacao.
- [ ] Confirmar que BLL nao depende diretamente de SQLite.
- [ ] Confirmar que repositories usam exclusivamente `BD_Service_Provider`.
- [ ] Confirmar que provider nao contem regra de negocio de dominio.
- [ ] Confirmar que UI/API/Controllers/Forms consomem BLL Service e nao DAL diretamente.
- [ ] Quando houver CRUD, validar create, read by id, update, soft delete e listagem ativa em banco temporario ou ambiente controlado.
- [ ] Confirmar que listagens ativas ignoram registros com `deleted_at`.

## Checklist de entrega

- [ ] Schema/documentos alterados.
- [ ] Dump gerado.
- [ ] Compatibilidade SQLite/PostgreSQL.
- [ ] Pendencias de confirmacao.
- [ ] Migrations criadas/alteradas e resultado de aplicacao.
- [ ] Build da solucao quando houver alteracao C#.
- [ ] Regra de escalabilidade documentada quando houver criacao de nova entidade/repository/service.

## Riscos comuns

- Transformar banco em executor operacional.
- Persistir comandos fora do contrato SDH.
- Misturar cloud futura com modelo local v1.
- Espalhar SQL na BLL.
- Acoplar regra de negocio ao provider SQLite.
- Declarar compatibilidade PostgreSQL/Supabase sem manter tipos e nomes portaveis.
- Criar provider que vire super repository de dominio.
- Criar CRUD de entidade sem necessidade funcional real.
- Criar CRUD novo copiando `module_profiles` sem adaptar validacoes e sem ETAPA propria.
- Fazer UI/API consumir repository, provider, factory ou SQLite diretamente.

## Regras de nao regressao

- Banco permanece base de configuracao/teste.
- Contrato SDH continua fonte semantica.
- Nao alterar UI/API/firmware sem ETAPA propria.
- Banco local deve continuar substituivel por provider futuro.
- Dumps nao substituem documentacao oficial em `/docs/`.
- Alteracao de schema sempre deve ocorrer via migration.
- Toda nova entidade deve atualizar documentacao e preservar compatibilidade futura SQLite/PostgreSQL/Supabase.

## Documentacao humana equivalente

`docs/agents/skills/module-database-skill.md`
