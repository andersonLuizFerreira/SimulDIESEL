# SD-GW-LINK â€” Interface de IntegraÃ§Ã£o (DAL â†” Protocolo â†” Consumidor)

**Projeto:** SimulDIESEL  
**Protocolo:** SD-GW-LINK  
**VersÃ£o:** 1.0.0  
**Status:** EstÃ¡vel (Interface de IntegraÃ§Ã£o)

---

## 1. Objetivo

Este documento define a **interface conceitual** (contratos) para integrar o motor do protocolo
**SD-GW-LINK** com:

- a **camada inferior (DAL / Transporte fÃ­sico)**, responsÃ¡vel por enviar/receber bytes; e
- a **camada superior (Consumidor / AplicaÃ§Ã£o)**, responsÃ¡vel por interpretar comandos e payloads de aplicaÃ§Ã£o.

> Este documento **nÃ£o** define detalhes fÃ­sicos (baudrate, RTS/DTR, IP, sockets, etc.).  
> Tais detalhes pertencem Ã  DAL especÃ­fica (Serial, Wiâ€‘Fi, Bluetooth).

---

## 2. Componentes e Responsabilidades

### 2.1 DAL (Data Access Layer / Transporte FÃ­sico)

A DAL Ã© responsÃ¡vel por:

- abrir/fechar a conexÃ£o fÃ­sica;
- **enviar bytes** e **receber bytes** (stream 8-bit);
- reportar falhas fÃ­sicas (desconexÃ£o, timeouts de porta, erros do driver).

A DAL **nÃ£o** deve:

- interpretar COBS, CRC, SEQ, ACK, comandos;
- segmentar frames SD-GW-LINK.

Exemplos de DAL:
- `SerialTransport` (COM/USB-CDC)
- `WifiTransport` (TCP/UDP/WebSocket, etc.)
- `BtTransport` (SPP/Serial BT)

---

### 2.2 Motor do Protocolo (SD-GW-LINK Engine)

O Engine Ã© responsÃ¡vel por:

- delimitaÃ§Ã£o por `0x00` e framing via COBS;
- validaÃ§Ã£o do **CRC-8/ATM**;
- parse e montagem de frames (`CMD|FLAGS|SEQ|PAYLOAD|CRC8`);
- gerenciamento de **SEQ por direÃ§Ã£o**;
- **ACK/NACK de transporte** (Stop-and-Wait opcional quando `ACK_REQ=1`);
- deduplicaÃ§Ã£o de retransmissÃµes;
- roteamento interno de `T_ACK`/`T_ERR` (nÃ£o sobe como â€œframe de aplicaÃ§Ã£oâ€ por padrÃ£o).

O Engine **nÃ£o** deve:

- interpretar semÃ¢ntica de comandos de aplicaÃ§Ã£o (CAN, perifÃ©ricos, firmware);
- assumir meio fÃ­sico (serial/wifi/bt).

---

### 2.3 Camada Superior (Consumidor / AplicaÃ§Ã£o)

O Consumidor Ã© responsÃ¡vel por:

- definir e interpretar **comandos de aplicaÃ§Ã£o** (tabela de CMDs);
- decidir se um envio exige confiabilidade (`ACK_REQ`);
- definir polÃ­ticas de timeout e retries (por comando/caso de uso);
- tratar respostas de aplicaÃ§Ã£o (sucesso/erro lÃ³gico);
- orquestrar casos de uso (ex.: â€œsetar nÃ­velâ€, â€œenviar CANâ€).

---

## 3. Contrato Inferior: Engine â†” DAL

### 3.1 Interface mÃ­nima da DAL (conceitual)

A DAL deve fornecer ao Engine, no mÃ­nimo:

- **Open() / Close()**
- **Write(bytes)**: envia bytes no stream
- **OnBytesReceived(bytesChunk)**: entrega ao Engine os bytes recebidos (em blocos arbitrÃ¡rios)
- **OnTransportFault(fault)**: notifica erro fÃ­sico (opcional, recomendado)
- **IsOpen / State** (opcional)

**Regras:**
- `OnBytesReceived` pode entregar chunks de qualquer tamanho (inclusive 1 byte).
- A DAL nÃ£o deve reter bytes propositalmente; deve entregar o mais rÃ¡pido possÃ­vel.
- O Engine Ã© o Ãºnico responsÃ¡vel por recompor frames a partir do stream.

### 3.2 Requisitos de desempenho (nÃ£o vinculados ao meio)

- A DAL deve suportar throughput suficiente para frames atÃ© 250 bytes.
- A DAL deve possuir buffer/loop de leitura que minimize perda de bytes em picos.
- Em caso de backpressure, a DAL deve sinalizar falhas (ex.: `Write` falhou).

---

## 4. Contrato Superior: Engine â†” Consumidor

### 4.1 Modelo de dados (Frame de AplicaÃ§Ã£o)

O Engine expÃµe ao Consumidor um **AppFrame** (frame de aplicaÃ§Ã£o) com:

- `Cmd` (byte)
- `Flags` (byte)
- `Seq` (byte)
- `Payload` (byte[])
- `TimestampRx` (opcional)
- `Direction` (opcional)

**Regra:** Por padrÃ£o, `T_ACK` e `T_ERR` sÃ£o consumidos internamente pelo Engine
e nÃ£o sÃ£o entregues como `AppFrame` (exceto em modo diagnÃ³stico).

---

### 4.2 Envio (TX) â€” OperaÃ§Ãµes do Engine

O Engine deve oferecer uma operaÃ§Ã£o conceitual equivalente a:

- **Send(cmd, payload, options)**

onde `options` inclui:

