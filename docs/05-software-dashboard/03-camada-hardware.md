# Camada Hardware do Software

## Estado atual

A camada de hardware do software local é a parte da aplicação que conecta a UI WinForms ao link serial SDGW/SDH da BPM.

Hoje ela é organizada em torno de:

- `SerialTransport`
- `BpmSerialService`
- `SdGwLinkEngine`
- `SdGwTxScheduler`
- `SdgwSession`
- `SdhClient`
- `SdGwLinkSupervisor`
- clients funcionais por board, como `GsaClient` e `BpmClient`

`SerialLink`, `SerialLinkService`, `SdGgwClient` e `SdgwHealthService` não fazem mais parte da arquitetura ativa.

## Componentes centrais

### Transporte e link

- `SerialTransport`: I/O serial bruto
- `BpmSerialService`: fachada funcional do link serial da BPM
- `SdGwLinkEngine`: framing SDGW, `ACK`, `ERR`, timeout/retry e stop-and-wait
- `SdGwTxScheduler`: fila central de transmissão com prioridade
- `SdGwLinkSupervisor`: watchdog lógico por silêncio de RX válido

### Sessão e semântica

- `SdgwSession`: sessão de alto nível do SDGW sobre o scheduler
- `SdhClient`: camada semântica de comandos SDH
- `SdhValidator`: validação estrutural do comando
- `SdhToSdgwMapper`: adaptação de SDH para SDGW compacto

### Clients funcionais

- `GsaClient`: operações da GSA
- `BpmClient`: operações funcionais da própria BPM

## Responsabilidades por classe

### `BpmSerialService`

Responsabilidades:

- compor todo o link host
- controlar conexão/desconexão
- executar handshake textual inicial
- publicar estado do link
- coordenar supervisor, sessão e clients

Ponto global transitório atual:

    BpmSerialService.Shared

### `SdGwTxScheduler`

É o centro de arbitragem de TX.

Responsabilidades:

- enfileirar solicitações
- priorizar envios
- despachar um item por vez para o engine
- evitar competição direta entre comandos funcionais e pings internos

Prioridades:

- `High`
- `Normal`
- `Low`

Uso corrente:

- funcional da aplicação: `High`
- supervisor: `Low`

### `SdGwLinkSupervisor`

Supervisiona o link sem ping fixo periódico.

Regras atuais:

- RX SDGW válido prova vida
- silêncio abaixo de `IdleBeforePingMs` não gera ping
- silêncio acima do limiar pode agendar ping
- silêncio acima de `LinkTimeoutMs` derruba a saúde lógica

Configuração atual no host:

- `IdleBeforePingMs = 1500`
- `LinkTimeoutMs = 3000`

### `SdgwSession`

Fornece a API de envio e eventos sobre o scheduler.

O envio funcional agora passa por:

    SdgwSession -> SdGwTxScheduler -> SdGwLinkEngine

### `SdhClient`

Converte intenção funcional em envio SDGW compacto.

O fluxo atual é:

    SdhCommand
        -> validação
        -> mapeamento SDGW
        -> envio via sessão

## Fluxo técnico interno

O fluxo funcional atual no host é:

    UI / FormsLogic
      -> BpmSerialService.Shared
      -> GsaClient / BpmClient
      -> SdhClient.SendAsync(...)
      -> SdhToSdgwMapper.Map(...)
      -> SdgwSession.SendAsync(...)
      -> SdGwTxScheduler
      -> SdGwLinkEngine
      -> SerialTransport

Esse desenho mantém a UI fora de:

- `COBS`
- `CRC8`
- `SEQ`
- timeout/retry
- arbitragem de prioridade

## Keepalive e saúde do link

O comportamento atual do host é:

- a saúde do link é baseada em RX SDGW válido
- o supervisor agenda ping apenas sob silêncio
- o ping não é mais prova exclusiva de vida
- a BPM foi alinhada ao mesmo conceito de atividade válida

Isso substitui o modelo antigo de health service por ping periódico fixo.

## Recepção e recuperação do link

O host continua usando bootstrap textual no começo da conexão.

Depois do primeiro `Linked` da conexão atual:

- o `BpmSerialService` mantém a sessão SDGW da conexão como estabelecida
- tráfego binário SDGW continua podendo ser entregue ao engine mesmo se o estado lógico cair para `LinkFailed`

Esse ajuste evita descarte indevido de:

- `ACK`s tardios
- respostas tardias
- eventos SDGW que ainda chegam com a porta aberta

## Caso atual da GSA

Historicamente, o caso funcional mais exercitado no host foi `GSA.led set state=on|off`.

Esse fluxo continua preservado, mas o host agora também suporta a expansão funcional da GSA para:

- setpoint por canal;
- enable por canal;
- enable global;
- status por canal;
- status global;
- fault reset por canal;
- offsets por canal;
- save/reset de offsets;
- reset global de offsets;
- evento assíncrono de fault.

### Fluxo funcional vigente

Fluxo base:

    FrmGsaLogic
      -> GsaClient
      -> SdhClient
      -> SdhToSdgwMapper
      -> SdgwSession (priority: High)
      -> SdGwTxScheduler
      -> SdGwLinkEngine

Fluxos expostos hoje pelo `GsaClient`:

- `SetBuiltinLedAsync(bool)`
- `SetChannelSetpointAsync(...)`
- `SetChannelEnableAsync(...)`
- `SetChannelsEnableAsync(...)`
- `GetChannelStatusAsync(...)`
- `GetChannelsStatusAsync()`
- `ResetChannelFaultAsync(...)`
- `SetChannelOffsetAsync(...)`
- `GetChannelOffsetAsync(...)`
- `SaveChannelOffsetAsync(...)`
- `ResetChannelOffsetAsync(...)`
- `ResetOffsetsAsync()`

### Recepção funcional da GSA

O `GsaClient` consome:

- respostas funcionais recebidas como `SggwFrame`;
- erros funcionais TLV da GSA;
- evento assíncrono de `fault` via `SdgwSession.EventReceived`.

Regras observadas no host:

- não há evento assíncrono normal para enable/disable;
- não há telemetria contínua por evento;
- o evento assíncrono publicado ao host é apenas o snapshot de `fault` do canal.

### Compatibilidade preservada

O fluxo do LED builtin continua compatível e não foi removido.

Houve, porém, uma inconsistência histórica no contrato TLV da GSA:

- o LED builtin já usava `type = 0x12`;
- uma fase intermediária da expansão da GSA também chegou a documentar `type = 0x12` para `channel.status`.

O contrato oficial atual resolve esse ponto da seguinte forma:

- `0x12` permanece dedicado ao LED builtin legado;
- `0x1B` passa a ser o type oficial de `channel.status`.

Isso preserva o LED sem regressão e remove a necessidade de distinguir o status por canal por `len`.

## Limitações atuais

- o host ainda opera com uma sessão serial por vez
- a recepção funcional ainda é entregue como `SggwFrame`
- `BpmSerialService.Shared` ainda é um ponto global transitório
- o catálogo SDH suportado no host ainda é parcial em relação ao catálogo documental geral
- o conflito histórico de `type 0x12` foi resolvido migrando `channel.status` para `0x1B`

## Referência adicional

Para o detalhamento formal do host atual, consulte:

- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/06-protocolos/06-gsa-sdh-tlv.md`

[Retornar ao README principal](../README.md)
