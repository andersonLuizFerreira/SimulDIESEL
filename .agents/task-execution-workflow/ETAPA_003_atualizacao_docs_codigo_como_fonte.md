# ETAPA_003_atualizacao_docs_codigo_como_fonte

## STATUS

CONCLUIDA

## CRIADA_POR

Humano + ChatGPT

## EXECUTADA_POR

Codex

## VALIDADA_POR

2026-05-18

## DATA_CRIACAO

2026-05-18

## DATA_EXECUCAO

pendente de confirmacao

## DATA_VALIDACAO

pendente de confirmacao

## Objetivo

Atualizar os arquivos existentes em `docs/` para refletirem o estado atual do projeto, usando o codigo-fonte versionado como fonte de verdade exclusiva para esta ETAPA.

## Autorizacao humana explicita

Para esta ETAPA, fica autorizado o criterio:

```text
codigo -> docs
```

Ou seja: divergencias entre documentacao e codigo devem ser resolvidas atualizando a documentacao para refletir o codigo atual, desde que a evidencia esteja nos arquivos versionados do projeto.

Esta autorizacao vale somente para esta ETAPA e nao altera a regra transitoria geral da skill `docs-governance`.

## Escopo permitido

- Ler codigo-fonte, firmware, schemas, catalogos e arquivos versionados do projeto.
- Editar documentos existentes em `docs/`.
- Substituir conteudo legado por conteudo atual comprovado no codigo.
- Preservar informacoes existentes que ainda forem verdadeiras.
- Marcar como `pendente de confirmacao` informacoes sem evidencia suficiente.
- Atualizar links internos impactados.
- Atualizar glossarios quando necessario.
- Atualizar estados documentais quando houver evidencia concreta.
- Gerar relatorio final em `out/dumps/relatorio_task/`.

## Fora de escopo

- Alterar codigo-fonte funcional.
- Alterar firmware.
- Alterar banco de dados real.
- Alterar contratos SDH/SDGW/SDCTP.
- Criar novas funcionalidades.
- Inventar comportamento nao comprovado.
- Remover conteudo util sem avaliar se ainda e verdadeiro.
- Recriar historico legado dentro de `docs/`.

## Regras aplicaveis

- `.agents/README.md`
- `.agents/skills/task-execution-workflow/SKILL.md`
- `.agents/skills/docs-governance/SKILL.md`
- `.agents/skills/simuldiesel-architecture/SKILL.md`
- demais skills locais conforme o dominio do documento editado

## Diretriz principal

Editar o que ja existe em `docs/`, substituindo o que estiver legado/desatualizado pelo que esta atual no projeto.

Manter no documento toda informacao existente que ainda seja verdadeira, util e coerente com o codigo atual.

Nao trocar texto por resumo raso: preservar densidade tecnica quando a informacao continuar valida.

## Criterios de evidencia

Uma informacao pode ser tratada como atual quando estiver sustentada por pelo menos uma destas fontes versionadas:

- codigo C# em `local-api/`;
- firmware em `hardware/firmware/`;
- schemas, migrations, JSONs, catalogos e arquivos de dados do projeto;
- arquivos de governanca em `.agents/`;
- documentos tecnicos consolidados ainda coerentes com o codigo.

Se a informacao nao puder ser confirmada, registrar `pendente de confirmacao` em vez de inventar.

## Tarefas

- [x] OK - Auditar os documentos existentes em `docs/` contra o codigo atual.
- [x] OK - Identificar documentos com conteudo legado ou desatualizado.
- [x] OK - Editar documentos existentes substituindo conteudo legado por conteudo atual comprovado.
- [x] OK - Preservar informacoes existentes que ainda forem verdadeiras.
- [x] OK - Atualizar estados documentais conforme evidencia atual.
- [x] OK - Atualizar nomenclaturas oficiais, especialmente `SDGW` e `SDCTP` quando aplicavel.
- [x] OK - Atualizar referencias a estruturas antigas de pastas/documentos quando necessario.
- [x] OK - Atualizar links internos impactados pelas edicoes.
- [x] OK - Atualizar glossarios quando necessario.
- [x] OK - Registrar divergencias sem evidencia suficiente como `pendente de confirmacao`.
- [x] OK - Gerar relatorio final em `out/dumps/relatorio_task/ETAPA_003_relatorio_atualizacao_docs_codigo.md`.

