# Dump - Reforco das regras de governanca em AGENTS.md e instructions.md

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para reforcar as regras de governanca contra alucinacao, extrapolacao de escopo e declaracao indevida de sucesso nos arquivos principais de bootstrap CODEX.

## Escopo executado

- `AGENTS.md`
- `.codex/instructions.md`
- `out/dumps/agents_bootstrap_governance_adjustments.md`

## Fora de escopo preservado

- Codigo funcional.
- Firmware.
- Banco.
- UI.
- Contratos SDH, SDGW e SDCTP.
- Skills.
- Branch, commit e tag.

## Arquivos alterados

- `AGENTS.md`
- `.codex/instructions.md`

## Arquivos criados

- `out/dumps/agents_bootstrap_governance_adjustments.md`

## Regras adicionadas

### AGENTS.md

- Nunca declarar uma implementacao como concluida sem validacao compativel com o escopo.
- Criada a secao `Regras de consistencia`.
- Reforcado que nao se deve inventar arquivos, APIs, endpoints, suporte de protocolo ou comportamento nao validado.
- Reforcado que duvidas devem ser registradas como `pendente de confirmacao`.
- Reforcado que algo nao deve ser promovido automaticamente de `PLANEJADO` para `IMPLEMENTADO` sem evidencia concreta no codigo, teste, build ou validacao aplicavel.

### .codex/instructions.md

- Nunca pular a leitura de `AGENTS.md` antes de executar uma ETAPA.
- Antes de agir, identificar e carregar a skill mais relevante em `.codex/skills/`.
- Nao expandir automaticamente o escopo da ETAPA.

## Motivo dos ajustes

Os ajustes reduzem risco de alucinacao documental, inferencia sem evidencia, ampliacao indevida de escopo e declaracao de conclusao sem validacao adequada.

## Validacao executada

- Conferir que somente `AGENTS.md`, `.codex/instructions.md` e este dump foram alterados nesta ETAPA.
- Conferir que `AGENTS.md` permanece coerente e sem duplicacao excessiva.
- Conferir que `.codex/instructions.md` permanece curto e funcionando como bootstrap.
- Buscar referencias de bootstrap para confirmar que `.codex/instructions.md` e o unico bootstrap citado.
- Buscar orientacao para uso do arquivo sem extensao no diretorio `.codex`.
- Executar `git status --short`.

## Resultado da validacao

- `AGENTS.md` permanece coerente: as regras novas foram inseridas em escopo, consistencia e validacao.
- `.codex/instructions.md` permanece curto e continua funcionando como bootstrap.
- A busca pelo caminho sem extensao no diretorio `.codex` nao retornou ocorrencias.
- As referencias de bootstrap encontradas apontam para `.codex/instructions.md`.
- `git status --short` foi executado.
- Observacao de worktree: alem dos arquivos desta ETAPA, o status ainda mostra alteracoes documentais anteriores relacionadas a consolidacao de `.codex/instructions.md`, incluindo a remocao do arquivo sem extensao no diretorio `.codex`, `docs/agents/agents_overview.md`, `out/dumps/codex_agents_skills_bootstrap.md` e `out/dumps/instructions_md_consolidation.md`.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Build C# nao executado: nao aplicavel a ETAPA exclusivamente documental.
- PlatformIO nao executado: nao aplicavel, pois firmware ficou fora de escopo.
- Validadores de banco nao executados: nao aplicavel, pois banco ficou fora de escopo.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
