# GSA

## Nome canônico

Gerador de Sinais Analógicos (GSA)

## Identificador SDH da board

GSA

## Código legado numérico no gateway

    PENDENTE DE DEFINIÇÃO OFICIAL

## Responsabilidade principal no firmware

Gerar sinais e estados controláveis via gateway, sendo o primeiro caso funcional oficial do SDH no host.

## Resumo funcional

Board atualmente mais concreta do projeto, já utilizada no caminho host -> gateway -> device.

## Resources lógicos observados/aprovados

- ch1
- led

## Targets SDH observados/aprovados

- GSA.ch1
- GSA.led

## Operações SDH aplicáveis

    read
    set
    cfg
    run
    status
    reset

## Exemplos de acesso via comando SDH

    sdh/1 GSA.ch1 set value=2.50 unit=V

    sdh/1 GSA.led set state=on

    sdh/1 GSA.led set state=off

## Observações

- O GSA já possui integração funcional no projeto atual.
- O caso GSA.led set state=on|off foi adotado como primeiro caso oficial de SDH no host.

## Pendências desta documentação

- Confirmar o código legado numérico da board no gateway.
- Confirmar a tabela oficial de resources internos.
- Confirmar o binding lógico-físico no firmware do gateway.

[Retornar ao README principal](../../README.md)
