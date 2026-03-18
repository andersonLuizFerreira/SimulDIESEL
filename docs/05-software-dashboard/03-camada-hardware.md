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

O caso funcional mais exercitado no host atual é `GSA.led set state=on|off`.

Fluxo:

    GsaClient.SetBuiltinLedAsync(bool)
      -> GsaClient.SetLedAsync(bool)
      -> SdhClient
      -> SdhToSdgwMapper.MapGsaLed(...)
      -> SdgwSession (priority: High)
      -> SdGwTxScheduler
      -> SdGwLinkEngine

Ajustes recentes de robustez:

- timeout de ACK do LED aumentado para `400 ms`
- retries do LED aumentados para `2`
- correlação de resposta reforçada no `GsaClient`
- conferência do estado aplicado esperado

Esses ajustes reduziram a instabilidade sob clique repetido no `LED_BUILTIN`.

## Limitações atuais

- o host ainda opera com uma sessão serial por vez
- a recepção funcional ainda é entregue como `SggwFrame`
- `BpmSerialService.Shared` ainda é um ponto global transitório
- o catálogo SDH suportado no host continua pequeno

## Referência adicional

Para o detalhamento formal do host atual, consulte:

- `docs/05-software-dashboard/04-sdh-host-architecture.md`

[Retornar ao README principal](../README.md)
