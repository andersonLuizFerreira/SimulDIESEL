# SD-GW-LINK â€” EspecificaÃ§Ã£o do Protocolo de Transporte (Gateway â†” API)

**Projeto:** SimulDIESEL  
**Protocolo:** SD-GW-LINK  
**VersÃ£o:** 1.0.0  
**Status:** EstÃ¡vel (Transport Layer)

---

## 0. Documentos Relacionados

- **EspecificaÃ§Ã£o do Protocolo (este documento):** `spec.pt-BR.md`
- **Interface de IntegraÃ§Ã£o (DAL â†” Protocolo â†” Consumidor):** `interface.pt-BR.md`
- **Vetores de Teste (hex):** `examples/` (ver `examples/SGGW_OVERVIEW.md`)
- **DecisÃ£o Arquitetural:** `specs/adr/ADR-0007-cobs-crc8.pt-BR.md`

## 1. Objetivo

Este documento especifica a camada de transporte binÃ¡ria utilizada na comunicaÃ§Ã£o entre:

- **Gateway embarcado** (ex.: ESP32 Bridge)
- **API Local / Host PC**

A camada SD-GW-LINK Ã© responsÃ¡vel exclusivamente por:

- DelimitaÃ§Ã£o e framing de mensagens
- Integridade via CRC
- Entrega opcional via ACK de transporte
- Suporte a eventos assÃ­ncronos

A camada **nÃ£o interpreta comandos de aplicaÃ§Ã£o** (CAN, perifÃ©ricos, firmware).

---

## 2. Escopo

### Inclui

- Framing COBS
- Estrutura de frame
- CRC-8/ATM
- Sequenciamento SEQ
- ACK/NACK de transporte
- Eventos espontÃ¢neos Gateway â†’ API

### NÃ£o inclui

- DefiniÃ§Ã£o de comandos de aplicaÃ§Ã£o
- Estruturas internas de payload (CAN, J1939)
- FragmentaÃ§Ã£o de firmware (reservada)

---

## 3. Terminologia

- **Frame cru:** mensagem antes do COBS
- **Frame codificado:** mensagem apÃ³s COBS + delimitador `0x00`
- **Gateway:** dispositivo embarcado que interliga hardware â†” API
- **API:** software host responsÃ¡vel por controle e interface
- **MTU:** tamanho mÃ¡ximo permitido do frame cru

---

## 4. Transporte e DelimitaÃ§Ã£o

O SD-GW-LINK opera sobre um **stream de bytes 8-bit**, independente do meio fÃ­sico.

### Delimitador

Todo frame transmitido termina com:

```
0x00
```

### Encoding

O conteÃºdo do frame cru Ã© codificado com:

- **COBS (Consistent Overhead Byte Stuffing)**

---

## 5. MTU

O tamanho mÃ¡ximo do frame cru Ã© fixado em:

- **250 bytes**

Portanto:

- **PAYLOAD mÃ¡ximo = 246 bytes**

---

## 6. Estrutura do Frame

Formato do frame cru:

```
CMD | FLAGS | SEQ | PAYLOAD | CRC8
```

| Offset | Campo    | Tamanho |
|-------|----------|---------|
| 0     | CMD      | 1 byte  |
| 1     | FLAGS    | 1 byte  |
| 2     | SEQ      | 1 byte  |
| 3..n  | PAYLOAD  | 0..246  |
| last  | CRC8     | 1 byte  |

---

## 7. SEQ (SequÃªncia)

O campo `SEQ` Ã© um contador 8-bit:

- Range: 0..255
- Contadores independentes por direÃ§Ã£o

---

## 8. FLAGS

| Bit | Nome      | DescriÃ§Ã£o |
|-----|----------|-----------|
| 0   | ACK_REQ  | Solicita ACK de transporte |
| 1   | IS_ACK   | Frame Ã© ACK de transporte |
| 2   | IS_ERR   | Frame Ã© erro/NACK transporte |
| 3   | IS_EVT   | Evento assÃ­ncrono Gatewayâ†’API |
| 4   | FRAG     | Reservado para fragmentaÃ§Ã£o |
| 5â€“7 | â€”        | Reservados |

---

## 9. Comandos Reservados de Transporte

### `CMD=0x01` â€” T_ACK

Confirma recepÃ§Ã£o correta de um frame.

### `CMD=0x02` â€” T_ERR

Indica falha de transporte.

---

## 10. Erros de Transporte

| CÃ³digo | Nome            |
|-------|-----------------|
| 0x01  | ERR_BAD_FRAME   |
| 0x02  | ERR_BUSY        |
| 0x03  | ERR_UNSUPPORTED |
| 0x04  | ERR_SEQ         |
| 0x05  | ERR_INTERNAL    |

---

## 11. ACK e RetransmissÃ£o

Frames crÃ­ticos usam `ACK_REQ=1`.

Timeout padrÃ£o: **100 ms** (configurÃ¡vel).

---

## 12. DeduplicaÃ§Ã£o

Reenvio do mesmo SEQ nÃ£o reprocessa aplicaÃ§Ã£o, apenas reenviar ACK.

---

## 13. Eventos AssÃ­ncronos

Gateway pode transmitir frames espontÃ¢neos com `IS_EVT=1`.

---

## 14. CRC Oficial

### CRC-8/ATM

- Poly: `0x07`
- Init: `0x00`
- RefIn/RefOut: `false`
- XorOut: `0x00`

CRC cobre:

```
CMD + FLAGS + SEQ + PAYLOAD
```

---

## 16. Exemplos em Hex

### 16.1 PING

```hex
05 10 01 05 AC 00
```

### 16.2 ACK

```hex
05 01 02 05 5A 00
```

### 16.3 Evento

```hex
08 22 08 33 02 03 E8 9C 00
```

### 16.4 Payload com zero

```hex
05 30 01 10 12 03 34 58 00
```

---

**Fim da EspecificaÃ§Ã£o SD-GW-LINK v1.0.0**

