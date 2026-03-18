⚠️ Documento histórico. Pode não refletir a arquitetura atual do SimulDIESEL.

# 📘 CONTRATO OFICIAL – GSA (Gerador de Sinais Analógicos)
**Versão:** 1.2  
**Status:** Ativo  
**Compatível com:** CONTRATO_CENTRAL v1.0

---

## 1) Responsabilidades do GSA

O GSA:

1. Não gerencia link.
2. Não interpreta endereço lógico (ADDR).
3. Processa apenas OP (0..15) e subcomandos no payload.

---

## 2) Camadas Internas

- Transport (I2C slave)
- Link (TLV + CRC + buffer de erro)
- Service (dispatcher)
  - Set
  - Get
  - Config

---

## 3) Protocolo I2C

Formato único (TX e RX):

`[T][L][V...][CRC]`

- `T = OP`
- `CRC = CRC-8/ATM` sobre `[T L V]`

---

## 4) OPs (0..15)

| OP  | Função      |
|-----|------------|
| 0x0 | SOFT_RESET |
| 0x1 | SET        |
| 0x2 | GET        |
| 0x3 | CONFIG     |

---

## 5) Subcomandos

Formato:

- `V[0] = SUBCMD`
- `V[1..] = parâmetros`

### SUBCMD_LED = 0x01

**SET LED**
- `T=0x01 (SET)`
- `L=0x02`
- `V=[0x01, state]` (state: 0/1)

**GET LED**
- `T=0x02 (GET)`
- `L=0x01`
- `V=[0x01]`

### SUBCMD_ERR = 0x02

**GET ERR**
- `T=0x02 (GET)`
- `L=0x01`
- `V=[0x02]`
- Resposta: `V=[errCode]` (ex: L=1)

**SET ERR CLEAR**
- `T=0x01 (SET)`
- `L=0x02`
- `V=[0x02, 0x00]`

---

## 6) Regras de erro (CRC)

CRC inválido:
- não executa o comando
- registra o erro interno
- retorna resposta normal apenas quando consultado (GET ERR)

---

**CONTRATO_GSA v1.2**

