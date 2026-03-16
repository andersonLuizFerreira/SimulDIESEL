# BPM

## Nome canônico

BPM

## Identificador SDH da board

BPM

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Concentrar funções de gateway, configuração de modo de comunicação e leitura de identificação de X-CONN.

## Resumo funcional

Board principal associada às funções de gateway e identificação de X-CONN.

## Resources lógicos observados/aprovados

- gateway
- gateway.serial
- xconn

## Targets SDH observados/aprovados

- BPM.gateway
- BPM.gateway.serial
- BPM.xconn

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 BPM.gateway cfg mode=serial

    sdh/1 BPM.gateway.serial cfg baudrate=115200 databits=8 parity=none stopbits=1

    sdh/1 BPM.xconn read

## Observações

- No material atual do projeto, a BPM aparece associada à função de gateway.
- A leitura de X-CONN é tratada como recurso lógico da própria board.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar a tabela oficial de resources internos.
- Confirmar o binding lógico-físico no firmware do gateway.

[Retornar ao README principal](../../README.md)
