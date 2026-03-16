# SDH Command Model

## Visão Geral

O protocolo SDH define um envelope semântico único para comandos da camada Hardware do SimulDIESEL.

Todo comando é composto por:

    version
    target
    op
    args
    meta

Esse modelo permite:

- roteamento simples no firmware;
- API estável no software local;
- integração futura com API web;
- operação manual via terminal/UHM.

## Forma textual canônica

Sintaxe:

    sdh/1 <target> <op> chave=valor ...

Exemplos:

    sdh/1 BPM.gateway cfg mode=serial
    sdh/1 PSU.power.main set state=on
    sdh/1 GSA.ch1 set value=2.50 unit=V
    sdh/1 BPM.xconn read

## Forma JSON equivalente

    {
      "version": "sdh/1",
      "target": "BPM.gateway.serial",
      "op": "cfg",
      "args": {
        "baudrate": 115200
      },
      "meta": {}
    }

## Estrutura de target

Formato:

    <BOARD>.<resource>.<subresource>

Exemplos:

    BPM.gateway
    BPM.gateway.serial
    PSU.power.main
    GSA.ch1
    URL.relay3
    UCO.can1
    UIOD.do5

## Verbos padronizados

    read
    set
    cfg
    run
    status
    reset

Formas qualificadas:

    read.id
    read.cfg
    set.state
    run.scan
    run.apply

## Regras de projeto

- evitar argumentos posicionais;
- usar nomes explícitos;
- manter target lógico (não físico);
- separar set (estado) de cfg (estrutura).

## Evolução

Versões futuras:

    sdh/2
    sdh/3

Devem manter compatibilidade controlada.

## Referências

- `docs/06-protocolos/02-sdh-response-model.md`
- `docs/06-protocolos/03-sdh-examples.md`

[Retornar ao README principal](../README.md)
