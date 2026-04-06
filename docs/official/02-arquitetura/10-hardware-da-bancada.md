⬅ [Retornar para Visão Física do Projeto](02-visao-fisica.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Hardware da Bancada

Esta página agrupa a leitura **ONDE** do hardware físico restante do projeto.

## Inventário físico confirmado

| bloco | evidência real | papel | status |
| --- | --- | --- | --- |
| BPM | `hardware/firmware/BPM - BACKPLANE MANAGER MODULE` | gateway embarcado em ESP32 | `IMPLEMENTADO` |
| GSA | `hardware/firmware/GSA - Gerador de sinais analógicos`, `hardware/boards/GSA -gerador-sinais-analogicos` | baby board analógica | `IMPLEMENTADO` |
| backplane raiz SimulDIESEL | `hardware/boards/SimulDIESEL` | estrutura reservada da bancada | `PARCIALMENTE IMPLEMENTADO` |
| interligação BPM <-> GSA | `SdgwDefs.h`, `config.h`, esquemático da GSA | `I2C`, `IRQ`, reset | `IMPLEMENTADO` |
| X-CONN / chicote / módulo em teste | documentação viva e slots estruturais do host | interface inferior da bancada | `PARCIALMENTE IMPLEMENTADO` |

## Empilhamento físico real

```text
Host local
  -> BPM
  -> barramento físico BPM <-> GSA
  -> GSA
  -> backplane / X-CONN / chicote
  -> módulo em teste
```

## Limite de evidência desta camada

O diretório `hardware/boards/SimulDIESEL` existe, mas o arquivo `SimulDIESEL.kicad_sch` ainda não materializa um esquemático populado equivalente ao nível de detalhe da GSA.

Por isso:

- a posição estrutural do backplane é clara;
- a pinagem completa da placa mãe da bancada ainda não está no mesmo nível de confirmação da GSA.

## Papel desta página na árvore

Esta página não descreve handshake, parser ou fila de eventos. Ela só fixa onde cada parte física confirmada está na bancada.

## Glossário

- **Bancada**: conjunto físico de host, gateway, boards e módulo em teste.
- **Backplane**: base física de interligação da bancada.
- **Baby board**: placa especializada conectada à bancada.
- **X-CONN**: interface física entre bancada e chicote do módulo.

## Próximas camadas

- [Backplane](../03-hardware/01-backplane.md)
