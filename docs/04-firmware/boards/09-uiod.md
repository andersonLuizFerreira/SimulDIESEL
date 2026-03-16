# UIOD

## Nome canônico

UIOD

## Identificador SDH da board

UIOD

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Controlar e monitorar entradas e saídas digitais sob padronização SDH.

## Resumo funcional

Board identificada como UIOD, associada a entradas e saídas digitais.

## Resources lógicos observados/aprovados

- do1
- do5
- di1

## Targets SDH observados/aprovados

- UIOD.do1
- UIOD.do5
- UIOD.di1

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 UIOD.do5 set state=high

    sdh/1 UIOD.do5 set state=low

    sdh/1 UIOD.di1 read

## Observações

- O target UIOD.do5 já foi usado como referência no modelo SDH.
- O padrão di/do segue a convenção multicanal aprovada.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar a tabela oficial de resources internos.
- Confirmar o binding lógico-físico no firmware do gateway.

[Retornar ao README principal](../../README.md)
