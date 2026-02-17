# SdGwLinkEngine (Engine de Link / Protocolo)

## Propósito
Implementar o protocolo SGGW em nível de enlace: framing (COBS), verificação CRC, stop-and-wait com ACK/ERR, controle de sequência, retransmissão e entrega de frames de aplicação.

## Escopo
Classe: `SimulDIESEL.BLL.SdGwLinkEngine`

## Responsabilidades
- Construir frames brutos, calcular CRC8-ATM e aplicar COBS + delimitador 0x00 antes de enviar pelo transporte lógico (delegate `_write`).
- Receber stream de bytes, dividir por delimitador 0x00, fazer COBS decode, validar CRC, detectar e responder ACKs para frames que requerem ack.
- Implementar stop-and-wait quando `SendOptions.RequireAck = true` com timeout, retries e report de resultado (`SendOutcome`).
- Expor eventos: `AppFrameReceived`, `ProtocolError`, `SendCompleted`, `TransportFault`.

## Configuração (Config)
- `CmdAck` / `CmdErr` (valores padrão: 0xF1, 0xF2).
- `FlagAckReq` (0x01), `FlagIsEvt` (0x02).
- `MaxRawFrameLen` (padrão 250).
- `DeliverAckErrToApp` (false por padrão): controla se ACK/ERR são entregues ao app via `AppFrameReceived`.

## Stop-and-wait
- Envio com ack:
  - `SendAsync` monta frame e entra em seções protegidas por `lock(_swSync)`.
  - Se já existe `_waitAck`, retorna `SendOutcome.Busy`.
  - Armazena `_lastTxFrame`, `_waitSeq`, `_retriesLeft`, cria `TaskCompletionSource`.
  - Invoca `_write(stream)`. Se `_write` retornar `false`, completa com `TransportDown`.
  - Cria `_ackTimer` com timeout `_timeoutMs`.
- Recebimento de ACK/ERR:
  - `HandleAck(seq)` e `HandleErr(seq)` completam a espera com `Acked` ou `Nacked`.
- Timeouts / retransmissões:
  - `AckTimeoutTick` é disparado pelo timer: se `_retriesLeft > 0`, reenvia `_lastTxFrame`, decrementa e reinicia timer; caso contrário completa com `Timeout`.
- Finalização:
  - `CompleteWait_NoLock` (assume lock) cancela timer, limpa `_waitAck`, completa TCS e dispara `SendCompleted` fora do lock via `Task.Run`.

## Controle de sequência e duplicados
- `NextTxSeq()` incrementa `_txSeq` (wrap-around via unchecked).
- Receptor mantém `_hasLastRxSeq` e `_lastRxSeq`. Se receber um frame com `seq` igual ao último processado:
  - Responde com ACK (reenvio duplicado) e não repassa para a aplicação.

## Parsing RX
- Acumula bytes em lista `_rx` até encontrar `0x00`.
- Chama `CobsDecode`, valida comprimento mínimo (>=4) e checa CRC.
- Se `cmd` == `CmdAck` ou `CmdErr`, executa `HandleAck/HandleErr`.
- Se `flags & FlagAckReq`, envia ACK de transporte (`SendTransportAck(seq)`) e aplica lógica de deduplicação.
- Em casos de COBS/CRC/overflow, dispara `ProtocolError` e descarta frame.

## Eventos
- `AppFrameReceived(SdGwLinkEngine.AppFrame)` — frame de aplicação pronto (cmd, flags, seq, payload).
- `ProtocolError(string)` — erro de parsing/protocolo.
- `SendCompleted(byte seq, SendOutcome outcome)` — completude de envio (invocado fora do lock).
- `TransportFault(string)` — chamado por `OnTransportDown`.

## Tratamento de falha do transporte
- Método `OnTransportDown(string reason)`:
  - Dispara `TransportFault` com razão (se fornecida).
  - Completa qualquer stop-and-wait pendente com `TransportDown`.
  - Limpa buffer de RX.

## Thread safety / concorrência
- Operações de stop-and-wait são protegidas por `lock(_swSync)`.
- `_rx` manipulado no contexto de recepção de bytes (chamado possivelmente a partir de thread de IO). Proteção contra overflow implementada.
- `CompleteWait_NoLock` dispara eventos (`SendCompleted`) fora do lock via `Task.Run` para evitar deadlocks e reentrância.
- `OnBytesReceived` processa bytes sequencialmente no thread que chama o evento de transporte; não há uso de filas internas além de `_rx`.

## Limitações / Not implemented
- Não há fragmentação de payload maior que MTU — `SendAsync` valida e lança se exceder.
- Não há mecanismo de multiplexação de envios com ack simultâneos (apenas stop-and-wait).

## Exemplo de uso (resumido)
- Registrar `AppFrameReceived` para receber frames.
- Chamar `SendAsync(cmd, payload, options)` para enviar; aguardar `SendOutcome`.