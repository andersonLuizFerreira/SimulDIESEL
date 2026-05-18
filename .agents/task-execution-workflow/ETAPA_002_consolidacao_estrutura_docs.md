# ETAPA_002_consolidacao_estrutura_docs

## STATUS

PENDENTE

## CRIADA_POR

Humano + ChatGPT

## EXECUTADA_POR

pendente de confirmacao

## VALIDADA_POR

pendente de confirmacao

## DATA_CRIACAO

2026-05-18

## DATA_EXECUCAO

pendente de confirmacao

## DATA_VALIDACAO

pendente de confirmacao

## Objetivo

Consolidar a estrutura oficial de `docs/` utilizando como base a arvore atualmente existente em `docs/official/`, eliminando estruturas documentais paralelas e alinhando a documentacao com a nova governanca documental.

## Escopo permitido

- Auditar `docs/official/`.
- Absorver `docs/official/` para a raiz viva de `docs/`.
- Atualizar `00-INDICE.md`.
- Atualizar `README.md` de `docs/`.
- Atualizar `.agents/skills/docs-governance/SKILL.md`.
- Identificar redundancias documentais.
- Consolidar conteudo util.
- Remover `DOCUMENTATION_RULES.md` apos validacao de absorcao.
- Identificar estruturas paralelas restantes.
- Propor saneamento final da estrutura documental.

## Fora de escopo

- Alterar firmware.
- Alterar codigo-fonte funcional.
- Escolher autonomamente docs -> codigo ou codigo -> docs.
- Saneamento automatico de divergencias funcionais.
- Alterar contratos SDH/SDGW/SDCTP.

## Regras aplicaveis

- `.agents/README.md`
- `.agents/skills/task-execution-workflow/SKILL.md`
- `.agents/skills/docs-governance/SKILL.md`

## Decisoes humanas consolidadas

- A estrutura oficial viva de `docs/` deve ser baseada na estrutura atualmente existente em `docs/official/`.
- `docs/official/` nao deve permanecer como raiz paralela.
- `docs/legacy/` nao deve permanecer como estrutura viva.
- `DOCUMENTATION_RULES.md` deve ser removido apos absorcao completa das regras relevantes.
- Na raiz de `docs/` devem existir apenas:

```text
docs/
|-- README.md
|-- 00-INDICE.md
|-- <pastas associadas>
```

## Tarefas

- [ ] Auditar estrutura atual de `docs/official/`.
- [ ] Definir arvore final proposta para `docs/`.
- [ ] Identificar conflitos de nomenclatura.
- [ ] Identificar redundancias entre `docs/`, `docs/official/` e `docs/legacy/`.
- [ ] Consolidar conteudo util antes de mover/remover documentos.
- [ ] Atualizar `.agents/skills/docs-governance/SKILL.md` com a nova arvore oficial.
- [ ] Atualizar `00-INDICE.md` para refletir a nova estrutura.
- [ ] Atualizar `docs/README.md`.
- [ ] Remover `DOCUMENTATION_RULES.md` apos validacao de absorcao.
- [ ] Propor destino final para `docs/legacy/`.
- [ ] Verificar links internos basicos.
- [ ] Gerar relatorio de consolidacao documental.

## Validacao obrigatoria

- Verificar coerencia com `.agents/README.md`.
- Verificar coerencia com `.agents/skills/docs-governance/SKILL.md`.
- Verificar ausencia de governanca documental paralela.
- Verificar preservacao de conteudo util.
- Verificar navegacao documental basica.

## Entrega esperada

- Estrutura documental consolidada.
- Estrutura paralela removida ou classificada.
- `DOCUMENTATION_RULES.md` absorvido/removido.
- Relatorio de consolidacao documental.
- Atualizacao das skills/documentacao impactadas.

## Restricoes

- Nao remover conteudo util sem absorcao.
- Nao sobrescrever historico util sem consolidacao.
- Nao decidir autonomamente conflitos docs x codigo.
- Nao alterar governanca global sem autorizacao humana.

## Resultado da execucao

pendente de execucao
