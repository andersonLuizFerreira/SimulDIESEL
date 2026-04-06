⬅ [Retornar para DAL do Host](../06-dal-do-host.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# Framing, Scheduler e Supervisor

## Posição estrutural

Este degrau da DAL fica entre `SdgwSession` e `SwitchableTransport`.

```text
SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SdGwLinkSupervisor
  -> SwitchableTransport
```

## Classes reais

| arquivo | classe | papel estrutural | estado |
| --- | --- | --- | --- |
| `DAL/Protocols/SDGW/SdGwTxScheduler.cs` | `SdGwTxScheduler` | fila única e arbitragem de prioridade | `IMPLEMENTADO` |
| `DAL/Protocols/SDGW/SdgwLinkEngine.cs` | `SdGwLinkEngine` | framing, ACK/ERR, timeout, retry e deduplicação | `IMPLEMENTADO` |
| `DAL/Protocols/SDGW/SdgwLinkSupervisor.cs` | `SdGwLinkSupervisor` | watchdog por silêncio de RX válido | `IMPLEMENTADO` |
| `DAL/Protocols/SDGW/SdgwFrameReader.cs` | `SdgwFrameReader` | decode unitário reutilizável | `IMPLEMENTADO` |
| `DAL/Protocols/SDGW/SdgwFrameWriter.cs` | `SdgwFrameWriter` | encode unitário reutilizável | `IMPLEMENTADO` |
| `DAL/Protocols/SDGW/SdgwFrameCodec.cs` | `SdgwFrameCodec` | fachada estática para CRC8 e COBS | `IMPLEMENTADO` |

## Configuração confirmada no código

- `SdGwLinkEngine.Config.CmdAck = 0xF1`
- `SdGwLinkEngine.Config.CmdErr = 0xF2`
- `SdGwLinkEngine.Config.FlagAckReq = 0x01`
- `SdGwLinkEngine.Config.FlagIsEvt = 0x02`
- `SdGwLinkEngine.Config.MaxRawFrameLen = 250`
- `SdGwLinkSupervisor.Config.IdleBeforePingMs = 1500`
- `SdGwLinkSupervisor.Config.LinkTimeoutMs = 3000`
- `SdGwLinkSupervisor.Config.PingTimeoutMs = 150`
- `SdGwLinkSupervisor.Config.PingRetries = 2`
- `SdGwLinkSupervisor.Config.TickPeriodMs = 50`

## Trecho comentado: fila única

Em `SdGwTxScheduler.EnqueueAsync(...)`, a DAL protege a fila contra uso sem transporte:

```csharp
if (_disposed || !_transportAvailable)
    return Task.FromResult(SdGwLinkEngine.SendOutcome.TransportDown);

Enqueue_NoLock(item);

if (!_pumpRunning)
{
    _pumpRunning = true;
    shouldStartPump = true;
}
```

O que esse trecho faz:

- impede acúmulo de requests quando o transporte caiu;
- enfileira o item na fila da prioridade escolhida;
- sobe uma única bomba de processamento para toda a DAL.

## Trecho comentado: deduplicação e ACK

Em `SdGwLinkEngine.OnBytesReceived(...)`, o engine protege o receptor contra reentrega do mesmo `seq`:

```csharp
bool ackReq = (flags & _cfg.FlagAckReq) != 0;
if (ackReq)
{
    if (_hasLastRxSeq && seq == _lastRxSeq)
    {
        SendTransportAck(seq);
        continue;
    }
```

O que esse trecho faz:

- só entra na deduplicação quando o emissor exigiu ACK;
- se o `seq` já foi visto, reenvia o ACK mas não entrega o frame de novo para a aplicação;
- protege a BLL contra duplicação causada por retry do outro lado do link.

## Trecho comentado: timeout e retransmissão

Ainda em `SdGwLinkEngine`, `AckTimeoutTick(...)` controla o retry:

```csharp
if (_retriesLeft > 0)
{
    _retriesLeft--;
    bool ok = _write(_lastTxFrame);
    ...
    _ackTimer?.Change(_timeoutMs, Timeout.Infinite);
    return;
}

CompleteWait_NoLock(_waitSeq, SendOutcome.Timeout);
```

Esse bloco existe para manter o modelo stop-and-wait do host atual: um frame com ACK obrigatório só é concluído como `Acked`, `Nacked`, `TransportDown`, `Busy` ou `Timeout`.

## Observações de fidelidade

- `IMPLEMENTADO`: prioridades `High`, `Normal` e `Low`.
- `IMPLEMENTADO`: `Busy` quando já existe um frame aguardando ACK no engine.
- `IMPLEMENTADO`: watchdog por silêncio de RX válido, não por contagem cega de pings.
- `PARCIALMENTE IMPLEMENTADO`: `SdGwLinkSupervisor.Config.PingCmd` existe como slot de configuração, mas o comando emitido hoje vem do método `SdgwHostSession.SendSupervisorPingAsync()` com `cmd: 0x55`.
- `PARCIALMENTE IMPLEMENTADO`: o caminho de TX sem ACK existe, porém não é usado pelos comandos funcionais ativos do host.

## Glossário

- **Stop-and-wait**: modelo em que o emissor espera o resultado do frame atual antes de soltar o próximo com ACK.
- **Deduplicação**: descarte de reentrega do mesmo `seq` após reenviar ACK.
- **Silêncio de RX válido**: período sem frames que passaram por delimitação, COBS, CRC e parsing básico.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
