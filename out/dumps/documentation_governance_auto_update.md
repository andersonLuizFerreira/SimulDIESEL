# Dump - Governanca automatica de documentacao ao concluir ETAPAS

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para tornar obrigatoria a revisao e atualizacao da documentacao oficial sempre que uma ETAPA for concluida.

Objetivo: transformar `/docs/` em documentacao viva, sincronizada com o estado real consolidado do projeto.

## Escopo executado

- `AGENTS.md`
- `.codex/instructions.md`
- `.codex/skills/simuldiesel-architecture/SKILL.md`
- `docs/agents/project_conventions.md`
- `docs/agents/validation_checklist.md`
- `out/dumps/documentation_governance_auto_update.md`

## Fora de escopo preservado

- Codigo funcional.
- Firmware.
- Banco.
- UI.
- Contratos SDH, SDGW e SDCTP.
- Logica do sistema.
- Branch, commit e tag.

## Arquivos alterados

- `AGENTS.md`
- `.codex/instructions.md`
- `.codex/skills/simuldiesel-architecture/SKILL.md`
- `docs/agents/project_conventions.md`
- `docs/agents/validation_checklist.md`

## Arquivos criados

- `out/dumps/documentation_governance_auto_update.md`

## Regras adicionadas

- `AGENTS.md`: criada a secao `Atualizacao obrigatoria de documentacao`, determinando que ETAPAS concluidas devem revisar documentacao impactada, atualizar `/docs/`, preservar coerencia estrutural, refletir estado real e registrar mudancas arquiteturais, contratos, fluxos ou limitacoes.
- `.codex/instructions.md`: adicionada regra curta indicando que ETAPAS concluidas exigem atualizacao da documentacao oficial impactada em `/docs/`.
- `.codex/skills/simuldiesel-architecture/SKILL.md`: adicionada regra para revisar `/docs/official/` e documentos de arquitetura relacionados ao concluir ETAPAS arquiteturais ou funcionais.
- `docs/agents/project_conventions.md`: adicionada a secao `Documentacao viva`, separando `/docs/` como estado consolidado e dumps como historico/auditoria.
- `docs/agents/validation_checklist.md`: adicionados itens para validar revisao/atualizacao da documentacao oficial impactada.

## Objetivo da governanca documental

- Evitar que ETAPAS concluam deixando `/docs/` divergente do estado real.
- Impedir que dumps sejam usados como substituto da documentacao oficial.
- Tornar a revisao documental parte obrigatoria da entrega.
- Manter arquitetura, contratos, fluxos, estados e limitacoes sincronizados com a implementacao consolidada.

## Impacto esperado no fluxo de ETAPAS

- Ao concluir uma ETAPA, o agente deve identificar documentos impactados.
- Se houver impacto consolidado, os arquivos correspondentes em `/docs/` devem ser atualizados dentro do escopo autorizado.
- Se nao houver documentacao oficial impactada, isso deve ser registrado na entrega/dump.
- Dumps continuam servindo como registro historico, mas nao como fonte unica da arquitetura atual.

## Validacao executada

- Conferir que apenas arquivos documentais foram alterados.
- Conferir coerencia entre `AGENTS.md`, `.codex/instructions.md`, skill de arquitetura e convencoes de projeto.
- Conferir que nao houve duplicacao excessiva nem conflito de governanca.
- Executar `git status --short`.
- Builds nao executados por nao se aplicarem a ETAPA exclusivamente documental.

## Resultado da validacao

- Regras novas localizadas em `AGENTS.md`, `.codex/instructions.md`, `.codex/skills/simuldiesel-architecture/SKILL.md`, `docs/agents/project_conventions.md` e `docs/agents/validation_checklist.md`.
- `AGENTS.md` centraliza a regra completa; `.codex/instructions.md` mantem apenas a regra curta de bootstrap.
- A skill de arquitetura reforca revisao de `/docs/official/` e documentos de arquitetura relacionados.
- `docs/agents/project_conventions.md` diferencia `/docs/` como estado consolidado e dumps como historico/auditoria.
- `docs/agents/validation_checklist.md` inclui item obrigatorio de revisao/atualizacao documental.
- `git status --short` executado.
- Observacao de worktree: alem desta ETAPA, o status ainda mostra alteracoes documentais anteriores relacionadas a consolidacao do bootstrap CODEX e ajustes de governanca ja realizados.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Firmware, banco, UI, contratos SDH/SDGW/SDCTP e logica do sistema nao foram alterados.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
