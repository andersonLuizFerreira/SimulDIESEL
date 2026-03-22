# GSA

## Nome canônico

Gerador de Sinais Analógicos (GSA)

## Identificador SDH da board

GSA

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Gerar sinais analógicos por canal, responder status elétrico real, aplicar offsets de calibração e expor fault latched via gateway.

## Resumo funcional

Board atualmente mais concreta do projeto no caminho host -> gateway -> device, com contrato oficial de 16 canais e telemetria/status por TLV.

## Modelo funcional documentado

- `16` canais no total
- canais `1..8` na faixa `0..5 V`
- canais `9..16` na faixa `0..12 V`
- setpoint lógico transportado como `0..255`
- conversão para tensão real feita pela própria board
- status por canal com:
  - `setpoint`
  - `vout`
  - `iread`
  - `enabled`
  - `fault`
- offsets por canal:
  - `vout`
  - `vread`
  - `iread`

## Targets SDH vigentes da GSA

- GSA.led
- GSA.channel.setpoint
- GSA.channel.enable
- GSA.channels.enable
- GSA.channel.status
- GSA.channels.status
- GSA.channel.fault
- GSA.channel.offset
- GSA.offset

## Operações SDH efetivamente suportadas hoje no host

    set
    get
    reset
    save

## Comandos SDH vigentes

### LED builtin

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

### Setpoint por canal

    sdh/1 GSA.channel.setpoint set channel=6 value=128

### Enable por canal

    sdh/1 GSA.channel.enable set channel=6 state=on
    sdh/1 GSA.channel.enable set channel=6 state=off

### Enable global

    sdh/1 GSA.channels.enable set state=on
    sdh/1 GSA.channels.enable set state=off

### Status

    sdh/1 GSA.channel.status get channel=6
    sdh/1 GSA.channels.status get

### Fault reset por canal

    sdh/1 GSA.channel.fault reset channel=6

### Offsets

    sdh/1 GSA.channel.offset set channel=6 kind=vout value=-500
    sdh/1 GSA.channel.offset get channel=6 kind=vout
    sdh/1 GSA.channel.offset save channel=6
    sdh/1 GSA.channel.offset reset channel=6

### Reset global de offsets

    sdh/1 GSA.offset reset

## Exemplos de acesso via comando SDH

    sdh/1 GSA.led set state=on

    sdh/1 GSA.led set state=off

    sdh/1 GSA.channel.status get channel=1

    sdh/1 GSA.channels.status get

## Observações

- O GSA já possui integração funcional no projeto atual.
- O caso `GSA.led set state=on|off` foi o primeiro caso oficial de SDH no host e permanece compatível.
- A expansão atual do host adiciona o catálogo completo de setpoint, enable, status, fault reset e offsets.
- O evento assíncrono documentado para a board é exclusivamente o de `fault`.
- O contrato TLV detalhado está em `docs/06-protocolos/06-gsa-sdh-tlv.md`.
- Há uma inconsistência histórica envolvendo o type `0x12`, documentada explicitamente no contrato oficial da GSA.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar o binding lógico-físico no firmware do gateway.
- Consolidar futuramente uma decisão oficial para o conflito de type `0x12`.

[Retornar ao README principal](../../README.md)