## Validacao obrigatoria

- Verificar que nenhum arquivo funcional foi alterado.
- Verificar que as edicoes documentais possuem evidencia no codigo ou arquivos versionados.
- Verificar que informacoes verdadeiras existentes foram preservadas.
- Verificar links internos basicos dos arquivos editados.
- Verificar coerencia com `.agents/skills/docs-governance/SKILL.md`.
- Verificar que a autorizacao `codigo -> docs` foi aplicada somente nesta ETAPA.

## Entrega esperada

- Documentos de `docs/` atualizados conforme codigo atual.
- Conteudo legado substituido por conteudo atual comprovado.
- Informacoes ainda verdadeiras preservadas.
- Divergencias sem evidencia registradas como `pendente de confirmacao`.
- Relatorio final com arquivos alterados, evidencias usadas, pendencias e validacoes.

## Restricoes

- Nao apagar conteudo amplo sem absorver informacao util.
- Nao transformar duvida em fato.
- Nao usar memoria conversacional como evidencia unica.
- Nao alterar codigo para combinar com docs nesta ETAPA.
- Nao alterar governanca global nesta ETAPA.

## Resultado da execucao

Execucao concluida em 2026-05-18.

Arquivos alterados:

- `docs/00-INDICE.md`
- `docs/01-visao-geral/01-visao-geral-projeto.md`
- `docs/02-arquitetura/04-api-e-host-local.md`
- `docs/02-arquitetura/05-bll-do-host.md`
- `docs/02-arquitetura/05-bll-do-host/01-formslogic-e-fachadas.md`
- `docs/02-arquitetura/06-dal-do-host.md`
- `docs/02-arquitetura/06-dal-do-host/01-sessao-sdh-e-sdgw.md`
- `docs/02-arquitetura/07-dtl-do-host.md`
- `docs/02-arquitetura/07-dtl-do-host/01-contratos-sdh-e-dtos.md`
- `docs/04-firmware/01-arquitetura-firmware.md`
- `docs/04-firmware/02-drivers.md`
- `docs/04-firmware/04-sdh-gateway-architecture.md`
- `docs/04-firmware/boards/UCE/11-uce.md`
- `docs/05-software-dashboard/01-arquitetura-software.md`
- `docs/05-software-dashboard/04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md`
- `docs/06-protocolos/07-uce-sdh-tlv.md`
- `docs/09-desenvolvimento/01-organizacao-repositorio.md`
- `docs/11-planejamento/01-planejamento.md`
- `docs/11-planejamento/02-proximas-funcionalidades.md`
- `out/dumps/relatorio_task/ETAPA_003_relatorio_atualizacao_docs_codigo.md`

Resumo:

- Documentacao atualizada para refletir UCE como board ja presente na rota `SPI` da BPM e na pilha host.
- BLL, DAL, DTL, firmware, software dashboard, protocolos, planejamento e organizacao do repositorio alinhados ao codigo atual.
- Referencias a estruturas antigas de UCE e a pastas documentais removidas foram substituidas por nomes atuais comprovados na arvore versionada.

Validacoes:

- `git status --short` confirmou alteracoes restritas a documentacao, relatorio e controle da ETAPA.
- Verificador basico de links Markdown em `docs/` retornou `broken_links=0`.
- Busca por referencias legadas criticas foi executada e nao encontrou ocorrencias desatualizadas fora de contextos ainda validos.
- A autorizacao `codigo -> docs` foi aplicada somente nesta ETAPA.

Pendencias:

- Validacao fisica ampla de bancada UCE/CAN/SDCTP segue fora do escopo desta ETAPA.
- O hash do primeiro commit desta execucao sera registrado em commit posterior.

Commit de documentacao:

`2b061186` - `Update docs from code state`
