# Arquitetura do Software Dashboard (Local API)

## Estrutura atual da aplicação WinForms

A aplicação local segue o fluxo:

```text
UI (Forms)
  -> FormsLogic / BLL
  -> BpmSerialService
  -> DAL de protocolo e transporte
  -> BPM
```

Os componentes centrais hoje são:

- UI: `DashBoard`, `frmPortaSerial_UI`, `frmGSA_UI`
- BLL: `BpmSerialService`, `GsaClient`, `BpmClient`, `FrmBpmLogic`, `FrmGsaLogic`
- DAL/protocolo: `SerialTransport`, `SdGwLinkEngine`, `SdGwTxScheduler`, `SdgwSession`, `SdhClient`, `SdGwLinkSupervisor`

## Núcleo de orquestração

O núcleo funcional atual do software local é:

    BpmSerialService

Ele:

- controla conexão e desconexão
- mantém o estado lógico do link
- compõe sessão, scheduler, supervisor e clients funcionais

O acesso global transitório atualmente consumido pela UI é:

    BpmSerialService.Shared

## Fluxo de execução atual

1. a UI solicita conexão
2. o `SerialTransport` abre a COM
3. o `BpmSerialService` executa o bootstrap textual
4. após o banner válido, o link entra em `Linked`
5. comandos funcionais são enviados por `SdhClient -> SdgwSession -> SdGwTxScheduler -> SdGwLinkEngine`

## Arbitragem de TX

Todo TX normal do host passa pelo `SdGwTxScheduler`.

Prioridades atuais:

- `High`
- `Normal`
- `Low`

Uso atual:

- comandos funcionais: `High`
- pings do supervisor: `Low`

Isso evita que supervisão e tráfego funcional disputem diretamente o stop-and-wait do engine.

## Estado e saúde do link

O `SdGwLinkSupervisor` é o supervisor vigente.

Comportamento atual:

- RX SDGW válido mantém o link vivo
- ping é agendado apenas sob silêncio
- o host não depende de ping periódico fixo
- a BPM foi alinhada ao mesmo conceito de atividade válida

## Caso funcional atual

O caso funcional mais exercitado é o LED embutido da GSA.

Fluxo:

```text
frmGSA_UI
  -> FrmGsaLogic
  -> BpmSerialService.Shared.Gsa
  -> GsaClient
  -> SdhClient
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> BPM
  -> GSA
```

## Pontos de estabilidade observados

- o envio funcional não depende mais de competição direta com o supervisor
- o host aceita tráfego SDGW binário tardio após o primeiro `Linked`
- o fluxo da GSA LED já usa timeout/retry mais tolerantes e correlação de resposta reforçada

[Retornar ao README principal](../README.md)
