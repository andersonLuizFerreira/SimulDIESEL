# Nome

Task Execution Workflow

## Objetivo

Orientar a execucao de ETAPAS versionadas em `.agents/task-execution-workflow/`.

## Quando usar

Use automaticamente apos ler `.agents/README.md`, antes de executar qualquer ETAPA registrada em `.agents/task-execution-workflow/`.

## Fluxo obrigatorio

1. Auditar o estado atual da branch local.
2. Atualizar a branch local com a remota antes de ler a ETAPA.
3. Ler a ETAPA pendente em `.agents/task-execution-workflow/`.
4. Executar a ETAPA topico a topico.
5. Atualizar o status de cada topico no proprio arquivo da ETAPA.
6. Registrar `OK`, `SUCESSO`, `FALHA`, `BLOQUEADO` ou `NAO APLICAVEL` em cada item executado.
7. Executar validacoes aplicaveis.
8. Ao final, stagear apenas arquivos da ETAPA.
9. Commitar com mensagem objetiva.
10. Enviar para o remoto.
11. Registrar no arquivo da ETAPA o resumo final, commit hash e validacoes executadas.

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
