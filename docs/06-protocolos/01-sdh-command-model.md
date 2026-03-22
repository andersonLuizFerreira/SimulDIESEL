# SDH Command Model

## Visão Geral

O protocolo SDH define um envelope semântico único para comandos da camada Hardware do SimulDIESEL.

É importante diferenciar:

- o padrão documental geral;
- o subconjunto efetivamente implementado hoje no host;
- as expansões específicas já incorporadas para a GSA.

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
    get
    set
    cfg
    run
    status
    reset
    save
    ping

Formas qualificadas:

    read.id
    read.cfg
    set.state
    run.scan
    run.apply

Observação:

- `read`, `status` e `cfg` continuam existindo no modelo documental geral;
- o host atual da GSA já usa explicitamente `get`, `set`, `reset` e `save`;
- o target local da BPM mantém `ping` como operação suportada.

## Regras de projeto

- evitar argumentos posicionais;
- usar nomes explícitos;
- manter target lógico (não físico);
- separar set (estado) de cfg (estrutura).

## Estado real de implementação no host

O catálogo acima descreve o padrão documental geral do SDH.

Ele não significa que todos os targets e verbos exemplificados nesta página já estejam implementados no host atual.

### Subconjunto efetivamente suportado hoje

No estado atual do host C#, os comandos já suportados são:

    sdh/1 BPM.gateway ping

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

    sdh/1 GSA.channel.setpoint set channel=<1..16> value=<0..255>
    sdh/1 GSA.channel.enable set channel=<1..16> state=on|off
    sdh/1 GSA.channels.enable set state=on|off
    sdh/1 GSA.channel.status get channel=<1..16>
    sdh/1 GSA.channels.status get
    sdh/1 GSA.channel.fault reset channel=<1..16>
    sdh/1 GSA.channel.offset set channel=<1..16> kind=vout|vread|iread value=<int16>
    sdh/1 GSA.channel.offset get channel=<1..16> kind=vout|vread|iread
    sdh/1 GSA.channel.offset save channel=<1..16>
    sdh/1 GSA.channel.offset reset channel=<1..16>
    sdh/1 GSA.offset reset

### Exemplos arquiteturais não equivalem a suporte real

Alvos como:

    PSU.power.main
    UCO.can1
    GSA.ch1

continuam úteis como exemplos do modelo documental geral, mas não devem ser tratados como suporte implementado no host atual sem documento específico confirmando isso.

## Evolução

Versões futuras:

    sdh/2
    sdh/3

Devem manter compatibilidade controlada.

## Referências

- `docs/06-protocolos/02-sdh-response-model.md`
- `docs/06-protocolos/03-sdh-examples.md`
- `docs/06-protocolos/06-gsa-sdh-tlv.md`

[Retornar ao README principal](../README.md)
