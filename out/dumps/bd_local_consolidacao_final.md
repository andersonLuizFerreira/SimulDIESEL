# Dump - Consolidacao final da infraestrutura do Banco Local

Data: 2026-05-12

## Resumo executivo

Esta ETAPA consolida e congela a infraestrutura do Banco Local da API do SimulDIESEL como baseline arquitetural oficial.

O baseline validado inclui:

- core SQLite local;
- inicializacao automatica do banco;
- migrations controladas por `schema_migrations`;
- `BD_Service_Provider`;
- CRUD piloto funcional de `module_profiles`;
- regra oficial de consumo por BLL Service;
- documentacao oficial atualizada;
- skill de Banco de Modulos atualizada;
- compatibilidade planejada para PostgreSQL/Supabase.

## ETAPAS consolidadas

1. `BD LOCAL ESTRUTURA`
2. `BD SERVICE PROVIDER`
3. `CRUD PILOTO MODULE PROFILE`
4. `REGRA DE CONSUMO DO CRUD PELO RESTANTE DO SISTEMA`
5. `RASTREAMENTO DE DADOS CANDIDATOS À PERSISTÊNCIA`

## Arquitetura final validada

Fluxo oficial:

```text
UI/API/Controllers/Forms -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database
```

Fluxo piloto:

```text
ModuleProfile DTO -> ModuleProfileService -> IModuleProfileRepository -> IBdServiceProvider -> SQLite
```

## Provider validado

Componentes:

- `IBdServiceProvider`
- `SqliteBdServiceProvider`
- `BdCommandParameter`
- `SqliteConnectionFactory`
- `SqliteDatabaseInitializer`
- `SqliteMigrationRunner`
- `ModuleDatabaseRuntimeFactory`

Responsabilidades confirmadas:

- abrir conexao via factory;
- executar comandos parametrizados;
- executar queries;
- executar scalar;
- executar transacao basica;
- manter tecnologia SQLite isolada no DAL;
- nao conter regra de negocio;
- nao substituir repositories de dominio.

## CRUD piloto validado

Entidade piloto: `module_profiles`.

Operacoes validadas:

- create;
- read by id;
- update;
- soft delete;
- listagem ativa ignorando `deleted_at`.

Campos usados:

- `id`;
- `created_at`;
- `updated_at`;
- `sync_status`;
- `cloud_id`;
- `deleted_at`;
- demais campos do perfil de modulo ja previstos no schema v1.

## Regras oficiais aprovadas

- UI nunca acessa repository diretamente.
- UI nunca acessa `BD_Service_Provider` diretamente.
- API REST futura nunca acessa repository diretamente.
- API REST futura deve consumir BLL Service.
- Services BLL sao a porta oficial de entrada para operacoes de dominio.
- Repository e detalhe interno da persistencia.
- `BD_Service_Provider` e detalhe interno do DAL.
- DTOs publicos nao carregam tipos SQLite/PostgreSQL/Supabase.
- Regras de negocio ficam na BLL.
- SQL fica no repository ou DAL apropriado.
- Execucao real do banco fica no `BD_Service_Provider`.
- Novo CRUD deve nascer primeiro como BLL Service + repository validado.
- UI/API so podem nascer depois do CRUD BLL/DAL estar funcional e documentado.
- Nenhuma nova entidade/repository/service nasce por especulacao.

## Skills atualizadas

- `.codex/skills/module-database/SKILL.md`
- `docs/agents/skills/module-database-skill.md`

As skills registram:

- fluxo oficial de persistencia;
- fluxo oficial de consumo externo;
- regra de escalabilidade;
- uso obrigatorio do provider;
- proibicao de UI/API chamar repository/provider diretamente;
- CRUD piloto `module_profiles` como referencia.

## Documentacao atualizada

- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`
- dumps em `out/dumps/` relacionados ao Banco Local:
  - `bd_local_estrutura.md`
  - `bd_service_provider.md`
  - `bd_persistence_candidates.md`
  - `bd_service_provider_module_profile_crud.md`
  - `bd_crud_consumption_rule.md`
  - `bd_local_consolidacao_final.md`

## Validacoes finais executadas

### Build final

Comando:

```text
MSBuild SimulDIESEL.sln /t:Restore,Build /p:Configuration=Debug /p:OutDir=out/build-bd-local-final/
```

Resultado:

```text
0 Aviso(s)
0 Erro(s)
```

Observacao: a primeira tentativa no sandbox falhou por acesso negado ao SDK local em `C:\Users\Escritório\AppData\Local\Microsoft SDKs`; a validacao foi repetida fora do sandbox e passou.

### Inicializacao, migrations e CRUD piloto

Smoke executado em banco temporario sob `out/build-bd-local-final/validation/`, removido apos a validacao.

Resultado:

```text
AppliedMigrations=0001_sqlite_schema_v1,0002_sync_metadata
ReinitAppliedMigrations=
CreatedId=17b51878-2d30-479f-8245-86d679495d77
ReadName=Baseline Final
Updated=True
UpdatedName=Baseline Final Validado
UpdatedStatus=active
BeforeDeleteCount=1
Deleted=True
AfterDeleteIsNull=True
AfterDeleteCount=0
```

Validado:

- criacao automatica do banco temporario;
- migrations aplicadas;
- reinicializacao sem reaplicar migrations;
- create;
- read by id;
- update;
- soft delete;
- listagem ativa ignorando soft deleted.

### Validacao arquitetural

Buscas finais confirmaram:

- BLL de banco sem SQL, `SQLiteConnection`, `CreateOpenConnection` ou `PRAGMA`;
- repositories sem `SQLiteConnection`, `CreateOpenConnection` ou `System.Data.SQLite`;
- SQLite restrito a `DAL/Database` e ao `PackageReference`;
- novos arquivos C# incluidos no `.csproj`;
- nenhum arquivo C# orfao nas pastas de banco alteradas;
- documentacao e skill contendo as regras oficiais atualizadas.

## Status final do banco local

- Banco local SQLite: `IMPLEMENTADO`.
- Inicializacao automatica: `IMPLEMENTADO`.
- Migrations: `IMPLEMENTADO`.
- `BD_Service_Provider`: `IMPLEMENTADO`.
- CRUD piloto `module_profiles`: `IMPLEMENTADO`.
- Regra oficial de consumo: `IMPLEMENTADO`.
- PostgreSQL/Supabase runtime: `PLANEJADO`.
- Sincronizacao cloud: `PLANEJADO`.
- UI/API REST para banco: `PLANEJADO`.
- CRUDs de demais entidades: `PLANEJADO`.

## Arquivos temporarios e binarios

- Diretorios temporarios de build/smoke removidos:
  - `out/build-bd-module-profile-crud`
  - `out/build-bd-local-final`
  - `out/validation-bd-local`
- Nenhum binario de build foi intencionalmente incluido no commit.
- Bancos temporarios de validacao nao foram incluidos no commit.

## Proximos passos sugeridos

- Criar UI/API apenas apos ETAPA propria.
- Manter `ModuleProfileService` como padrao de consumo para futuras superficies.
- Criar CRUDs adicionais somente quando uma ETAPA funcional exigir.
- Planejar PostgreSQL/Supabase runtime em ETAPA separada.
- Definir importador/editor de perfis em ETAPA futura.

## Git

Branch utilizada: `main`.

Commit planejado para esta consolidacao:

```text
ETAPA Banco Local: consolidacao da infraestrutura SQLite + BD_Service_Provider + CRUD piloto ModuleProfile
```

Tag planejada:

```text
v0.8.0-bd-local-foundation
```

Hash do commit gerado: registrado na entrega final apos criacao do commit.

Confirmacao do push: registrada na entrega final apos push remoto.

Observacao: o hash do commit que contem este arquivo nao pode ser conhecido antes da criacao do proprio commit. Por isso o hash final e a confirmacao remota ficam registrados na entrega final da ETAPA.

## Confirmacao de escopo

Nao foram alterados nesta consolidacao final:

- firmware;
- SDGW;
- SDCTP;
- UCE/BPM/GSA;
- novas entidades;
- novos CRUDs;
- endpoints REST;
- UI;
- contratos de protocolo.
