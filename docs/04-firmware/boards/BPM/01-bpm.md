⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# BPM

Esta página responde à trilha **ONDE** para a BPM: onde a board está na pilha, quais classes a compõem e onde ela se conecta fisicamente.

## Papel estrutural

A BPM é o gateway embarcado do projeto.

Ela fica entre:

- acima: host local por Serial ou Bluetooth
- abaixo: `GwRouter`, `I2C`, `SPI` e boards remotas

## Arquivos e classes reais

| arquivo real | classe/bloco | fica acima de | fica abaixo de | status |
| --- | --- | --- | --- | --- |
| `src/main.cpp` | composição estática da BPM | host | `SdgwLink`, `GatewayApp`, barramentos | `IMPLEMENTADO` |
| `lib/SdgwTransport/SdgwTransport.h` | `SdgwTransport` | `SdgwEndpointMux` | `HardwareSerial` | `IMPLEMENTADO` |
| `lib/SdgwTransport/SdgwBluetoothEndpoint.h` | `SdgwBluetoothEndpoint` | `SdgwEndpointMux` | `BluetoothSerial` | `IMPLEMENTADO` |
| `lib/SdgwTransport/SdgwSessionOwner.h` | `SdgwSessionOwner` | `SdgwEndpointMux`, `SdgwLink` | nenhum | `IMPLEMENTADO` |
| `lib/SdgwLink/SdgwLink.h` | `SdgwLink` | host / endpoint mux | `GatewayApp` | `IMPLEMENTADO` |
| `lib/Gateway/GatewayApp.h` | `GatewayApp` | `SdgwLink` | `GwRouter` | `IMPLEMENTADO` |
| `lib/GwRouter/GwRouter.h` | `GwRouter` | `GatewayApp` | `GwI2cBus`, `GwSpiBus` | `IMPLEMENTADO` |
| `lib/GwI2cBus/GwI2cBus.h` | `GwI2cBus` | `GwRouter` | `Wire` | `IMPLEMENTADO` |
| `lib/GwSpiBus/GwSpiBus.h` | `GwSpiBus` | `GwRouter` | `SPI` | `IMPLEMENTADO` |

## Empilhamento real

```text
Serial / Bluetooth
  -> SdgwTransport / SdgwBluetoothEndpoint
  -> SdgwEndpointMux
  -> SdgwSessionOwner
  -> SdgwLink
  -> GatewayApp
  -> GwRouter
  -> GwI2cBus / GwSpiBus
```

## Conectores físicos confirmados

As definições vivas estão em `include/SdgwDefs.h`.

| função | pino BPM | destino | status |
| --- | --- | --- | --- |
| `I2C SDA` | `21` | `A4` da GSA | `IMPLEMENTADO` |
| `I2C SCL` | `22` | `A5` da GSA | `IMPLEMENTADO` |
| `IRQ` de entrada | `19` | `D4` da GSA | `IMPLEMENTADO` |
| `reset` global | `23` | reset externo da GSA | `IMPLEMENTADO` |
| `SPI SCK` | `18` | UCE `SPI SCK` | `IMPLEMENTADO` |
| `SPI MISO` | `26` | UCE `SPI MISO` | `IMPLEMENTADO` |
| `SPI MOSI` | `25` | UCE `SPI MOSI` | `IMPLEMENTADO` |
| `SPI CS` | `33` | UCE `D10 / NPCS0` | `IMPLEMENTADO` |
| `IRQ` de entrada | `27` | UCE `D2` | `IMPLEMENTADO` |
| `reset` compartilhado | `23` | reset físico da GSA e da UCE | `IMPLEMENTADO` |

## Comentário orientado a código

Em `main.cpp`, esta linha mostra onde a BPM fecha a pilha do host para o hardware:

```cpp
static GwRouter router(i2cBus, spiBus);
```

Ela existe para manter `GatewayApp` desacoplado do barramento concreto.

Em seguida:

```cpp
static GatewayApp app(sdgwLink, router);
```

Essa composição mostra que a BPM não fala direto com `Wire` ou `SPI` no nível da aplicação; ela passa pelo roteador.

## O que a BPM não é no código atual

- não é parser `SDH`
- não é regra de negócio da GSA
- não é catálogo persistente amplo de boards

Ela é, hoje, um gateway compacto e roteador físico.

## Glossário

- **BPM**: Backplane Manager Module.
- **Endpoint mux**: camada que decide qual endpoint alimenta a sessão.
- **Roteador**: camada que escolhe o barramento certo para a board remota.
- **Gateway local**: comandos tratados dentro da própria BPM.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
