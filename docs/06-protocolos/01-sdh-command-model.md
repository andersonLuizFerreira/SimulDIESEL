# SDH Command Model (SGGW)

## Contexto

Este documento descreve o modelo de comando efetivamente implementado para a comunicação entre `local-api` e `ESP32 Gateway`.

## Estrutura de frame

Formato lógico:

`CMD | FLAGS | SEQ | PAYLOAD | CRC8`

- Delimitação de transporte: COBS + byte `0x00`.
- CRC: `CRC8` em `CMD+FLAGS+SEQ+PAYLOAD`.

## Flags e semântica de transporte

- `ACK_REQ (0x01)`
- `IS_EVT (0x02)`

`ACK` e `ERR` do transporte existem e são tratados pelo engine.

## Comandos implementados (enum `SggwCmd`)

- `Ping = 0x55`
- `GetVersion = 0x01` (não é foco central da implementação atual)
- `Echo = 0x02`
- `LED = 0x03`
- `LOGOUT = 0x04`

## Restrições e observações

- `CMD_ACK = 0xF1` e `CMD_ERR = 0xF2` são tratados como controle interno do transporte.
- O gateway ignora frames ACK/ERR na camada app e trata como sinais de confiabilidade.

## Contrato com GSA

- `CMD = [ADDR:4][OP:4]`.
- `ADDR` é interpretado apenas no gateway localmente.
- `OP` é repassado para `TLV T` no caminho interno.

[Retornar ao README principal](../README.md)
