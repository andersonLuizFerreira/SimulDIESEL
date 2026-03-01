# SGGW — Especificação do Protocolo de Transporte (Gateway ↔ API)

**Projeto:** SimulDIESEL  
**Protocolo:** SGGW  
**Versão:** 1.0.0  
**Status:** Estável (Transport Layer)

---

## 0. Documentos Relacionados

- **Especificação do Protocolo (este documento):** `spec.pt-BR.md`
- **Interface de Integração (DAL ↔ Protocolo ↔ Consumidor):** `interface.pt-BR.md`
- **Vetores de Teste (hex):** `examples/` (ver `examples/README.md`)
- **Decisão Arquitetural:** `docs/04_desenvolvimento/adr/ADR-0007-cobs-crc8.pt-BR.md`

## 1. Objetivo

Este documento especifica a camada de transporte binária utilizada na comunicação entre:

- **Gateway embarcado** (ex.: ESP32 Bridge)
- **API Local / Host PC**

A camada SGGW é responsável exclusivamente por:

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

### Fronteira de framing entre enlaces

- **PC ↔ Gateway:** usa framing SGGW (`CMD|FLAGS|SEQ|PAYLOAD|CRC8` + COBS + delimitador `0x00`).
- **Gateway ↔ Baby Board:** **não** usa framing SGGW; usa protocolo de barramento (ex.: TLV + CRC em I2C/SPI), conforme contratos de integração.

---

## 3. Terminologia

- **Frame cru:** mensagem antes do COBS
- **Frame codificado:** mensagem após COBS + delimitador `0x00`
- **Gateway:** dispositivo embarcado que interliga hardware ↔ API
- **API:** software host responsável por controle e interface
- **MTU:** tamanho máximo permitido do frame cru

---

## 4. Transporte e Delimitação

O SGGW opera sobre um **stream de bytes 8-bit**, independente do meio físico.

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

## 7. Interpretação do CMD pelo Gateway

No enlace PC → Gateway, o campo `CMD` (1 byte) é interpretado como:

```
CMD = [ADDR:4][OP:4]
```

Esta decodificação por nibble é uma convenção da aplicação Gateway; no SGGW, `CMD` permanece definido como um byte opaco de comando.

- `ADDR = CMD >> 4`
- `OP = CMD & 0x0F`

Comportamento esperado no Gateway:

- `ADDR=0x0`: comando local do próprio Gateway.
- `ADDR=0xF`: broadcast reservado.
- `ADDR` diferente de `0x0` e `0xF`: roteamento para a Baby Board destino via tabela de dispositivos e barramento correspondente.

## 8. SEQ (Sequência)

O campo `SEQ` é um contador 8-bit:

- Range: 0..255
- Contadores independentes por direção

---

## 9. FLAGS

| Bit | Nome      | Descrição |
|-----|----------|-----------|
| 0   | ACK_REQ  | Solicita confirmação de transporte (ACK/ERR) |
| 1   | IS_EVT   | Evento assíncrono Gateway→API |
| 2–7 | RESERVED | Reservados e **devem ser 0** |

Numeração de bits: `ACK_REQ` é o bit 0 (LSB), `IS_EVT` é o bit 1 e os bits 2..7 são reservados.

---

## 10. Comandos Reservados de Transporte

### `CMD=0xF1` — ACK

ACK: `CMD=0xF1`, enviado em resposta a frames com `ACK_REQ=1`; o `SEQ` no cabeçalho correlaciona a confirmação.

### `CMD=0xF2` — ERR

ERR: `CMD=0xF2`, enviado em resposta a frames com `ACK_REQ=1`; incluir `err_code` no payload; o `SEQ` no cabeçalho correlaciona o erro.
Formato mínimo de payload do ERR: `PAYLOAD[0]=err_code` (1 byte), `PAYLOAD[1..]` detalhes opcionais.

---

## 11. Erros de Transporte

| Código | Nome            |
|-------|-----------------|
| 0x01  | ERR_BAD_FRAME   |
| 0x02  | ERR_BUSY        |
| 0x03  | ERR_UNSUPPORTED |
| 0x04  | ERR_SEQ         |
| 0x05  | ERR_INTERNAL    |

---

## 12. ACK e Retransmissão

Frames críticos usam `ACK_REQ=1`.

Timeout padrão: **100 ms** (configurável).

---

## 13. Deduplicação

Reenvio do mesmo SEQ não reprocessa aplicação, apenas reenviar ACK.

---

## 14. Eventos Assíncronos

Gateway pode transmitir frames espontâneos com `IS_EVT=1`.

---

## 15. CRC Oficial

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

**Fim da Especificação SGGW v1.0.0**
