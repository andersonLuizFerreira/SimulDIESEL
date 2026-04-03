# Testes de Integração

## Estado atual

Os testes de integração mais relevantes do projeto são os que atravessam toda a cadeia:

    WinForms -> host SDGW/SDH -> BPM -> baby board

Historicamente, o caso mais representativo foi o fluxo do LED da GSA.

Com a expansão atual da GSA no host, esse cenário continua válido como teste-base, mas já não é o único fluxo funcional relevante.

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
7. a GSA devolve a resposta TLV síncrona
8. a BPM devolve essa resposta ao host
9. a GSA conclui a etapa física, aciona IRQ e publica `0x31`
10. a BPM busca o evento e o reencaminha ao host
11. o `GsaClient` valida a resposta síncrona e o evento físico final

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

- o caso GSA LED é o principal cenário ponta a ponta já exercitado
- ainda faltam roteiros equivalentes para setpoint, status, offsets e fault event da GSA
- o roteiro oficial precisa validar também o caminho físico `D21/D22`, `D4/D19` e `D23`
- ainda não há cobertura equivalente para múltiplas boards em paralelo
- a recepção funcional ainda é baseada em `SggwFrame`

## Evolução prevista

Os próximos ganhos naturais são:

- mais cenários além do LED
- roteiros específicos para:
  - `GSA.channel.status`
  - `GSA.channels.status`
  - `GSA.channel.offset`
  - evento assíncrono de fault
- validação de eventos assíncronos
- testes cruzando múltiplos destinos da BPM
- maior formalização dos roteiros de integração

[Retornar ao README principal](../README.md)
