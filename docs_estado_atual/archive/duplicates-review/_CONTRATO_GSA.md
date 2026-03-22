⚠️ Documento histórico. Pode não refletir a arquitetura atual do SimulDIESEL.

# 📘 CONTRATO OFICIAL -- GSA (Gerador de Sinais Analógicos)

**Versão:** 1.2\
**Status:** Ativo

------------------------------------------------------------------------

# 1️⃣ OBJETIVO

Definir as regras obrigatórias de funcionamento da Baby Board GSA dentro
da arquitetura SimulDIESEL.

Este documento é complementar ao CONTRATO_GATEWAY.md.

------------------------------------------------------------------------

# 2️⃣ POSIÇÃO NA ARQUITETURA

PC (App)\
↓ SGGW\
Gateway\
↓ I2C (TLV + CRC)\
GSA

------------------------------------------------------------------------

# 3️⃣ RESPONSABILIDADES DO GSA

O GSA:

1.  NÃO gerencia link.
2.  NÃO interpreta endereço lógico.
3.  NÃO conhece SGGW.
4.  NÃO interpreta ADDR (nibble alto).
5.  Processa apenas OP (0..15).

------------------------------------------------------------------------

# 4️⃣ CAMADAS INTERNAS DO GSA

Estrutura obrigatória:

-   Transport (I2C slave)
-   Link (TLV + CRC + buffer de erro)
-   Service
    -   Set
    -   Get
    -   Config

------------------------------------------------------------------------

# 5️⃣ PROTOCOLO I2C (OBRIGATÓRIO)

Formato único:

\[T\]\[L\]\[V...\]\[CRC\]

Onde:

-   T = OP (0..15)
-   L = tamanho do payload
-   V = dados
-   CRC = CRC-8/ATM sobre \[T L V\]

------------------------------------------------------------------------

# 6️⃣ ESTRUTURA DE COMANDOS (OP 0..15)

Comandos são GENÉRICOS.\
Funcionalidades específicas são definidas via SUBCOMANDOS no payload.

  OP    Função
  ----- ------------
  0x0   SOFT_RESET
  0x1   SET
  0x2   GET
  0x3   CONFIG

------------------------------------------------------------------------

# 7️⃣ SUBCOMANDOS (Dentro do Payload)

Formato geral:

V\[0\] = SUBCOMANDO\
V\[1..\] = Dados específicos

------------------------------------------------------------------------

## 🔹 SUBCOMANDO: LED

  Código   Significado
  -------- -------------
  0x01     LED

### SET LED

T = 0x01 (SET)\
L = 0x02\
V\[0\] = 0x01 (LED)\
V\[1\] = 0x00 ou 0x01

### GET LED

T = 0x02 (GET)\
L = 0x01\
V\[0\] = 0x01

------------------------------------------------------------------------

## 🔹 SUBCOMANDO: ERR

  Código   Significado
  -------- -------------
  0x02     ERR

### GET ERR

T = 0x02 (GET)\
L = 0x01\
V\[0\] = 0x02

Retorno: V\[0\] = código do erro atual

### SET ERR CLEAR

T = 0x01 (SET)\
L = 0x02\
V\[0\] = 0x02 (ERR)\
V\[1\] = 0x00 (CLEAR)

------------------------------------------------------------------------

# 8️⃣ PROCESSAMENTO INTERNO

Fluxo:

1.  Transport recebe dados I2C
2.  Link valida CRC
3.  Se CRC inválido → armazena erro interno
4.  Service identifica OP
5.  Service identifica SUBCOMANDO
6.  Executa ação
7.  Monta TLV resposta
8.  Link adiciona CRC
9.  Transport envia resposta

------------------------------------------------------------------------

# 9️⃣ REGRAS IMUTÁVEIS (V1.2)

1.  Sempre TLV+CRC no I2C.
2.  CRC-8/ATM obrigatório.
3.  GSA nunca interpreta ADDR lógico.
4.  GSA nunca gerencia link.
5.  Expansão sempre via subcomando no payload.
6.  ERR é tratado como subcomando (não como OP direto).

------------------------------------------------------------------------

**CONTRATO_GSA v1.2**

