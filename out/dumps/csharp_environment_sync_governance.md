# Dump - Governanca de sincronizacao entre codigo C# runtime e ambiente de desenvolvimento

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para adicionar governanca permanente sobre sincronizacao entre arquivos C# no filesystem, `.csproj`, `.sln` e ambiente Visual Studio.

Objetivo: impedir que codigo C# exista apenas fisicamente no disco, fora do projeto carregado, ou que a solucao compile sem incluir arquivos esperados.

## Escopo executado

- `AGENTS.md`
- `.codex/instructions.md`
- `.codex/skills/bll-dal-dtl/SKILL.md`
- `.codex/skills/build-validation/SKILL.md`
- `docs/agents/project_conventions.md`
- `docs/agents/validation_checklist.md`
- `docs/agents/skills/bll-dal-dtl-skill.md`
- `docs/agents/skills/build-validation-skill.md`
- `out/dumps/csharp_environment_sync_governance.md`

## Fora de escopo preservado

- Codigo funcional.
- Firmware.
- Banco.
- UI.
- Contratos SDH, SDGW e SDCTP.
- Arquivos `.sln` e `.csproj`.
- Branch, commit e tag.

## Arquivos alterados

- `AGENTS.md`
- `.codex/instructions.md`
- `.codex/skills/bll-dal-dtl/SKILL.md`
- `.codex/skills/build-validation/SKILL.md`
- `docs/agents/project_conventions.md`
- `docs/agents/validation_checklist.md`
- `docs/agents/skills/bll-dal-dtl-skill.md`
- `docs/agents/skills/build-validation-skill.md`

## Arquivos criados

- `out/dumps/csharp_environment_sync_governance.md`

## Regras adicionadas

- ETAPAS C# devem manter `.sln` e `.csproj` sincronizados quando arquivos forem criados, removidos ou movidos.
- Arquivos C# devem aparecer corretamente no ambiente Visual Studio/Solution Explorer.
- Codigo C# orfao fora da solucao/projeto e erro de ETAPA.
- Implementacao C# incompleta no ambiente de desenvolvimento nao e entrega valida.
- Build C# deve utilizar os arquivos corretos, nao apenas um subconjunto acidentalmente incluido no projeto.
- Mudancas C# devem ser visiveis e rastreaveis pelo desenvolvedor.

## Motivo da governanca

- Facilitar supervisao humana no Visual Studio.
- Preservar rastreabilidade entre filesystem e projeto carregado.
- Evitar codigo criado em disco, mas ausente do build.
- Evitar falsa conclusao por build que ignora arquivos C# novos.
- Manter integridade da solucao e rollback rastreavel.

## Impacto esperado nas ETAPAS C#

- Toda ETAPA que alterar C# deve conferir `.csproj`, `.sln` quando aplicavel e Solution Explorer.
- A validacao de build passa a incluir a pergunta: o build esta compilando os arquivos corretos?
- Dumps e entregas devem registrar sincronizacao do ambiente quando houver criacao, remocao ou movimentacao de arquivos C#.

## Validacao executada

- Conferido que a ETAPA alterou apenas arquivos documentais/governanca e este dump.
- Conferido que nao houve alteracao funcional nesta ETAPA.
- Conferida coerencia entre `AGENTS.md`, `.codex/instructions.md`, skills estruturadas e skills humanas equivalentes.
- Conferido que as novas regras nao conflitam com rollback nem arquitetura.
- `git status --short` executado.
- Build nao executado: nao aplicavel a ETAPA exclusivamente documental.

## Resultado da validacao

- Busca textual confirmou as regras novas em `AGENTS.md`, `.codex/instructions.md`, skills estruturadas, skills humanas e checklists.
- Nenhum arquivo `.sln` foi alterado nesta ETAPA.
- Nenhum arquivo `.csproj` foi alterado nesta ETAPA; o `git status --short` ainda mostra `SimulDIESEL.csproj` modificado por ETAPA funcional anterior no mesmo worktree.
- A governanca adicionada e complementar ao rollback: ela exige rastreabilidade, nao autoriza commit, branch, tag ou comandos destrutivos.
- Nao houve alteracao funcional nesta ETAPA.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Nenhum arquivo `.sln` ou `.csproj` foi alterado nesta ETAPA.
- Firmware, banco, UI e contratos SDH/SDGW/SDCTP nao foram alterados nesta ETAPA.

## Pendencias

- Sem pendencias identificadas para a governanca documental.
