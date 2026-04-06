⬅ [Retornar para Arquitetura SDH no Host](../04-sdh-host-architecture.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# Scheduler, Retry e Supervisão do Link

## Função lógica

Este ramo cobre como o host decide a ordem do envio, espera ACK e derruba a saúde lógica do link quando o RX válido some.

## Papel de cada classe

| classe | função lógica | estado |
| --- | --- | --- |
| `SdGwTxScheduler` | fila única com prioridades `High`, `Normal` e `Low` | `IMPLEMENTADO` |
| `SdGwLinkEngine` | stop-and-wait, COBS, CRC8, ACK/ERR, timeout e retry | `IMPLEMENTADO` |
| `SdGwLinkSupervisor` | watchdog por silêncio de RX válido | `IMPLEMENTADO` |

## Regras confirmadas

- pings do supervisor saem com prioridade `Low`;
- operações funcionais da GSA e o ping da BPM saem com prioridade `High`;
- `SdGwLinkEngine` deduplica RX repetido pelo mesmo `seq` quando o emissor exige ACK;
- timeout de ACK dispara retransmissão enquanto ainda houver `MaxRetries`;
- se o engine já estiver aguardando ACK, um novo envio com ACK obrigatório retorna `Busy`.

## Trecho comentado: fila de transmissão

Em `SdGwTxScheduler.EnqueueAsync(...)`:

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

- recusa enfileirar quando o transporte já caiu;
- coloca o item na fila da prioridade escolhida;
- garante que só exista uma bomba de TX ativa por vez.

## Trecho comentado: timeout e retry

Em `SdGwLinkEngine.AckTimeoutTick(...)`:

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

O que esse trecho faz:

- reenvia exatamente o último frame transmitido;
- renova o temporizador de ACK;
- só fecha como `Timeout` quando o orçamento de retry acabou.

## Trecho comentado: watchdog do link

Em `SdGwLinkSupervisor.Tick(...)`:

```csharp
if (silenceMs < _cfg.IdleBeforePingMs)
{
    SetAlive(true);
    return;
}

if (silenceMs >= _cfg.LinkTimeoutMs)
{
    _awaitingPingReply = false;
    SetAlive(false);
    return;
}
```

O que esse trecho faz:

- mantém o link saudável enquanto o RX válido ainda é recente;
- só tenta pingar no intervalo intermediário;
- derruba a saúde lógica quando o silêncio cruza `LinkTimeoutMs`.

## Nuances importantes do código atual

- `PARCIALMENTE IMPLEMENTADO`: `SdGwLinkSupervisor.Config.PingCmd` existe, mas o comando real do ping é emitido hoje em `SdgwHostSession.SendSupervisorPingAsync()` com `cmd: 0x55`.
- `IMPLEMENTADO`: `ValidFrameReceived` do engine é o gatilho que alimenta `OnValidFrameReceived()` no supervisor.
- `PARCIALMENTE IMPLEMENTADO`: o caminho sem ACK e com `IsEvent` existe no engine, mas os casos ativos do host não o usam para requests funcionais.

## Glossário

- **Stop-and-wait**: modelo em que o emissor espera o resultado do frame atual antes de soltar o próximo com ACK.
- **RX válido**: frame que passou por delimitação, COBS, CRC e parsing estrutural.
- **Supervisor**: watchdog que mede silêncio e agenda ping quando necessário.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
