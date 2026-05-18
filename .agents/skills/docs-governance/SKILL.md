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

## Fonte unica documental

- `docs/` e a unica e suficiente fonte de verdade para documentacao consolidada do projeto.
- Nao deve haver outras fontes documentais concorrentes dentro de `docs/`, como `legacy`, `oficial`, `official`, `historico`, `old`, `archive` ou equivalentes.
- Versionamento, historico e rollback pertencem ao Git, nao a pastas documentais legadas.
- Documentos legados, duplicados ou historicos dentro de `docs/` podem induzir erro e devem ser saneados por ETAPA propria.
- Dumps, chats, prompts e arquivos temporarios nao substituem documentacao oficial.

## Relacao docs -> codigo

- O estado atual da documentacao ainda esta em consolidacao e saneamento.
- Enquanto `docs/` nao estiver totalmente atualizado e confiavel, divergencias entre documentacao e codigo nao devem ser resolvidas automaticamente pela IA.
- Divergencias entre documentacao e codigo devem ser registradas e apresentadas ao humano para decisao.
- Somente uma ETAPA/task com instrucao humana explicita pode autorizar:
  - atualizar docs para seguir o codigo;
  - atualizar codigo para seguir docs;
  - escolher uma fonte como verdade operacional naquele contexto.
- Nao usar codigo divergente como justificativa automatica para alterar documentacao oficial.
- Futuramente, apos consolidacao documental, `docs/` podera se tornar oficialmente a fonte de verdade para o codigo.

## Arvore oficial de `docs/`

A estrutura autorizada de `docs/` deve ser registrada nesta skill.

Arvore atualmente autorizada:

```text
docs/
|-- README.md
|-- 00-INDICE.md
|-- 01-visao-geral/
|-- 02-arquitetura/
|-- 03-hardware/
|-- 04-firmware/
|-- 05-software-dashboard/
|-- 06-protocolos/
|-- 07-simulacoes/
|-- 08-casos-de-uso/
|-- 09-desenvolvimento/
|-- 10-testes/
|-- 11-planejamento/
`-- 12-documentacao-tecnica/
```

A arvore acima representa a documentacao viva consolidada. As antigas estruturas `docs/official/`, `docs/legacy/`, `docs/architecture/` e o arquivo `docs/DOCUMENTATION_RULES.md` nao pertencem mais a arvore viva.

Qualquer alteracao na arvore autorizada de `docs/` deve:

1. ser solicitada ou aprovada pelo humano;
2. atualizar esta skill;
3. atualizar os indices/documentos impactados;
4. registrar divergencias encontradas;
5. preservar conteudo util antes de consolidar ou remover documentos.

## Navegacao documental

- Toda pagina oficial deve possuir navegacao clara para seu contexto documental.
- Toda pagina oficial deve possuir, no topo da pagina, link de retorno ao pai imediato e link de retorno ao indice geral.
- A relacao estrutural oficial da documentacao deve seguir:

```text
pai -> filhos imediatos
```

- O indice geral deve permanecer como ponto central de navegacao documental.
- O indice geral e a unica pagina autorizada a apontar para qualquer pagina da arvore viva.
- Revisoes futuras da organizacao documental devem ser registradas pela governanca documental em `.agents/` ou por ETAPA correspondente.

## Glossario

- Toda pagina viva da documentacao deve possuir secao `Glossario` quando utilizar termos tecnicos relevantes.
- O glossario deve conter apenas termos efetivamente usados no documento.


## Separacao ONDE vs COMO

Trilhas documentais profundas nao devem misturar:

- `ONDE`: empilhamento, arquivos, interfaces, conectores e posicao estrutural;
- `COMO`: comportamento, estados, fluxo, retry, parsing, eventos e resposta operacional.

A separacao deve permanecer clara em documentacoes arquiteturais, firmware, software, hardware, protocolos e fluxos.

## Regras locais

- A documentacao deve refletir a arquitetura e as decisoes oficiais consolidadas.
- A nomenclatura oficial do gateway do projeto e `SDGW (SimulDiesel GateWay)`.
- Nao documentar comportamento nao validado como implementado.
- Nao promover automaticamente algo de `PLANEJADO` para `IMPLEMENTADO`.
- Documentacao duplicada deve ser consolidada.
- Antes de remover documentacao, verificar se existe conteudo util a absorver.
- Antes de sobrescrever documentacao existente, absorver conteudo util e preservar informacao valida.
- Nao apagar secoes antigas sem verificar impacto arquitetural e documental.
- Em caso de conflito documental, preservar a versao mais completa, atual e coerente.
- Se houver ambiguidade documental, registrar `pendente de confirmacao`.
- Links internos devem permanecer validos.
- Documentacao deve evitar contradizer governanca oficial.

## Estrutura documental

A organizacao interna de `docs/` deve:

- ser unica, direta e suficiente;
- evitar redundancia;
- manter nomenclatura consistente;
- manter navegacao clara;
- evitar arquivos gigantes sem necessidade;
- nao depender de pastas historicas para explicar o estado atual.

## Validacao documental

Toda ETAPA documental deve validar:

- coerencia com a arquitetura oficial;
- coerencia com governanca em `.agents`;
- ausencia de contradicao relevante;
- links internos basicos;
- ausencia de duplicidade evidente;
- estados documentais corretos;
- ausencia de fontes paralelas dentro de `docs/`.

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

- registrar a divergencia;
- nao decidir autonomamente qual lado prevalece;
- solicitar decisao humana quando necessario;
- seguir instrucoes explicitas da ETAPA/task quando existirem.

## Checklist de entrega

- [ ] Documentacao auditada.
- [ ] Duplicidades identificadas.
- [ ] Contradicoes registradas.
- [ ] Fontes documentais paralelas identificadas.
- [ ] Links revisados.
- [ ] Estrutura documental preservada ou saneamento proposto.
- [ ] Conteudo util absorvido antes de consolidacao.
- [ ] Nenhuma documentacao oficial removida sem justificativa.
- [ ] Skill atualizada se a arvore autorizada de `docs/` mudar.
