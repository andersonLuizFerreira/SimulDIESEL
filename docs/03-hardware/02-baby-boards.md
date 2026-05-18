⬅ [Retornar para Backplane](01-backplane.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Baby Boards

Esta página inventaria as boards físicas existentes no repositório.

## Inventário físico real

| board | evidência física | evidência de firmware | papel | status |
| --- | --- | --- | --- | --- |
| GSA | `hardware/boards/GSA -gerador-sinais-analogicos` | `hardware/firmware/GSA - Gerador de sinais analógicos` | geração de sinais analógicos | `IMPLEMENTADO` |
| BPM | não aparece aqui como baby board; atua acima da camada | `hardware/firmware/BPM - BACKPLANE MANAGER MODULE` | gateway da bancada | `IMPLEMENTADO` |
| legacy-comunicacao | `hardware/boards/SimulDIESEL/legacy-comunicacao` | nenhuma evidência ativa na pilha atual | acervo histórico | `LEGADO` |
| demais boards nomeadas na documentação | apenas páginas/documentação | não encontrada em `hardware/firmware` | catálogo reservado | `PLANEJADO` |

## Posição das baby boards na bancada

```text
BPM
  -> barramento físico
  -> baby board
  -> circuito local
  -> sinais para a bancada
```

## Observação importante

A GSA é hoje a única baby board com:

- pasta de hardware viva
- firmware vivo
- contrato funcional vivo no host

Por isso ela é a referência principal deste ramo.

## Glossário

- **Baby board**: placa especializada conectada ao backplane.
- **Acervo legado**: hardware mantido no repositório sem papel ativo na arquitetura corrente.
- **Referência física**: board que já possui evidência suficiente para documentar construção e integração.

## Próximas camadas

- [Construção Física das Boards](05-boards-fisicas.md)
