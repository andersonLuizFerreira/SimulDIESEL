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
      "target": "GSA.ch1",
      "op": "set",
      "code": "OK",
      "message": "Canal atualizado",
      "data": {
        "value": 2.50
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

- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/03-sdh-examples.md`

[Retornar ao README principal](../README.md)
