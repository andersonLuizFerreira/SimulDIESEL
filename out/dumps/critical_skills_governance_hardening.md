# Dump - Reforco das skills criticas de arquitetura, dumps e Git

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para reforcar as skills criticas do CODEX contra leitura otimista do estado real, mistura entre documentacao/codigo/build/validacao, dumps sem evidencia e operacoes Git destrutivas ou acidentais.

## Escopo executado

- `.codex/skills/simuldiesel-architecture/SKILL.md`
- `.codex/skills/dump-generation/SKILL.md`
- `.codex/skills/git-checkpoint/SKILL.md`
- `docs/agents/skills/simuldiesel-architecture-skill.md`
- `docs/agents/skills/dump-generation-skill.md`
- `docs/agents/skills/git-checkpoint-skill.md`
- `out/dumps/critical_skills_governance_hardening.md`

## Fora de escopo preservado

- Codigo funcional.
- Firmware.
- Banco.
- UI.
- Contratos SDH, SDGW e SDCTP.
- Branch, commit e tag.
- Comandos Git destrutivos, force push e rebase.

## Arquivos alterados

- `.codex/skills/simuldiesel-architecture/SKILL.md`
- `.codex/skills/dump-generation/SKILL.md`
- `.codex/skills/git-checkpoint/SKILL.md`
- `docs/agents/skills/simuldiesel-architecture-skill.md`
- `docs/agents/skills/dump-generation-skill.md`
- `docs/agents/skills/git-checkpoint-skill.md`

## Arquivos criados

- `out/dumps/critical_skills_governance_hardening.md`

## Regras adicionadas por skill

### SimulDIESEL Architecture

- Nao inferir implementacao apenas por documentacao, nomes de arquivos, classes, namespaces ou estrutura de pasta.
- Diferenciar codigo existente, codigo compilavel, codigo validado e codigo operacional em bancada.
- Nao promover implementacoes experimentais, fake, mock ou bench-only para estado oficial sem validacao explicita.
- Registrar limitacoes e incertezas como `pendente de confirmacao` quando nao houver evidencia suficiente.
- Separar fato observado de inferencia ao analisar arquitetura.

### Dump Generation

- Diferenciar `nao implementado`, `implementado`, `compilado`, `validado`, `validado parcialmente` e `validado em bancada`.
- Registrar evidencias sempre que possivel: comandos executados, resultado de build, scripts usados, mensagens relevantes e observacoes verificadas.
- Registrar impacto em camadas, contratos e fluxos nos dumps de ETAPAS arquiteturais.
- Nao transformar inferencia em fato.
- Nao declarar validacao completa quando houve apenas validacao parcial ou documental.

### Git Checkpoint

- Proibir sem autorizacao clara: `git clean -fd`, `git checkout .`, `git restore .`, `git reset --hard`, `git push --force` e rebase destrutivo.
- Revisar staged, unstaged e untracked antes de commit.
- Confirmar explicitamente o branch alvo antes de merge ou push.
- Nunca assumir que alteracoes existentes no worktree pertencem a ETAPA atual.
- Separar alteracoes preexistentes das alteracoes da ETAPA.
- Nao incluir alteracoes funcionais preexistentes em commit documental sem autorizacao explicita.

## Motivo dos ajustes

- Reduzir risco de declarar estado oficial sem evidencia.
- Separar documentacao, codigo existente, codigo compilavel, validacao real e validacao em bancada.
- Fazer dumps servirem como registro de evidencias e nao como narrativa conclusiva.
- Proteger rollback e evitar perda ou inclusao acidental de trabalho preexistente.

## Validacao executada

- Conferir que somente arquivos permitidos e este dump foram alterados nesta ETAPA.
- Conferir que nao houve alteracao em codigo funcional, firmware, banco, UI ou contratos SDH/SDGW/SDCTP.
- Conferir que as tres skills estruturadas mantem os cabecalhos obrigatorios.
- Executar `git status --short`.
- Builds nao executados por nao se aplicarem a ETAPA exclusivamente documental.

## Resultado da validacao

- As tres skills estruturadas mantem os cabecalhos obrigatorios: `Nome`, `Objetivo`, `Quando usar`, `Quando nao usar`, `Escopo permitido`, `Escopo proibido`, `Arquivos/pastas provaveis`, `Checklist de validacao`, `Checklist de entrega`, `Riscos comuns` e `Regras de nao regressao`.
- Busca textual confirmou as regras novas sobre evidencia arquitetural, estados de validacao, comandos Git destrutivos e revisao de staged/unstaged/untracked.
- `git status --short` foi executado.
- Observacao de worktree: alem desta ETAPA, o status ainda mostra alteracoes documentais anteriores de ETAPAS recentes. Elas nao foram revertidas nem incluidas em commit.

## Divergencias ou pontos pendentes

- O worktree ja continha alteracoes documentais anteriores de ETAPAS recentes; elas nao foram revertidas nem consolidadas nesta ETAPA.
- Nao ha pendencia tecnica identificada nas tres skills reforcadas.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Firmware, banco, UI, contratos SDH/SDGW/SDCTP e logica do sistema nao foram alterados.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
