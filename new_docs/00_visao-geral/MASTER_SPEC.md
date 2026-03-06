# SimulDIESEL — Master Specification (pt-BR)

## 1. Objetivo

O SimulDIESEL é uma plataforma modular para simulação, diagnóstico e validação de módulos eletrônicos automotivos.

O sistema permite:

- simulação de sinais analógicos e digitais
- simulação de comunicação CAN
- comunicação estruturada entre PC e Gateway
- arquitetura modular baseada em Gateway + Baby Boards

---

## 2. Arquitetura Macro

A arquitetura é composta por quatro blocos principais.

### 2.1 Aplicação PC (.NET)

- UI WinForms
- BLL
- DAL
- DTL e contratos de protocolo
- comunicação com o Gateway por porta serial

### 2.2 Gateway (ESP32)

- ponte entre PC e hardware modular
- handshake inicial com a aplicação PC
- transporte SGGW no enlace PC ↔ Gateway
- roteamento para barramentos internos
- integração com Baby Boards

### 2.3 Baby Boards

Placas especializadas conectadas ao Gateway, por exemplo:

- GSA (Gerador de Sinais Analógicos)
- futuras placas de relés, alimentação, comunicação e outras

### 2.4 Backplane

Infraestrutura física modular para conexão das placas.

---

## 3. Comunicação

### 3.1 Enlace PC ↔ Gateway

O enlace entre a aplicação PC e o Gateway utiliza:

- abertura de transporte serial
- handshake textual inicial
- framing binário SGGW após o estado `Linked`
- COBS
- delimitador `0x00`
- CRC-8/ATM
- ACK/ERR de transporte opcionais
- health check por ping periódico

Documentação principal:

- `01_arquitetura/04_pc/`
- `01_arquitetura/01_protocolos/sggw/`

### 3.2 Enlace Gateway ↔ Baby Boards

O enlace Gateway ↔ Baby Board não utiliza framing SGGW.

Ele utiliza protocolo de barramento, atualmente documentado como:

- TLV
- CRC-8/ATM
- roteamento por tabela de dispositivos

Documentação principal:

- `01_arquitetura/00_contratos/`
- `01_arquitetura/03_gateway/`
- `01_arquitetura/05_babyboards/`

---

## 4. Organização do Repositório

Estrutura macro esperada:

- `new_docs/`
- `hardware/`
- `firmware/`
- `local-api/`

A documentação está organizada por domínio técnico.

---

## 5. Princípios Arquiteturais

- separação clara de camadas
- transporte desacoplado do protocolo
- protocolo desacoplado da UI
- estados explícitos
- evolução incremental por versão
- documentação versionada junto com o código

---

## 6. Estado Atual da Aplicação PC

Com base no código atual da solução .NET:

- `SerialTransport` implementa o transporte serial cru
- `SerialLinkService` gerencia conexão, handshake textual, health e integração geral
- `SdGwLinkEngine` implementa framing SGGW, CRC, ACK/ERR, timeout e retransmissão
- `SdGwHealthService` implementa ping periódico do link
- `SdGgwClient` expõe API tipada de alto nível para consumidores
- `SerialLink` atua como fachada estática para manter o serviço vivo durante a execução
- a UI atual consome o estado do link através de `SerialLink`

---

## 7. Referências Principais

- `01_arquitetura/00_contratos/`
- `01_arquitetura/01_protocolos/sggw/`
- `01_arquitetura/04_pc/`
- `04_desenvolvimento/adr/ADR-0007-cobs-crc8.pt-BR.md`

SimulDIESEL — Plataforma Modular de Simulação Automotiva
