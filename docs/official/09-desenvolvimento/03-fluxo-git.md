⬅ [Retornar para Padrões de Código](02-padroes-codigo.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Fluxo Git

## Política oficial de versionamento

O Git do SimulDIESEL deve preservar fontes oficiais, contratos, documentação e evidências técnicas controladas. Artefatos de runtime, build, cache e bancos locais mutáveis não são fonte de verdade do projeto.

## Deve ser versionado

- Código fonte da aplicação local, firmware, ferramentas e testes automatizados.
- Arquivos de projeto necessários ao build, incluindo `.sln`, `.csproj`, `platformio.ini`, recursos reais da aplicação e arquivos WinForms associados.
- `.agents/README.md` e `.agents/skills/**`.
- Documentacao consolidada em `docs/` e governanca oficial de agentes em `.agents/`.
- Schemas SQL, migrations SQL e seeds/catalogs JSON.
- Catálogos J1939 versionados em `Data/Protocols/J1939/catalogs/**` e JSONs oficiais de protocolo.
- Dumps técnicos Markdown autorizados em `out/dumps/**`.
- Testes automatizados e evidências técnicas pequenas ou curadas, quando úteis para reprodução, engenharia reversa ou validação.

## Não deve ser versionado

- Bancos runtime mutáveis, incluindo `Data/Modules/*.db`, `*.sqlite` e arquivos auxiliares WAL/SHM/journal.
- Caches SQLite de IDE, especialmente `visual-studio-cache`.
- `bin/`, `obj/`, `.vs/`, `.vscode/`, `.pio/`, builds e outputs de validação runtime.
- Logs, temporários, caches e arquivos gerados automaticamente.
- Dumps binários e capturas CAN/J1939 brutas em formatos binários ou de ferramenta, salvo decisão explícita de governança.
- Pacotes de dependência locais como `node_modules/`, ambientes virtuais Python e caches equivalentes.

## Política do Banco de Módulos

O banco local `Data/Modules/modules.db` é runtime mutável. Ele pode existir no workspace do desenvolvedor, mas não deve ser rastreado pelo Git.

A fonte de verdade do Banco de Módulos é composta por:

- `Data/Modules/schema/sqlite_schema_v1.sql`;
- `Data/Modules/schema/postgres_schema_v1.sql`;
- `Data/Modules/schema/migrations/**`;
- `Data/Protocols/J1939/catalogs/**`;
- demais seeds e catalogs JSON versionados.

Mudanças de estrutura devem ocorrer por schema/migration versionada. Dados operacionais locais devem ser recriados ou inicializados a partir dessas fontes versionadas.

## Política para dumps

`out/dumps/**` é a área autorizada para dumps técnicos textuais de ETAPA. Esses arquivos devem ser preferencialmente Markdown, JSON ou texto auditável.

Builds, saídas de compilação, bancos temporários e validações runtime devem permanecer fora do Git, mesmo quando estiverem em `out/`.

## Política para capturas CAN/J1939

Capturas pequenas, curadas e textuais podem ser versionadas como evidência técnica quando forem úteis para reprodução ou engenharia reversa.

Capturas binárias ou brutas em formatos como `.blf`, `.asc`, `.trc`, `.pcap`, `.pcapng` e `.candump` não devem entrar no Git por padrão. Quando uma captura grande for necessária, a ETAPA deve justificar a curadoria e registrar a decisão no dump correspondente.

## Política para caches e builds

Caches de IDE, PlatformIO, Visual Studio, VS Code, outputs C# e builds em `out/build-*` são gerados automaticamente. Eles não representam estado oficial do projeto e não devem ser rastreados.

Se um arquivo gerado já estiver rastreado, a correção deve ser feita em ETAPA própria com `git rm --cached`, preservando o arquivo local quando ele for útil ao desenvolvedor.

## Comissionamento de mudanças

Antes de commit de governança Git, a ETAPA deve conferir:

- `git status --short`;
- arquivos removidos do rastreamento sem exclusão local;
- `git check-ignore -v` para os padrões novos;
- build aplicável quando houver impacto no ambiente de desenvolvimento;
- ausência de build/cache no staged;
- dump técnico em `out/dumps/**` com decisões e validações.

## Limitações

Esta política não reescreve histórico Git e não remove blobs antigos já presentes em packs. Auditoria de histórico, `git filter-repo` ou limpeza agressiva exigem ETAPA separada e autorização explícita.

## Glossário

- **Fonte de verdade**: arquivo ou conjunto de arquivos versionados que permitem reconstruir o estado oficial.
- **Runtime mutável**: arquivo alterado pela execução local da aplicação ou ferramentas.
- **Dump técnico**: registro textual de ETAPA usado para auditoria e rastreabilidade.
- **Evidência técnica**: captura ou saída curada que ajuda a reproduzir comportamento observado.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
