# SGGW --- Especificação do Protocolo

Versão: 1.0.0

------------------------------------------------------------------------

## Estrutura do frame cru

CMD \| FLAGS \| SEQ \| PAYLOAD \| CRC8

  Campo     Tamanho
  --------- ---------
  CMD       1 byte
  FLAGS     1 byte
  SEQ       1 byte
  PAYLOAD   0..246
  CRC8      1 byte

MTU do frame cru: **250 bytes**.

------------------------------------------------------------------------

## Delimitação

Frames no stream são:

COBS(frame) + 0x00

------------------------------------------------------------------------

## CRC

CRC‑8/ATM

-   Poly: 0x07
-   Init: 0x00
-   RefIn/RefOut: false
-   XorOut: 0x00

CRC cobre:

CMD + FLAGS + SEQ + PAYLOAD

------------------------------------------------------------------------

## Sequência

SEQ é contador de 8 bits.

Range:

0..255

Contadores independentes por direção.

------------------------------------------------------------------------

## Flags

  Bit   Nome       Descrição
  ----- ---------- --------------
  0     ACK_REQ    Solicita ACK
  1     IS_EVT     Evento
  2‑7   Reserved   Deve ser 0

------------------------------------------------------------------------

## ACK

CMD = 0xF1

Confirmação de transporte.

## ERR

CMD = 0xF2

Erro de transporte.
