⬅ [Retornar para Testes de Bancada](../08-casos-de-uso/03-testes-bancada.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Testes de Integração

## Estado atual

Os testes de integração mais relevantes do projeto são os que atravessam toda a cadeia:

    WinForms -> host SDGW/SDH -> BPM -> baby board

Historicamente, o caso mais representativo foi o fluxo do LED da GSA.

Com a entrada da UCE no host e no gateway, esse cenário continua válido como teste-base, mas já não é o único fluxo funcional relevante. A tela da UCE agora também possui integração real para configuração da porta CAN usando a mesma rota compacta da UCE.

## Caminho integrado real

```text
WinForms
  -> FrmGsaLogic / FrmUceLogic / FrmBpmLogic
  -> BpmSerialService.Shared
  -> GsaClient / UceClient / BpmClient
  -> SdhClient
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SerialTransport
  -> BPM / link do gateway / GatewayApp
  -> GwRouter
  -> I2C / SPI
  -> GSA / UCE
```

## Cenários de integração mais representativos

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

Caso de teste: alterar o estado do `LED_BUILTIN` da UCE.

1. a UI dispara o comando em `frmUCE_UI`
2. o `UceClient` monta `SdhCommand` para `UCE.led`
3. o `SdhClient` valida e mapeia para `SDGW_CMD_UCE_TLV`
4. o `SdGwTxScheduler` envia em prioridade `High`
5. a BPM valida o frame e roteia a transação para `GwSpiBus`
6. a UCE devolve a resposta TLV síncrona por `SPI`
7. a BPM valida o `CRC` da resposta da UCE
8. o `UceClient` confirma o estado aceito do `LED_BUILTIN`

Caso de teste: configurar a porta CAN da UCE.

1. a UI dispara `UCE.can.config set controller=can0 bitrate=... mode=...`
2. `FrmUceLogic` mantém o controller fixo em `can0`
3. `UceClient` monta o `SdhCommand` e espera uma response fixa de `0x20`
4. o `SdhClient` valida e mapeia para `SDGW_CMD_UCE_TLV`
5. a BPM mantém o mesmo binding lógico `0x2` e roteia a transação para `GwSpiBus`
6. a UCE despacha `CMD_CAN_CONFIG` em `Service` e chama `CanService::configure(...)`
7. a UCE devolve response TLV síncrona com controller, bitrate e modo aceitos
8. o host atualiza a UI a partir de `UCE.can.status get`

Caso de teste: habilitar, desabilitar e consultar status da porta CAN da UCE.

1. a UI envia `UCE.can.enable set controller=can0 state=on|off`
2. a UCE mapeia `state=on` para `CanService::open()` e `state=off` para `CanService::close()`
3. a UI envia `UCE.can.status get controller=can0`
4. a UCE responde com `controller`, `interface_state`, `bitrate_code` e `mode`
5. o `UceClient` e `UceParsers` atualizam a UI com status textual compacto

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
- o caso UCE LED já foi validado em bancada com resposta síncrona e `CRC` estável
- a feature CAN da UCE foi comissionada por análise de fluxo e build, mas ainda não teve validação física registrada em bancada nesta rodada
- ainda faltam roteiros equivalentes para setpoint, status, offsets e fault event da GSA
- o roteiro oficial precisa validar também o caminho físico `D21/D22`, `D4/D19` e `D23`
- o roteiro oficial agora também cobre `SPI 18/26/25`, `CS 33`, `IRQ 27` e `RESET 23` para a UCE
- ainda não há cobertura equivalente para múltiplas boards em paralelo
- a recepção funcional ainda é baseada em frame lógico tipado do enlace
- não há evento assíncrono novo da UCE para a feature CAN
- o comando `UCE.can reset ...` existe no contrato e na implementação, mas não está ligado a um botão da UI

## Evolução prevista

Os próximos ganhos naturais são:

- mais cenários além do LED
- validação física em bancada da configuração CAN da UCE
- roteiros específicos para:
  - `GSA.channel.status`
  - `GSA.channels.status`
  - `GSA.channel.offset`
  - evento assíncrono de fault
- roteiros específicos para `UCE.can.config`, `UCE.can.enable` e `UCE.can.status`
- validação de eventos assíncronos
- testes cruzando múltiplos destinos da BPM
- maior formalização dos roteiros de integração

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.
- **SDH**: SimulDiesel Hardware Command, envelope semântico de comandos do projeto.
- **TLV**: Type-Length-Value, formato interno de payload usado em transações específicas.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
