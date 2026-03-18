# Fluxo de Comunicação

## Estado atual

O fluxo de comunicação implementado no SimulDIESEL continua híbrido:

- handshake textual no bootstrap da conexão serial
- comunicação binária SDGW durante a operação normal

A diferença em relação ao desenho antigo é que o host agora possui um agendador central de TX e o keepalive não depende mais de ping periódico fixo.

## Fase 1: estabelecimento inicial do link

### Host

O `BpmSerialService` controla a transição:

    Disconnected
      -> SerialConnected
      -> Draining
      -> BannerSent
      -> Linked
      -> LinkFailed

Fluxo:

1. a porta serial é aberta
2. o host drena ruído inicial
3. o host envia o banner `SIMULDIESELAPI`
4. a BPM responde com a linha de identificação
5. o host marca `Linked`

### BPM

O `SggwLink` do firmware controla:

    WaitingBanner
      -> Linked

Enquanto está em `WaitingBanner`, a BPM aceita apenas o handshake textual.

## Fase 2: operação binária SDGW

Depois do bootstrap, o fluxo binário passa a ser:

    UI / FormsLogic
      -> BpmSerialService.Shared
      -> Client funcional
      -> SdhClient
      -> SdgwSession
      -> SdGwTxScheduler
      -> SdGwLinkEngine
      -> SerialTransport
      -> BPM / SggwLink
      -> GatewayApp
      -> GwRouter
      -> barramento físico
      -> device

## Estrutura do frame SDGW

O wire format continua:

    CMD | FLAGS | SEQ | PAYLOAD... | CRC8

Depois disso:

- o quadro é codificado em `COBS`
- o delimitador final é `0x00`

Nada mudou em:

- `CRC-8/ATM`
- framing `COBS`
- `ACK`
- `ERR`
- flags de transporte

## Arbitragem de transmissão no host

O host não envia mais direto do client funcional para o engine.

Todo TX normal passa por:

    SdgwSession -> SdGwTxScheduler -> SdGwLinkEngine

O scheduler aplica prioridade:

- `High`
- `Normal`
- `Low`

Uso atual:

- comandos funcionais da aplicação: `High`
- pings internos do supervisor: `Low`

Dentro da mesma prioridade, a ordem é FIFO.

Essa fila central evita que o supervisor dispute diretamente o slot stop-and-wait com comandos funcionais.

## Keepalive atual

### No host

O `SdGwLinkSupervisor` trabalha como watchdog lógico de silêncio.

Regras:

- RX SDGW válido mantém o link vivo
- ping só é agendado sob ociosidade
- o silêncio prolongado é que derruba o link

Configuração atual do host:

- `IdleBeforePingMs = 1500`
- `LinkTimeoutMs = 3000`
- `PingTimeoutMs = 150`
- `PingRetries = 2`

### Na BPM

O firmware da BPM foi alinhado ao mesmo conceito.

Regras atuais:

- qualquer frame SDGW válido renova a atividade da sessão
- `PING 0x55` continua suportado, mas não é mais a única prova de vida
- o watchdog do firmware mede silêncio de frames válidos

Parâmetros atuais:

- timeout de atividade do link: `4000 ms`
- timeout do router/gateway: `100 ms`

## Recepção após o primeiro `Linked`

O host foi ajustado para ser mais tolerante com tráfego tardio.

Depois que a conexão serial já atingiu `Linked` pelo menos uma vez:

- o `BpmSerialService` passa a considerar a sessão SDGW estabelecida naquela conexão
- tráfego binário SDGW continua sendo entregue ao `SdGwLinkEngine` mesmo se o estado lógico cair para `LinkFailed`
- isso vale enquanto a porta serial continuar aberta

Objetivo:

- não tratar `ACK`s tardios como texto de handshake
- não perder respostas tardias por regressão prematura ao bootstrap textual

## Exemplo funcional atual: GSA LED

Fluxo:

1. a UI dispara `GsaClient.SetBuiltinLedAsync(bool)`
2. o `GsaClient` monta `SdhCommand` para `GSA.led set state=on|off`
3. o `SdhClient` valida e mapeia para SDGW compacto
4. o envio entra no `SdGwTxScheduler` com prioridade `High`
5. o `SdGwLinkEngine` envia o frame e aguarda `ACK`
6. a BPM roteia a transação para a GSA
7. a resposta TLV retorna como evento SDGW
8. o `GsaClient` valida a resposta e correlaciona o estado aplicado

Ajustes recentes desse fluxo:

- timeout do LED: `400 ms`
- retries do LED: `2`
- correlação de resposta reforçada

## Limitações atuais

- o host continua com uma única transação ativa no engine por vez
- a fila resolve arbitragem, não paralelismo real
- a recepção funcional ainda usa `SggwFrame`
- o catálogo funcional suportado continua pequeno

## Referências

- `docs/05-software-dashboard/03-camada-hardware.md`
- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/04-firmware/01-arquitetura-firmware.md`

[Retornar ao README principal](../README.md)
