# ETAPA_003_atualizacao_docs_codigo_como_fonte

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

- [ ] Auditar os documentos existentes em `docs/` contra o codigo atual.
- [ ] Identificar documentos com conteudo legado ou desatualizado.
- [ ] Editar documentos existentes substituindo conteudo legado por conteudo atual comprovado.
- [ ] Preservar informacoes existentes que ainda forem verdadeiras.
- [ ] Atualizar estados documentais conforme evidencia atual.
- [ ] Atualizar nomenclaturas oficiais, especialmente `SDGW` e `SDCTP` quando aplicavel.
- [ ] Atualizar referencias a estruturas antigas de pastas/documentos quando necessario.
- [ ] Atualizar links internos impactados pelas edicoes.
- [ ] Atualizar glossarios quando necessario.
- [ ] Registrar divergencias sem evidencia suficiente como `pendente de confirmacao`.
- [ ] Gerar relatorio final em `out/dumps/relatorio_task/ETAPA_003_relatorio_atualizacao_docs_codigo.md`.

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

pendente de execucao
