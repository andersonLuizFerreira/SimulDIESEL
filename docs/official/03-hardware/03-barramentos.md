⬅ [Retornar para Backplane](01-backplane.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Barramentos

Esta página fixa **onde** cada barramento físico confirmado entra na bancada.

## Barramentos confirmados

| barramento | origem | destino | evidência | status |
| --- | --- | --- | --- | --- |
| Serial | host | BPM | firmware BPM e `local-api` | `IMPLEMENTADO` |
| Bluetooth SPP | host | BPM | firmware BPM e `local-api` | `IMPLEMENTADO` |
| `I2C` físico | BPM `21/22` | GSA `A4/A5` | `SdgwDefs.h`, `config.h`, esquemático GSA | `IMPLEMENTADO` |
| `IRQ` dedicado | GSA `D4` | BPM `19` | `SdgwDefs.h`, `config.h` | `IMPLEMENTADO` |
| reset dedicado | BPM `23` | reset externo da GSA | `SdgwDefs.h`, `config.h` | `IMPLEMENTADO` |
| `I2C` lógico | GSA `D2/D3` | `TCA9548A` + `MCP4725` | `config.h`, firmware GSA | `IMPLEMENTADO` |
| `SPI` | BPM `18/26/25` | slot futuro | firmware BPM | `PARCIALMENTE IMPLEMENTADO` |

## Empilhamento físico dos barramentos

```text
Host
  -> Serial / Bluetooth
  -> BPM
  -> I2C físico / IRQ / reset
  -> GSA
  -> I2C lógico
  -> TCA9548A / MCP4725
```

## Comentário orientado a código

Em `GwSpiBus::begin(...)`, o firmware fixa a pinagem `SPI` explicitamente:

```cpp
_spi.begin(_sckPin, _misoPin, _mosiPin, -1);
```

Isso existe para não usar o mapeamento padrão do ESP32, que conflitaria com o reset global em `GPIO23`.

Em `Transport::setIrqActive(...)` da GSA, a linha de `IRQ` é tratada como open-drain por software:

```cpp
if (active) {
  pinMode(GSA_IRQ_PIN, OUTPUT);
  digitalWrite(GSA_IRQ_PIN, LOW);
} else {
  digitalWrite(GSA_IRQ_PIN, LOW);
  pinMode(GSA_IRQ_PIN, INPUT);
}
```

Esse trecho existe para a GSA poder sinalizar evento à BPM sem dirigir a linha em nível alto.

## Glossário

- **I2C físico**: barramento entre gateway BPM e GSA.
- **I2C lógico**: barramento interno da GSA para periféricos.
- **IRQ**: linha dedicada de sinalização assíncrona.
- **Open-drain por software**: técnica em que o firmware só força nível baixo e libera a linha no estado inativo.

## Próximas camadas

- Esta é uma página terminal do ramo físico. Os protocolos associados aos barramentos ficam no ramo lógico.
