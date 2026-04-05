⬅ [Retornar para Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)

# Catálogo de Baby Boards e Targets SDH

## Objetivo

Este documento centraliza a documentação das baby boards do SimulDIESEL sob o ponto de vista do firmware e do acesso por comandos SDH.

O foco desta seção é registrar:

- identificador lógico da board;
- nome canônico da board;
- responsabilidade principal;
- targets SDH associados;
- exemplos de comandos SDH;
- situação do código legado numérico de acesso no gateway.

## Regra importante

Nesta fase, os identificadores SDH estão formalizados, mas os códigos numéricos legados de roteamento no gateway (como `ADDR` ou nibble alto do campo de comando) ainda devem ser considerados:

    PENDENTE DE DEFINIÇÃO OFICIAL

Portanto, esta documentação formaliza o domínio lógico sem inventar mapeamentos físicos ainda não fechados.

Evidência atual no repositório:

- `BPM` e `GSA` possuem firmware e referências concretas em `hardware/firmware/`.
- `PSU`, `GSC`, `URL`, `SLU`, `UCO`, `UCS`, `UIOD` e `UHM` permanecem como identificadores reservados ou provisórios na documentação, sem implementação confirmada nesta auditoria.

## Boards documentadas

- [README da pasta boards](boards/README.md)
- [BPM](boards/BPM/01-bpm.md)
- [PSU](boards/PSU/02-psu.md)
- [GSA](boards/GSA/03-gsa.md)
- [GSC](boards/04-gsc.md)
- [URL](boards/05-url.md)
- [SLU](boards/06-slu.md)
- [UCO](boards/07-uco.md)
- [UCS](boards/08-ucs.md)
- [UIOD](boards/09-uiod.md)
- [UHM](boards/10-uhm.md)

## Resumo rápido

| Board | Código SDH | Exemplo de target | Código legado numérico |
|-------|------------|-------------------|------------------------|
| BPM   | `BPM`      | `BPM.gateway`     | PENDENTE DE DEFINIÇÃO |
| PSU   | `PSU`      | `PSU.power.main`  | PENDENTE DE DEFINIÇÃO |
| GSA   | `GSA`      | `GSA.led` / `GSA.channel.status` | PENDENTE DE DEFINIÇÃO |
| GSC   | `GSC`      | `GSC.signal1`     | PENDENTE DE DEFINIÇÃO |
| URL   | `URL`      | `URL.relay3`      | PENDENTE DE DEFINIÇÃO |
| SLU   | `SLU`      | PENDENTE DE DEFINIÇÃO | PENDENTE DE DEFINIÇÃO |
| UCO   | `UCO`      | `UCO.can1`        | PENDENTE DE DEFINIÇÃO |
| UCS   | `UCS`      | PENDENTE DE DEFINIÇÃO | PENDENTE DE DEFINIÇÃO |
| UIOD  | `UIOD`     | `UIOD.do5`        | PENDENTE DE DEFINIÇÃO |
| UHM   | `UHM`      | PENDENTE DE DEFINIÇÃO | PENDENTE DE DEFINIÇÃO |

## Operações SDH base

As operações base aprovadas para o modelo SDH são:

    read
    get
    set
    cfg
    run
    status
    reset
    save
    ping

Formas qualificadas podem existir quando necessário:

    read.id
    read.cfg
    set.state
    run.scan
    run.apply

## Referências

- `docs/official/06-protocolos/01-sdh-command-model.md`
- `docs/official/06-protocolos/02-sdh-response-model.md`
- `docs/official/06-protocolos/03-sdh-examples.md`
- `docs/official/06-protocolos/06-gsa-sdh-tlv.md`
- `docs/official/04-firmware/04-sdh-gateway-architecture.md`

## Próximas camadas

- [Boards de Firmware](boards/README.md)


