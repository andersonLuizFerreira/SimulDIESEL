# SDH Examples

## Configurar gateway serial

Comando:

    sdh/1 BPM.gateway.serial cfg baudrate=115200 databits=8 parity=none stopbits=1

Resposta:

    sdh/1 ok BPM.gateway.serial cfg code=OK message="Configuração aplicada"

## Ler X-CONN

Comando:

    sdh/1 BPM.xconn read

Resposta:

    sdh/1 ok BPM.xconn read code=OK data.raw=37 message="X-CONN lida"

## Ligar alimentação principal

Comando:

    sdh/1 PSU.power.main set state=on

Resposta:

    sdh/1 ok PSU.power.main set code=OK data.state=on

## Definir tensão GSA

Comando:

    sdh/1 GSA.ch1 set value=2.50 unit=V

Resposta:

    sdh/1 ok GSA.ch1 set code=OK data.value=2.50 data.unit=V

## Configurar CAN

Comando:

    sdh/1 UCO.can1 cfg bitrate=250000 mode=normal

Resposta:

    sdh/1 ok UCO.can1 cfg code=OK

## Evento assíncrono

    sdh/1 evt BPM.xconn changed value=37

## Referências

- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/02-sdh-response-model.md`

[Retornar ao README principal](../README.md)
