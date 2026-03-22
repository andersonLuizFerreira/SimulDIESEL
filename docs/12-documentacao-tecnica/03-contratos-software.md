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

Historicamente, o caso funcional ativo mais importante era:

    GSA.led set state=on|off

Esse fluxo permanece compatível.

### Expansão vigente da GSA no host

Além do LED builtin, o host agora suporta:

    GSA.channel.setpoint set channel=<1..16> value=<0..255>
    GSA.channel.enable set channel=<1..16> state=on|off
    GSA.channels.enable set state=on|off
    GSA.channel.status get channel=<1..16>
    GSA.channels.status get
    GSA.channel.fault reset channel=<1..16>
    GSA.channel.offset set/get/save/reset
    GSA.offset reset

### Contrato atual do host para a GSA

- envio via `SdhClient`
- mapeamento para SDGW compacto por `SdhToSdgwMapper`
- serialização TLV específica da GSA no host
- envio em prioridade `High`
- correlação funcional de respostas no `GsaClient`
- tratamento de erro funcional TLV `0x7F`
- consumo de evento assíncrono de `fault` via `SdgwSession.EventReceived`

### Contrato funcional da GSA documentado hoje

- existem `16` canais
- canais `1..8` em `0..5 V`
- canais `9..16` em `0..12 V`
- setpoint lógico em `0..255`
- status deve responder mesmo com canal `OFF`
- status retorna valores reais lidos
- `setpoint set` é permitido com canal `OFF`
- `setpoint set` é permitido com `fault latched`
- `enable on` por canal falha se houver `fault latched`
- `channels.enable on` respeita fault latched
- `channels.enable off` não limpa fault
- offsets usam `int16` com sinal
- `vout` e `vread` em `mV`
- `iread` em `mA`
- evento assíncrono existe apenas para `fault`

### Inconsistência histórica documentada

Há um conflito histórico no contrato TLV da GSA:

- o LED builtin já usava `type 0x12`;
- o status por canal também passou a ser documentado como `type 0x12`.

O host preservou compatibilidade por parser, diferenciando:

- LED builtin por `len = 0x01`
- status por canal por `len = 0x06`

Essa inconsistência ainda precisa de convergência futura no contrato da board/gateway.

### Contrato específico do LED

Para o fluxo de LED builtin, permanecem válidos:

- envio via `SdhClient`
- mapeamento para SDGW compacto por `SdhToSdgwMapper`
- envio em prioridade `High`
- timeout de ACK do LED em `400 ms`
- retries do LED em `2`
- correlação de resposta reforçada no `GsaClient`

## Documentos oficiais relacionados

- `docs/04-firmware/boards/03-gsa.md`
- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/06-protocolos/06-gsa-sdh-tlv.md`

## Conflitos legados preservados para limpeza futura

Os documentos abaixo podem conflitar com a documentação oficial vigente da GSA e precisam de revisão futura, mas não foram alterados nesta etapa:

- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_GSA.md`
- `docs/legacy-docs/05_hardware/gerador-sinais-analogicos-GSA/PROTOCOLO.md`
- `docs/legacy-docs/01_arquitetura/05_babyboards/gsa/PROTOCOLO.md`

Os conflitos mais prováveis nesses materiais são:

- modelo antigo `GSA.ch1`
- ausência do catálogo completo de 16 canais
- ausência do contrato atual de offsets
- ausência do evento assíncrono de fault
- possível divergência quanto ao uso do type `0x12`

## Regra de manutenção

Qualquer mudança de comportamento do link host/BPM deve atualizar simultaneamente:

1. implementação
2. esta documentação técnica
3. documentação arquitetural do host
4. documentação arquitetural do firmware BPM

[Retornar ao README principal](../README.md)
