# SD-GW-LINK — Especificação do Protocolo de Transporte (Gateway ↔ API)

**Projeto:** SimulDIESEL  
**Protocolo:** SD-GW-LINK  
**Versão:** 1.0.0  
**Status:** Estável (Transport Layer)

---

## 0. Documentos Relacionados

- **Especificação do Protocolo (este documento):** `spec.pt-BR.md`
- **Interface de Integração (DAL ↔ Protocolo ↔ Consumidor):** `interface.pt-BR.md`
- **Vetores de Teste (hex):** `examples/` (ver `examples/README.md`)
- **Decisão Arquitetural:** `specs/adr/ADR-0007-cobs-crc8.pt-BR.md`

## 1. Objetivo

Este documento especifica a camada de transporte binária utilizada na comunicação entre:

- **Gateway embarcado** (ex.: ESP32 Bridge)
- **API Local / Host PC**

A camada SD-GW-LINK é responsável exclusivamente por:

- Delimitação e framing de mensagens
- Integridade via CRC
- Entrega opcional via ACK de transporte
- Suporte a eventos assíncronos

A camada **não interpreta comandos de aplicação** (CAN, periféricos, firmware).

---

## 2. Escopo

### Inclui

- Framing COBS
- Estrutura de frame
- CRC-8/ATM
- Sequenciamento SEQ
- ACK/NACK de transporte
- Eventos espontâneos Gateway → API

### Não inclui

- Definição de comandos de aplicação
- Estruturas internas de payload (CAN, J1939)
- Fragmentação de firmware (reservada)

---

## 3. Terminologia

- **Frame cru:** mensagem antes do COBS
- **Frame codificado:** mensagem após COBS + delimitador `0x00`
- **Gateway:** dispositivo embarcado que interliga hardware ↔ API
- **API:** software host responsável por controle e interface
- **MTU:** tamanho máximo permitido do frame cru

---

## 4. Transporte e Delimitação

O SD-GW-LINK opera sobre um **stream de bytes 8-bit**, independente do meio físico.

### Delimitador

Todo frame transmitido termina com:

```
0x00
```

### Encoding

O conteúdo do frame cru é codificado com:

- **COBS (Consistent Overhead Byte Stuffing)**

---

## 5. MTU

O tamanho máximo do frame cru é fixado em:

- **250 bytes**

Portanto:

- **PAYLOAD máximo = 246 bytes**

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

## 7. SEQ (Sequência)

O campo `SEQ` é um contador 8-bit:

- Range: 0..255
- Contadores independentes por direção

---

## 8. FLAGS

| Bit | Nome      | Descrição |
|-----|----------|-----------|
| 0   | ACK_REQ  | Solicita ACK de transporte |
| 1   | IS_ACK   | Frame é ACK de transporte |
| 2   | IS_ERR   | Frame é erro/NACK transporte |
| 3   | IS_EVT   | Evento assíncrono Gateway→API |
| 4   | FRAG     | Reservado para fragmentação |
| 5–7 | —        | Reservados |

---

## 9. Comandos Reservados de Transporte

### `CMD=0x01` — T_ACK

Confirma recepção correta de um frame.

### `CMD=0x02` — T_ERR

Indica falha de transporte.

---

## 10. Erros de Transporte

| Código | Nome            |
|-------|-----------------|
| 0x01  | ERR_BAD_FRAME   |
| 0x02  | ERR_BUSY        |
| 0x03  | ERR_UNSUPPORTED |
| 0x04  | ERR_SEQ         |
| 0x05  | ERR_INTERNAL    |

---

## 11. ACK e Retransmissão

Frames críticos usam `ACK_REQ=1`.

Timeout padrão: **100 ms** (configurável).

---

## 12. Deduplicação

Reenvio do mesmo SEQ não reprocessa aplicação, apenas reenviar ACK.

---

## 13. Eventos Assíncronos

Gateway pode transmitir frames espontâneos com `IS_EVT=1`.

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

**Fim da Especificação SD-GW-LINK v1.0.0**
