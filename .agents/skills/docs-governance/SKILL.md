# Nome

Docs Governance

## Objetivo

Definir regras locais para organizacao, auditoria, manutencao, validacao e consolidacao da pasta `docs/`.

## Quando usar

Use esta skill em qualquer ETAPA que:

- crie documentacao;
- altere documentacao;
- reorganize `docs/`;
- audite documentacao;
- consolide documentacao;
- remova documentacao;
- valide coerencia documental.

## Escopo

Esta skill governa exclusivamente:

```text
/docs
```

## Regras locais

- `docs/` e a fonte oficial de documentacao consolidada do projeto.
- Dumps, chats, prompts e arquivos temporarios nao substituem documentacao oficial.
- A documentacao deve refletir o estado real do projeto.
- Nao documentar comportamento nao validado.
- Nao promover automaticamente algo de `PLANEJADO` para `IMPLEMENTADO`.
- Documentacao duplicada deve ser consolidada.
- Antes de remover documentacao, verificar se existe outra referencia oficial equivalente.
- Antes de sobrescrever documentacao existente, absorver conteudo util e preservar historico relevante.
- Nao apagar secoes antigas sem verificar impacto arquitetural e documental.
- Em caso de conflito documental, preservar a versao mais completa, atual e coerente.
- Se houver ambiguidade documental, registrar `pendente de confirmacao`.
- Links internos devem permanecer validos.
- Documentacao deve evitar contradizer contratos, codigo consolidado ou governanca oficial.

## Estrutura documental

A organizacao interna de `docs/` deve:

- separar documentacao oficial de dumps e artefatos temporarios;
- evitar redundancia;
- manter nomenclatura consistente;
- manter organizacao navegavel;
- evitar arquivos gigantes sem necessidade.

## Validacao documental

Toda ETAPA documental deve validar:

- coerencia com a arquitetura oficial;
- coerencia com governanca em `.agents`;
- ausencia de contradicao relevante;
- links internos basicos;
- ausencia de duplicidade evidente;
- estados documentais corretos.

## Estados documentais

Estados documentais seguem exclusivamente as regras globais em:

```text
.agents/README.md
```

Esta skill nao redefine estados globais.

## Fora de escopo

Esta skill nao governa:

- workflow operacional de ETAPAS;
- Git;
- firmware;
- codigo-fonte;
- regras arquiteturais globais;
- governanca global.

## Conflitos

Em caso de conflito entre documentacao e codigo:

- nao assumir automaticamente que a documentacao esta correta;
- nao assumir automaticamente que o codigo representa implementacao final;
- registrar divergencia;
- solicitar decisao humana quando necessario.

## Checklist de entrega

- [ ] Documentacao auditada.
- [ ] Duplicidades identificadas.
- [ ] Contradicoes registradas.
- [ ] Links revisados.
- [ ] Estrutura documental preservada.
- [ ] Conteudo util absorvido antes de consolidacao.
- [ ] Nenhuma documentacao oficial removida sem justificativa.
