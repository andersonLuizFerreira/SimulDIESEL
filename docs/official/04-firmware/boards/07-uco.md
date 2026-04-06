⬅ [Retornar para Boards de Firmware](README.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# UCO

## Nome canônico

UCO

## Identificador SDH da board

UCO

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Configurar e operar interfaces CAN e seus canais lógicos no contexto do gateway.

## Resumo funcional

Board identificada como UCO, associada a interface(s) CAN no padrão SDH.

## Resources lógicos observados/aprovados

- can1

## Targets SDH observados/aprovados

- UCO.can1

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 UCO.can1 cfg bitrate=250000 mode=normal

    sdh/1 UCO.can1 status

    sdh/1 UCO.can1 reset

## Observações

- O target UCO.can1 já foi usado como referência no modelo SDH.
- O binding físico para hardware CAN ainda deve ser fechado no gateway.

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
