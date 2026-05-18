# ETAPA_001_saneamento_docs

## STATUS

CONCLUIDA

## CRIADA_POR

Humano + ChatGPT

## EXECUTADA_POR

Codex

## VALIDADA_POR

pendente de confirmacao

## DATA_CRIACAO

2026-05-18

## DATA_EXECUCAO

2026-05-18

## DATA_VALIDACAO

pendente de confirmacao

## Objetivo

Auditar, consolidar e saneiar a pasta `docs/` para alinhamento com a nova governanca documental oficial.

## Escopo permitido

- Auditar estrutura atual de `docs/`.
- Identificar duplicidades documentais.
- Identificar fontes documentais paralelas.
- Identificar contradicoes.
- Identificar estruturas `legacy`, `official`, `historico` ou equivalentes.
- Consolidar regras documentais na skill `.agents/skills/docs-governance/SKILL.md`.
- Atualizar indices e navegacao documental.
- Propor saneamento estrutural.

## Fora de escopo

- Alterar firmware.
- Alterar codigo-fonte funcional.
- Alterar contratos SDH/SDGW/SDCTP.
- Executar saneamento automatico de codigo.
- Escolher autonomamente docs -> codigo ou codigo -> docs.

## Regras aplicaveis

- `.agents/README.md`
- `.agents/skills/task-execution-workflow/SKILL.md`
- `.agents/skills/docs-governance/SKILL.md`

## Tarefas

- [x] OK - Auditar arvore atual de `docs/`.
- [x] OK - Identificar estruturas paralelas ou redundantes.
- [x] OK - Identificar documentos contraditorios.
- [x] OK - Identificar documentos duplicados.
- [x] OK - Identificar links quebrados.
- [x] OK - Identificar documentos sem navegacao adequada.
- [x] OK - Identificar documentos sem glossario relevante.
- [x] OK - Identificar divergencias relevantes entre docs e codigo.
- [x] OK - Consolidar conteudo util de `DOCUMENTATION_RULES.md` na skill oficial.
- [x] OK - Propor estrutura documental final alvo.
- [x] OK - Propor plano de saneamento documental.

## Validacao obrigatoria

- Verificar coerencia com `.agents/README.md`.
- Verificar coerencia com `.agents/skills/docs-governance/SKILL.md`.
- Verificar ausencia de governanca documental paralela.
- Verificar navegacao documental basica.

## Entrega esperada

- Relatorio de auditoria documental.
- Lista de divergencias.
- Lista de redundancias.
- Estrutura alvo proposta.
- Plano de saneamento.
- Atualizacao das skills/documentacao impactadas.
- A saida dos relatórios deve ser feita em /out/dumps/relatorio_task/

## Restricoes

- Nao remover documentacao oficial sem absorver conteudo util.
- Nao sobrescrever historico util.
- Nao assumir automaticamente docs ou codigo como fonte de verdade.
- Nao alterar estrutura autorizada sem registrar divergencia.

## Resultado da execucao

Execucao concluida em 2026-05-18.

Arquivos criados:

- `out/dumps/relatorio_task/ETAPA_001_relatorio_auditoria_documental.md`

Arquivos alterados:

- `.agents/skills/docs-governance/SKILL.md`
- `.agents/task-execution-workflow/ETAPA_001_saneamento_docs.md`

Resumo:

- Auditada a arvore atual de `docs/`, com 138 arquivos Markdown.
- Identificadas estruturas paralelas ou divergentes: `docs/official/`, `docs/legacy/`, `docs/architecture/` e `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`.
- Registradas contradicoes entre `docs/DOCUMENTATION_RULES.md`, `docs/README.md`, `docs/00-INDICE.md`, `docs/official/09-desenvolvimento/01-organizacao-repositorio.md` e `.agents/skills/docs-governance/SKILL.md`.
- Identificadas duplicidades exatas e titulo duplicado relevante em documentos legados.
- Verificador basico de links Markdown internos executado: 0 links quebrados encontrados.
- Identificados documentos sem navegacao padrao e sem glossario.
- Registradas divergencias docs x codigo sem escolher fonte de verdade.
- Consolidado conteudo util de `DOCUMENTATION_RULES.md` em `.agents/skills/docs-governance/SKILL.md`: navegacao de topo, papel do indice geral e nomenclatura `SDGW (SimulDiesel GateWay)`.
- Propostos estrutura alvo e plano de saneamento documental.

Validacoes executadas:

- Coerencia com `.agents/README.md`: OK, conflitos apenas registrados.
- Coerencia com `.agents/skills/docs-governance/SKILL.md`: OK, divergencias estruturais registradas.
- Ausencia de governanca documental paralela: OK como verificacao; governanca paralela identificada e registrada em `docs/DOCUMENTATION_RULES.md`.
- Navegacao documental basica: OK parcial; links internos relativos sem quebra, mas documentos sem topo padrao registrados.
- Build funcional: NAO APLICAVEL por ETAPA exclusivamente documental.

Warnings e pendencias:

- Decisao humana necessaria sobre manter, consolidar ou remover `docs/official/`, `docs/legacy/`, `docs/architecture/` e ETAPA operacional em `docs/`.
- Decisao humana necessaria sobre prioridade entre codigo, contratos e docs durante divergencias.
- Saneamento estrutural efetivo deve ocorrer em ETAPA propria, com absorcao de conteudo util antes de remocao.

Rollback:

- Preservado via Git. Nenhum arquivo funcional foi alterado.

Commit hash:

- pendente ate commit/push desta ETAPA.
