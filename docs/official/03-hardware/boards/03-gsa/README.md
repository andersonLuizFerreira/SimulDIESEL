⬅ [Retornar para Construção Física das Boards](../../05-boards-fisicas.md)
⬅ [Retornar para Índice Geral](../../../../00-INDICE.md)

# GSA — Gerador de Sinais Analógicos

Esta página descreve a GSA como placa física.

## Evidências materiais usadas aqui

- `hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_sch`
- `hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE NÍVEIS/GERADOR_NIVEIS.kicad_sch`
- firmware da GSA em `hardware/firmware/GSA - Gerador de sinais analógicos`

## Estrutura física confirmada

| bloco físico | evidência | papel | status |
| --- | --- | --- | --- |
| MCU principal | `Arduino_Nano_Every` no esquemático | lógica local da board | `IMPLEMENTADO` |
| `I2C` físico em `A4/A5` | esquemático + firmware | interface com a BPM | `IMPLEMENTADO` |
| `I2C` lógico em `D2/D3` | esquemático + firmware | barramento interno da board | `IMPLEMENTADO` |
| `IRQ` em `D4` | esquemático + firmware | evento assíncrono para a BPM | `IMPLEMENTADO` |
| reset do hub em `D8` | esquemático + firmware | controle do `TCA9548A` | `IMPLEMENTADO` |
| `TCA9548A` | `GERADOR_NIVEIS.kicad_sch` | seleção de ramo | `IMPLEMENTADO` |
| `MCP4725` | firmware e subprojeto da GSA | DAC por canal | `IMPLEMENTADO` |

## Empilhamento físico da placa

```text
BPM
  -> I2C físico
  -> MCU da GSA
  -> I2C lógico
  -> TCA9548A
  -> MCP4725
  -> estágio analógico
  -> saída do canal
```

## Observações importantes

- O firmware confirma 16 canais lógicos.
- O hub `TCA9548A` divide esses 16 canais em 8 ramos com 2 DACs por ramo.
- A GSA é hoje a principal referência física do projeto; outras boards ainda não apresentam o mesmo conjunto de evidências.

## Glossário

- **MCU**: microcontrolador principal da placa.
- **Ramo**: saída selecionável do `TCA9548A`.
- **DAC**: conversor digital-analógico responsável pelo valor base do canal.

## Próximas camadas

- [Funcionamento eletrônico](01-funcionamento-eletronico.md)
