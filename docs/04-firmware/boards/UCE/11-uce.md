⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# UCE

Esta página responde à trilha **ONDE** para a UCE: onde a board se encaixa, quais classes compõem o firmware atual e como ela se conecta fisicamente à BPM.

## Papel estrutural

A UCE é uma board remota controlada pela BPM por `SPI` em modo slave.

Ela fica entre:

- acima: BPM por `SPI`, `CS`, `IRQ` e reset físico
- abaixo, no fluxo lógico: `SpiLink`, `UceTransport`, `UceServiceDispatcher`, `LedService`, `SdctpService` e serviços CAN

## Árvore física atual da UCE

Na árvore real do firmware da UCE, a organização física vigente é:

- `lib/core/link`
- `lib/core/transport`
- `lib/core/services`
- `lib/services/led`
- `lib/services/can/service`
- `lib/services/can/driver`
- `lib/services/can/protocol`
- `lib/services/can/rxhub`
- `lib/services/can/table`
- `lib/services/can/sdctp`

## Arquivos e classes reais

| arquivo real | classe/bloco | acima de | abaixo de | status |
| --- | --- | --- | --- | --- |
| `src/main.cpp` | composição estática da UCE | BPM | `SpiLink`, `UceTransport`, serviços | `IMPLEMENTADO` |
| `lib/core/link/SpiLink.h` | `SpiLink` | `UceTransport` | `SPI0`, `CS`, `IRQ` | `IMPLEMENTADO` |
| `lib/core/transport/UceTransport.h` | `UceTransport` | `UceServiceDispatcher` | `SpiLink` | `IMPLEMENTADO` |
| `lib/core/services/UceServiceDispatcher.h` | `UceServiceDispatcher` | `UceTransport` | `LedService`, `SdctpService` | `IMPLEMENTADO` |
| `lib/services/led/LedService.h` | `LedService` | `UceServiceDispatcher` | `LED_BUILTIN` | `IMPLEMENTADO` |
| `lib/services/can/sdctp/SdctpService.h` | `SdctpService` | `UceServiceDispatcher` | `CanService`, tabelas RX/TX SDCTP | `IMPLEMENTADO` |
| `lib/services/can/service/CanService.h` | `CanService` | `SdctpService` | `CanDriver`, `CanCrudProtocol`, `CanRxHub`, tabelas CAN | `IMPLEMENTADO` |
| `lib/services/can/driver/CanDriver.h` | `CanDriver` | `CanService` | controlador CAN da Arduino Due | `IMPLEMENTADO` |
| `lib/services/can/driver/CanDriver_fake.h` | `CanDriverFake` | `CanService` | fake para validação/bench | `IMPLEMENTADO` |
| `lib/services/can/protocol/CanCrudProtocol.h` | `CanCrudProtocol` | `CanService` | payloads CRUD de tabelas CAN | `IMPLEMENTADO` |

## Empilhamento lógico real

```text
SPI da BPM
  -> SpiLink
  -> UceTransport
  -> UceServiceDispatcher
  -> LedService / SdctpService
  -> CanService
  -> LED_BUILTIN / controlador CAN da Arduino Due / tabelas CAN-SDCTP
```

## Conectores físicos confirmados

As definições vivas estão em `include/config.h` na UCE e `include/SdgwDefs.h` na BPM.

| função | pino UCE | pino BPM | observação | status |
| --- | --- | --- | --- | --- |
| `SPI SCK` | `SPI header SCK` | `18` | clock do master BPM | `IMPLEMENTADO` |
| `SPI MISO` | `SPI header MISO` | `26` | resposta da UCE para a BPM | `IMPLEMENTADO` |
| `SPI MOSI` | `SPI header MOSI` | `25` | comando da BPM para a UCE | `IMPLEMENTADO` |
| `CS / NPCS0` | `D10 / PA28` | `33` | slave select nativo do `SPI0` | `IMPLEMENTADO` |
| `IRQ` | `D2` | `27` | saída da UCE para sinalizar resposta pronta | `IMPLEMENTADO` |
| reset | `RESET` físico | `23` | compartilhado com o reset global da BPM | `IMPLEMENTADO` |

## Contrato lógico e resposta TLV

