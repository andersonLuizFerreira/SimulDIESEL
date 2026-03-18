# Testes de Integração

## Estado atual

Os testes de integração mais relevantes do projeto são os que atravessam toda a cadeia:

    WinForms -> host SDGW/SDH -> BPM -> baby board

Hoje, o caso mais representativo continua sendo o fluxo do LED da GSA.

## Caminho integrado real

```text
WinForms
  -> FrmGsaLogic / FrmBpmLogic
  -> BpmSerialService.Shared
  -> GsaClient / BpmClient
  -> SdhClient
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SerialTransport
  -> BPM / SggwLink / GatewayApp
  -> GwRouter
  -> I2C
  -> GSA
```

## Cenário de integração mais representativo

Caso de teste: alterar o estado do LED embutido da GSA.

1. a UI dispara o comando
2. o `GsaClient` monta `SdhCommand` para `GSA.led`
3. o `SdhClient` valida e mapeia para SDGW compacto
4. o `SdGwTxScheduler` envia em prioridade `High`
5. o `SdGwLinkEngine` aguarda `ACK`
6. a BPM valida o frame e roteia a transação para a GSA
7. a GSA responde em TLV
8. a BPM devolve a resposta ao host como evento SDGW
9. o `GsaClient` valida a resposta e confere o estado aplicado

## Evidências atuais de robustez

- estados de link explícitos no `BpmSerialService`
- stop-and-wait técnico concentrado no `SdGwLinkEngine`
- arbitragem de TX centralizada no `SdGwTxScheduler`
- keepalive por atividade SDGW válida no host e na BPM
- tolerância do host para `ACK`s e respostas tardias após o primeiro `Linked`

## O que não deve mais ser tratado como fluxo principal

Os testes de integração não devem mais assumir:

- envio manual de ping como passo central da operação normal
- concorrência interna resolvida por `Busy`
- arquitetura baseada em `SerialLink`, `SerialLinkService`, `SdGgwClient` ou `SdgwHealthService`

## Limitações

O conjunto de testes de integração ainda é pequeno em diversidade funcional.

Hoje:

- o caso GSA LED é o principal cenário ponta a ponta
- ainda não há cobertura equivalente para múltiplas boards em paralelo
- a recepção funcional ainda é baseada em `SggwFrame`

## Evolução prevista

Os próximos ganhos naturais são:

- mais cenários além do LED
- validação de eventos assíncronos
- testes cruzando múltiplos destinos da BPM
- maior formalização dos roteiros de integração

[Retornar ao README principal](../README.md)
