⚠️ Documento histórico. Pode não refletir a arquitetura atual do SimulDIESEL.

# 📘 CONTRATO OFICIAL – GATEWAY ↔ BABY BOARDS
**Versão:** 1.2  
**Status:** Ativo  
**Compatível com:** CONTRATO_CENTRAL v1.0

---

## 1) Objetivo

Padronizar o comportamento do Gateway para endereçamento, roteamento e conversão de mensagens entre:

PC (SGGW) → Gateway → Baby Boards (I2C TLV+CRC)

---

## 2) Endereçamento (SGGW CMD)

`CMD = [ADDR:4][OP:4]`

- `ADDR=0x0` → Gateway local
- `ADDR=0x1` → GSA (exemplo)
- `ADDR=0xF` → Broadcast (reservado)

---

## 3) Ponto Único de Interpretação

O Gateway interpreta `ADDR/OP` apenas em:

**`GatewayApp::onCommand(cmd, flags, seq, data, dataLen)`**

---

## 4) Roteamento por DeviceTable

O roteamento é definido por uma tabela:

`ADDR lógico → BUS → endereço físico`

Exemplo:

| ADDR | Board | BUS | Endereço físico |
|------|-------|-----|-----------------|
| 0x1  | GSA   | I2C | 0x23            |

---

## 5) Encaminhamento no barramento (I2C)

O Gateway monta e envia para a baby board:

`[T=OP][L=dataLen][V=data...][CRC8]`

CRC: CRC-8/ATM sobre `[T L V]`.

A resposta da baby board também é TLV+CRC.

---

## 6) Exemplo – Ligar LED da GSA

### PC → Gateway (SGGW)
- Destino: `ADDR=0x1 (GSA)`
- Operação: `OP=0x1 (SET)`

Logo: `cmd = 0x11`.

Payload (subcomandos do GSA):
- `V[0] = 0x01` (SUBCMD_LED)
- `V[1] = 0x01` (state ON)

SGGW:
- `cmd = 0x11`
- `data = [0x01, 0x01]`

### Gateway → GSA (I2C)
Monta TLV do device:
- `T = OP = 0x01 (SET)`
- `L = 0x02`
- `V = [0x01, 0x01]`
- `CRC = crc8([T L V])`

TX I2C:
- `[0x01][0x02][0x01][0x01][CRC]`

### GSA → Gateway (I2C)
Exemplo de ACK simples:
- `[0x01][0x00][CRC]`

### Gateway → PC (SGGW EVENT)
- `cmd` do evento = `0x11`
- `payload` = TLV sem CRC no formato do app:
  - `[T=0x11][L=0x00]`

---

## 7) Erros vindos de baby boards

- CRC inválido no device: o device armazena erro.
- O Gateway consulta via `GET + SUBCMD_ERR`.
- Erros internos do Gateway continuam podendo ser enviados como TLV `T=0xFE` no payload do evento.

---

**CONTRATO_GATEWAY v1.2**

