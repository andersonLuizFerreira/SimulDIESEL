# Nome

Task Execution Workflow

## Objetivo

Orientar a execucao de ETAPAS versionadas em `.agents/task-execution-workflow/`.

## Quando usar

Use automaticamente apos ler `.agents/README.md`, antes de executar qualquer ETAPA registrada em `.agents/task-execution-workflow/`.

## Pasta oficial de tarefas

A pasta oficial de tarefas versionadas e:

```text
.agents/task-execution-workflow/
```

Essa pasta contem arquivos de ETAPA a serem executados pelo agente.

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

## Estados oficiais da ETAPA

Cada arquivo de ETAPA deve declarar um estado geral.

Estados permitidos:

- `PENDENTE`: existe ao menos uma tarefa `[ ]` a executar.
- `EM_EXECUCAO`: ETAPA em andamento.
- `BLOQUEADA`: ETAPA interrompida por conflito, duvida, dependencia ou erro externo.
- `FALHA`: houve erro de execucao ou validacao.
- `VALIDACAO_PENDENTE`: execucao concluida, mas validacao final ainda nao realizada.
- `CONCLUIDA`: todas as tarefas aplicaveis foram executadas, validadas e registradas.

## Imutabilidade de historico

ETAPAS concluidas sao historico operacional.

Nao reutilizar, limpar, sobrescrever ou reabrir uma ETAPA `CONCLUIDA` por iniciativa propria.

Correcoes, ajustes ou retrabalho sobre uma ETAPA concluida devem ser registrados em nova ETAPA, salvo solicitacao humana explicita para reabrir ou corrigir o arquivo historico.

Tics, logs, validacoes, hash de commit e resumo final de uma ETAPA concluida nao devem ser removidos.

## Nomenclatura de ETAPAS

O nome do arquivo deve seguir o padrao:

```text
ETAPA_000_descricao_curta.md
```

Regras:

- usar `ETAPA` em maiusculo;
- usar numeracao sequencial com tres digitos;
- usar descricao curta em `snake_case` ou `kebab-case`;
- nao reutilizar numero de ETAPA;
- nao renomear ETAPA concluida sem autorizacao humana explicita.

## Ownership da ETAPA

Cada ETAPA deve registrar, quando aplicavel:

- `CRIADA_POR`;
- `EXECUTADA_POR`;
- `VALIDADA_POR`;
- `DATA_CRIACAO`;
- `DATA_EXECUCAO`;
- `DATA_VALIDACAO`.

Se algum campo nao for conhecido, registrar `pendente de confirmacao`.

## Arquivamento

Por padrao, ETAPAS permanecem em `.agents/task-execution-workflow/`.

Quando o volume crescer, o arquivamento deve ocorrer por nova regra ou nova ETAPA autorizada.

Nenhuma ETAPA concluida deve ser movida para arquivo morto, removida ou compactada sem autorizacao humana explicita.

## Regras

- Nao executar tarefa fora do escopo da ETAPA.
- Nao executar ETAPA se houver conflito de governanca.
- Nao alterar item ja concluido sem registrar motivo.
- Nao misturar alteracoes preexistentes com alteracoes da ETAPA.
- Se a branch local divergir da remota, interromper e reportar.
- Se a ETAPA estiver ambigua, interromper e reportar.
- Se uma validacao falhar, registrar `FALHA` e nao ocultar erro.
- Regras especificas de tasks pertencem a esta skill local, nao ao README global.

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
