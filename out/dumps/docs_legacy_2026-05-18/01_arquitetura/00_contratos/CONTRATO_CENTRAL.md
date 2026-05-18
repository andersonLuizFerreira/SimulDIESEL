⚠️ Documento histórico. Pode não refletir a arquitetura atual do SimulDIESEL.

# 📘 CONTRATO CENTRAL – GATEWAY ↔ BABY BOARDS (SimulDIESEL)
**Versão:** 1.0  
**Status:** Ativo  
**Escopo:** Este documento é a *fonte da verdade* para endereçamento, roteamento e formato de mensagens entre PC → Gateway → Baby Boards.

---

## 1) Visão Geral

O sistema possui duas fronteiras principais:

1. **PC ↔ Gateway** via **SGGW** (UART + framing/CRC do SGGW).  
2. **Gateway ↔ Baby Board** via **I2C** usando **TLV + CRC8/ATM**.

Este contrato define regras imutáveis para evitar divergência entre documentação e firmware.

---

## 2) Endereçamento no SGGW (PC → Gateway)

O campo `cmd` (1 byte) do SGGW é sempre:

`CMD = [ADDR:4][OP:4]`

- **ADDR (bits 7..4)**: endereço lógico do destino.
- **OP (bits 3..0)**: operação direta (0..15).

Endereços reservados:

- `ADDR=0x0` → Gateway (local)
- `ADDR=0xF` → Broadcast (reservado)

Cada baby board recebe um `ADDR` próprio (ex: `0x1 = GSA`).

---

## 3) Roteamento no Gateway

O Gateway **interpreta ADDR/OP em um único ponto**: `GatewayApp::onCommand()`.

Fluxo obrigatório:

1. Extrair `addr = cmd >> 4` e `op = cmd & 0x0F`
2. Se `addr == 0x0` → tratar local
3. Se `addr != 0x0` → consultar **DeviceTable**: `ADDR lógico → BUS → endereço físico`
4. Encaminhar para o bus correspondente (I2C/SPI)

> `ADDR lógico` **não é** endereço físico (ex: não é endereço I2C).

---

## 4) Protocolo Gateway ↔ Baby Board (I2C)

Formato único no barramento (TX e RX):

`[T][L][V...][CRC]`

- `T = OP` (0..15)
- `L = tamanho de V`
- `CRC = CRC-8/ATM` calculado sobre `[T][L][V...]`

A baby board **não conhece ADDR** e **não gerencia link**.

---

## 5) Subcomandos (expansão)

Quando OP for genérico (ex: SET/GET/CONFIG), o payload define o alvo:

- `V[0] = SUBCMD`
- `V[1..] = parâmetros`

---

## 6) Erros

- CRC inválido na baby board: comando **não é executado**; erro é armazenado internamente.
- Consulta/controle de erro é feito via **subcomando** (ex.: `GET + SUBCMD_ERR`, `SET + SUBCMD_ERR + CLEAR`).

---

## 7) Resposta do Gateway ao PC

Padrão recomendado e consistente com o firmware atual do GatewayApp:

- `cmd` do evento SGGW = o mesmo `cmd` recebido do PC.
- `payload` do evento SGGW = **TLV sem CRC** no formato do app:

`[T = cmd][L][V...]`

---

## 8) Regra de Versionamento

- Alterações neste contrato exigem incremento de versão.
- `CONTRATO_GATEWAY.md` e contratos de baby boards devem declarar compatibilidade com esta versão.

---

**CONTRATO_CENTRAL v1.0**

