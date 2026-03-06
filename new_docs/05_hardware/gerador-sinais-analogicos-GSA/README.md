# Gerador de Sinais Analógicos (GSA) — Firmware

Este diretório documenta o firmware do módulo GSA do projeto SimulDIESEL.

## Visão geral

O firmware implementa um dispositivo I2C slave que recebe e responde comandos em formato TLV com validação por CRC-8.

## Funções identificadas

- comunicação I2C via `Wire.h`
- processamento de frames TLV
- controle de LED
- gerenciamento de erros e estado interno

## Referências internas

- `PROTOCOLO.md`
- `PINOUT.md`
- `ARQUITETURA.md`
- `CONFIGURACAO.md`
- `FLUXO_EXECUCAO.md`