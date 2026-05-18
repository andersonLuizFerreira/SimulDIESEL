⬅ [Retornar para Visão Arquitetural](01-visao-arquitetural.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Visão Lógica do Projeto

Esta página descreve **como** o SimulDIESEL funciona, desde a geração da intenção do operador até a resposta produzida pela bancada sobre o módulo em teste.

A leitura lógica organiza responsabilidades, protocolos, fluxos, supervisão, sessão, roteamento e contratos entre as camadas do sistema, sem detalhar a localização física dos elementos.

## Fluxo lógico central

```text
Operador
→ UI
→ FormsLogic / clients
→ SDH / SDGW
→ gateway BPM
→ board especializada
→ resposta funcional ou efeito físico final
```

## Perguntas que este ramo responde

A Visão Lógica existe para explicar, por exemplo:

* como a UI gera um comando operacional
* como o protocolo encapsula a intenção funcional
* como o host supervisiona sessão, timeout e retry
* como o gateway interpreta e roteia a solicitação
* como a board executa a operação recebida
* como a resposta retorna ao software host

Um exemplo obrigatório deste ramo é entender **como um comando de tensão enviado pela UI se transforma em um nível elétrico na porta do módulo em teste**.

## Encadeamento entre camadas

Nesta leitura, cada camada adiciona significado ao fluxo:

* a UI expressa a intenção do operador
* a aplicação organiza a ação como caso de uso
* a pilha SDH/SDGW garante entrega, supervisão e correlação
* o gateway escolhe o destino correto
* a board executa a função embarcada
* o sistema devolve status, evento, medição ou confirmação

A localização física deste fluxo é descrita na Visão Física, sem navegação cruzada direta entre os ramos.

## Glossário

- **Camada**: nível de responsabilidade dentro da arquitetura do sistema.
- **Gateway**: ponto de passagem entre host, roteamento interno e hardware.
- **Visão lógica**: leitura focada em como os comandos e respostas fluem pelo sistema.
- **Fluxo**: sequência funcional entre intenção, transporte, execução e resposta.

## Próximas camadas

* [Camadas do Sistema](02-camadas-do-sistema.md)
* [Arquitetura de Firmware](../04-firmware/01-arquitetura-firmware.md)
* [Arquitetura do Software Dashboard (Local API)](../05-software-dashboard/01-arquitetura-software.md)
