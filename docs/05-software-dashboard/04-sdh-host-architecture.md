# Arquitetura SDH no Host

## Objetivo

Este documento descreve a arquitetura atual do host SDGW/SDH no SimulDIESEL.

O foco é o comportamento realmente implementado no código C#:

- composição central em `BpmSerialService`
- transmissão arbitrável por `SdGwTxScheduler`
- sessão funcional em `SdgwSession`
- camada semântica em `SdhClient`
- supervisão de link em `SdGwLinkSupervisor`

Os componentes `SerialLink`, `SerialLinkService`, `SdGgwClient` e `SdgwHealthService` pertencem ao legado removido e não devem mais ser usados como referência arquitetural vigente.

## Composição atual

O host é composto assim:

    UI / FormsLogic
        -> BpmSerialService.Shared
        -> GsaClient / BpmClient
        -> SdhClient
        -> SdgwSession
        -> SdGwTxScheduler
        -> SdGwLinkEngine
        -> SerialTransport

Além desse caminho principal, o `BpmSerialService` também coordena:

- handshake textual inicial com a BPM
- recebimento bruto da serial
- `SdGwLinkSupervisor`
- transição de estados do link

## Responsabilidades por componente

### `BpmSerialService`

É o dono funcional do link serial da BPM.

Responsabilidades:

- abrir e fechar a porta serial
- executar o bootstrap textual até `Linked`
- compor `SdGwLinkEngine`, `SdGwTxScheduler`, `SdgwSession`, `SdhClient`, `SdGwLinkSupervisor`, `GsaClient` e `BpmClient`
- expor estado do link à UI
- manter o engine recebendo tráfego SDGW binário depois que a conexão já atingiu `Linked` ao menos uma vez

O acesso global transitório atualmente usado pela UI é:

    BpmSerialService.Shared

### `SdGwLinkEngine`

É a camada técnica do protocolo SDGW.

Responsabilidades:

- framing
- `COBS`
- `CRC-8/ATM`
- `ACK`
- `ERR`
- timeout/retry
- stop-and-wait
- publicação de `ValidFrameReceived`

Ele não implementa fila de prioridades.

### `SdGwTxScheduler`

É o agendador central de TX acima do engine.

Responsabilidades:

- receber solicitações de envio
- aplicar prioridade
- manter FIFO por prioridade
- despachar um item por vez
- completar pendências com o resultado do engine
- encerrar a fila com `TransportDown` quando o transporte cai

Prioridades atualmente implementadas:

- `High`
- `Normal`
- `Low`

Uso atual:

- comandos funcionais da aplicação: `High`
- uso interno não urgente: `Normal`
- ping do supervisor: `Low`

### `SdgwSession`

É a sessão de alto nível do SDGW sobre o scheduler.

Responsabilidades:

- expor `SendAsync(...)`
- publicar `FrameReceived`
- publicar `EventReceived`

Nenhum cliente funcional deve enviar direto no `SdGwLinkEngine`.

### `SdhClient`

É a camada semântica do host.

Responsabilidades:

- validar `SdhCommand`
- mapear SDH para SDGW compacto via `SdhToSdgwMapper`
- encaminhar o envio para o `SdgwSession`

### `SdGwLinkSupervisor`

É o supervisor lógico do link.

Ele usa watchdog por silêncio de RX válido e agenda ping apenas quando necessário.

Não existe mais ping periódico fixo como mecanismo central de vida da sessão.

## Fluxo de transmissão

O fluxo de TX atual para um comando funcional é:

    Client funcional
        -> SdhClient
        -> SdhToSdgwMapper
        -> SdgwSession
        -> SdGwTxScheduler (High)
        -> SdGwLinkEngine
        -> SerialTransport

O fluxo de TX do supervisor é:

    SdGwLinkSupervisor
        -> callback de ping
        -> SdGwTxScheduler (Low)
        -> SdGwLinkEngine
        -> SerialTransport

Com isso:

- o supervisor não compete mais diretamente com comandos funcionais
- a arbitragem principal ocorre no scheduler
- `Busy` deixa de ser o mecanismo normal de concorrência interna

## Estratégia atual de keepalive

### No host

O host considera o link vivo quando há RX SDGW válido recente.

Regras atuais:

- qualquer frame SDGW válido recebido renova o watchdog do link
- isso inclui `ACK`, `ERR`, frame normal e evento
- ping só é tentado após ociosidade
- o link só é marcado como falho quando o silêncio ultrapassa o timeout lógico configurado

Configuração usada hoje no `BpmSerialService`:

- `IdleBeforePingMs = 1500`
- `LinkTimeoutMs = 3000`
- `PingTimeoutMs = 150`
- `PingRetries = 2`
- `TickPeriodMs = 50`

### Na BPM

O firmware da BPM foi alinhado ao mesmo conceito:

- a sessão é mantida por atividade SDGW válida
- `PING 0x55` continua suportado, mas não é a única prova de vida
- o watchdog do firmware mede silêncio de frames válidos

Valores atuais no firmware:

- timeout de atividade do link: `4000 ms`
- timeout interno do router/gateway: `100 ms`

## Recepção e tolerância após o primeiro `Linked`

O bootstrap serial continua híbrido:

- texto no handshake inicial
- binário SDGW na operação normal

Mas o comportamento atual do host foi reforçado:

- depois que a conexão atinge `Linked` pela primeira vez
- o `BpmSerialService` passa a tratar a sessão SDGW como estabelecida naquela conexão serial
- a partir daí, tráfego binário SDGW continua podendo ser entregue ao engine mesmo se o estado lógico cair temporariamente para `LinkFailed`

Esse ajuste existe para não descartar `ACK`s e respostas tardias como se fossem texto de handshake.

## Fluxo atual da GSA

O caso funcional de LED embutido da GSA é o fluxo mais sensível da UI atual.

Fluxo:

    GsaClient.SetBuiltinLedAsync(bool)
        -> SdhClient.SendAsync(...)
        -> SdhToSdgwMapper.MapGsaLed(...)
        -> SdgwSession.SendAsync(..., priority: High)
        -> SdGwTxScheduler
        -> SdGwLinkEngine

Ajustes já incorporados:

- timeout do comando de LED: `400 ms`
- retries do comando de LED: `2`
- correlação de resposta reforçada no `GsaClient`
- validação do payload de resposta antes de completar a pendência
- conferência do `AppliedState` esperado para reduzir aceite de resposta tardia incorreta

## Estado atual da recepção

A recepção ainda é baseada em `SggwFrame` no nível de consumo da sessão.

Isso significa:

- TX funcional já é mediado por SDH + scheduler
- RX funcional ainda chega como frame SDGW validado

Essa escolha foi mantida para preservar compatibilidade incremental com o restante do host.

## Conclusão

A arquitetura atual do host pode ser resumida assim:

- `BpmSerialService` é a composição central do link
- `SdGwTxScheduler` é o único caminho normal de TX
- `SdGwLinkEngine` continua técnico e stop-and-wait
- `SdGwLinkSupervisor` supervisiona silêncio de RX válido
- `SdhClient` mantém a semântica de comando acima do SDGW
- a BPM e o host agora compartilham a mesma ideia de keepalive por atividade válida, e não por ping periódico fixo

[Retornar ao README principal](../README.md)
