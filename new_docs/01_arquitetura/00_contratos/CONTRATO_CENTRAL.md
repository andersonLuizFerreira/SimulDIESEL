# CONTRATO CENTRAL --- Gateway ↔ Baby Boards (SimulDIESEL)

**Versão:** 1.0\
**Status:** Ativo

## 1. Visão Geral

O sistema possui duas fronteiras principais:

1.  **PC ↔ Gateway** via SGGW no enlace serial.
2.  **Gateway ↔ Baby Board** via barramento interno baseado em **TLV +
    CRC-8/ATM**.

Este documento define as regras de **endereçamento, roteamento e formato
de mensagens** entre PC → Gateway → Baby Boards.

------------------------------------------------------------------------

## 2. Endereçamento

O campo `CMD` possui a seguinte convenção no contexto do Gateway:

CMD = \[ADDR:4\]\[OP:4\]

-   **ADDR** (bits 7..4): endereço lógico do destino
-   **OP** (bits 3..0): operação

Endereços reservados:

-   `0x0` → Gateway local
-   `0xF` → Broadcast reservado

Cada Baby Board recebe um endereço lógico próprio.

Exemplo:

-   `0x1` → GSA

------------------------------------------------------------------------

## 3. Roteamento no Gateway

Fluxo esperado:

1.  extrair `addr = cmd >> 4`
2.  extrair `op = cmd & 0x0F`
3.  se `addr == 0x0` → tratar localmente
4.  caso contrário → consultar **Device Table**
5.  encaminhar para o barramento correspondente

Device Table:

ADDR lógico → BUS → endereço físico

------------------------------------------------------------------------

## 4. Protocolo Gateway ↔ Baby Board

Formato:

\[T\]\[L\]\[V...\]\[CRC\]

Onde:

-   `T = OP`
-   `L = tamanho do payload`
-   `V = payload`
-   `CRC = CRC‑8/ATM` calculado sobre `[T][L][V...]`

------------------------------------------------------------------------

## 5. Expansão por Subcomandos

Quando `OP` for genérico:

V\[0\] = SUBCMD\
V\[1..\] = parâmetros

------------------------------------------------------------------------

## 6. Regras de erro

CRC inválido:

-   comando não deve ser executado
-   erro pode ser armazenado internamente
-   consulta de erro ocorre via subcomando
