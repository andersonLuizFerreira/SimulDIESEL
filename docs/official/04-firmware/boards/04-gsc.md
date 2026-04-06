⬅ [Retornar para Boards de Firmware](README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

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

## Glossário

- **Firmware**: software embarcado executado nas boards e no gateway.
- **Gateway**: firmware central responsável por receber, rotear e responder transações.
- **TLV**: formato interno de transação baseado em Type-Length-Value.
- **Board**: unidade funcional conectada à bancada e controlada pelo gateway.
- **SDH**: SimulDiesel Hardware Command, envelope semântico de comandos do projeto.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
