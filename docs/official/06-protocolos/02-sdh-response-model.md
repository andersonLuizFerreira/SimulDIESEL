⬅ [Retornar para SDH Command Model](01-sdh-command-model.md)

# SDH Response Model

## Envelope de resposta

Toda resposta segue:

    version
    ok
    target
    op
    code
    message
    data
    meta

## Forma textual

Sucesso:

    sdh/1 ok <target> <op> code=OK message="..."

Erro:

    sdh/1 err <target> <op> code=INVALID_ARG message="..."

## Forma JSON

    {
      "version": "sdh/1",
      "ok": true,
      "target": "GSA.channel.status",
      "op": "get",
      "code": "OK",
      "message": "Status do canal lido",
      "data": {
        "channel": 6,
        "setpoint": 128,
        "vout": 134,
        "iread": 17,
        "enabled": true,
        "fault": false
      },
      "meta": {}
    }

## Códigos de erro

    OK
    INVALID_TARGET
    INVALID_OP
    INVALID_ARG
    MISSING_ARG
    OUT_OF_RANGE
    UNSUPPORTED
    BUSY
    FAULT
    TIMEOUT

## Tipos

- confirmação;
- leitura;
- status;
- erro.

## Referências

- `docs/official/06-protocolos/01-sdh-command-model.md`
- `docs/official/06-protocolos/03-sdh-examples.md`
- `docs/official/06-protocolos/06-gsa-sdh-tlv.md`

## Próximas camadas

- [SDH Examples](03-sdh-examples.md)

