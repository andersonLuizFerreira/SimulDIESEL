⬅ [Retornar para SimulDIESEL — Documentação Oficial](README.md)
⬅ [Retornar para Índice Geral](00-INDICE.md)

# Regras Oficiais da Documentação

Este documento é a **verdade oficial** da estrutura documental ativa do SimulDIESEL.

## 1. Objetivo da documentação

A documentação oficial do projeto existe para registrar o estado vigente do sistema de forma navegável, auditável e progressiva.

Ela é organizada em:

* árvore hierárquica
* aprofundamento progressivo
* visão física
* visão lógica

Essa estrutura permite leitura por contexto, por responsabilidade e por nível de detalhe, sem perder a coerência entre pai e filho.

## 2. Regras de navegação

### Regra 1

Toda página oficial deve possuir, **no topo da página**:

* link de retorno ao pai imediato
* link de retorno ao índice geral

### Regra 2

A relação estrutural da árvore oficial é sempre:

```text
pai -> filhos imediatos
```

### Regra 3

O índice geral é a **única página autorizada a apontar para qualquer página da árvore**.

### Regra 4

Toda revisão futura desta organização deve ser registrada neste documento.

### Regra 5

Toda página viva da documentação deve conter uma seção `## Glossário` com os termos efetivamente usados naquele documento.

### Regra 6

A nomenclatura oficial do gateway do projeto é `SDGW (SimulDiesel GateWay)`.

### Regra 7

A documentação oficial deve sempre refletir o **estado implementado do projeto**.

Quando houver divergência entre fontes, a prioridade de verdade documental é:

1. código-fonte implementado
2. contratos técnicos efetivos
3. documentação existente

### Regra 8

Toda funcionalidade descrita em páginas arquiteturais, de software, firmware, hardware, protocolos, casos de uso e testes deve indicar seu estado quando isso for relevante para a leitura:

* `IMPLEMENTADO`
* `PARCIALMENTE IMPLEMENTADO`
* `PLANEJADO`
* `LEGADO`

### Regra 9

As trilhas `ONDE` e `COMO` não devem se misturar nas páginas profundas:

* `ONDE`: empilhamento, arquivos, interfaces, conectores e posição estrutural
* `COMO`: estados, função lógica, fluxo, retry, parsing, eventos e resposta operacional

## Glossário

- **Governança documental**: regras formais que definem a estrutura oficial da documentação.
- **Índice geral**: mapa global que pode acessar qualquer página da árvore viva.
- **Glossário**: seção obrigatória com os termos usados em cada página viva.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.
- **ONDE**: trilha física que explica posição e conectores das classes no host e no hardware.
- **COMO**: trilha lógica que explica comportamento efetivo do host, firmware e fluxo operacional.

## Histórico de revisões documentais

| data | alteração | arquivos impactados | observações |
| --- | --- | --- | --- |
| 2026-04-06 | Aprofundamento técnico de firmware e hardware e fechamento da etapa documental atual | `docs/official/04-firmware/`, `docs/official/03-hardware/`, `docs/official/02-arquitetura/10-hardware-da-bancada.md`, `docs/official/02-arquitetura/11-modulo-em-teste-e-xconn.md`, `docs/official/01-visao-geral/01-visao-geral-projeto.md`, `docs/official/11-planejamento/` | Documentação alinhada ao código real de BPM e GSA; backplane e alimentação mantidos como parciais quando o repositório não sustentou mais detalhe; fechamento da etapa registra GSA ainda pendente e prontidão para a UCE. |
| 2026-04-05 | Aprofundamento técnico da API local com trilhas físicas e lógicas ancoradas em classes, métodos e trechos reais do host | `docs/00-INDICE.md`, `docs/DOCUMENTATION_RULES.md`, `docs/official/02-arquitetura/04-api-e-host-local.md`, `docs/official/02-arquitetura/05-bll-do-host.md`, `docs/official/02-arquitetura/06-dal-do-host.md`, `docs/official/02-arquitetura/07-dtl-do-host.md`, `docs/official/02-arquitetura/08-transporte-do-host.md`, `docs/official/02-arquitetura/09-serial-e-bluetooth.md`, `docs/official/05-software-dashboard/01-arquitetura-software.md`, `docs/official/05-software-dashboard/03-camada-hardware.md`, `docs/official/05-software-dashboard/04-sdh-host-architecture.md` | Revisão conduzida a partir do código real de `local-api`; detalhes de firmware não observáveis pelo host foram removidos desta trilha. |
| 2026-04-05 | Reestruturação da árvore ONDE vs COMO com base no estado implementado do host e do firmware | `docs/00-INDICE.md`, `docs/DOCUMENTATION_RULES.md`, `docs/official/02-arquitetura/`, `docs/official/03-hardware/`, `docs/official/04-firmware/`, `docs/official/05-software-dashboard/`, `docs/official/06-protocolos/`, `docs/official/07-simulacoes/`, `docs/official/08-casos-de-uso/` | A navegação passou a separar trilha física da API, hardware físico e módulo em teste do ramo lógico de fluxo, firmware, software e protocolos, sempre priorizando o código como fonte de verdade. |
| 2026-04-05 | Consolidação da nomenclatura oficial do gateway e inclusão da regra de glossário por página | `docs/DOCUMENTATION_RULES.md`, `docs/official/`, `docs/README.md`, `docs/00-INDICE.md` | Documentação viva alinhada ao nome `SDGW`; código permaneceu apenas auditado. |
| 2026-04-05 | Limpeza estrutural da documentação oficial e remoção de artefatos temporários | `docs/`, `out/`, `_rebuild_logs/` | `docs/` consolidado como única documentação oficial; artefatos paralelos e temporários removidos. |
