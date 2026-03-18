# Visão Arquitetural

## Estado atual

A arquitetura do SimulDIESEL continua multicamada e orientada a gateway, mas a pilha do host foi consolidada em torno da BPM como dona funcional do link serial.

Hoje o desenho vigente é:

```text
UI / FormsLogic
    -> BpmSerialService
    -> SdhClient / Clients por board
    -> SdgwSession
    -> SdGwTxScheduler
    -> SdGwLinkEngine
    -> SerialTransport
    -> BPM / SggwLink / GatewayApp / GwRouter
    -> I2C / SPI
    -> dispositivo
```

## Camada host

Os componentes centrais do host são:

- `BpmSerialService`: dono funcional do link serial da BPM
- `SdGwLinkEngine`: infraestrutura técnica de framing, `ACK`, timeout e retry
- `SdGwTxScheduler`: arbitragem central de TX com prioridades
- `SdgwSession`: sessão de alto nível sobre engine + scheduler
- `SdhClient`: camada semântica SDH sobre SDGW compacto
- `SdGwLinkSupervisor`: supervisão atual da saúde do link
- `GsaClient` e `BpmClient`: fachada funcional por board

O acesso global transitório usado pela UI atual é:

    BpmSerialService.Shared

## Camada gateway

Na BPM, a arquitetura vigente é:

- `SggwLink`: handshake textual, framing SDGW, `ACK`, `ERR` e watchdog de atividade
- `GatewayApp`: tratamento local da BPM e despacho de comandos roteados
- `GwRouter`: seleção de barramento e destino físico
- `GwI2cBus` / `GwSpiBus`: execução da transação física

## Camada de dispositivo

No estado atual, o caso funcional mais exercitado é a GSA.

O contrato interno continua baseado em TLV curto com CRC próprio da transação da baby board.

## Fluxo arquitetural real

1. a UI dispara uma ação
2. o client funcional monta um `SdhCommand`
3. o `SdhClient` valida e mapeia o comando
4. o `SdgwSession` envia pela fila do `SdGwTxScheduler`
5. o `SdGwLinkEngine` aplica o stop-and-wait
6. a BPM valida o frame SDGW
7. o `GatewayApp` resolve se o comando é local ou roteado
8. o barramento entrega a transação à board
9. a resposta volta como tráfego SDGW válido ao host

## Keepalive e saúde do link

O enlace atual é supervisionado por atividade SDGW válida, não por ping fixo periódico.

No host:

- `SdGwLinkSupervisor` mede silêncio de RX válido
- ping só é agendado sob ociosidade

Na BPM:

- qualquer frame SDGW válido renova a sessão
- watchdog de atividade do link: `4000 ms`
- timeout do router/gateway: `100 ms`

## Limitações

A arquitetura está mais madura no enlace host/gateway do que no volume de serviços embarcados.

Hoje:

- o caso GSA LED é o principal fluxo ponta a ponta
- o catálogo SDH suportado continua pequeno
- a recepção funcional no host ainda é baseada em `SggwFrame`

## Evolução prevista

As decisões atuais favorecem expansão sem romper o enlace:

- inclusão de novos comandos SDH sobre a mesma base
- ampliação de boards e serviços na BPM
- redução gradual de pontos transitórios da UI
- evolução da observabilidade e dos testes de integração

[Retornar ao README principal](../README.md)
