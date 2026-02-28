# SimulDIESEL — Master Specification (pt-BR)

## 1. Objetivo

O SimulDIESEL é uma plataforma modular para simulação, diagnóstico e validação de módulos eletrônicos automotivos.

O sistema permite:

- Simulação de sinais analógicos e digitais
- Simulação de comunicação CAN
- Comunicação estruturada via protocolo próprio (SGGW)
- Arquitetura modular baseada em Gateway + Baby Boards

---

## 2. Arquitetura Macro

A arquitetura é composta por quatro blocos principais:

### 2.1 PC Application
- UI (WinForms)
- BLL (Business Logic Layer)
- DAL (Serial Transport Layer)
- Comunicação via Serial/USB

### 2.2 Gateway (ESP32)
- Bridge entre PC e hardware modular
- Implementa protocolo SGGW
- Roteamento para I2C/SPI
- DeviceTable estática
- Controle de estados

### 2.3 Baby Boards
Placas especializadas conectadas ao Gateway:

- GSA (Gerador de Sinais Analógicos)
- Outras futuras (Relés, CAN, etc.)

### 2.4 Backplane
Infraestrutura física modular para conexão das placas.

---

## 3. Protocolo de Comunicação

### 3.1 SGGW

O protocolo SGGW utiliza:

- TLV (Type-Length-Value)
- Framing via COBS
- Delimitador `0x00`
- CRC-8/ATM (poly `0x07`)

Detalhes completos:
`docs/01_arquitetura/01_protocolos/sggw/`

---

## 4. Organização do Repositório

docs/  
hardware/  
firmware/  
src/  

A documentação está organizada por domínio técnico:

- 00_visao-geral → visão consolidada
- 01_arquitetura → contratos, protocolos e integração
- 04_desenvolvimento → roadmap e ADRs
- 05_hardware → especificações físicas

---

## 5. Princípios Arquiteturais

- Separação clara de camadas
- Transporte desacoplado do protocolo
- Protocolo desacoplado da UI
- Estados explícitos
- Evolução incremental por versão
- Cada feature em branch própria

---

## 6. Estado Atual

- Gateway operacional
- Protocolo SGGW definido
- Framing via COBS + CRC-8 formalizado (ADR-0007)
- CI automatizado para firmware
- Estrutura documental consolidada
