# SGGW — Interface de Integração (DAL ↔ Protocolo ↔ Consumidor)

**Projeto:** SimulDIESEL  
**Protocolo:** SGGW  
**Versão:** 1.0.0  
**Status:** Estável (Interface de Integração)

---

## 1. Objetivo

Este documento define a **interface conceitual** (contratos) para integrar o motor do protocolo
**SGGW** com:

- a **camada inferior (DAL / Transporte físico)**, responsável por enviar/receber bytes; e
- a **camada superior (Consumidor / Aplicação)**, responsável por interpretar comandos e payloads de aplicação.

> Este documento **não** define detalhes físicos (baudrate, RTS/DTR, IP, sockets, etc.).  
> Tais detalhes pertencem à DAL específica (Serial, Wi‑Fi, Bluetooth).

> Fronteira explícita: o framing SGGW aplica-se somente em **PC ↔ Gateway**; no enlace **Gateway ↔ Baby Board** usa-se protocolo de barramento (ex.: TLV + CRC), sem framing SGGW.

---

## 2. Componentes e Responsabilidades

### 2.1 DAL (Data Access Layer / Transporte Físico)

A DAL é responsável por:

- abrir/fechar a conexão física;
- **enviar bytes** e **receber bytes** (stream 8-bit);
- reportar falhas físicas (desconexão, timeouts de porta, erros do driver).

A DAL **não** deve:

- interpretar COBS, CRC, SEQ, ACK, comandos;
- segmentar frames SGGW.

Exemplos de DAL:
- `SerialTransport` (COM/USB-CDC)
- `WifiTransport` (TCP/UDP/WebSocket, etc.)
- `BtTransport` (SPP/Serial BT)

---

### 2.2 Motor do Protocolo (SGGW Engine)

O Engine é responsável por:

- delimitação por `0x00` e framing via COBS;
- validação do **CRC-8/ATM**;
- parse e montagem de frames (`CMD|FLAGS|SEQ|PAYLOAD|CRC8`);
- gerenciamento de **SEQ por direção**;
- **ACK/NACK de transporte** (Stop-and-Wait opcional quando `ACK_REQ=1`);
- deduplicação de retransmissões;
- roteamento interno de `ACK`/`ERR` (`CMD=0xF1`/`CMD=0xF2`) (não sobe como “frame de aplicação” por padrão).

O Engine **não** deve:

- interpretar semântica de comandos de aplicação (CAN, periféricos, firmware);
- assumir meio físico (serial/wifi/bt).

---

### 2.3 Camada Superior (Consumidor / Aplicação)

O Consumidor é responsável por:

- definir e interpretar **comandos de aplicação** (tabela de CMDs);
- decidir se um envio exige confiabilidade (`ACK_REQ`);
- definir políticas de timeout e retries (por comando/caso de uso);
- tratar respostas de aplicação (sucesso/erro lógico);
- orquestrar casos de uso (ex.: “setar nível”, “enviar CAN”).

---

## 3. Contrato Inferior: Engine ↔ DAL

### 3.1 Interface mínima da DAL (conceitual)

A DAL deve fornecer ao Engine, no mínimo:

- **Open() / Close()**
- **Write(bytes)**: envia bytes no stream
- **OnBytesReceived(bytesChunk)**: entrega ao Engine os bytes recebidos (em blocos arbitrários)
- **OnTransportFault(fault)**: notifica erro físico (opcional, recomendado)
- **IsOpen / State** (opcional)

**Regras:**
- `OnBytesReceived` pode entregar chunks de qualquer tamanho (inclusive 1 byte).
- A DAL não deve reter bytes propositalmente; deve entregar o mais rápido possível.
- O Engine é o único responsável por recompor frames a partir do stream.

### 3.2 Requisitos de desempenho (não vinculados ao meio)

- A DAL deve suportar throughput suficiente para frames até 250 bytes.
- A DAL deve possuir buffer/loop de leitura que minimize perda de bytes em picos.
- Em caso de backpressure, a DAL deve sinalizar falhas (ex.: `Write` falhou).

---

## 4. Contrato Superior: Engine ↔ Consumidor

### 4.1 Modelo de dados (Frame de Aplicação)

O Engine expõe ao Consumidor um **AppFrame** (frame de aplicação) com:

- `Cmd` (byte)
- `Flags` (byte)
- `Seq` (byte)
- `Payload` (byte[])
- `TimestampRx` (opcional)
- `Direction` (opcional)

**Regra:** Por padrão, `ACK` e `ERR` são consumidos internamente pelo Engine
e não são entregues como `AppFrame` (exceto em modo diagnóstico).

---

### 4.2 Envio (TX) — Operações do Engine

O Engine deve oferecer uma operação conceitual equivalente a:

- **Send(cmd, payload, options)**

onde `options` inclui:

- `RequireAck` (bool): se verdadeiro, o Engine seta `ACK_REQ=1` e aplica Stop-and-Wait
- `TimeoutMs` (int): default 100 ms
- `MaxRetries` (int): política definida pelo Consumidor
- `IsEvent` (bool): se verdadeiro, o Engine seta `IS_EVT=1` (quando aplicável)
- `AdditionalFlags` (byte): permitir flags adicionais quando necessário

**Resultado do envio (contrato):**
- Para `RequireAck=false`: entrega “aceite local” (enfileirado para TX) ou falha imediata (DAL indisponível).
- Para `RequireAck=true`: entrega sucesso somente após `ACK`, ou falha após timeout/retries ou `ERR`.

---

### 4.3 Recepção (RX) — Eventos do Engine para o Consumidor

O Engine deve notificar o Consumidor por eventos/callbacks (conceitual):

- **OnAppFrameReceived(frame)**: frame válido de aplicação recebido
- **OnSendCompleted(seq, outcome)**: envio confiável concluído (sucesso/falha) (recomendado)
- **OnProtocolError(error)**: erro de protocolo (CRC fail, frame inválido, etc.) (opcional)

**Nota:** Erros de CRC devem, por padrão, apenas atualizar contadores e descartar o frame.

---

## 5. Estados Lógicos do Engine (Stop-and-Wait)

Quando `RequireAck=true`, o Engine opera em Stop-and-Wait para aquele envio.

Estados mínimos:

- **IDLE**: pronto para enviar
- **WAIT_ACK(seq)**: aguarda `ACK` (ou `ERR`) para o `seq` em questão
- **RETRY(seq, k)**: retransmissão após timeout (k = tentativa atual)
- **FAIL(seq)**: falha definitiva (retries esgotados ou erro fatal)

Regras:
- Retransmissões devem usar o **mesmo SEQ**.
- Recebimento duplicado do mesmo SEQ pelo outro lado deve resultar em reenvio do ACK correspondente,
  sem reprocessar a aplicação (dedupe).

---

## 6. Fluxos de Dados (end-to-end)

### 6.1 RX (bytes → AppFrame)

1. DAL entrega bytes via `OnBytesReceived(chunk)`
2. Engine acumula até encontrar delimitador `0x00`
3. Engine aplica COBS decode
4. Engine valida CRC-8/ATM
5. Se válido:
   - se `CMD` é `ACK`/`ERR`: tratar internamente (liberar WAIT_ACK, etc.)
   - senão:
     - se `ACK_REQ=1`: Engine envia `ACK` automaticamente
     - Engine dispara `OnAppFrameReceived(frame)`

### 6.2 TX (AppFrame → bytes)

1. Consumidor chama `Send(cmd, payload, options)`
2. Engine monta frame cru e calcula CRC
3. Engine codifica em COBS e adiciona `0x00`
4. Engine chama `DAL.Write(bytes)`
5. Se `RequireAck=true`: aguarda `ACK` com `SEQ` correspondente

---

## 7. Erros e Propagação (quem vê o quê)

### 7.1 Erros físicos (DAL)

Exemplos:
- porta fechada
- desconexão
- falha de escrita

Devem subir como falha de envio (imediata) ou como evento `OnTransportFault`.

### 7.2 Erros de protocolo (Engine)

Exemplos:
- COBS decode inválido
- CRC inválido
- frame curto/estruturalmente inválido

Por padrão:
- descartar frame e atualizar estatísticas
- **não** gerar `ERR` para CRC inválido

### 7.3 Erros de transporte (ERR)

`ERR` é um **frame válido** recebido, indicando rejeição do outro lado (ex.: `ERR_BUSY`).

Deve:
- encerrar o envio confiável com falha para o Consumidor
- permitir que o Consumidor decida retry no nível superior (se desejar)

---

## 8. Observabilidade e Diagnóstico (recomendado)

O Engine deve expor contadores/estatísticas:

- frames RX válidos
- frames RX descartados por CRC
- frames RX descartados por estrutura
- ACKs enviados/recebidos
- retries executados
- timeouts
- `ERR` recebidos por código

---

## 9. Conformidade

Uma implementação é considerada conforme se:

- delimita frames por `0x00` e usa COBS corretamente;
- valida CRC-8/ATM conforme especificação;
- respeita MTU (250 bytes no frame cru);
- trata `ACK`/`ERR` conforme definido;
- implementa dedupe para envios com `ACK_REQ=1`;
- não mistura semântica de aplicação dentro do Engine.

---

## 10. Referências

- Especificação do protocolo: `spec.pt-BR.md`
- Vetores de teste: `examples/*.hex` e `examples/README.md`
- ADR: `docs/04_desenvolvimento/adr/ADR-0007-cobs-crc8.pt-BR.md`

---

**Fim — Interface SGGW v1.0.0**