- endereço lógico da UCE no gateway: `0x2`
- operação compacta usada entre host e BPM: `GW_OP_UCE_TLV_TRANSACT = 0x0`
- rota compacta da UCE continua única para LED e CAN: `SDGW_CMD_UCE_TLV`
- comandos SDH implementados hoje:
  - `UCE.led set state=on|off`
  - `UCE.can.config set controller=can0|can1 bitrate=125|250|500|1000 mode=normal|listen`
  - `UCE.can.enable set controller=can0|can1 state=on|off`
  - `UCE.can.status get controller=can0|can1`
  - `UCE.can.rx poll controller=can0|can1`
  - `UCE.can.driverLog poll controller=can0|can1`
  - `UCE.can.tx send|direct|create|edit|delete|stop ...`
  - `UCE.can reset controller=can0|can1`
- TLVs funcionais da UCE hoje:
  - LED: `type 0x12`, `len 0x01`
  - CAN config: `type 0x20`, `len 0x03`
  - CAN enable: `type 0x21`, `len 0x02`
  - CAN status: `type 0x22`, `len 0x01` na request e `len 0x04` na response
  - CAN reset: `type 0x23`, `len 0x01` na request e `len 0x02` na response
- erro funcional da UCE: `type 0x7F`

## Observações elétricas confirmadas

- o `CS` da UCE permanece em função periférica `PA28/NPCS0` e recebe pull-up por registrador `PIOA->PIO_PUER`
- a `IRQ` da UCE é saída e não recebe `INPUT_PULLUP` no lado da UCE
- a BPM aplica `INPUT_PULLUP` no `GPIO27` para ler a `IRQ` da UCE em nível estável
- `MISO`, `MOSI` e `SCK` não exigiram pull-up externo para o caso de uso validado

## Caso de uso validado

O caso funcional já validado em bancada continua sendo o acionamento do `LED_BUILTIN` da Arduino Due.

Fluxo confirmado:

1. o host envia `UCE.led set state=on|off`
2. a BPM valida, roteia para `SPI` e seleciona a UCE por `CS`
3. a UCE monta a resposta TLV síncrona em `Link`
4. a UCE sinaliza resposta pronta por `IRQ`
5. a BPM lê `header` e `payload+CRC` em dois bursts SPI
6. o host confirma o estado aplicado do `LED_BUILTIN`

## Feature CAN nesta rodada

A UCE agora também expõe controle CAN, filas RX/TX e base SDCTP pela mesma rota compacta já existente.

Escopo efetivamente implementado no código:

- configuração de controller, bitrate e modo
- habilitação e desabilitação da interface
- leitura de status síncrono
- reset funcional da interface por comando
- polling de RX CAN e diagnóstico do driver
- envio direto e CRUD de linhas TX CAN
- tabelas SDCTP de RX/TX com snapshot/buffer no host

Encadeamento lógico da feature:

```text
frmUCE_UI
  -> FrmUceLogic
  -> UceClient
  -> BPM / GW_OP_UCE_TLV_TRANSACT
  -> SpiLink
  -> UceTransport
  -> UceServiceDispatcher
  -> SdctpService / CanService
```

Observações importantes:

- a UI atual usa `controller=can0` como caminho principal, enquanto o contrato aceita `can0` e `can1`
- a BPM não ganhou rota nova; continua apenas roteando `0x2` para a UCE por `SPI`
- o dispatcher da UCE despacha LED e SDCTP/CAN por TLVs no mesmo envelope `SDGW_CMD_UCE_TLV`
- `CMD_CAN_READ_ALL` ainda existe no firmware como compatibilidade, mas o mapper do host rejeita `UCE.can.rx readAll`; o caminho oficial é SDCTP por snapshot/buffer
- o LED permanece como recurso residual de validação de rota, não como foco funcional da tela
- esta rodada confirmou compilação de host, BPM e UCE, mas não registrou validação física em bancada da feature CAN

## Comentário orientado a código

Em `SpiLink::begin()`, este trecho fixa a característica elétrica mais delicada da integração:

```cpp
PIOA->PIO_PDR = kUceSpiSignalPins;
PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
PIOA->PIO_PUER = kUceSpiNativeCsPin;
```

Ele mantém `PA28/NPCS0` sob controle do periférico `SPI0` e, ao mesmo tempo, polariza o `CS` em nível alto quando a BPM não está selecionando a UCE.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
