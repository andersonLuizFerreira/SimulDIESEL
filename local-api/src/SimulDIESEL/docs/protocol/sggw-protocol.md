# Protocolo SGGW (documentação técnica)

## Propósito
Descrever o formato de frame, mecanismos de delimitação, CRC, flags, comandos, ACK/ERR, sequência, retransmissão e MTU conforme implementado em `SdGwLinkEngine.cs`.

## Escopo
Documenta apenas os comportamentos implementados no código fonte (SdGwLinkEngine). Não inventa recursos não presentes.

## Formato do frame lógico (antes de COBS)
Um frame "raw" (bruto) construído pelo engine tem o layout (bytes em ordem):

- `cmd` (1 byte) — código do comando (ex.: `SggwCmd` ou ack/err configurados).
- `flags` (1 byte) — bits de flags (ver abaixo).
- `seq` (1 byte) — número de sequência (incrementa no envio).
- `payload` (0..N bytes) — dados do comando.
- `crc` (1 byte) — CRC8-ATM calculado sobre todos os bytes anteriores (`cmd` até último byte de payload).

Total mínimo do frame cru: 4 bytes (cmd + flags + seq + crc).

## Framing na camada de transporte (COBS + delimitação)
- Após montar o frame cru, o engine aplica COBS encode (`CobsEncode`) ao array cru.
- Ao final do stream codificado, o engine adiciona um byte delimitador `0x00`.
- No recebimento, o engine acumula bytes até encontrar um `0x00` e usa `CobsDecode` para obter o frame cru.

Observações:
- `SdGwLinkEngine.OnBytesReceived` separa frames pelo delimitador `0x00`.
- Frames sem delimitador que excedam `MaxRawFrameLen + 16` causam `ProtocolError` e descarte do buffer.

## CRC (Crc8-ATM)
- Algoritmo implementado em `Crc8Atm(byte[] data, int offset, int len)`:
  - Inicializa `crc = 0x00`.
  - Para cada byte: `crc ^= byte` e 8 passos de shift/escrita com polinômio 0x07 (modo ATM).
- O CRC recebido (último byte do frame cru) deve ser igual ao CRC calculado; caso contrário `ProtocolError` é disparado.

## COBS
- Implementação completa de `CobsEncode` e `CobsDecode` presente em `SdGwLinkEngine`.
- Em decodificação, `CobsDecode` valida codes e pode lançar `InvalidOperationException` em caso de COBS inválido; o engine captura e gera `ProtocolError`.

## Comandos (SGGW)
Comandos lógicos definidos em `SggwCmd` (DTL):
- `Ping = 0x55`
- `GetVersion = 0x10`
- `Echo = 0x11`
- `CanTx = 0x20`
- `CanRxEvt = 0x21`
- `SetParameter = 0x30` (reservado)
- `GetParameter = 0x31` (reservado)

ACK/ERR especiais (configuráveis no `Config` do engine):
- `CmdAck` padrão: `0xF1`
- `CmdErr` padrão: `0xF2`
Esses são tratados internamente pelo engine para completar o stop-and-wait.

## Flags
Configuração padrão (`Config`):
- `FlagAckReq = 0x01` — se setado no frame recebido, o receptor envia um ACK de transporte (CmdAck) com o mesmo `seq`. No envio, setar este bit solicita ACK.
- `FlagIsEvt = 0x02` — indica que o frame é um evento (não é um request tradicional) e é repassado como evento lógico ao cliente.

Outros bits: `AdditionalFlags` em `SendOptions` permite adicionar bits arbitrários.

## Sequência
- Campo `seq` é um byte e é gerenciado pelo engine.
- `NextTxSeq()` incrementa `_txSeq` sem overflow explícito (wrap-around via unchecked).
- O engine mantém `_hasLastRxSeq` e `_lastRxSeq` para detectar duplicados (reenvio do transmissor): se o receptor recebe um frame com seq já visto, responde com ACK e não entrega novamente ao app.

## Controle de confiabilidade (Stop-and-Wait)
- Modo de envio com `RequireAck = true` implementa stop-and-wait:
  - Apenas um envio pendente por vez (campo `_waitAck` controla).
  - `_waitSeq` guarda a sequência aguardada.
  - `_lastTxFrame` armazena bytes COBS+delim do último envio para retransmissão.
  - `_ackTimer` (System.Threading.Timer) dispara `AckTimeoutTick` quando expira `TimeoutMs`.
  - `MaxRetries` controla quantas retransmissões serão tentadas antes de declarar timeout.
- Resultado (enum `SendOutcome`):
  - `Enqueued` — enviado sem requerer ack.
  - `Acked` — ack recebido.
  - `Nacked` — erro (ERR) recebido.
  - `Timeout` — tentativas esgotadas.
  - `TransportDown` — falha de transporte detectada (por exemplo `SerialTransport.Write` retornou false).
  - `Busy` — já existe um envio com ack em curso.

## ACK / ERR
- `CmdAck` e `CmdErr` frames são tratados internamente:
  - Ao receber `CmdAck` com seq correspondente, `CompleteWait_NoLock(..., Acked)` é acionado.
  - Ao receber `CmdErr` com seq correspondente, `CompleteWait_NoLock(..., Nacked)` é acionado.
- Se `DeliverAckErrToApp` estiver `true` no `Config`, ACK/ERR também são entregues via `AppFrameReceived`.

## Retransmissão
- Na expiração do timer, se `_retriesLeft > 0`, o engine reenvia `_lastTxFrame`, decrementa `_retriesLeft` e reinicia o timer.
- Se `Write` retornar `false` durante retransmissão, `TransportDown` é reportado.

## MTU / limites
- `Config.MaxRawFrameLen` (padrão 250) é o limite do frame cru (bytes). `SendAsync` verifica: `payload.Length + 4 <= MaxRawFrameLen`.
- Buffer `_rx` tem limite lógico: se exceder `MaxRawFrameLen + 16` sem delimitação, considera overflow.

## Eventos públicos do engine
- `AppFrameReceived(SdGwLinkEngine.AppFrame)` — frame de aplicação decodificado.
- `ProtocolError(string)` — problemas de framing/CRC/COBS.
- `SendCompleted(byte seq, SendOutcome outcome)` — completude de envio (invocado fora do lock).
- `TransportFault(string)` — indicação opcional de falha física do transporte (ex.: `OnTransportDown`).

## Exemplo de sequência (Resumo)
- Emissor monta frame cru → CRC → COBS → adiciona 0x00 → `write(stream)`.
- Receptor acumula bytes até 0x00 → COBS decode → valida CRC → trata cmd/flags/seq/payload.