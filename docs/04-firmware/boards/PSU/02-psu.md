⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# PSU

## Nome canônico

PSU

## Identificador SDH da board

PSU

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Controlar e monitorar recursos de alimentação, energização e estado de potência.

## Resumo funcional

Board associada aos recursos de alimentação da bancada.

## Resources lógicos observados/aprovados

- power.main

## Targets SDH observados/aprovados

- PSU.power.main

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 PSU.power.main set state=on

    sdh/1 PSU.power.main set state=off

    sdh/1 PSU.power.main status

## Observações

- O target observado/aprovado no modelo SDH atual é PSU.power.main.
- Os detalhes elétricos da PSU devem ser documentados separadamente quando consolidados.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar a tabela oficial de resources internos.
- Confirmar o binding lógico-físico no firmware do gateway.

## Glossário

- **Firmware**: software embarcado executado nas boards e no gateway.
- **Gateway**: firmware central responsável por receber, rotear e responder transações.
- **TLV**: formato interno de transação baseado em Type-Length-Value.
- **Board**: unidade funcional conectada à bancada e controlada pelo gateway.
- **SDH**: SimulDiesel Hardware Command, envelope semântico de comandos do projeto.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
