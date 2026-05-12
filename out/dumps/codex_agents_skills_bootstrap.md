# Dump - Bootstrap CODEX com AGENTS.md e Skills

Data: 2026-05-12

Escopo: atualizar a estrutura `.codex` do projeto SimulDIESEL para usar `AGENTS.md`, `docs/agents/` e skills estruturadas em `.codex/skills/` como referencias oficiais para o CODEX.

## Arquivos alterados

- `.codex/instructions`
- `.codex/instructions.md`

Observacao: a pasta `.codex` possuia `instructions.md`, mas nao possuia `.codex/instructions`. Para cumprir o pedido literalmente e manter compatibilidade com a estrutura existente, foram mantidos os dois arquivos com o mesmo bootstrap curto.

## Arquivos criados

### Indice de skills CODEX

- `.codex/skills/README.md`

### Skills estruturadas

- `.codex/skills/simuldiesel-architecture/SKILL.md`
- `.codex/skills/winforms-ui/SKILL.md`
- `.codex/skills/bll-dal-dtl/SKILL.md`
- `.codex/skills/sdh-contract/SKILL.md`
- `.codex/skills/sdctp-contract/SKILL.md`
- `.codex/skills/sdgw-transport/SKILL.md`
- `.codex/skills/j1939-decode/SKILL.md`
- `.codex/skills/module-database/SKILL.md`
- `.codex/skills/firmware-uce/SKILL.md`
- `.codex/skills/firmware-bpm/SKILL.md`
- `.codex/skills/git-checkpoint/SKILL.md`
- `.codex/skills/build-validation/SKILL.md`
- `.codex/skills/dump-generation/SKILL.md`

### AGENTS locais documentais

- `local-api/AGENTS.md`
- `hardware/AGENTS.md`
- `Data/AGENTS.md`

### Dump

- `out/dumps/codex_agents_skills_bootstrap.md`

## Resumo da nova estrutura

- `.codex/instructions` agora e um bootstrap curto que aponta para `AGENTS.md`, `docs/agents/`, `docs/agents/skills/` e `.codex/skills/`.
- `.codex/instructions.md` foi alinhado ao mesmo conteudo por compatibilidade, pois era o arquivo existente antes desta ETAPA.
- `.codex/skills/README.md` lista as 13 skills disponiveis, a documentacao humana equivalente e quando usar cada uma.
- Cada skill em `.codex/skills/<nome>/SKILL.md` preserva o conteudo tecnico da documentacao humana em `docs/agents/skills/*.md`, com cabecalho estruturado para uso do CODEX.
- `local-api/AGENTS.md`, `hardware/AGENTS.md` e `Data/AGENTS.md` foram criados como ponte curta para as regras principais e skills relevantes, sem duplicar conteudo extenso.

## Confirmacao de escopo

- Codigo C# funcional nao foi alterado.
- Firmware nao foi alterado.
- Banco de dados nao foi alterado.
- UI nao foi alterada.
- Contratos SDH, SDGW e SDCTP nao foram alterados.
- Documentacao existente em `docs/agents/skills/` nao foi apagada nem substituida.
- Nenhum commit, branch ou tag foi criado.

## Divergencias ou incertezas encontradas

- O pedido citou `.codex/instructions`, mas a estrutura existente continha `.codex/instructions.md`. Foram mantidos ambos, com conteudo equivalente, para atender o caminho solicitado e preservar a convencao ja presente.
- As skills estruturadas foram criadas manualmente a partir das skills humanas existentes; a fonte humana oficial continua em `docs/agents/skills/`.

## Validacao executada

- Confirmado que `.codex/instructions` existe.
- Confirmado que `.codex/skills/README.md` existe.
- Confirmado que existem 13 pastas de skills em `.codex/skills/`.
- Confirmado que cada pasta possui `SKILL.md`.
- Confirmado que os 13 `SKILL.md` possuem os cabecalhos obrigatorios: Nome, Objetivo, Quando usar, Quando nao usar, Escopo permitido, Escopo proibido, Arquivos/pastas provaveis, Checklist de validacao, Checklist de entrega, Riscos comuns e Regras de nao regressao.
- Confirmado que nao houve alteracao intencional em codigo funcional, firmware, banco, UI ou contratos.
- `git status --short` mostra `.codex/instructions.md` modificado e novos arquivos documentais desta ETAPA (`.codex/instructions`, `.codex/skills/`, `Data/AGENTS.md`, `hardware/AGENTS.md`, `local-api/AGENTS.md`, este dump), alem das alteracoes funcionais preexistentes no worktree.

## Validacoes nao executadas

- Build C# nao executado: nao aplicavel, pois nao houve alteracao de codigo funcional.
- PlatformIO nao executado: nao aplicavel, pois nao houve alteracao de firmware.
- Validadores de banco nao executados: nao aplicavel, pois nao houve alteracao de schema ou dados.
