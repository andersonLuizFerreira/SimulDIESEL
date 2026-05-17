# Dump - Padronizacao definitiva SDCTP

Data: 2026-05-12

## Objetivo

Eliminar referencias incorretas a grafias digitadas incorretamente do protocolo de transporte CAN e padronizar exclusivamente:

`SDCTP` = SimulDIESEL CAN Transport Protocol

Esta ETAPA nao trata grafias incorretas como alias, termo historico, divergencia valida ou compatibilidade nomenclatural.

## Arquivos corrigidos

- `.agents/README.md`
- `.agents/skills/simuldiesel-architecture/SKILL.md`
- `.agents/skills/sdh-contract/SKILL.md`
- `.agents/skills/simuldiesel-architecture/SKILL.md`
- `.agents/skills/sdh-contract/SKILL.md`
- `out/dumps/agents_skills_prompts_creation.md`
- `out/dumps/sdh_sdctp_architecture_current_state.md`

## Arquivos renomeados

- O dump arquitetural anterior com grafia incorreta no nome foi renomeado para `out/dumps/sdh_sdctp_architecture_current_state.md`.

## Ocorrencias removidas

- Removida de `.agents/README.md` a instrucao que tratava a grafia incorreta como divergencia ou erro historico a avaliar.
- Removidos de skills humanas e estruturadas os riscos que citavam a grafia incorreta diretamente.
- Removida de `out/dumps/agents_skills_prompts_creation.md` a observacao que registrava a grafia incorreta como divergencia historica ou erro a confirmar.
- Corrigido o titulo, escopo e diagnostico do dump arquitetural para `SDCTP`.
- Removidas perguntas pendentes sobre escolher entre grafias. A terminologia oficial agora esta registrada diretamente como `SDCTP`.

## Locais onde nomes incorretos ainda existem em codigo

Nenhum local encontrado.

Nao foram encontradas ocorrencias em namespaces, classes ou arquivos compilados durante a busca textual final.

## Justificativa da padronizacao

A unica nomenclatura oficial do protocolo e:

`SDCTP` = SimulDIESEL CAN Transport Protocol

As demais grafias eram apenas erros de digitacao em documentacao/prompts anteriores e foram eliminadas, sem tratamento como alias ou variante aceitavel.

## Confirmacao de escopo

- Nenhum codigo C# funcional foi alterado.
- Nenhum firmware funcional foi alterado.
- Nenhum banco de dados foi alterado.
- Nenhuma UI foi alterada.
- Nenhum namespace, classe ou arquivo compilado foi renomeado.
- Nenhum comportamento de protocolo foi alterado.
- Nenhum commit, branch ou tag foi criado.

## Validacao executada

- Busca textual completa com arquivos ocultos:
  - padrao: grafias incorretas em maiusculas.
  - resultado: sem ocorrencias.
- Busca textual case-insensitive com arquivos ocultos:
  - padrao: grafias incorretas em qualquer combinacao de caixa.
  - resultado: sem ocorrencias.
- Busca textual para a nomenclatura oficial:
  - `SDCTP` encontrado de forma consistente em documentacao ativa, skills, estrutura `.agents` e dumps.
- `git status --short` executado apos as correcoes.

## Build

Build C# e PlatformIO nao foram executados, pois a ETAPA alterou apenas documentacao e dumps Markdown.
