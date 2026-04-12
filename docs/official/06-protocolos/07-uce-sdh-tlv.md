⬅ [Retornar para Protocolos e Contratos](README.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Contrato UCE SDH <-> SDGW <-> TLV

## Objetivo

Este documento fixa o contrato lógico hoje implementado para a UCE.

Nesta entrega, a UCE expõe um caso funcional compacto e validado em bancada: o comando `UCE.led`.

## Endereçamento lógico

| camada | valor | observação |
| --- | --- | --- |
| endereço lógico SDGW | `0x2` | `GW_ADDR_UCE` |
| operação compacta | `0x0` | `GW_OP_UCE_TLV_TRANSACT` |
| comando compacto | `0x20` | `SDGW_CMD_UCE_TLV` |

## Comando SDH suportado

Forma canônica:

```text
UCE.led set state=on
UCE.led set state=off
```

Regras de validação:

- `target` deve ser `UCE.led`
- `op` deve ser `set`
- `state` deve ser `on` ou `off`

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

```text
frmUCE_UI
  -> FrmUceLogic
  -> UceClient
  -> SdhClient
  -> SdhValidator / SdhToSdgwMapper
  -> SdgwSession
  -> BPM / GwRouter / GwSpiBus
  -> UCE / Transport / Link / Service / LedService
```

## Observações elétricas relevantes

- a BPM usa `GPIO33` como `CS` da UCE
- a UCE usa `PA28/NPCS0` em função periférica `SPI0`
- o pull-up do `CS` na UCE é habilitado por registrador PIO, não por `pinMode`
- a `IRQ` da UCE sobe para a BPM por `D2 -> GPIO27`
- a BPM lê a `IRQ` com `INPUT_PULLUP`

## Próximas camadas

- [Boards de Firmware da UCE](../04-firmware/boards/UCE/11-uce.md)
