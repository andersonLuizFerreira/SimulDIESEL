# SimulDIESEL

Plataforma de bancada para comunicação entre um software local em C# WinForms e uma BPM que atua como gateway para baby boards, com destaque atual para o fluxo funcional da GSA.

## Estrutura principal do repositório

```text
SimulDIESEL/
├─ README.md
├─ docs/                     Documentação técnica oficial
├─ local-api/                Aplicação local WinForms e pilha host SDGW/SDH
├─ hardware/                 Hardware e firmware embarcado
├─ cloud/                    Camada de nuvem e contratos remotos
├─ tests/                    Testes de sistema e HIL
└─ tools/                    Scripts auxiliares
```

## Áreas mais relevantes hoje

### `local-api/`

Contém a aplicação local em C# WinForms e a pilha atual do host:

- `BpmSerialService` como dono funcional do link serial da BPM
- `SdGwLinkEngine` como camada técnica de framing, `ACK`, timeout e retry
- `SdGwTxScheduler` como caminho central de TX com prioridades `High`, `Normal` e `Low`
- `SdgwSession` como sessão de alto nível sobre engine + scheduler
- `SdhClient` como camada semântica SDH sobre SDGW compacto
- `SdGwLinkSupervisor` como supervisor atual de saúde do link

O ponto global transitório usado pela UI atual é:

    BpmSerialService.Shared

### `hardware/`

Contém o firmware da BPM e das baby boards.

No estado atual do enlace:

- a BPM mantém a sessão por atividade SDGW válida
- o watchdog de atividade do link na BPM usa `4000 ms`
- o timeout interno do router/gateway usa `100 ms`
- o `PING 0x55` continua suportado, mas não é mais o único keepalive

## Fluxo operacional atual

O caminho principal hoje é:

```text
WinForms / FormsLogic
  -> BpmSerialService.Shared
  -> GsaClient / BpmClient
  -> SdhClient
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SerialTransport
  -> BPM / GatewayApp / GwRouter
  -> GSA
```

Esse fluxo substitui a arquitetura antiga baseada em `SerialLink`, `SerialLinkService`, `SdGgwClient` e `SdgwHealthService`.

## Comportamento atual do link

- o host usa bootstrap textual apenas no início da conexão
- depois do primeiro `Linked`, o host continua podendo entregar tráfego binário SDGW ao engine mesmo se o estado lógico cair temporariamente para `LinkFailed`, desde que a porta siga aberta
- o keepalive do host é baseado em RX SDGW válido
- o supervisor agenda ping apenas sob silêncio
- a arbitragem de envio ocorre no scheduler, e não por disputa direta de slot no engine

## Estado atual do caso GSA

O caso funcional mais exercitado é:

    GSA.led set state=on|off

A cadeia já incorpora:

- prioridade alta para o comando funcional
- timeout mais tolerante no LED da GSA (`400 ms`)
- `2` retries no mapeamento funcional
- correlação de resposta reforçada no `GsaClient`

## Referências principais

- `local-api/src/SimulDIESEL/SimulDIESEL/SDH_HOST_OVERVIEW.md`
- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/05-software-dashboard/03-camada-hardware.md`
- `docs/02-arquitetura/03-fluxo-de-comunicacao.md`
- `docs/12-documentacao-tecnica/03-contratos-software.md`
- `docs/04-firmware/01-arquitetura-firmware.md`
- `docs/04-firmware/boards/01-bpm.md`
