# Nome

Task Execution Workflow

## Objetivo

Orientar a execucao de ETAPAS versionadas em `.agents/task-execution-workflow/`.

## Quando usar

Use automaticamente apos ler `.agents/README.md`, antes de executar qualquer ETAPA registrada em `.agents/task-execution-workflow/`.

## Fluxo obrigatorio

1. Auditar o estado atual da branch local.
2. Atualizar a branch local com a remota antes de ler a ETAPA.
3. Ler as ETAPAS em `.agents/task-execution-workflow/`.
4. Por padrao, executar somente topicos com tic vazio `[ ]`.
5. Se nao houver topico com tic vazio `[ ]`, nao executar nada e registrar que nao ha tarefa pendente.
6. Executar a ETAPA topico a topico.
7. Atualizar o status de cada topico no proprio arquivo da ETAPA.
8. Registrar `OK`, `SUCESSO`, `FALHA`, `BLOQUEADO` ou `NAO APLICAVEL` em cada item executado.
9. Executar validacoes aplicaveis.
10. Ao final, stagear apenas arquivos da ETAPA.
11. Commitar com mensagem objetiva.
12. Enviar para o remoto.
13. Registrar no arquivo da ETAPA o resumo final, commit hash e validacoes executadas.

## Selecao de tarefas

A execucao padrao deve considerar apenas itens com tic vazio `[ ]`.

Itens marcados como `[x]`, `[!]`, `[-]` ou `[~]` nao devem ser executados por padrao.

Itens com `[!] FALHA`, `[-] BLOQUEADO` ou outro estado diferente de `[ ]` so devem ser executados quando houver solicitacao humana explicita.

A solicitacao humana pode indicar:

- executar todos os itens `[!]`;
- executar uma ETAPA especifica;
- executar um topico especifico;
- reexecutar um item ja marcado.

Se nao existir nenhuma ETAPA ou nenhum item `[ ]` em `.agents/task-execution-workflow/`, o agente deve parar sem alterar arquivos e informar que nao ha tarefa pendente para executar.

## Estados permitidos por item

- `[ ] PENDENTE`
- `[x] OK`
- `[x] SUCESSO`
- `[!] FALHA`
- `[-] BLOQUEADO`
- `[~] NAO APLICAVEL`

## Regras

- Nao executar tarefa fora do escopo da ETAPA.
- Nao executar ETAPA se houver conflito de governanca.
- Nao alterar item ja concluido sem registrar motivo.
- Nao misturar alteracoes preexistentes com alteracoes da ETAPA.
- Se a branch local divergir da remota, interromper e reportar.
- Se a ETAPA estiver ambigua, interromper e reportar.
- Se uma validacao falhar, registrar `FALHA` e nao ocultar erro.

## Checklist de entrega

- [ ] Branch auditada antes da execucao.
- [ ] Branch atualizada com remoto.
- [ ] ETAPA executada topico a topico.
- [ ] Itens ticados no arquivo da ETAPA.
- [ ] Validacoes registradas.
- [ ] Arquivos stageados de forma controlada.
- [ ] Commit realizado.
- [ ] Push realizado.
- [ ] Commit hash registrado na ETAPA.
