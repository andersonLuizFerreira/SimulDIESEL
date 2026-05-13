# ETAPA - Catalogo J1939/81 no Banco Local

Data: 2026-05-13

## Objetivo da ETAPA

Criar no banco local SQLite do SimulDIESEL as tabelas de catalogo/referencia J1939/81 necessarias para interpretar identidade de rede CAN, mantendo esses dados separados dos cadastros operacionais de modulos reais.

Esta ETAPA cria apenas estrutura. Nao popula dados, nao cria CRUD, nao cria UI, nao altera firmware, SDGW, SDCTP ou fluxo CAN RX.

## Branch utilizada

- Branch: `feature/j1939-reference-catalogs`

## Resultado da atualizacao da main

- Branch inicial: `main`.
- `git fetch origin`: executado com sucesso.
- `main`: alinhada com `origin/main`.
- Commit local/remoto validado: `599b27f40096b3b5b0fbfb83f2e92b356595196c`.
- Build pre-ETAPA da `main`: sucesso, 0 avisos, 0 erros.

## Arquivos criados

- `Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql`
- `out/dumps/etapa_j1939_81_reference_catalog_tables.md`

## Arquivos alterados

- `Data/Modules/schema/postgres_schema_v1.sql`
- `Data/Modules/docs/module_database_model_v1.md`
- `docs/official/02-arquitetura/12-banco-local-api.md`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## Nome da migration criada

- `0003_j1939_reference_catalogs`

## SQL das tabelas criadas

```sql
CREATE TABLE IF NOT EXISTS j1939_industry_groups (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0 AND code <= 7),
    name TEXT NOT NULL,
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS j1939_manufacturers (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0),
    name TEXT NOT NULL,
    country TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS j1939_functions (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0 AND code <= 255),
    name TEXT NOT NULL,
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS j1939_vehicle_systems (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL CHECK (code >= 0 AND code <= 127),
    name TEXT NOT NULL,
    description TEXT,
    industry_group_code INTEGER CHECK (industry_group_code IS NULL OR (industry_group_code >= 0 AND industry_group_code <= 7)),
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS j1939_preferred_addresses (
    id TEXT PRIMARY KEY,
    address INTEGER NOT NULL CHECK (address >= 0 AND address <= 253),
    name TEXT NOT NULL,
    description TEXT,
    function_code INTEGER CHECK (function_code IS NULL OR (function_code >= 0 AND function_code <= 255)),
    industry_group_code INTEGER CHECK (industry_group_code IS NULL OR (industry_group_code >= 0 AND industry_group_code <= 7)),
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS j1939_name_field_definitions (
    id TEXT PRIMARY KEY,
    field_name TEXT NOT NULL UNIQUE,
    bit_start INTEGER CHECK (bit_start IS NULL OR bit_start >= 0),
    bit_length INTEGER CHECK (bit_length IS NULL OR bit_length > 0),
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

## Indices criados

```sql
CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_industry_groups_code
    ON j1939_industry_groups(code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_manufacturers_code
    ON j1939_manufacturers(code);

CREATE INDEX IF NOT EXISTS idx_j1939_manufacturers_name
    ON j1939_manufacturers(name);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_functions_code
    ON j1939_functions(code);

CREATE INDEX IF NOT EXISTS idx_j1939_functions_name
    ON j1939_functions(name);

CREATE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_code
    ON j1939_vehicle_systems(code);

CREATE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_industry_group_code
    ON j1939_vehicle_systems(industry_group_code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_code_industry_group_code
    ON j1939_vehicle_systems(code, industry_group_code);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_address
    ON j1939_preferred_addresses(address);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_function_code
    ON j1939_preferred_addresses(function_code);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_industry_group_code
    ON j1939_preferred_addresses(industry_group_code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_address_industry_group_code
    ON j1939_preferred_addresses(address, industry_group_code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_name_field_definitions_field_name
    ON j1939_name_field_definitions(field_name);
```

## Resultado da aplicacao da migration

Validacao executada com o runner real do projeto (`SqliteDatabaseInitializer` + `SqliteMigrationRunner`) sobre banco SQLite controlado em:

`out/validation-bd-local/j1939_reference_catalogs_20260513_092727/modules.db`

Resultado:

- Primeira inicializacao aplicou: `0001_sqlite_schema_v1`, `0002_sync_metadata`, `0003_j1939_reference_catalogs`.
- `schema_migrations`: `0001_sqlite_schema_v1`, `0002_sync_metadata`, `0003_j1939_reference_catalogs`.
- Tabelas J1939/81 criadas:
  - `j1939_industry_groups`
  - `j1939_manufacturers`
  - `j1939_functions`
  - `j1939_vehicle_systems`
  - `j1939_preferred_addresses`
  - `j1939_name_field_definitions`
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
- Contagem das tabelas J1939/81 apos migration: todas `0`, confirmando ausencia de popularizacao nesta ETAPA.

## Resultado da reinicializacao sem reaplicar migration

- Segunda inicializacao aplicada: nenhuma migration.
- Linha de validacao inserida em `module_profiles` antes da segunda inicializacao permaneceu presente: `1`.
- Nenhum dado operacional existente no banco de validacao foi apagado.
- `Data/Modules/modules.db` nao foi alterado durante a validacao.

## Resultado do build

Build pre-ETAPA da `main`:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

Build final da ETAPA:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

## Observacoes de compatibilidade com PostgreSQL/Supabase

- `Data/Modules/schema/postgres_schema_v1.sql` recebeu estrutura equivalente das seis tabelas.
- No PostgreSQL documental, IDs foram modelados como `UUID PRIMARY KEY DEFAULT gen_random_uuid()`, seguindo o padrao existente do arquivo.
- `created_at` e `updated_at` usam `TIMESTAMPTZ NOT NULL DEFAULT now()`.
- Nao foi executado Supabase.
- Nao foi criado runtime PostgreSQL/Supabase.
- Nao foram criadas FKs rigidas para `industry_group_code` ou `function_code`, preservando suporte futuro a codigos desconhecidos observados na rede.

## Limitacoes

- Sem dados SAE/J1939 carregados.
- Sem importador.
- Sem CRUD BLL/DAL para esses catalogos.
- Sem UI.
- Sem alteracao de parsers J1939 existentes.
- Sem alteracao de firmware, SDGW, SDCTP ou fluxo CAN RX.
- A primeira tentativa de validacao encontrou que comentarios SQL com `;` quebram o splitter simples do runner; o comentario da migration foi ajustado e a validacao passou em seguida.

## Proximos passos sugeridos

- Criar ETAPA propria para popular catalogos com fonte rastreavel.
- Criar ETAPA propria para consulta BLL/DAL dos catalogos, caso a aplicacao precise exibir nomes a partir de codigos.
- Criar ETAPA propria para integrar esses catalogos ao fluxo J1939/81 sem falhar quando codigos estiverem ausentes.

## Rollback

Rollback preservado:

- A mudanca esta isolada na branch `feature/j1939-reference-catalogs`.
- A estrutura nova esta concentrada na migration `0003_j1939_reference_catalogs.sql`.
- Nenhuma migration antiga foi alterada.
- Nenhum dado real foi apagado.
- Nenhum commit, tag ou push foi executado.
