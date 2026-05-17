# Dump - Consolidacao definitiva do bootstrap CODEX usando .agents/README.md

Data: 2026-05-12

## Tema e objetivo

ETAPA documental para eliminar a ambiguidade entre o arquivo sem extensao no diretorio `.agents` e `.agents/README.md`.

Objetivo: consolidar o bootstrap CODEX oficial usando somente `.agents/README.md`.

## Escopo executado

- Documentacao de bootstrap CODEX.
- `.agents/README.md`.
- `.agents/README.md`.
- `.agents/`.
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

- `.agents/README.md`
- `.agents/README.md`
- `.agents/README.md`
- `out/dumps/codex_agents_skills_bootstrap.md`

## Arquivos removidos

- Arquivo sem extensao no diretorio `.agents`.

## Arquivos criados

- `out/dumps/instructions_md_consolidation.md`

## Referencias corrigidas

- `.agents/README.md`: passou a declarar o fluxo oficial CODEX:
  1. `.agents/README.md`
  2. `.agents/README.md`
  3. `.agents/skills/`
  4. `.agents/`
- `.agents/README.md`: passou a se declarar como arquivo oficial de bootstrap CODEX e a listar a ordem oficial.
- `.agents/README.md`: passou a incluir `.agents/README.md` no fluxo de leitura.
- `out/dumps/codex_agents_skills_bootstrap.md`: o registro historico da duplicidade foi marcado como supersedido, sem manter instrucao ativa de uso do arquivo sem extensao.

## Confirmacao da remocao

- O arquivo sem extensao no diretorio `.agents` foi removido.
- `.agents/README.md` foi preservado como bootstrap oficial.

## Referencias restantes

- Referencias validas a `.agents/README.md` permanecem em documentacao ativa.
- Nenhuma referencia ativa ao arquivo sem extensao deve permanecer.
- O termo `instructions` ainda pode aparecer como palavra generica ou como parte de `.agents/README.md`.

## Riscos encontrados

- Nao foram encontrados scripts automatizados ou integracoes reais apontando para o arquivo sem extensao no diretorio `.agents`.
- O dump `out/dumps/codex_agents_skills_bootstrap.md` continha registro historico da duplicidade; foi atualizado para indicar que essa decisao foi supersedida.

## Validacao

- Busca textual inicial identificou a duplicidade fisica no diretorio `.agents`.
- Busca textual inicial identificou referencias historicas de duplicidade em `out/dumps/codex_agents_skills_bootstrap.md`.
- Confirmado que o arquivo sem extensao no diretorio `.agents` nao existe: `Test-Path` retornou `False`.
- Confirmado que `.agents/README.md` continua integro e legivel.
- Busca textual final para o caminho sem extensao nao encontrou ocorrencias.
- `git status --short` executado ao final da ETAPA; mostrou a remocao do arquivo sem extensao no diretorio `.agents`, quatro arquivos documentais modificados e este dump novo ainda nao rastreado.

## Confirmacao funcional

- Nao houve alteracao funcional.
- Build C# nao executado: nao aplicavel a ETAPA exclusivamente documental.
- PlatformIO nao executado: nao aplicavel, pois firmware ficou fora de escopo.
- Validadores de banco nao executados: nao aplicavel, pois banco ficou fora de escopo.

## Rollback

- Rollback preservado por alteracoes documentais pequenas e rastreaveis.
- Nenhum commit, branch ou tag foi criado.
