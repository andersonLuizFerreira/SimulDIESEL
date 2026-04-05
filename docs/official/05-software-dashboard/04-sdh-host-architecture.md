⬅ [Retornar para Camada Hardware do Software](03-camada-hardware.md)

# Arquitetura SDH no Host

## Objetivo

Este documento descreve a arquitetura atual do host SDGW/SDH no SimulDIESEL, com foco no comportamento efetivamente implementado na `local-api` e na UI WinForms.

## Composição atual

```text
UI / FormsLogic
    -> BpmSerialService.Shared
    -> GsaClient / BpmClient
    -> SdhClient
    -> SdgwSession
    -> SdGwTxScheduler
    -> SdGwLinkEngine
    -> SwitchableTransport
    -> SerialTransport / BluetoothTransport
```

Além desse caminho principal, o `BpmSerialService` coordena:

- seleção do transporte ativo;
- handshake textual inicial com a BPM;
- `SdGwLinkSupervisor`;
- transição de estados do link.

## Contrato atual do link

- `SdGwTxScheduler` é o único caminho normal de TX;
- `SwitchableTransport` mantém somente um transporte ativo por vez;
- comandos funcionais usam prioridade `High`;
- RX SDGW válido continua provando vida do link;
- ping só ocorre sob silêncio;
- o host não depende de `Busy` como mecanismo de concorrência interna.

## Fluxo atual da GSA

O catálogo já ativo na stack do host continua incluindo:

- `GSA.led`
- `GSA.channel.setpoint`
- `GSA.channel.enable`
- `GSA.channels.enable`
- `GSA.channel.status`
- `GSA.channel.fault`
- `GSA.channel.offset`
- `GSA.offset`

## Mudança arquitetural oficial da GSA

O host não usa mais o modelo anterior de BUSY/IDLE com retry aguardando `IDLE`.

O fluxo oficial agora é:

1. a UI dispara um comando semântico;
2. o `GsaClient` envia o comando pela stack SDGW;
3. a BPM entrega o TLV à GSA por `I2C` físico em `D21/D22` -> `A4/A5`;
4. a resposta síncrona apenas confirma recepção/aceite;
5. a GSA executa a operação física internamente no barramento lógico `D2/D3`;
6. a BPM recebe IRQ da GSA em `D19`;
7. a BPM encaminha um evento assíncrono `0x31`;
8. o `GsaClient` publica esse evento para `FormsLogic` e UI.

## Papel do `GsaClient`

O `GsaClient` agora:

- correlaciona apenas a resposta síncrona imediata do comando;
- trata erro funcional `0x7F`;
- trata evento de fault `0x30`;
- trata evento de resultado físico `0x31`;
- não mantém mais estado remoto BUSY/IDLE;
- não faz retry automático aguardando `IDLE`.

## Evento assíncrono `0x31`

O novo evento físico da GSA tem payload:

```text
[origin_type][channel][status]
```

Status reconhecidos no host:

- `0x01` = operação OK
- `0x02` = falha de ACK no `TCA9548A`
- `0x03` = falha de ACK no `MCP4725`

No host, esse evento percorre:

```text
SdgwSession.EventReceived
    -> GsaClient
    -> FrmGsaLogic
    -> frmGSA_UI
```

## Papel atual da UI da GSA

A UI preserva o que já havia sido integrado:

- checkbox do `LED_BUILTIN`
- enable/disable por canal
- slider com envio apenas no soltar
- conversão `raw <-> volts`
- refresh de status por canal

Além disso, agora ela também:

- exibe o resultado físico recebido via `0x31`;
- atualiza o canal afetado após o evento assíncrono.

## Observações

- o fluxo ativo do host não depende mais de BUSY/IDLE;
- o caso do LED builtin continua sendo o teste ponta a ponta mais simples;
- o contrato TLV detalhado da GSA está em `docs/official/06-protocolos/06-gsa-sdh-tlv.md`.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.

