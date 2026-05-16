# ETAPA - Governanca Git e remocao de artefatos runtime

- Data: 2026-05-16
- Tema: Governanca Git do SimulDIESEL: correcao de arquivos versionados, `.gitignore`, comissionamento global e push.
- Objetivo: aplicar a politica oficial de versionamento definida a partir de `out/dumps/git_repository_audit.md`.
- Escopo permitido: `.gitignore`, documentacao oficial de fluxo Git, dump tecnico, remocao de rastreamento sem apagar arquivos locais, avaliacao de `FrmRedeCan.resx` e captura J1939 textual.
- Fora de escopo: reescrever historico, `git gc`, `git filter-repo`, alterar arquitetura, alterar banco/migrations, limpar workspace ou apagar capturas.

## Status Git antes

```text
## feature/j1939-reference-catalogs
 M Data/Modules/modules.db
?? local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx
?? out/dumps/git_repository_audit.md
?? "tests/Leitura de alarmes/3 - somente motor.md"
```

## Arquivos removidos do rastreamento

Remocao feita com `git rm --cached`, preservando arquivos fisicos locais:

- `Data/Modules/modules.db`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite`

Validacao:

```text
git ls-files Data/Modules/modules.db
git ls-files "hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite"
```

Resultado: ambos retornaram vazio.

Existencia local:

```text
Data/Modules/modules.db -> True
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite -> True
```

## Regras adicionadas ao `.gitignore`

- Capturas CAN/J1939 binarias ou brutas: `*.blf`, `*.asc`, `*.trc`, `*.pcap`, `*.pcapng`, `*.candump`.
- Cache de IDE: `**/visual-studio-cache/*.sqlite`, `**/visual-studio-cache/*.db`.
- Banco local runtime: `Data/Modules/*.db`, `Data/Modules/*.sqlite`, `Data/Modules/*.sqlite3`, `Data/Modules/*.db-journal`, `Data/Modules/*.db-wal`, `Data/Modules/*.db-shm`.
- Auxiliares SQLite gerais: `**/*.sqlite-wal`, `**/*.sqlite-shm`, `**/*.db-wal`, `**/*.db-shm`.

Validacao:

```text
.gitignore:152:Data/Modules/*.db Data/Modules/modules.db
.gitignore:48:**/visual-studio-cache/*.sqlite hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite
```

## Fontes oficiais preservadas

Continuam rastreados:

```text
Data/Modules/schema/migrations/0002_sync_metadata.sql
Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql
Data/Modules/schema/postgres_schema_v1.sql
Data/Modules/schema/sqlite_schema_v1.sql
Data/Protocols/J1939/catalogs/j1939_functions.json
Data/Protocols/J1939/catalogs/j1939_industry_groups.json
Data/Protocols/J1939/catalogs/j1939_manufacturers.json
Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json
Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json
Data/Protocols/J1939/j1939-71-mini-catalog.json
Data/Protocols/J1939/j1939-pgn-standard-catalog.json
```

Decisao: banco runtime nao e fonte de verdade. A fonte de verdade do Banco de Modulos passa a ser schema, migrations e seeds/catalogs versionados.

## Arquivos avaliados

### `local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx`

Decisao: versionar.

Evidencia:

- `FrmRedeCan.cs` e um `Form` real.
- `SimulDIESEL.csproj` ja possuia `Compile Include="UI\FrmRedeCan.cs"` com `SubType` de Form.
- O `.resx` e o recurso WinForms correspondente e foi incluido no `.csproj` como `EmbeddedResource` com `DependentUpon>FrmRedeCan.cs</DependentUpon>`.
- Build processou `UI\FrmRedeCan.resx` em `obj\Debug\SimulDIESEL.UI.FrmRedeCan.resources`.

### `tests/Leitura de alarmes/3 - somente motor.md`

Decisao: versionar como evidencia tecnica textual controlada.

Evidencia:

- Contem sessao J1939/CAN com resumo por PGN, source address, address claim e periodicidade.
- Arquivo e Markdown textual, nao binario.
- Tamanho aproximado: 601,37 KiB. Classificacao: evidencia tecnica versionavel com risco de crescimento; futuras capturas grandes devem justificar curadoria.

### `out/dumps/git_repository_audit.md`

Decisao: versionar.

Motivo: dump tecnico autorizado da auditoria anterior em `out/dumps/**`.

## Documentacao oficial atualizada

Arquivo:

- `docs/official/09-desenvolvimento/03-fluxo-git.md`

Conteudo consolidado:

- o que deve ser versionado;
- o que nao deve ser versionado;
- politica para banco local;
- politica para dumps;
- politica para capturas CAN/J1939;
- politica para caches e builds;
- regra de que banco runtime nao e fonte de verdade;
- regra de que a fonte de verdade do banco e schema + migrations + seeds/catalogs.

## Validacoes executadas

### Status Git

Comando:

```text
git status --short
```

Resultado apos remocao do rastreamento e antes do stage final:

```text
 M .gitignore
D  Data/Modules/modules.db
 M docs/official/09-desenvolvimento/03-fluxo-git.md
D  "hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite"
 M local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj
?? local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx
?? out/dumps/git_repository_audit.md
?? "tests/Leitura de alarmes/3 - somente motor.md"
```

### Build completo local-api

Comando:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-git-versioning-governance/
```

Resultado:

```text
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

Observacao: uma primeira tentativa dentro do sandbox falhou por acesso negado ao SDK local em `C:\Users\Escritorio\AppData\Local\Microsoft SDKs`. O build foi reexecutado fora do sandbox com aprovacao e passou.

### Smoke minimo do banco

Execucao:

- Usou as classes reais `SqliteConnectionFactory`, `SqliteMigrationRunner`, `J1939CatalogSeedImporter` e `SqliteDatabaseInitializer`.
- Banco temporario: `out/validation-git-versioning-governance/runtime-smoke.db`.
- O banco local `Data/Modules/modules.db` nao foi apagado nem usado como fonte obrigatoria.

Resultado:

```text
DatabasePath      : G:\PROJETOS\SIMULADORES\SimulDIESEL\out\validation-git-versioning-governance\runtime-smoke.db
Exists            : True
AppliedMigrations : 0001_sqlite_schema_v1,0002_sync_metadata,0003_j1939_reference_catalogs
MigrationCount    : 3
ManufacturerCount : 26
```

### `.gitignore`

Comando:

```text
git check-ignore -v Data/Modules/modules.db
git check-ignore -v "hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite"
```

Resultado: ambos ignorados pelas novas regras.

### Staged parcial antes do stage final

Comando:

```text
git diff --cached --stat
```

Resultado naquele ponto:

```text
Data/Modules/modules.db                           | Bin 348160 -> 0 bytes
.../artifacts/ide/visual-studio-cache/slnx.sqlite | Bin 278528 -> 0 bytes
2 files changed, 0 insertions(+), 0 deletions(-)
```

## Warnings e observacoes

- `git diff --stat` emitiu avisos de conversao futura LF -> CRLF em `.gitignore` e `SimulDIESEL.csproj`; nao sao warnings de build.
- Nenhum warning novo de compilacao foi aceito; build passou com 0 avisos.
- Outputs criados em `out/build-git-versioning-governance/` e `out/validation-git-versioning-governance/` permanecem ignorados.

## Rollback

Rollback preservado:

- arquivos fisicos removidos do rastreamento continuam locais;
- mudancas sao pequenas e rastreaveis;
- nenhuma limpeza destrutiva foi executada;
- historico Git nao foi reescrito.
