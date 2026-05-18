⬅ [Retornar para Pai Imediato (Índice Geral)](../00-INDICE.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Visão Geral do Projeto

Esta página consolida o estado oficial do SimulDIESEL após o aprofundamento documental desta etapa.

## O que o projeto é hoje

O SimulDIESEL é uma plataforma de bancada formada por:

- software local WinForms
- gateway embarcado BPM
- baby board GSA
- baby board UCE
- estrutura física de bancada ainda em consolidação progressiva

## Estado após esta etapa

- **IMPLEMENTADO**: arquitetura documental consolidada em trilhas `ONDE` e `COMO`.
- **IMPLEMENTADO**: aprofundamento da API/host local com base no código real.
- **IMPLEMENTADO**: aprofundamento de firmware BPM, GSA e UCE com base no código real.
- **IMPLEMENTADO**: aprofundamento do hardware físico no limite confirmado por firmware e esquemáticos vivos.
- **IMPLEMENTADO**: trilha host para comandos `UCE.*`, SDCTP e contratos CAN/J1939 já presente no código.
- **PARCIALMENTE IMPLEMENTADO**: validação ampla de bancada, catálogo físico completo de boards e fechamento total de documentação elétrica.

## Núcleo funcional atual

```text
Host local
  -> SDGW / BPM
  -> GSA
  -> UCE
  -> bancada física parcial
```

Hoje o conjunto mais maduro do projeto é:

- host local
- enlace `SDGW`
- gateway BPM
- GSA
- UCE por `SPI`, com LED, controle CAN e base SDCTP

## O que ainda não deve ser lido como concluído

- catálogo amplo de boards além de GSA e UCE
- backplane totalmente detalhado por netlist
- finalização completa da bancada em todos os aspectos físicos e funcionais

## Direção de continuidade

Com a etapa atual encerrada, a leitura oficial do projeto passa a ser:

1. base documental consolidada
2. BPM, GSA e UCE suficientemente aprofundadas para manutenção técnica
3. validação de bancada, SDCTP/CAN/J1939 e catálogo físico como próximos focos
4. novas boards só devem entrar na documentação oficial depois de existirem como código, firmware, contrato ou evidência técnica

## Glossário

- **Etapa atual**: ciclo de consolidação documental e técnica encerrado nesta rodada.
- **UCE**: Unidade de Comunicação Externa, board já presente na rota `SPI` da BPM e na pilha host.
- **SDCTP**: contrato de transporte/tabelas CAN usado pela trilha UCE.
- **Consolidação documental**: alinhamento entre árvore viva, código real e contratos técnicos.

## Próximas camadas

- [Visão Arquitetural](../02-arquitetura/01-visao-arquitetural.md)
