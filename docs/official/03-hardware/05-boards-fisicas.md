⬅ [Retornar para Baby Boards](02-baby-boards.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Construção Física das Boards

Esta camada aprofunda a construção física das boards que possuem evidência suficiente no repositório.

## Estado real

- **IMPLEMENTADO**: a GSA possui esquemático, PCB e firmware coerentes entre si.
- **PARCIALMENTE IMPLEMENTADO**: a placa raiz `SimulDIESEL` existe como hardware, mas ainda não sustenta o mesmo nível de documentação física por netlist.
- **LEGADO**: `legacy-comunicacao` preserva uma solução anterior fora da arquitetura corrente.
- **PLANEJADO**: demais boards físicas ainda não atingiram o mesmo nível de detalhamento.

## Caminho deste ramo

```text
Board física
  -> MCU / lógica local
  -> barramentos locais
  -> periféricos elétricos
  -> conectores / saída
```

## Referência viva

A GSA é a board física mais madura desta árvore porque já mostra:

- microcontrolador identificado no esquemático
- barramento físico com a BPM
- barramento lógico interno
- periféricos `TCA9548A` e `MCP4725`
- firmware correspondente

## Glossário

- **Construção física**: leitura focada em componentes, conectores e interligações materiais.
- **Referência viva**: board que já possui evidência suficiente para aprofundamento.
- **Netlist**: material que permite afirmar conexões elétricas concretas.

## Próximas camadas

- [Abrir documentação física da GSA](./boards/03-gsa/README.md)
