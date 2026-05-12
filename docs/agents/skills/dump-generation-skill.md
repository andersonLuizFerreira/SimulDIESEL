# Skill: Dump Generation

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

## Checklist de validacao

- [ ] Dump contem arquivos criados/alterados.
- [ ] Dump contem resumo por arquivo.
- [ ] Dump contem decisoes adotadas.
- [ ] Dump contem incertezas.
- [ ] Dump contem recomendacoes futuras.

## Checklist de entrega

- [ ] Caminho do dump.
- [ ] Conteudo resumido.
- [ ] Validacao executada.
- [ ] Pendencias.

## Riscos comuns

- Dump virar narrativa sem evidencias.
- Omitir incerteza para parecer conclusivo.
- Misturar recomendacao com decisao congelada.

## Regras de nao regressao

- Dump deve preservar historico da ETAPA.
- Nao substituir contratos oficiais sem autorizacao.
- Nao esconder divergencias.
