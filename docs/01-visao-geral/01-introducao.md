# Introdução

## Estado atual

O SimulDIESEL é um ambiente de bancada em que um software local C# WinForms se comunica com a BPM por serial e usa a BPM como gateway para recursos e baby boards da bancada.

O caminho funcional hoje consolidado no repositório é:

- host local em WinForms
- BPM como dona do gateway
- roteamento para barramentos internos
- caso funcional ativo de LED da GSA

A composição observada no código atual é:

- no host: `BpmSerialService`, `SdGwLinkEngine`, `SdGwTxScheduler`, `SdgwSession`, `SdhClient`, `SdGwLinkSupervisor`, `GsaClient` e `BpmClient`
- na BPM: `SggwLink`, `GatewayApp`, `GwRouter` e barramentos internos
- na GSA: transação TLV curta com CRC próprio da baby board

```text
PC/WinForms -> Serial (bootstrap textual + SDGW binário) -> BPM Gateway -> I2C/SPI -> dispositivo endereçado
```

## Funcionamento técnico

O host abre a serial e o `BpmSerialService` executa o bootstrap textual até o primeiro `Linked`.

Depois disso:

- o `SdGwLinkEngine` trata o frame SDGW
- o `SdGwTxScheduler` arbitra todo o TX do link
- o `SdgwSession` expõe envio e recepção em nível de sessão
- o `SdhClient` traduz intenção funcional para SDGW compacto

O scheduler possui prioridades:

- `High` para comandos funcionais
- `Normal` para uso interno geral
- `Low` para ping do supervisor

No firmware, a BPM valida o quadro SDGW, trata comandos locais ou roteia a transação ao barramento correto. A resposta da baby board volta para a BPM e é enviada ao host como tráfego SDGW válido.

## Keepalive atual

O projeto não depende mais de ping periódico fixo para manter a sessão.

No host:

- RX SDGW válido mantém o link vivo
- o `SdGwLinkSupervisor` só agenda ping quando há silêncio

Na BPM:

- qualquer frame SDGW válido renova a atividade da sessão
- o watchdog de atividade do link usa `4000 ms`
- o timeout interno do router/gateway usa `100 ms`

## Robustez atual do enlace

O host foi ajustado para continuar aceitando tráfego binário SDGW após o primeiro `Linked` bem-sucedido da conexão atual, mesmo se o estado lógico cair temporariamente para `LinkFailed` com a porta ainda aberta.

Esse comportamento evita descarte indevido de:

- `ACK`s tardios
- respostas tardias
- eventos SDGW ainda válidos

## Limitações

O repositório ainda está mais maduro na infraestrutura de comunicação do que no volume de serviços de domínio.

Hoje:

- o caso funcional mais exercitado é o LED da GSA
- a recepção funcional no host ainda é baseada em `SggwFrame`
- a camada de nuvem ainda não é o centro do projeto

## Evolução prevista

A evolução natural do projeto é:

- ampliar o catálogo de boards e comandos SDH suportados
- expandir os serviços da BPM e das baby boards
- formalizar mais cenários de integração
- reduzir pontos transitórios de composição como `BpmSerialService.Shared`

[Retornar ao README principal](../README.md)
