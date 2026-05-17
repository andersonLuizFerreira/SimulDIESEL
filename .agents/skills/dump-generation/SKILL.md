# Nome

Dump Generation

## Objetivo

Orientar a criacao de dumps de ETAPA, inventarios, validacoes, congelamentos e registros de decisoes.

## Quando usar

Use quando a ETAPA pedir dump, congelamento, inventario, validacao, resumo tecnico ou registro de decisoes.

## Quando nao usar

Nao use para substituir documentacao oficial quando a ETAPA exige atualizar docs oficiais.

## Escopo permitido

- Criar arquivos em `out/dumps/`.
- Registrar arquivos criados/alterados, decisoes, validacoes, divergencias e pendencias.

## Escopo proibido

- Alterar codigo funcional.
- Esconder incertezas.
- Registrar como fato o que e inferencia.

## Arquivos/pastas provaveis

- `out/dumps/<nome_da_etapa>.md`
- subpastas em `out/dumps/` quando houver inventario amplo.

## Padroes do projeto

- Nome do dump deve ser descritivo e ligado a ETAPA.
- Conteudo deve permitir rollback e continuidade.
- Divergencias devem ser explicitas.
- Dumps devem diferenciar `nao implementado`, `implementado`, `compilado`, `validado`, `validado parcialmente` e `validado em bancada`.
- Sempre que possivel, registrar evidencias: comandos executados, resultado de build, scripts usados, mensagens relevantes e observacoes verificadas.
- Dumps de ETAPAS arquiteturais devem registrar impacto nas camadas, contratos e fluxos.

## Checklist de validacao

- [ ] Dump contem arquivos criados/alterados.
- [ ] Dump contem resumo por arquivo.
- [ ] Dump contem decisoes adotadas.
- [ ] Dump contem incertezas.
- [ ] Dump contem recomendacoes futuras.
- [ ] Dump separa inferencia, evidencia observada e validacao real.
- [ ] Dump registra comandos, builds, scripts ou motivo de nao aplicacao quando pertinente.

## Checklist de entrega

- [ ] Caminho do dump.
- [ ] Conteudo resumido.
- [ ] Validacao executada.
- [ ] Pendencias.

## Riscos comuns

- Dump virar narrativa sem evidencias.
- Omitir incerteza para parecer conclusivo.
- Misturar recomendacao com decisao congelada.
- Transformar inferencia em fato.
- Declarar validacao completa quando houve apenas validacao parcial ou documental.

## Regras de nao regressao

- Dump deve preservar historico da ETAPA.
- Nao substituir contratos oficiais sem autorizacao.
- Nao esconder divergencias.
- Nao transformar inferencia em fato.
- Nao usar dump para promover estado oficial sem evidencia compativel.


