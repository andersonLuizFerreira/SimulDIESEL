⬅ [Retornar para Arquitetura de Firmware](01-arquitetura-firmware.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Drivers de Firmware

Esta página lista os drivers e adaptadores realmente usados hoje pelos firmwares da BPM e da GSA.

## Drivers confirmados na BPM

| arquivo real | classe | driver/base | papel | status |
| --- | --- | --- | --- | --- |
| `lib/SdgwTransport/SdgwTransport.h` | `SdgwTransport` | `HardwareSerial` | endpoint serial `SDGW` em `115200 8N1` | `IMPLEMENTADO` |
| `lib/SdgwTransport/SdgwBluetoothEndpoint.h` | `SdgwBluetoothEndpoint` | `BluetoothSerial` | endpoint Bluetooth SPP para a mesma sessão | `IMPLEMENTADO` |
| `lib/SdgwTransport/SdgwEndpointMux.h` | `SdgwEndpointMux` | composição de endpoints | escolhe o endpoint dono da sessão | `IMPLEMENTADO` |
| `lib/GwI2cBus/GwI2cBus.h` | `GwI2cBus` | `TwoWire` | transação `TLV + CRC` com boards `I2C` | `IMPLEMENTADO` |
| `lib/GwSpiBus/GwSpiBus.h` | `GwSpiBus` | `SPIClass` | transação curta com devices `SPI` | `IMPLEMENTADO` |

## Drivers confirmados na GSA

| arquivo real | classe | driver/base | papel | status |
| --- | --- | --- | --- | --- |
| `lib/Transport/Transport.h` | `Transport` | `Wire` | slave `I2C` físico em `A4/A5` | `IMPLEMENTADO` |
| `src/main.cpp` | `SoftwareWire g_logicalI2c` | `SoftwareWire` | master `I2C` lógico em `D2/D3` | `IMPLEMENTADO` |
| `lib/Tca9548Service/Tca9548Service.h` | `Tca9548Service` | `SoftwareWire` | seleção de ramo no `TCA9548A` | `IMPLEMENTADO` |
| `lib/Mcp4725Service/Mcp4725Service.h` | `Mcp4725Service` | `SoftwareWire` | escrita e disable nos DACs `MCP4725` | `IMPLEMENTADO` |
| `lib/LedService/LedService.cpp` | `LedService` | `digitalWrite` | LED builtin para teste local | `IMPLEMENTADO` |
| `lib/EepromService/EepromService.h` | `EepromService` | `EEPROM` | persistência de offsets por canal | `IMPLEMENTADO` |

## Comentário orientado a código

Em `SdgwTransport`, a política de ownership do endpoint serial é mínima:

```cpp
bool shouldClaimOwnership() override { return _ser.available() > 0; }
```

Isso existe para permitir que a Serial só roube a sessão quando realmente houver tráfego entrando.

Em `SdgwBluetoothEndpoint`, o critério muda:

```cpp
bool shouldClaimOwnership() override
{
    return isConnected();
}
```

Aqui o Bluetooth pede ownership assim que há cliente SPP conectado, mesmo antes de bytes chegarem.

## Observações importantes

- A documentação antiga com `SggwBluetoothEndpoint` está desatualizada; o nome real vivo é `SdgwBluetoothEndpoint`.
- O caminho `SPI` existe e é inicializado na BPM, mas a tabela viva de devices ainda não publica nenhuma board `SPI`.
- Na GSA, `Wire` e `SoftwareWire` têm papéis diferentes e não devem ser tratados como o mesmo barramento.

## Glossário

- **Driver**: camada que fala diretamente com periférico, barramento ou biblioteca de base.
- **Endpoint**: origem física de bytes para a sessão `SDGW`.
- **I2C físico**: barramento entre BPM e GSA.
- **I2C lógico**: barramento interno da GSA para `TCA9548A` e `MCP4725`.
- **SPP**: Serial Port Profile usado pelo Bluetooth Classic da BPM.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
