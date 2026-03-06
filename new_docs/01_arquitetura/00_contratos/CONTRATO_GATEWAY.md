# CONTRATO OFICIAL --- Gateway ↔ Baby Boards

**Versão:** 1.2\
**Status:** Ativo\
**Compatível com:** CONTRATO_CENTRAL v1.0

------------------------------------------------------------------------

## 1. Objetivo

Padronizar o comportamento do Gateway para:

-   endereçamento lógico
-   roteamento
-   conversão entre SGGW e TLV

------------------------------------------------------------------------

## 2. Convenção de comando

CMD = \[ADDR:4\]\[OP:4\]

-   `ADDR=0x0` → Gateway
-   `ADDR=0x1` → GSA
-   `ADDR=0xF` → broadcast reservado

------------------------------------------------------------------------

## 3. Roteamento

O Gateway deve possuir um ponto único de decisão para roteamento.

Fluxo:

1.  extrair ADDR/OP
2.  consultar Device Table
3.  selecionar barramento
4.  enviar TLV para o dispositivo

------------------------------------------------------------------------

## 4. Encaminhamento

Mensagem enviada à Baby Board:

\[T=OP\]\[L\]\[V...\]\[CRC\]

CRC calculado sobre `[T][L][V...]`.

------------------------------------------------------------------------

## 5. Exemplo --- LED da GSA

PC → Gateway

cmd = 0x11

Payload:

V = \[SUBCMD_LED, state\]

Gateway → GSA

T = 0x01\
L = 0x02\
V = \[0x01, state\]
