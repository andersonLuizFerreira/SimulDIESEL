# SGGW --- Gateway ↔ API Transport Protocol

**Projeto:** SimulDIESEL\
**Protocolo:** SGGW\
**Versão:** 1.0.0\
**Status:** Estável

------------------------------------------------------------------------

## Visão Geral

O **SGGW** é um protocolo de transporte binário utilizado entre:

-   Gateway embarcado (ex: ESP32)
-   Aplicação PC (.NET)

Ele opera sobre um **stream de bytes 8‑bit**, independente do meio
físico.

Possíveis transportes:

-   UART / Serial
-   USB‑CDC
-   Wi‑Fi
-   Bluetooth

------------------------------------------------------------------------

## Responsabilidades

O SGGW é responsável por:

-   Framing de mensagens
-   Delimitação por `0x00`
-   Codificação COBS
-   CRC‑8/ATM
-   Sequenciamento
-   ACK/NACK opcional
-   Eventos assíncronos

O protocolo **não interpreta semântica de aplicação**.

------------------------------------------------------------------------

## Documentos

-   `spec.pt-BR.md` --- especificação completa do protocolo
-   `interface.pt-BR.md` --- contrato entre transporte, engine e
    consumidor
-   `examples/` --- vetores de teste em hex

------------------------------------------------------------------------

Este diretório contém **apenas documentação do protocolo**.
Implementações pertencem aos módulos de firmware ou software.
