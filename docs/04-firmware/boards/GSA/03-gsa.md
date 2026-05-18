⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# GSA

Esta página responde à trilha **ONDE** para a GSA: onde a board se encaixa, quais classes a compõem e onde ficam os dois barramentos que ela usa.

## Papel estrutural

A GSA é uma board remota controlada pela BPM.

Ela fica entre:

- acima: BPM por `I2C` físico
- abaixo: `TCA9548A`, `MCP4725`, EEPROM e saídas analógicas

## Arquivos e classes reais

| arquivo real | classe/bloco | acima de | abaixo de | status |
| --- | --- | --- | --- | --- |
| `src/main.cpp` | composição estática da GSA | BPM | `Transport`, `Link`, serviços | `IMPLEMENTADO` |
| `lib/Transport/Transport.h` | `Transport` | `Link` | `Wire` físico | `IMPLEMENTADO` |
| `lib/Link/Link.h` | `Link` | `Transport` | `Service` | `IMPLEMENTADO` |
| `lib/Service/Service.h` | `Service` | `Link` | `LedService`, `AnalogService` | `IMPLEMENTADO` |
| `lib/AnalogService/AnalogService.h` | `AnalogService` | `Service` | `BusArbiterService`, `EepromService` | `IMPLEMENTADO` |
| `lib/BusArbiterService/BusArbiterService.h` | `BusArbiterService` | `AnalogService` | `Tca9548Service`, `Mcp4725Service` | `IMPLEMENTADO` |
| `lib/Tca9548Service/Tca9548Service.h` | `Tca9548Service` | `BusArbiterService` | `SoftwareWire` | `IMPLEMENTADO` |
| `lib/Mcp4725Service/Mcp4725Service.h` | `Mcp4725Service` | `BusArbiterService` | `SoftwareWire` | `IMPLEMENTADO` |
| `lib/EepromService/EepromService.h` | `EepromService` | `AnalogService` | `EEPROM` | `IMPLEMENTADO` |

## Empilhamento real

```text
I2C físico com a BPM
  -> Transport
  -> Link
  -> Service
  -> AnalogService / LedService
  -> BusArbiterService
  -> Tca9548Service / Mcp4725Service / EepromService
  -> circuito analógico
```

## Conectores físicos confirmados

As definições vivas estão em `include/config.h` e são coerentes com o esquemático `hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_sch`.

| função | pino GSA | destino | status |
| --- | --- | --- | --- |
| `I2C SDA` físico | `A4` | BPM `21` | `IMPLEMENTADO` |
| `I2C SCL` físico | `A5` | BPM `22` | `IMPLEMENTADO` |
| `I2C SDA` lógico | `D2` | `SoftwareWire` interno | `IMPLEMENTADO` |
| `I2C SCL` lógico | `D3` | `SoftwareWire` interno | `IMPLEMENTADO` |
| `IRQ` | `D4` | BPM `19` | `IMPLEMENTADO` |
| `reset TCA` | `D8` | `TCA9548A` | `IMPLEMENTADO` |

## Mapeamento estrutural dos canais

`Tca9548Service::switchIndexForChannel(channel)` prova que dois canais compartilham cada ramo do hub:

- canais `1-2` -> `SC0`
- canais `3-4` -> `SC1`
- canais `5-6` -> `SC2`
- canais `7-8` -> `SC3`
- canais `9-10` -> `SC4`
- canais `11-12` -> `SC5`
- canais `13-14` -> `SC6`
- canais `15-16` -> `SC7`

`Mcp4725Service::addressForChannel(channel)` completa o par:

- canal ímpar -> `0x61`
- canal par -> `0x60`

## Comentário orientado a código

Em `src/main.cpp`, a linha abaixo materializa a separação física mais importante da GSA:

```cpp
static SoftwareWire g_logicalI2c(GSA_LOGICAL_I2C_SDA_PIN, GSA_LOGICAL_I2C_SCL_PIN, false);
```

Ela existe para manter o barramento interno da GSA separado do barramento físico onde a BPM a enxerga como slave.

## Limite importante desta página

Esta página diz **onde** cada bloco da GSA está.

O fluxo de aceite síncrono, enfileiramento, execução física, `IRQ`, evento `0x31` e faults fica na trilha **COMO** em `04-sdh-gateway-architecture.md` e filhos.

## Glossário

- **GSA**: Gerador de Sinais Analógicos.
- **Barramento físico**: `I2C` usado pela BPM para falar com a GSA.
- **Barramento lógico**: `I2C` interno da GSA para seus periféricos.
- **Hub**: `TCA9548A`, usado para selecionar o ramo do canal.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
