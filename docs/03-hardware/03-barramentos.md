⬅ [Retornar para Backplane](01-backplane.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Barramentos

Esta página fixa **onde** cada barramento físico confirmado entra na bancada.

## Barramentos confirmados

| barramento | origem | destino | evidência | status |
| --- | --- | --- | --- | --- |
| Serial | host | BPM | firmware BPM e `local-api` | `IMPLEMENTADO` |
| Bluetooth SPP | host | BPM | firmware BPM e `local-api` | `IMPLEMENTADO` |
| `I2C` físico | BPM `21/22` | GSA `A4/A5` | `SdgwDefs.h`, `config.h`, esquemático GSA | `IMPLEMENTADO` |
| `IRQ` dedicado | GSA `D4` | BPM `19` | `SdgwDefs.h`, `config.h` | `IMPLEMENTADO` |
| `SPI` | BPM `18/26/25` | UCE `SCK/MISO/MOSI` | `SdgwDefs.h`, `config.h`, `GwSpiBus`, `lib/core/transport/Transport.*` | `IMPLEMENTADO` |
| `CS` dedicado | BPM `33` | UCE `D10 / PA28 / NPCS0` | `SdgwDefs.h`, `config.h`, `lib/core/transport/Transport.*` | `IMPLEMENTADO` |
| `IRQ` dedicado | UCE `D2` | BPM `27` | `SdgwDefs.h`, `config.h`, `GwSpiBus`, `lib/core/transport/Transport.*` | `IMPLEMENTADO` |
| reset compartilhado | BPM `23` | reset físico da GSA e da UCE | `SdgwDefs.h`, `config.h` | `IMPLEMENTADO` |
| `I2C` lógico | GSA `D2/D3` | `TCA9548A` + `MCP4725` | `config.h`, firmware GSA | `IMPLEMENTADO` |

## Empilhamento físico dos barramentos

```text
Host
  -> Serial / Bluetooth
  -> BPM
  -> I2C físico / IRQ / reset
  -> GSA
  -> I2C lógico
  -> TCA9548A / MCP4725
  -> SPI / CS / IRQ / reset
  -> UCE
```

## Comentário orientado a código

Em `GwSpiBus::begin(...)`, o firmware fixa a pinagem `SPI` explicitamente:

```cpp
_spi.begin(_sckPin, _misoPin, _mosiPin, -1);
```

Isso existe para não usar o mapeamento padrão do ESP32, que conflitaria com o reset global em `GPIO23` e quebraria o contrato físico da UCE.

Na UCE, o `CS` fica sob o periférico nativo `SPI0` e recebe pull-up por registrador PIO:

```cpp
PIOA->PIO_PDR = kUceSpiSignalPins;
PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
PIOA->PIO_PUER = kUceSpiNativeCsPin;
```

Esse trecho existe para manter `PA28/NPCS0` em função periférica, sem perder o nível alto de repouso do `CS`.

## Glossário

- **I2C físico**: barramento entre gateway BPM e GSA.
- **SPI físico**: barramento entre gateway BPM e UCE.
- **I2C lógico**: barramento interno da GSA para periféricos.
- **IRQ**: linha dedicada de sinalização assíncrona.
- **Open-drain por software**: técnica em que o firmware só força nível baixo e libera a linha no estado inativo.

## Próximas camadas

- Esta é uma página terminal do ramo físico. Os protocolos associados aos barramentos ficam no ramo lógico.
