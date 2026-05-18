⬅ [Retornar para Camada Hardware do Software](03-camada-hardware.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Arquitetura SDH no Host

## Objetivo

Este documento descreve o comportamento do host SDH/SDGW realmente implementado em `local-api`, sem misturar detalhes de firmware que o código do PC não observa.

## Pilha lógica ativa

```text
UI / FormsLogic
  -> BpmSerialService
  -> GsaClient / BpmClient
  -> SdhClient
  -> SdhValidator / SdhToSdgwMapper
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SwitchableTransport
  -> SerialTransport / BluetoothTransport
```

## Comportamentos confirmados

| comportamento | evidência principal | estado |
| --- | --- | --- |
| handshake textual antes do binário | `SdgwHostSession.LinkTick(...)`, `HandleHandshakeBytes(...)` | `IMPLEMENTADO` |
| sessão `Linked` habilita supervisor | `SdgwHostSession.OnStateChanged_ForLinkSupervisor(...)` | `IMPLEMENTADO` |
| envio concorrente passa por fila única | `SdGwTxScheduler.EnqueueAsync(...)` | `IMPLEMENTADO` |
| stop-and-wait com ACK/ERR/retry | `SdGwLinkEngine.SendWithSeq(...)`, `AckTimeoutTick(...)` | `IMPLEMENTADO` |
| saúde do link baseada em RX válido | `SdGwLinkSupervisor.OnValidFrameReceived(...)`, `Tick(...)` | `IMPLEMENTADO` |
| GSA com resposta síncrona e evento assíncrono | `GsaClient.ExecuteOperationAsync(...)`, `OnEventReceived(...)` | `IMPLEMENTADO` |
| parsing e tratamento de resposta | `SdhTextParser`, `SdhValidator`, `SdhToSdgwMapper`, `GsaParsers` | `IMPLEMENTADO` |

## Limites do host atual

- O host só valida e mapeia targets SDH conhecidos; outros targets geram `NotSupportedException`.
- O caminho funcional maduro está concentrado em `GSA.*` e no ping `BPM.gateway`.
- Bluetooth no host ainda é SPP sobre COM.
- O contrato `SdhResponse` existe, mas não participa do hot path desta aplicação.

## Aprofundamentos deste ramo

- [Handshake e Estados da Sessão](04-sdh-host-architecture/01-handshake-e-estados-da-sessao.md)
- [Scheduler, Retry e Supervisão do Link](04-sdh-host-architecture/02-scheduler-retry-e-supervisao.md)
- [Fluxo GSA do Comando ao Evento](04-sdh-host-architecture/03-fluxo-gsa-do-comando-ao-evento.md)
- [Parsing e Tratamento de Respostas](04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md)

## Glossário

- **SDH**: envelope semântico que a aplicação monta antes do envio.
- **SDGW**: enlace compacto e enquadrado que circula no fio.
- **Linked**: estado em que o handshake terminou e o supervisor do link já pode operar.

## Próximas camadas

- [Handshake e Estados da Sessão](04-sdh-host-architecture/01-handshake-e-estados-da-sessao.md)
- [Scheduler, Retry e Supervisão do Link](04-sdh-host-architecture/02-scheduler-retry-e-supervisao.md)
- [Fluxo GSA do Comando ao Evento](04-sdh-host-architecture/03-fluxo-gsa-do-comando-ao-evento.md)
- [Parsing e Tratamento de Respostas](04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md)
