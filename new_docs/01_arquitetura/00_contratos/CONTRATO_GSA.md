# CONTRATO OFICIAL --- GSA (Gerador de Sinais Analógicos)

**Versão:** 1.2\
**Status:** Ativo\
**Compatível com:** CONTRATO_CENTRAL v1.0

------------------------------------------------------------------------

## 1. Responsabilidades

O GSA:

-   não gerencia o link PC ↔ Gateway
-   não interpreta endereço lógico
-   interpreta apenas `OP` e subcomandos
-   responde via TLV + CRC

------------------------------------------------------------------------

## 2. Camadas Internas

Estrutura recomendada:

Transport → Link → Service

### Transport

Interface com barramento físico.

### Link

Validação de TLV e CRC.

### Service

Interpretação de comandos.

------------------------------------------------------------------------

## 3. Protocolo de Barramento

Formato:

\[T\]\[L\]\[V...\]\[CRC\]

-   T = operação
-   L = tamanho
-   V = payload
-   CRC = CRC‑8/ATM

------------------------------------------------------------------------

## 4. Operações

  OP    Função
  ----- --------
  0x0   RESET
  0x1   SET
  0x2   GET
  0x3   CONFIG

------------------------------------------------------------------------

## 5. Subcomandos

Formato:

V\[0\] = SUBCMD\
V\[1..\] = parâmetros

### LED

SET LED:

T=0x01\
L=0x02\
V=\[0x01,state\]

GET LED:

T=0x02\
L=0x01\
V=\[0x01\]

------------------------------------------------------------------------

## 6. Regras de erro

CRC inválido:

-   comando ignorado
-   erro pode ser registrado internamente
