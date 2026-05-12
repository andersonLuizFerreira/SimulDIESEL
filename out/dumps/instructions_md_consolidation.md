# Dump - Consolidacao definitiva do bootstrap CODEX usando instructions.md

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para eliminar a ambiguidade entre o arquivo sem extensao no diretorio `.codex` e `.codex/instructions.md`.

Objetivo: consolidar o bootstrap CODEX oficial usando somente `.codex/instructions.md`.

## Escopo executado

- Documentacao de bootstrap CODEX.
- `AGENTS.md`.
- `.codex/instructions.md`.
- `docs/agents/`.
- Dumps com registro historico da duplicidade.

## Fora de escopo preservado

- Codigo funcional.
- Firmware.
- Banco.
- UI.
- Contratos SDH, SDGW e SDCTP.
- Scripts automatizados.
- Branch, commit e tag.

## Arquivos alterados

- `AGENTS.md`
- `.codex/instructions.md`
- `docs/agents/agents_overview.md`
- `out/dumps/codex_agents_skills_bootstrap.md`

## Arquivos removidos

- Arquivo sem extensao no diretorio `.codex`.

## Arquivos criados

- `out/dumps/instructions_md_consolidation.md`

## Referencias corrigidas

- `AGENTS.md`: passou a declarar o fluxo oficial CODEX:
  1. `AGENTS.md`
  2. `.codex/instructions.md`
  3. `.codex/skills/`
  4. `docs/agents/`
- `.codex/instructions.md`: passou a se declarar como arquivo oficial de bootstrap CODEX e a listar a ordem oficial.
- `docs/agents/agents_overview.md`: passou a incluir `.codex/instructions.md` no fluxo de leitura.
- `out/dumps/codex_agents_skills_bootstrap.md`: o registro historico da duplicidade foi marcado como supersedido, sem manter instrucao ativa de uso do arquivo sem extensao.

## Confirmacao da remocao

- O arquivo sem extensao no diretorio `.codex` foi removido.
- `.codex/instructions.md` foi preservado como bootstrap oficial.

## Referencias restantes

- Referencias validas a `.codex/instructions.md` permanecem em documentacao ativa.
- Nenhuma referencia ativa ao arquivo sem extensao deve permanecer.
- O termo `instructions` ainda pode aparecer como palavra generica ou como parte de `.codex/instructions.md`.

## Riscos encontrados

- Nao foram encontrados scripts automatizados ou integracoes reais apontando para o arquivo sem extensao no diretorio `.codex`.
- O dump `out/dumps/codex_agents_skills_bootstrap.md` continha registro historico da duplicidade; foi atualizado para indicar que essa decisao foi supersedida.

## Validacao

- Busca textual inicial identificou a duplicidade fisica no diretorio `.codex`.
- Busca textual inicial identificou referencias historicas de duplicidade em `out/dumps/codex_agents_skills_bootstrap.md`.
- Confirmado que o arquivo sem extensao no diretorio `.codex` nao existe: `Test-Path` retornou `False`.
- Confirmado que `.codex/instructions.md` continua integro e legivel.
- Busca textual final para o caminho sem extensao nao encontrou ocorrencias.
- `git status --short` executado ao final da ETAPA; mostrou a remocao do arquivo sem extensao no diretorio `.codex`, quatro arquivos documentais modificados e este dump novo ainda nao rastreado.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Build C# nao executado: nao aplicavel a ETAPA exclusivamente documental.
- PlatformIO nao executado: nao aplicavel, pois firmware ficou fora de escopo.
- Validadores de banco nao executados: nao aplicavel, pois banco ficou fora de escopo.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
