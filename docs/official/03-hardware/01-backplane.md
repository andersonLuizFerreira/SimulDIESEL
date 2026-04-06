⬅ [Retornar para Hardware da Bancada](../02-arquitetura/10-hardware-da-bancada.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Backplane

Esta página documenta o backplane sob a trilha **ONDE**.

## Estado real do backplane

- **PARCIALMENTE IMPLEMENTADO**: existe uma pasta física dedicada em `hardware/boards/SimulDIESEL`.
- **PARCIALMENTE IMPLEMENTADO**: `SimulDIESEL.kicad_pcb` e `SimulDIESEL.kicad_pro` existem como artefatos vivos.
- **PARCIALMENTE IMPLEMENTADO**: `SimulDIESEL.kicad_sch` ainda não materializa um esquemático populado no mesmo nível de detalhe da GSA.
- **IMPLEMENTADO**: a função estrutural do backplane aparece no firmware e na arquitetura física como base de interligação entre BPM, GSA, X-CONN e módulo em teste.

## Onde o backplane fica

```text
BPM
  -> backplane
  -> baby boards
  -> X-CONN
  -> módulo em teste
```

## O que já é possível afirmar com segurança

- a BPM ocupa o papel de gateway superior da bancada;
- a GSA é uma baby board efetivamente integrada à pilha;
- o backplane é a base física prevista para organizar essa ligação;
- a árvore física do projeto já reserva o caminho até X-CONN e chicote.

## O que ainda não é possível afirmar com a mesma segurança

- pinagem completa do backplane
- distribuição detalhada de cada net da placa raiz
- tabela definitiva de slots e conectores por board

Esses pontos permanecem parciais porque o esquemático raiz ainda não está populado como fonte de verdade equivalente à GSA.

## Glossário

- **Backplane**: base física de interligação da bancada.
- **Slot**: posição estrutural reservada para conexão de uma board.
- **Placa raiz**: PCB principal da bancada.

## Próximas camadas

- [Baby Boards](02-baby-boards.md)
- [Barramentos](03-barramentos.md)
- [Alimentação](04-alimentacao.md)
