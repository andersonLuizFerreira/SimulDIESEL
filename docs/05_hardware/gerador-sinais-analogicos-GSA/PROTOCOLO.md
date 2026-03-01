# GSA — Protocolo (TLV + CRC-8)

## Visão geral

O firmware utiliza um protocolo baseado em **TLV (Type-Length-Value)** com **CRC-8** ao final do frame.

Estrutura geral:
- `[T][L][V...] + CRC8`

Onde:
- `T` = Tipo/Comando (1 byte)
- `L` = Length (1 byte)
- `V` = Payload (L bytes)
- `CRC8` = 1 byte (calculado sobre TLV, conforme implementação do firmware)

## Tamanho máximo

- `TLV_MAX_LEN = 32` bytes  
  (definido em `defs.h`)

## Comandos identificados

Definidos em `defs.h`:

- `CMD_GET_ERR (0x01)` — obter erros
- `CMD_CLR_ERR (0x02)` — limpar erros
- `CMD_GET_LED (0x11)` — obter estado do LED
- `CMD_SET_LED (0x12)` — alterar estado do LED

## Exemplo (SET_LED)

Exemplo de frame informado:

- `[0x12][0x01][0x01][CRC]`

Interpretação:
- `T = 0x12` (`CMD_SET_LED`)
- `L = 0x01`
- `V = 0x01` (valor/payload de 1 byte)
- `CRC` = CRC-8 do frame TLV (T+L+V), conforme implementação

> Detalhes de endian, resposta exata, semântica de `V` para LED (ex.: 0/1) e comportamento do GET_LED/GET_ERR: **não identificado no resumo**.