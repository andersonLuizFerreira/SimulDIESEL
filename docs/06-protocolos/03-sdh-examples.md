# SDH Examples

## Observação importante

Esta página separa:

- exemplos compatíveis com o modelo documental geral do SDH;
- exemplos efetivamente suportados hoje no host.

O objetivo é evitar que exemplos arquiteturais antigos sejam lidos como se já fossem comandos implementados no código atual.

## Exemplos efetivamente válidos hoje no host

### Ping do gateway BPM

Comando:

    sdh/1 BPM.gateway ping

### LED builtin da GSA

Comandos:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

### Setpoint por canal da GSA

Comando:

    sdh/1 GSA.channel.setpoint set channel=6 value=128

### Enable por canal da GSA

Comandos:

    sdh/1 GSA.channel.enable set channel=6 state=on
    sdh/1 GSA.channel.enable set channel=6 state=off

### Enable global da GSA

Comandos:

    sdh/1 GSA.channels.enable set state=on
    sdh/1 GSA.channels.enable set state=off

### Status por canal

Comando:

    sdh/1 GSA.channel.status get channel=6

### Status global

Comando:

    sdh/1 GSA.channels.status get

### Fault reset por canal

Comando:

    sdh/1 GSA.channel.fault reset channel=6

### Offsets por canal

Comandos:

    sdh/1 GSA.channel.offset set channel=6 kind=vout value=-500
    sdh/1 GSA.channel.offset get channel=6 kind=vout
    sdh/1 GSA.channel.offset save channel=6
    sdh/1 GSA.channel.offset reset channel=6

### Reset global de offsets

Comando:

    sdh/1 GSA.offset reset

## Exemplos arquiteturais do modelo geral

Os exemplos abaixo continuam úteis para ilustrar o envelope SDH, mas não representam suporte confirmado no host atual:

    sdh/1 BPM.gateway.serial cfg baudrate=115200 databits=8 parity=none stopbits=1
    sdh/1 BPM.xconn read
    sdh/1 PSU.power.main set state=on
    sdh/1 UCO.can1 cfg bitrate=250000 mode=normal

## Observação específica da GSA

O modelo antigo baseado em:

    sdh/1 GSA.ch1 set value=2.50 unit=V

não é mais a forma oficial vigente da GSA no host atual.

O contrato oficial passou a ser orientado por:

- `GSA.channel.setpoint`
- `GSA.channel.enable`
- `GSA.channel.status`
- `GSA.channels.status`
- `GSA.channel.offset`

## Referências

- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/02-sdh-response-model.md`
- `docs/06-protocolos/06-gsa-sdh-tlv.md`

[Retornar ao README principal](../README.md)
