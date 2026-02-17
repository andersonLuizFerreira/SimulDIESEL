# SdGgwClient (Cliente de alto nível SGGW)

## Propósito
Fornecer API tipada e de alto nível para uso da aplicação sobre o protocolo SGGW, encapsulando `SdGwLinkEngine` e expondo eventos e helpers para DTOs.

## Escopo
Classe: `SimulDIESEL.BLL.SdGgwClient`

## Responsabilidades
- Receber frames do `SdGwLinkEngine` e convertê-los em `SggwFrame` (DTL).
- Disparar eventos públicos:
  - `FrameReceived(SggwFrame)` para qualquer frame recebido.
  - `EventReceived(SggwFrame)` para frames com flag de evento (`FlagIsEvt`).
- Expor métodos `SendAsync(...)` para enviar comandos:
  - `SendAsync(SggwCmd cmd, byte[] payload, bool requireAck = true, int timeoutMs = 150, int retries = 2)`
  - `SendAsync(SggwCmd cmd, bool requireAck = true, ...)` (sem payload)
  - `SendAsync(CanTxRequest req)` — serializa `CanTxRequest` para payload usando `CanTxCodec.ToPayload()` e envia com ajustes de timeout/retries.

## Integração com DTL
- Converte `SdGwLinkEngine.AppFrame` para `SggwFrame` e publica via `FrameReceived`.
- `SggwFrame` contém `Cmd`, `Seq`, `Flags` e `Payload`, com propriedade `CommandEnum` que converte `Cmd` para `SggwCmd`.
- `CanTxRequest` possui codec `ToPayload()` / `FromPayload()` em `CanTxCodec` para serialização.

## Tratamento de erros / disposed
- `SdGgwClient` verifica `_disposed` em `SendAsync` e lança `ObjectDisposedException` quando apropriado.
- No `Dispose()` remove subscription do engine (`AppFrameReceived`) e marca `_disposed = true`.

## Thread safety
- `SdGgwClient` delega concorrência e sincronização ao `SdGwLinkEngine`. O cliente apenas encaminha chamadas assíncronas e dispara eventos, sem locks próprios.

## Exemplo de uso (resumido)
- Subscrição:
  - `client.FrameReceived += frame => { /* tratar */ }`
  - `client.EventReceived += evt => { /* tratar evento */ }`
- Envio:
  - `await client.SendAsync(SggwCmd.GetVersion);`
  - `await client.SendAsync(new CanTxRequest { CanId = 0x123, Dlc = 8, Data = ... });`

## Not implemented / Observações
- Não provê parsing adicional de payloads; responsabilidade é do consumidor usar codecs em `DTL`.
- Não há reconexão automática do `SdGgwClient` — este é gerenciado por `SerialLinkService`.