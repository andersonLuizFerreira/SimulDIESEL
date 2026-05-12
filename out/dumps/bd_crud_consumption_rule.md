# Dump - Regra de consumo dos CRUDs do banco local

Data: 2026-05-12

## Objetivo da ETAPA

Atualizar a skill e a documentacao do Banco de Dados da API para definir a regra oficial de consumo dos CRUDs do banco local pelo restante do sistema, usando `ModuleProfileService` como piloto ja validado.

Esta ETAPA foi exclusivamente documental.

## Regra oficial aprovada

Nenhuma camada externa deve acessar repository ou provider diretamente.

Fluxo oficial:

```text
UI/API/Controllers/Forms -> BLL Service -> Repository especializado -> BD_Service_Provider -> Database
```

Para `ModuleProfile`:

```text
UI/API -> ModuleProfileService -> IModuleProfileRepository -> IBdServiceProvider -> SQLite
```

## Arquivos alterados

- `.codex/skills/module-database/SKILL.md`
- `docs/agents/skills/module-database-skill.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`
- `Data/Modules/docs/local_api_database_runtime.md`
- `out/dumps/bd_crud_consumption_rule.md`

## Exemplos documentados

### Correto

```text
ModuleProfileService.Create(request)
ModuleProfileService.GetById(id)
ModuleProfileService.Update(request)
ModuleProfileService.SoftDelete(id)
ModuleProfileService.ListActive()
```

### Proibido

```text
UI/API -> SqliteModuleProfileRepository
UI/API -> SqliteBdServiceProvider
UI/API -> SqliteConnectionFactory
UI/API -> SQLiteConnection
```

## Itens proibidos registrados

- UI acessar repository diretamente.
- UI acessar `BD_Service_Provider` diretamente.
- API REST futura acessar repository diretamente.
- API REST futura acessar provider/factory/SQLite diretamente.
- Controllers ou forms dependerem de tipos SQLite/PostgreSQL/Supabase.
- Criar UI/API antes de o CRUD BLL/DAL estar funcional e validado.
- Criar endpoint, tela, CRUD ou entidade especulativa nesta ETAPA.

## Sequencia correta para futuras entidades

1. Criar DTOs publicos sem tipos de banco.
2. Criar BLL Service com validacoes e regras de dominio.
3. Criar repository especializado.
4. Executar banco somente via `BD_Service_Provider`.
5. Validar CRUD BLL/DAL com build e smoke funcional.
6. Liberar UI/API somente apos o CRUD estar funcional e documentado.

## Relacao com PostgreSQL/Supabase

A regra preserva a escalabilidade futura porque UI/API dependem apenas de services BLL. Repository e provider permanecem internos ao DAL, permitindo trocar SQLite por PostgreSQL/Supabase futuramente sem alterar consumidores externos.

## Validacoes executadas

- Conferida a atualizacao dos quatro documentos obrigatorios.
- Confirmado que a ETAPA nao criou arquivos C#.
- Confirmado que nao houve endpoint, UI, migration, CRUD novo ou entidade nova nesta ETAPA.
- Build nao executado porque nenhum arquivo C# foi alterado nesta ETAPA.
- `git status --short` executado na entrega.

## Confirmacao de escopo

Nao houve implementacao funcional.

Nao foram alterados:

- UI;
- API REST;
- endpoints;
- novos CRUDs;
- novas entidades;
- migrations;
- Supabase runtime;
- sincronizacao cloud;
- firmware;
- SDGW;
- SDCTP;
- UCE/BPM/GSA.

## Proximos passos recomendados

- Aplicar esta regra em qualquer ETAPA futura que exponha UI, endpoint, controller ou form para dados persistidos.
- Antes de criar UI/API para nova entidade, exigir CRUD BLL/DAL validado e documentado.
- Manter `ModuleProfileService` como referencia de consumo ate que novos dominios sejam formalmente implementados.