- `RequireAck` (bool): se verdadeiro, o Engine seta `ACK_REQ=1` e aplica Stop-and-Wait
- `TimeoutMs` (int): default 100 ms
- `MaxRetries` (int): polÃ­tica definida pelo Consumidor
- `IsEvent` (bool): se verdadeiro, o Engine seta `IS_EVT=1` (quando aplicÃ¡vel)
- `AdditionalFlags` (byte): permitir flags adicionais quando necessÃ¡rio

**Resultado do envio (contrato):**
- Para `RequireAck=false`: entrega â€œaceite localâ€ (enfileirado para TX) ou falha imediata (DAL indisponÃ­vel).
- Para `RequireAck=true`: entrega sucesso somente apÃ³s `T_ACK`, ou falha apÃ³s timeout/retries ou `T_ERR`.

---

### 4.3 RecepÃ§Ã£o (RX) â€” Eventos do Engine para o Consumidor

O Engine deve notificar o Consumidor por eventos/callbacks (conceitual):

- **OnAppFrameReceived(frame)**: frame vÃ¡lido de aplicaÃ§Ã£o recebido
- **OnSendCompleted(seq, outcome)**: envio confiÃ¡vel concluÃ­do (sucesso/falha) (recomendado)
- **OnProtocolError(error)**: erro de protocolo (CRC fail, frame invÃ¡lido, etc.) (opcional)

**Nota:** Erros de CRC devem, por padrÃ£o, apenas atualizar contadores e descartar o frame.

---

## 5. Estados LÃ³gicos do Engine (Stop-and-Wait)

Quando `RequireAck=true`, o Engine opera em Stop-and-Wait para aquele envio.

Estados mÃ­nimos:

- **IDLE**: pronto para enviar
- **WAIT_ACK(seq)**: aguarda `T_ACK` (ou `T_ERR`) para o `seq` em questÃ£o
- **RETRY(seq, k)**: retransmissÃ£o apÃ³s timeout (k = tentativa atual)
- **FAIL(seq)**: falha definitiva (retries esgotados ou erro fatal)

Regras:
- RetransmissÃµes devem usar o **mesmo SEQ**.
- Recebimento duplicado do mesmo SEQ pelo outro lado deve resultar em reenvio do ACK correspondente,
  sem reprocessar a aplicaÃ§Ã£o (dedupe).

---

## 6. Fluxos de Dados (end-to-end)

### 6.1 RX (bytes â†’ AppFrame)

1. DAL entrega bytes via `OnBytesReceived(chunk)`
2. Engine acumula atÃ© encontrar delimitador `0x00`
3. Engine aplica COBS decode
4. Engine valida CRC-8/ATM
5. Se vÃ¡lido:
   - se `CMD` Ã© `T_ACK`/`T_ERR`: tratar internamente (liberar WAIT_ACK, etc.)
   - senÃ£o:
     - se `ACK_REQ=1`: Engine envia `T_ACK` automaticamente
     - Engine dispara `OnAppFrameReceived(frame)`

### 6.2 TX (AppFrame â†’ bytes)

1. Consumidor chama `Send(cmd, payload, options)`
2. Engine monta frame cru e calcula CRC
3. Engine codifica em COBS e adiciona `0x00`
4. Engine chama `DAL.Write(bytes)`
5. Se `RequireAck=true`: aguarda `T_ACK` com `SEQ` correspondente

---

## 7. Erros e PropagaÃ§Ã£o (quem vÃª o quÃª)

### 7.1 Erros fÃ­sicos (DAL)

Exemplos:
- porta fechada
- desconexÃ£o
- falha de escrita

Devem subir como falha de envio (imediata) ou como evento `OnTransportFault`.

### 7.2 Erros de protocolo (Engine)

Exemplos:
- COBS decode invÃ¡lido
- CRC invÃ¡lido
- frame curto/estruturalmente invÃ¡lido

Por padrÃ£o:
- descartar frame e atualizar estatÃ­sticas
- **nÃ£o** gerar `T_ERR` para CRC invÃ¡lido

### 7.3 Erros de transporte (T_ERR)

`T_ERR` Ã© um **frame vÃ¡lido** recebido, indicando rejeiÃ§Ã£o do outro lado (ex.: `ERR_BUSY`).

Deve:
- encerrar o envio confiÃ¡vel com falha para o Consumidor
- permitir que o Consumidor decida retry no nÃ­vel superior (se desejar)

---

## 8. Observabilidade e DiagnÃ³stico (recomendado)

O Engine deve expor contadores/estatÃ­sticas:

- frames RX vÃ¡lidos
- frames RX descartados por CRC
- frames RX descartados por estrutura
- ACKs enviados/recebidos
- retries executados
- timeouts
- `T_ERR` recebidos por cÃ³digo

---

## 9. Conformidade

Uma implementaÃ§Ã£o Ã© considerada conforme se:

- delimita frames por `0x00` e usa COBS corretamente;
- valida CRC-8/ATM conforme especificaÃ§Ã£o;
- respeita MTU (250 bytes no frame cru);
- trata `T_ACK`/`T_ERR` conforme definido;
- implementa dedupe para envios com `ACK_REQ=1`;
- nÃ£o mistura semÃ¢ntica de aplicaÃ§Ã£o dentro do Engine.

---

## 10. ReferÃªncias

- EspecificaÃ§Ã£o do protocolo: `spec.pt-BR.md`
- Vetores de teste: `examples/*.hex` e `examples/SGGW_OVERVIEW.md`
- ADR: `specs/adr/ADR-0007-cobs-crc8.pt-BR.md`

---

**Fim â€” Interface SD-GW-LINK v1.0.0**

