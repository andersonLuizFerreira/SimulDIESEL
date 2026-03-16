# GSC

## Nome canônico

GSC

## Identificador SDH da board

GSC

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Configurar e operar recursos de geração de sinal conforme perfis definidos no firmware.

## Resumo funcional

Board identificada como GSC, associada a recursos de geração/configuração de sinal.

## Resources lógicos observados/aprovados

- signal1

## Targets SDH observados/aprovados

- GSC.signal1

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 GSC.signal1 cfg mode=pulse freq=1000 duty=50

    sdh/1 GSC.signal1 status

## Observações

- A descrição funcional completa da GSC ainda deve ser consolidada oficialmente.
- O target GSC.signal1 já foi utilizado como referência no desenho do padrão SDH.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar a tabela oficial de resources internos.
- Confirmar o binding lógico-físico no firmware do gateway.

[Retornar ao README principal](../../README.md)
