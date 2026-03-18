# Contratos de Software

## Contratos de integração preservados

Os contratos históricos abaixo continuam sendo referência para wire format e semântica básica do protocolo legado:

- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_CENTRAL.md`
- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_GATEWAY.md`
- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_GSA.md`
- `docs/legacy-docs/01_arquitetura/01_protocolos/sggw/spec.pt-BR.md`
- `docs/legacy-docs/01_arquitetura/01_protocolos/sggw/interface.pt-BR.md`

Esses documentos ainda servem para:

- endereçamento `CMD` compacto
- framing SDGW/SGGW
- regras de `ACK` / `ERR`
- contratos TLV entre gateway e baby boards

## Contrato vigente do host

O contrato de implementação atual do host não é mais baseado em `SerialLinkService`.

A composição vigente é:

    BpmSerialService
        -> SdGwLinkEngine
        -> SdGwTxScheduler
        -> SdgwSession
        -> SdhClient
        -> SdGwLinkSupervisor

### Ponto global transitório

O ponto global usado hoje pela UI é:

    BpmSerialService.Shared

Esse acesso substitui o legado baseado em `SerialLink`.

### Contrato de estados do link

O estado lógico do link é modelado por:

    BpmSerialService.LinkState

Estados atuais:

- `Disconnected`
- `SerialConnected`
- `Draining`
- `BannerSent`
- `Linked`
- `LinkFailed`

### Contrato de sessão e envio

O envio de frames SDGW para consumidores do host deve passar por:

    SdgwSession.SendAsync(...)

Internamente, esse envio é arbitrado por:

    SdGwTxScheduler

O scheduler é o único caminho normal de transmissão do link.

Prioridades vigentes:

- `High`
- `Normal`
- `Low`

Contrato operacional:

- comandos funcionais usam `High`
- pings do supervisor usam `Low`
- o sistema não deve depender de `Busy` como arbitragem principal entre componentes internos

### Contrato técnico do engine

O `SdGwLinkEngine` continua responsável por:

- framing
- `COBS`
- `CRC-8/ATM`
- `ACK`
- `ERR`
- timeout/retry
- stop-and-wait

Além disso, ele publica:

- `AppFrameReceived`
- `ValidFrameReceived`
- `IsAwaitingAck`

### Contrato de supervisão

O componente vigente de supervisão é:

    SdGwLinkSupervisor

Contrato funcional atual:

- RX SDGW válido prova vida
- ping só ocorre sob silêncio
- timeout lógico é baseado em ausência de RX válido

`SdgwHealthService` é legado removido e não deve mais ser citado como serviço ativo.

## Contrato vigente do firmware BPM

Do lado da BPM, o comportamento documentado e implementado é:

- sessão mantida por atividade SDGW válida
- watchdog de atividade do link em `4000 ms`
- timeout interno do router/gateway em `100 ms`
- `PING 0x55` mantido apenas como comando suportado, não como única condição de keepalive

## Contrato vigente do fluxo GSA

O caso funcional ativo mais importante é:

    GSA.led set state=on|off

Contrato atual do host para esse fluxo:

- envio via `SdhClient`
- mapeamento para SDGW compacto por `SdhToSdgwMapper`
- envio em prioridade `High`
- timeout de ACK do LED em `400 ms`
- retries do LED em `2`
- correlação de resposta reforçada no `GsaClient`

## Regra de manutenção

Qualquer mudança de comportamento do link host/BPM deve atualizar simultaneamente:

1. implementação
2. esta documentação técnica
3. documentação arquitetural do host
4. documentação arquitetural do firmware BPM

[Retornar ao README principal](../README.md)
