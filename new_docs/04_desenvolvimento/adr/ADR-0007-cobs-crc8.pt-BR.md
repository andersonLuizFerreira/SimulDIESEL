# ADR-0007 — Framing COBS + CRC-8 no transporte serial (SGGW)

- **Status:** Aceita
- **Data:** 2026-02-28
- **Contexto:** Gateway (ESP32) ↔ PC (API local) via Serial/USB

## Contexto

O SimulDIESEL usa transporte serial entre o PC e o Gateway.

Em transporte serial, é comum ocorrer:

- perda de alinhamento de quadro
- bytes corrompidos
- necessidade de re-sincronização
- delimitação de frames em stream contínuo

## Decisão

Adotar:

1. **COBS** para framing, usando `0x00` como delimitador de frame.
2. **CRC-8/ATM** para detecção de corrupção do frame.

### Regra do frame no stream

Transmissor:

- constrói o frame cru
- calcula CRC
- aplica COBS
- adiciona `0x00`

Receptor:

- lê até `0x00`
- decodifica COBS
- valida CRC
- encaminha para a camada superior

## Motivação

### Por que COBS?
- framing robusto
- overhead pequeno e previsível
- boa re-sincronização

### Por que CRC-8/ATM?
- implementação simples
- custo baixo
- suficiente para o escopo atual

## Consequências

### Positivas
- framing robusto
- re-sincronização simples
- implementação compatível com PC e microcontrolador

### Negativas
- overhead adicional
- CRC-8 menos forte que CRC-16/32

## Referência

- `01_arquitetura/01_protocolos/sggw/`
