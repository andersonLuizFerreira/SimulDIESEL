# SD-GW-LINK — Gateway ↔ API Transport Protocol

**Projeto:** SimulDIESEL  
**Protocolo:** SD-GW-LINK  
**Versão:** 1.0.0  
**Status:** Estável  

---

## Visão Geral

O **SD-GW-LINK** é um **protocolo de transporte binário independente de meio físico**,
utilizado na comunicação entre:

- **Gateway embarcado** (ex.: ESP32)
- **API Local / Host PC**

O protocolo opera sobre um **stream de bytes 8-bit**, podendo ser transportado por:

- Serial (UART / USB-CDC)
- Wi-Fi
- Bluetooth
- Outros meios equivalentes

---

## Responsabilidades do Protocolo

O SD-GW-LINK é responsável exclusivamente por:

- Framing e delimitação de frames via **COBS**
- Integridade de dados via **CRC-8/ATM**
- Sequenciamento de mensagens (**SEQ**)
- ACK/NACK opcional de transporte (Stop-and-Wait)
- Suporte a eventos assíncronos

> ❗ O protocolo **não** interpreta comandos de aplicação  
> (CAN, periféricos, firmware, lógica de negócio).

---

## Documentação Oficial

### 📄 Especificação do Protocolo (framing)

Define o funcionamento interno do protocolo, formato de frame,
CRC, flags, ACK e exemplos em hex.

- [`spec.pt-BR.md`](spec.pt-BR.md)

---

### 🔌 Interface de Integração (contratos)

Define **como o protocolo se conecta**:

- à camada inferior (DAL / transporte físico)
- à camada superior (consumidor / aplicação)

Sem definir linguagem, POO ou detalhes físicos.

- [`interface.pt-BR.md`](interface.pt-BR.md)

---

## Exemplos e Vetores de Teste

Os vetores oficiais de teste (frames completos no stream)
estão disponíveis em:

- [`examples/`](examples/)
- [`examples/README.md`](examples/README.md)

Arquivos incluídos:

- `ping.hex`
- `ack.hex`
- `event-level.hex`
- `payload-with-zero.hex`

---

## Decisões de Arquitetura (ADR)

Decisões técnicas relevantes estão documentadas em:

- `specs/adr/ADR-0007-cobs-crc8.pt-BR.md`

---

## Escopo do Repositório

Este diretório contém **somente documentação do protocolo**.

Implementações específicas (ESP32, C#, Wi-Fi, Serial, etc.)
devem residir em seus respectivos módulos de firmware ou software.

---

**Fim — SD-GW-LINK**
