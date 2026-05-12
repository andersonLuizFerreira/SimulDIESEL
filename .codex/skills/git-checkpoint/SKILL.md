# Nome

Git Checkpoint

## Objetivo

Orientar inspecao Git, preparacao de checkpoint e consolidacao sem commit, branch ou tag automaticos.

## Quando usar

Use para `git status`, `git diff`, stage/commit/tag autorizados, relatorios de consolidacao e preservacao de rollback.

## Quando nao usar

Nao use para criar branch, commit, tag ou reset sem pedido claro.

## Escopo permitido

- `git status`
- `git diff`
- stage/commit/tag apenas autorizados.
- relatorios e dumps de consolidacao.

## Escopo proibido

- `git reset --hard`, checkout destrutivo ou limpeza de alteracoes de terceiros sem pedido explicito.
- Incluir alteracoes preexistentes por acidente.

## Arquivos/pastas provaveis

- Nao aplicavel; atua no repositorio.

## Padroes do projeto

- Nao ha politica Git formal completa documentada.
- Cada entrega deve preservar rollback.
- Mudancas de protocolo devem ser rastreaveis entre docs, API e firmware.

## Checklist de validacao

- [ ] `git status --short`.
- [ ] Conferir arquivos staged.
- [ ] Separar alteracoes preexistentes das criadas na ETAPA.
- [ ] Confirmar autorizacao antes de commit/tag/branch.

## Checklist de entrega

- [ ] Status antes/depois.
- [ ] Arquivos incluidos.
- [ ] Hash/branch/tag se criados.
- [ ] Itens nao incluidos e motivo.

## Riscos comuns

- Commitar alteracao funcional preexistente.
- Criar branch sem permissao.
- Perder rollback por reset.

## Regras de nao regressao

- Nunca apagar trabalho de outro autor.
- Nao fazer commit automatico.
- Nao criar tag sem autorizacao.

## Documentacao humana equivalente

`docs/agents/skills/git-checkpoint-skill.md`
