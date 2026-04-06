⬅ [Retornar para Fluxo de Comunicação](../02-arquitetura/03-fluxo-de-comunicacao.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Protocolos e Contratos

## Estado atual

Este ramo reúne os protocolos e contratos que participam da leitura lógica do sistema.

## Classificação de estado

- **IMPLEMENTADO**: cadeia SDH -> SDGW -> TLV curto para a GSA.
- **IMPLEMENTADO**: contrato funcional já exercitado para `BPM.gateway` e para o conjunto atual da GSA.
- **PLANEJADO**: CAN.
- **PLANEJADO**: J1939.

## Papel desta página

Ela separa a topologia física dos barramentos da semântica dos protocolos. Aqui o foco é **como os dados são modelados, encapsulados e interpretados**.

## Glossário

- **Protocolo**: conjunto de regras de comunicação e interpretação.
- **Contrato**: formato estável de um comando, resposta ou evento.
- **SDH**: envelope semântico de comandos do host.
- **SDGW**: enlace binário entre host e gateway.
- **TLV**: contrato compacto interno usado entre gateway e device.

## Próximas camadas

- [Onboarding — Arquitetura de Comandos (SDH)](00-onboarding-comandos.md)
- [CAN](04-can.md)
- [J1939](05-j1939.md)
