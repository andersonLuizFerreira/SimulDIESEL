⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# UCE

Esta página responde à trilha **ONDE** para a UCE: onde a board se encaixa, quais classes compõem o firmware atual e como ela se conecta fisicamente à BPM.

## Papel estrutural

A UCE é uma board remota controlada pela BPM por `SPI` em modo slave.

Ela fica entre:

- acima: BPM por `SPI`, `CS`, `IRQ` e reset físico
- abaixo, no fluxo lógico: `Transport`, `Link`, `Service`, `LedService` e `CanService`

## Árvore física atual da UCE

Na árvore real do firmware da UCE, esses blocos não ficam mais em diretórios achatados. A organização física vigente é funcional:

- `lib/core/transport`
- `lib/core/link`
- `lib/core/service`
- `lib/core/runtime`
- `lib/services/led`
- `lib/services/can`
- `lib/protocol/tlv`
- `lib/diag/trace`
- `lib/drivers/can`
- `lib/hal/board`
- `lib/hal/transceivers`

## Arquivos e classes reais

| arquivo real | classe/bloco | acima de | abaixo de | status |
| --- | --- | --- | --- | --- |
| `src/main.cpp` | composição estática da UCE | BPM | `Transport`, `Link`, serviços | `IMPLEMENTADO` |
| `lib/app/UceApp.h` | `UceApp` | `main.cpp` | `Transport`, `Link`, `Service`, `LedService`, `CanService` | `IMPLEMENTADO` |
| `lib/core/transport/Transport.h` | `Transport` | `Link` | `SPI0` nativo da Arduino Due | `IMPLEMENTADO` |
| `lib/core/link/Link.h` | `Link` | `Transport` | `Service` | `IMPLEMENTADO` |
| `lib/core/service/Service.h` | `Service` | `Link` | `LedService`, `CanService` | `IMPLEMENTADO` |
| `lib/services/led/LedService.h` | `LedService` | `Service` | `LED_BUILTIN` | `IMPLEMENTADO` |
| `lib/services/can/CanService.h` | `CanService` | `Service` | `Sam3xCanDriver`, transceiver e `CanConfig`/`CanStatus` | `IMPLEMENTADO` |
| `lib/drivers/can/Sam3xCanDriver.h` | `Sam3xCanDriver` | `CanService` | periférico CAN da Arduino Due | `IMPLEMENTADO` |
| `lib/protocol/tlv/Tlv.h` | `Tlv` | `Link`, `Service` | contrato TLV da UCE | `IMPLEMENTADO` |
| `lib/diag/trace/DiagTrace.h` | `DiagTrace` | firmware inteiro | `SerialUSB` | `IMPLEMENTADO` |

## Empilhamento lógico real

```text
SPI da BPM
  -> Transport
  -> Link
  -> Service
  -> LedService / CanService
  -> LED_BUILTIN / controlador CAN da Arduino Due
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

A UCE agora também expõe configuração da porta CAN pela mesma rota compacta já existente.

Escopo efetivamente implementado no código:

- configuração de controller, bitrate e modo
- habilitação e desabilitação da interface
- leitura de status síncrono
- reset funcional da interface por comando

Encadeamento lógico da feature:

```text
frmUCE_UI
  -> FrmUceLogic
  -> UceClient
  -> BPM / GW_OP_UCE_TLV_TRANSACT
  -> Transport
  -> Link
  -> Service
  -> CanService
```

Observações importantes:

- a UI atual trabalha fixamente com `controller=can0`
- a BPM não ganhou rota nova; continua apenas roteando `0x2` para a UCE por `SPI`
- o `Service` da UCE passou a despachar CAN por `switch(tlv.t)` usando `CMD_CAN_CONFIG`, `CMD_CAN_ENABLE`, `CMD_CAN_STATUS` e `CMD_CAN_RESET`
- o LED permanece como recurso residual de validação de rota, não como foco funcional da tela
- esta rodada confirmou compilação de host, BPM e UCE, mas não registrou validação física em bancada da feature CAN

## Comentário orientado a código

Em `Transport::begin()`, este trecho fixa a característica elétrica mais delicada da integração:

```cpp
PIOA->PIO_PDR = kUceSpiSignalPins;
PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
PIOA->PIO_PUER = kUceSpiNativeCsPin;
```

Ele mantém `PA28/NPCS0` sob controle do periférico `SPI0` e, ao mesmo tempo, polariza o `CS` em nível alto quando a BPM não está selecionando a UCE.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
