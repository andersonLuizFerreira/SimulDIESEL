⬅ [Retornar para Boards de Firmware](README.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# URL

## Nome canônico

URL

## Identificador SDH da board

URL

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Acionar e monitorar saídas baseadas em relés ou cargas comutada sob comando do gateway.

## Resumo funcional

Board identificada como URL, associada a recursos de relé no padrão SDH.

## Resources lógicos observados/aprovados

- relay1
- relay2
- relay3

## Targets SDH observados/aprovados

- URL.relay1
- URL.relay2
- URL.relay3

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 URL.relay3 set state=on

    sdh/1 URL.relay3 set state=off

    sdh/1 URL.relay3 status

## Observações

- A expansão completa dos recursos da URL ainda deve ser consolidada.
- O target relayN já está alinhado ao padrão multicanal aprovado.

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
