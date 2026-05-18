⬅ [Retornar para Protocolos e Contratos](README.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Contrato UCE SDH <-> SDGW <-> TLV

## Objetivo

Este documento fixa o contrato lógico hoje implementado para a UCE.

Nesta entrega, a UCE mantém o caso funcional do `LED_BUILTIN`, expõe controle CAN e atende a base SDCTP pela mesma rota compacta já existente.

## Endereçamento lógico

| camada | valor | observação |
| --- | --- | --- |
| endereço lógico SDGW | `0x2` | `GW_ADDR_UCE` |
| operação compacta | `0x0` | `GW_OP_UCE_TLV_TRANSACT` |
| comando compacto | `0x20` | `SDGW_CMD_UCE_TLV` |

## Comandos SDH suportados

Formas canônicas:

```text
UCE.led set state=on
UCE.led set state=off

UCE.can.config set controller=can0 bitrate=250 mode=normal
UCE.can.enable set controller=can0 state=on
UCE.can.status get controller=can0
UCE.can.rx poll controller=can0
UCE.can.driverLog poll controller=can0
UCE.can.tx send controller=can0 ...
UCE.can reset controller=can0
```

Regras de validação:

- `target` deve ser `UCE.led`
- `op` deve ser `set`
- `state` deve ser `on` ou `off`

Para CAN:

- `target` pode ser `UCE.can.config`, `UCE.can.enable`, `UCE.can.status`, `UCE.can.rx`, `UCE.can.driverLog`, `UCE.can.tx` ou `UCE.can`
- `op` deve ser `set`, `get` ou `reset`, conforme o target
- `controller` deve ser `can0` ou `can1`
- `bitrate` deve ser `125`, `250`, `500` ou `1000`
- `mode` deve ser `normal` ou `listen`
- `rxMode`, quando usado, seleciona o modo de recepção aceito pelo contrato UCE
- `state` deve ser `on` ou `off`

Observação de UI:

- a tela `frmUCE_UI` usa hoje apenas `controller=can0`
- o comando `UCE.can reset ...` está implementado no host e na UCE, mas não está ligado a um controle visual da UI nesta rodada

## TLV transportado entre BPM e UCE

### Request

| campo | valor |
| --- | --- |
| `type` | `0x12` |
| `len` | `0x01` |
| `value` | `0x01` para `on`, `0x00` para `off` |
| `crc` | `CRC-8/ATM` sobre `type`, `len` e `value` |

Exemplos:

```text
12 01 01 66   ; LED ON
12 01 00 CA   ; LED OFF
```

### Response síncrona de sucesso

| campo | valor |
| --- | --- |
| `type` | `0x12` |
| `len` | `0x01` |
| `value` | estado aceito pela UCE |
| `crc` | `CRC-8/ATM` |

## TLVs CAN da UCE

Todos os TLVs CAN continuam encapsulados no mesmo binding lógico-físico da UCE:

- endereço lógico `0x2`
- operação compacta `GW_OP_UCE_TLV_TRANSACT = 0x0`
- transporte BPM <-> UCE por `SPI`
- despacho interno da UCE por `switch(tlv.t)`

### `0x20` - `CMD_CAN_CONFIG`

Request com payload fixo de `3` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` |
| `1` | `bitrate_code` |
| `2` | `mode` |

Response com payload fixo de `3` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` aceito |
| `1` | `bitrate_code` aceito |
| `2` | `mode` aceito |

Semântica funcional:

- mapeia para `CanService::configure(...)`
- a UCE converte `bitrate_code` para `bitrateKbps` real antes de chamar `CanService`

### `0x21` - `CMD_CAN_ENABLE`

Request com payload fixo de `2` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` |
| `1` | `state` |

Response com payload fixo de `2` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` efetivo |
| `1` | `state` efetivo |

Semântica funcional:

- `state=on` mapeia para `CanService::open()`
- `state=off` mapeia para `CanService::close()`

### `0x22` - `CMD_CAN_STATUS`

Request com payload fixo de `1` byte:

| byte | semântica |
| --- | --- |
| `0` | `controller` |

Response com payload fixo de `4` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` |
| `1` | `interface_state` |
| `2` | `bitrate_code` |
| `3` | `mode` |

Semântica funcional:

- mapeia para `CanService::status()`
- a response é síncrona, compacta e sem canal de evento paralelo

### `0x23` - `CMD_CAN_RESET`

Request com payload fixo de `1` byte:

| byte | semântica |
| --- | --- |
| `0` | `controller` |

Response com payload fixo de `2` bytes:

| byte | semântica |
| --- | --- |
| `0` | `controller` |
| `1` | `reset_status` |

Semântica funcional:

- mapeia para `CanService::reset()`
- a base foi implementada para manter o contrato coerente, mesmo sem botão de reset na UI

## Codificações CAN implementadas

### `controller`

| código | valor |
| --- | --- |
| `0x00` | `can0` |
| `0x01` | `can1` |

### `bitrate_code`

| código | valor |
| --- | --- |
| `0x00` | `125 kbps` |
| `0x01` | `250 kbps` |
| `0x02` | `500 kbps` |
| `0x03` | `1000 kbps` |

### `mode`

| código | valor |
| --- | --- |
| `0x00` | `normal` |
| `0x01` | `listen` |

### `state` de enable

| código | valor |
| --- | --- |
| `0x00` | `off` |
| `0x01` | `on` |

### `interface_state`

| código | valor |
| --- | --- |
| `0x00` | `disabled` |
| `0x01` | `configured` |
| `0x02` | `open` |
| `0x03` | `fault` |

### `reset_status`

| código | valor |
| --- | --- |
| `0x00` | `failed` |
| `0x01` | `success` |

## Erros

### Erro funcional da UCE

| campo | valor |
| --- | --- |
| `type` | `0x7F` |
| `len` | `0x03` |
| `value[0]` | `requestType` original |
| `value[1]` | reservado |
| `value[2]` | código de erro funcional |

Erros funcionais já observáveis:

- `0x03`: estado inválido
- `0x07`: comando não suportado
- `0x08`: payload inválido
- `0x09`: CRC inválido no TLV recebido pela UCE

### Erro de gateway da BPM

| campo | valor |
| --- | --- |
| `type` | `0xFE` |
| `len` | variável |
| `value[0]` | código de erro do gateway |

Na rota da UCE, a BPM também pode anexar diagnóstico de `SPI` e `CRC` quando a falha ocorre entre o gateway e a board.

## Fluxo ponta a ponta

Na extremidade da UCE, a sequência abaixo descreve o pipeline lógico. A árvore física correspondente hoje fica em `lib/core/link`, `lib/core/transport`, `lib/core/services`, `lib/services/led`, `lib/services/can/service`, `lib/services/can/driver`, `lib/services/can/protocol`, `lib/services/can/rxhub`, `lib/services/can/table` e `lib/services/can/sdctp`.

```text
frmUCE_UI
  -> FrmUceLogic
  -> UceClient
  -> SdhClient
  -> SdhValidator / SdhToSdgwMapper
  -> SdgwSession
  -> BPM / GwRouter / GwSpiBus
  -> UCE (fluxo lógico: SpiLink -> UceTransport -> UceServiceDispatcher -> LedService / SdctpService -> CanService)
```

## Limites desta entrega

- não há loopback
- não há canal assíncrono novo da UCE no host
- o comando textual `UCE.can.rx readAll` é rejeitado pelo mapper; o caminho oficial é snapshot/buffer SDCTP
- J1939 existe no host como serviços/catálogos, mas não transforma a UCE em decodificador autônomo dentro do firmware
- a UI não seleciona `can1`; permanece fixa em `can0`
- a validação física em bancada da nova feature CAN ainda não está registrada nesta rodada

## Observações elétricas relevantes

- a BPM usa `GPIO33` como `CS` da UCE
- a UCE usa `PA28/NPCS0` em função periférica `SPI0`
- o pull-up do `CS` na UCE é habilitado por registrador PIO, não por `pinMode`
- a `IRQ` da UCE sobe para a BPM por `D2 -> GPIO27`
- a BPM lê a `IRQ` com `INPUT_PULLUP`

## Próximas camadas

- [Boards de Firmware da UCE](../04-firmware/boards/UCE/11-uce.md)
