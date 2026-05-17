# Dump - Bootstrap CODEX com .agents/README.md e Skills

Data: 2026-05-12

Escopo: atualizar a estrutura `.agents` do projeto SimulDIESEL para usar `.agents/README.md`, `.agents/` e skills estruturadas em `.agents/skills/` como referencias oficiais para o CODEX.

## Arquivos alterados

- Arquivo sem extensao no diretorio `.agents`
- `.agents/README.md`

Observacao supersedida em 2026-05-12 pela ETAPA de consolidacao definitiva do bootstrap CODEX usando apenas `.agents/README.md`. O registro anterior nao e mais valido.

## Arquivos criados

### Indice de skills CODEX

- `.agents/skills/README.md`

### Skills estruturadas

- `.agents/skills/simuldiesel-architecture/SKILL.md`
- `.agents/skills/winforms-ui/SKILL.md`
- `.agents/skills/bll-dal-dtl/SKILL.md`
- `.agents/skills/sdh-contract/SKILL.md`
- `.agents/skills/sdctp-contract/SKILL.md`
- `.agents/skills/sdgw-transport/SKILL.md`
- `.agents/skills/j1939-decode/SKILL.md`
- `.agents/skills/module-database/SKILL.md`
- `.agents/skills/firmware-uce/SKILL.md`
- `.agents/skills/firmware-bpm/SKILL.md`
- `.agents/skills/git-checkpoint/SKILL.md`
- `.agents/skills/build-validation/SKILL.md`
- `.agents/skills/dump-generation/SKILL.md`

### AGENTS locais documentais

- `local-api/.agents/README.md`
- `hardware/.agents/README.md`
- `Data/.agents/README.md`

### Dump

- `out/dumps/codex_agents_skills_bootstrap.md`

## Resumo da nova estrutura

- Registro historico supersedido: o arquivo sem extensao no diretorio `.agents` foi removido posteriormente.
- `.agents/README.md` e o unico bootstrap CODEX oficial.
- `.agents/skills/README.md` lista as 13 skills disponiveis, a documentacao humana equivalente e quando usar cada uma.
- Cada skill em `.agents/skills/<nome>/SKILL.md` preserva o conteudo tecnico da documentacao humana em `.agents/skills/*.md`, com cabecalho estruturado para uso do CODEX.
- `local-api/.agents/README.md`, `hardware/.agents/README.md` e `Data/.agents/README.md` foram criados como ponte curta para as regras principais e skills relevantes, sem duplicar conteudo extenso.

## Confirmacao de escopo

- Codigo C# funcional nao foi alterado.
- Firmware nao foi alterado.
- Banco de dados nao foi alterado.
- UI nao foi alterada.
- Contratos SDH, SDGW e SDCTP nao foram alterados.
- Documentacao existente em `.agents/skills/` nao foi apagada nem substituida.
- Nenhum commit, branch ou tag foi criado.

## Divergencias ou incertezas encontradas

- Divergencia supersedida: a duplicidade entre o arquivo sem extensao no diretorio `.agents` e `.agents/README.md` foi encerrada posteriormente; o bootstrap oficial usa apenas `.agents/README.md`.
- As skills estruturadas foram criadas manualmente a partir das skills humanas existentes; a fonte humana oficial continua em `.agents/skills/`.

## Validacao executada

- Validacao historica supersedida: a existencia do arquivo sem extensao no diretorio `.agents` nao e mais criterio valido.
- Confirmado que `.agents/skills/README.md` existe.
- Confirmado que existem 13 pastas de skills em `.agents/skills/`.
- Confirmado que cada pasta possui `SKILL.md`.
- Confirmado que os 13 `SKILL.md` possuem os cabecalhos obrigatorios: Nome, Objetivo, Quando usar, Quando nao usar, Escopo permitido, Escopo proibido, Arquivos/pastas provaveis, Checklist de validacao, Checklist de entrega, Riscos comuns e Regras de nao regressao.
- Confirmado que nao houve alteracao intencional em codigo funcional, firmware, banco, UI ou contratos.
- `git status --short` mostrado nesta ETAPA era um registro historico e nao representa o estado atual do bootstrap.

## Validacoes nao executadas

- Build C# nao executado: nao aplicavel, pois nao houve alteracao de codigo funcional.
- PlatformIO nao executado: nao aplicavel, pois nao houve alteracao de firmware.
- Validadores de banco nao executados: nao aplicavel, pois nao houve alteracao de schema ou dados.
