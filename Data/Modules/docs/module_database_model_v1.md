# Module Database Model v1

## Objetivo

Este modelo define a primeira estrutura local do Banco de Dados de Modulos do SimulDIESEL. A versao v1 e documental e estrutural: ela descreve como armazenar perfis de modulos, pinagem, alimentacao, redes CAN/J1939, sinais eletricos, comandos SDH, sequencias de teste e capturas.

Nesta etapa o banco nao executa comandos, nao sincroniza com Supabase, nao implementa autenticacao e nao adiciona telas.

## Compatibilidade

- SQLite local: `Data/Modules/schema/sqlite_schema_v1.sql`
- PostgreSQL/Supabase futuro: `Data/Modules/schema/postgres_schema_v1.sql`
- Nomes em `snake_case`.
- JSON no SQLite e armazenado como `TEXT` com `CHECK (json_valid(...))`.
- JSON no PostgreSQL e armazenado como `JSONB`.
- UUID no SQLite e armazenado como `TEXT` fornecido pela aplicacao/ferramenta.
- UUID no PostgreSQL usa `UUID DEFAULT gen_random_uuid()`.

## Entidades

### module_profiles

Cadastro principal do modulo eletronico. Guarda nome, fabricante, modelo, categoria, aplicacao, descricao e status.

Status aceitos: `draft`, `active`, `archived`.

### module_profile_versions

Versiona o perfil tecnico do modulo. Cada perfil pode ter varias versoes, permitindo evolucao controlada sem sobrescrever historico.

Status aceitos: `draft`, `validated`, `released`, `archived`.

### module_connectors

Representa conectores fisicos vinculados a uma versao de perfil. Cada conector possui nome, descricao e quantidade de pinos.

### module_pins

Representa a pinagem cadastrada de cada conector. A combinacao `connector_id + pin_number` e unica.

Direcoes aceitas: `input`, `output`, `bidirectional`, `power`, `ground`, `none`.

### module_power_requirements

Representa requisitos de alimentacao do modulo. Pode apontar para pinos positivo e negativo e informar ordem de habilitacao.

### module_can_networks

Representa redes CAN ou J1939 usadas pelo modulo. Inclui bitrate, controlador esperado (`can0` ou `can1`), source address esperado e pinos CAN H/L.

Protocolos aceitos: `CAN`, `J1939`.

### module_j1939_pgns

Representa PGNs conhecidos ou observados do modulo. Inclui SA, DA, PGN, direcao, periodo, flag proprietaria e amostra de dados em hexadecimal.

Direcoes aceitas: `rx`, `tx`, `observed`, `bidirectional`.

### module_signal_channels

Representa sinais eletricos simulados ou lidos pela bancada. Pode apontar para board (`BPM`, `UCE`, `GSA`), canal fisico e pino relacionado.

### module_sdh_commands

Representa comandos SDH parametrizados pertencentes a uma versao de perfil. Esta tabela e a ponte principal entre o Banco de Modulos e a abstracao executavel das boards.

Campos principais:

- `command_group`: grupo operacional, por exemplo `prepare`, `power`, `can`, `signals`, `diagnostics`.
- `execution_order`: ordem sugerida de preparacao/configuracao.
- `target`, `op`, `args_json`, `meta_json`: partes que formam um `SdhCommand`.
- `enabled`: permite manter comandos documentados sem executa-los futuramente.

### module_test_sequences

Representa uma bateria de testes vinculada a uma versao do perfil.

### module_test_steps

Representa passos de teste. O campo `sdh_command_json` pode guardar um comando SDH completo no formato documental `sdh/1`. Os campos `expected_response_json` e `expected_event_json` descrevem expectativas sem obrigar execucao nesta etapa.

Tipos aceitos: `sdh_command`, `wait`, `expect_event`, `note`.

### module_capture_sessions

Representa sessoes de captura ou aprendizado realizadas contra um modulo.

### module_capture_events

Representa eventos capturados durante aprendizado ou teste, incluindo eventos de board, CAN/J1939, PGN, CAN ID, dados hexadecimais e payload JSON.

### Catalogos J1939/81

Status: `PARCIALMENTE IMPLEMENTADO`.

As tabelas `j1939_industry_groups`, `j1939_manufacturers`, `j1939_functions`, `j1939_vehicle_systems`, `j1939_preferred_addresses` e `j1939_name_field_definitions` guardam referencias tecnicas para interpretar identidade J1939/81 observada na rede CAN.

Esses catalogos sao separados dos cadastros operacionais de modulos reais. A estrutura SQLite existe e a importacao inicial usa JSONs locais versionados em `Data/Protocols/J1939/catalogs/`.

Codigos desconhecidos devem continuar representaveis por fluxos futuros sem falha, por isso campos como `industry_group_code` e `function_code` nao usam FK rigida inicialmente.

O seed inicial cobre subconjuntos rastreaveis de grupos industriais, fabricantes, funcoes, enderecos preferenciais e definicoes de campos do NAME. Nao ha scraping online em runtime, sincronizacao web, CRUD dedicado ou UI.

## Relacionamentos Principais

- `module_profiles` 1:N `module_profile_versions`
- `module_profile_versions` 1:N conectores, alimentacoes, redes CAN, PGNs, sinais, comandos SDH, sequencias de teste e sessoes de captura
- `module_connectors` 1:N `module_pins`
- `module_test_sequences` 1:N `module_test_steps`
- `module_capture_sessions` 1:N `module_capture_events`

## Regras de Uso v1

- O schema v1 nao contem dados reais de modulo.
- Inserts de exemplo, quando criados no futuro, devem ficar marcados como exemplo e separados de dados reais.
- Comandos SDH salvos devem ser validados pelo `SdhValidator.ValidateOnly` antes de serem persistidos por qualquer fluxo futuro.
- O banco nao deve aceitar comandos que nao existam no catalogo SDH exportado.
- Nenhum comando SDH deve ser executado diretamente a partir do banco nesta etapa.
