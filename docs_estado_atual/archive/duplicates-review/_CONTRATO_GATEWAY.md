# 📘 CONTRATO OFICIAL -- GATEWAY ↔ BABY BOARDS

**Versão:** 1.0\
**Status:** Ativo

------------------------------------------------------------------------

# 1️⃣ OBJETIVO

Padronizar:

-   Endereçamento lógico
-   Roteamento interno do Gateway
-   Comunicação Gateway ↔ Baby Board
-   Formato de comandos
-   Formato de respostas
-   Expansão futura

------------------------------------------------------------------------

# 2️⃣ CAMADAS DO SISTEMA

PC (App)\
↓ SGGW (UART + COBS + CRC)\
Gateway (roteamento lógico)\
↓ I2C (TLV + CRC)\
Baby Board (ex: GSA)

------------------------------------------------------------------------

# 3️⃣ ENDEREÇAMENTO LÓGICO (SGGW CMD)

## 3.1 Formato do byte CMD (1 byte)

CMD = \[ADDR:4\]\[OP:4\]

  Bits   Significado
  ------ ------------------------------
  7..4   Endereço lógico (Baby Board)
  3..0   Operação direta (0..15)

------------------------------------------------------------------------

## 3.2 Endereços reservados

  ADDR   Destino
  ------ -----------------------
  0x0    Gateway (local)
  0x1    GSA
  0xF    Broadcast (reservado)

------------------------------------------------------------------------

# 4️⃣ TABELA DE ROTEAMENTO (Gateway)

ADDR lógico → BUS → endereço físico

Exemplo:

  ADDR   BUS   Endereço físico
  ------ ----- -----------------
  0x1    I2C   0x23 (GSA)

⚠️ ADDR lógico não é endereço I2C.

------------------------------------------------------------------------

# 5️⃣ FLUXO COMPLETO -- COMANDO NORMAL

## Exemplo: Ligar LED da GSA

### App envia (SGGW):

CMD = 0x11 (ADDR=0x1, OP=0x1)\
Payload = \[0x01\]

### Gateway interpreta:

addr = cmd \>\> 4\
op = cmd & 0x0F

Se addr == 0 → trata local\
Se addr != 0 → rotear

### Gateway monta TLV da baby board:

Formato I2C: \[T\]\[L\]\[V...\]\[CRC\]

Exemplo: \[0x01\]\[0x01\]\[0x01\]\[CRC\]

### GSA responde:

\[0x01\]\[0x00\]\[CRC\]

### Gateway responde ao App:

cmd = 0x11\
payload = \[0x11\]\[0x00\]

------------------------------------------------------------------------

# 6️⃣ PROTOCOLO I2C (BABY BOARD)

Formato obrigatório:

TX/RX = \[T\]\[L\]\[V...\]\[CRC\]

-   CRC = CRC-8/ATM\
-   Baby board não conhece ADDR lógico\
-   OP máximo direto: 16

------------------------------------------------------------------------

# 7️⃣ REGRAS DE ERRO

## Gateway:

T=0xFE, L=1, V=código de erro

## Baby Board:

CRC inválido → armazenar erro interno\
Erro consultado via GET_ERR

------------------------------------------------------------------------

# 8️⃣ EXPANSÃO DE COMANDOS

Limite direto: 16 comandos por baby board.

Expansão deve ocorrer dentro do payload (subcomandos).

------------------------------------------------------------------------

# 9️⃣ REGRAS IMUTÁVEIS (V1.0)

1.  CMD sempre = \[ADDR:4\]\[OP:4\]\
2.  Baby board sempre fala TLV+CRC\
3.  Gateway remove CRC antes de enviar ao App\
4.  Expansões sempre via payload

------------------------------------------------------------------------

**CONTRATO_GATEWAY v1.0**
