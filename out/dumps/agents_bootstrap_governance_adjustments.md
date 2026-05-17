# Dump - Reforco das regras de governanca em .agents/README.md e .agents/README.md

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para reforcar as regras de governanca contra alucinacao, extrapolacao de escopo e declaracao indevida de sucesso nos arquivos principais de bootstrap CODEX.

## Escopo executado

- `.agents/README.md`
- `.agents/README.md`
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

- `.agents/README.md`
- `.agents/README.md`

## Arquivos criados

- `out/dumps/agents_bootstrap_governance_adjustments.md`

## Regras adicionadas

### .agents/README.md

- Nunca declarar uma implementacao como concluida sem validacao compativel com o escopo.
- Criada a secao `Regras de consistencia`.
- Reforcado que nao se deve inventar arquivos, APIs, endpoints, suporte de protocolo ou comportamento nao validado.
- Reforcado que duvidas devem ser registradas como `pendente de confirmacao`.
- Reforcado que algo nao deve ser promovido automaticamente de `PLANEJADO` para `IMPLEMENTADO` sem evidencia concreta no codigo, teste, build ou validacao aplicavel.

### .agents/README.md

- Nunca pular a leitura de `.agents/README.md` antes de executar uma ETAPA.
- Antes de agir, identificar e carregar a skill mais relevante em `.agents/skills/`.
- Nao expandir automaticamente o escopo da ETAPA.

## Motivo dos ajustes

Os ajustes reduzem risco de alucinacao documental, inferencia sem evidencia, ampliacao indevida de escopo e declaracao de conclusao sem validacao adequada.

## Validacao executada

- Conferir que somente `.agents/README.md`, `.agents/README.md` e este dump foram alterados nesta ETAPA.
- Conferir que `.agents/README.md` permanece coerente e sem duplicacao excessiva.
- Conferir que `.agents/README.md` permanece curto e funcionando como bootstrap.
- Buscar referencias de bootstrap para confirmar que `.agents/README.md` e o unico bootstrap citado.
- Buscar orientacao para uso do arquivo sem extensao no diretorio `.agents`.
- Executar `git status --short`.

## Resultado da validacao

- `.agents/README.md` permanece coerente: as regras novas foram inseridas em escopo, consistencia e validacao.
- `.agents/README.md` permanece curto e continua funcionando como bootstrap.
- A busca pelo caminho sem extensao no diretorio `.agents` nao retornou ocorrencias.
- As referencias de bootstrap encontradas apontam para `.agents/README.md`.
- `git status --short` foi executado.
- Observacao de worktree: alem dos arquivos desta ETAPA, o status ainda mostra alteracoes documentais anteriores relacionadas a consolidacao de `.agents/README.md`, incluindo a remocao do arquivo sem extensao no diretorio `.agents`, `.agents/README.md`, `out/dumps/codex_agents_skills_bootstrap.md` e `out/dumps/instructions_md_consolidation.md`.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Build C# nao executado: nao aplicavel a ETAPA exclusivamente documental.
- PlatformIO nao executado: nao aplicavel, pois firmware ficou fora de escopo.
- Validadores de banco nao executados: nao aplicavel, pois banco ficou fora de escopo.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
