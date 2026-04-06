⬅ [Retornar para Pai Imediato (Índice Geral)](../../00-INDICE.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Visão Geral do Projeto

Esta página consolida o estado oficial do SimulDIESEL após o aprofundamento documental desta etapa.

## O que o projeto é hoje

O SimulDIESEL é uma plataforma de bancada formada por:

- software local WinForms
- gateway embarcado BPM
- baby board GSA
- estrutura física de bancada ainda em consolidação progressiva

## Estado após esta etapa

- **IMPLEMENTADO**: arquitetura documental consolidada em trilhas `ONDE` e `COMO`.
- **IMPLEMENTADO**: aprofundamento da API/host local com base no código real.
- **IMPLEMENTADO**: aprofundamento de firmware BPM e GSA com base no código real.
- **IMPLEMENTADO**: aprofundamento do hardware físico no limite confirmado por firmware e esquemáticos vivos.
- **PARCIALMENTE IMPLEMENTADO**: finalização da GSA ainda pendente, principalmente no amadurecimento total da etapa física e da documentação de bancada mais ampla.
- **PRONTO PARA PRÓXIMA ETAPA**: início da board `UCE (Unidade de Comunicação Externa)`.

## Núcleo funcional atual

```text
Host local
  -> SDGW / BPM
  -> GSA
  -> bancada física parcial
```

Hoje o conjunto mais maduro do projeto é:

- host local
- enlace `SDGW`
- gateway BPM
- GSA

## O que ainda não deve ser lido como concluído

- catálogo amplo de boards além da GSA
- backplane totalmente detalhado por netlist
- finalização completa da GSA em todos os aspectos físicos e funcionais

## Direção de continuidade

Com a etapa atual encerrada, a leitura oficial do projeto passa a ser:

1. base documental consolidada
2. BPM e GSA suficientemente aprofundadas para manutenção técnica
3. GSA ainda com fechamento final pendente
4. projeto apto para iniciar a UCE

## Glossário

- **Etapa atual**: ciclo de consolidação documental e técnica encerrado nesta rodada.
- **UCE**: Unidade de Comunicação Externa, próxima board explicitamente preparada como evolução.
- **Consolidação documental**: alinhamento entre árvore viva, código real e contratos técnicos.

## Próximas camadas

- [Visão Arquitetural](../02-arquitetura/01-visao-arquitetural.md)
