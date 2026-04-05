⬅ [Retornar para Backplane](01-backplane.md)

# Baby Boards

As **Baby Boards** são módulos especializados conectados ao backplane, responsáveis por executar funções específicas da bancada.

Cada board encapsula uma responsabilidade funcional e elétrica própria, permitindo que o sistema seja expandido de forma modular.

Essa arquitetura facilita:

* expansão de recursos
* manutenção
* substituição individual
* isolamento de falhas
* escalabilidade do sistema

---

## Papel das baby boards

As baby boards recebem sinais, comandos e alimentação por meio do backplane.

Sua função é executar operações específicas como:

* geração de sinais
* leitura de entradas
* monitoramento
* atuação física
* interfaces de comunicação
* alimentação controlada

Cada board é dedicada a uma função bem definida.

---

## Fluxo funcional

O fluxo de operação entre software, gateway e baby board pode ser resumido em:

```text id="g49p31"
Aplicação
    ↓
Gateway
    ↓
Backplane
    ↓
Baby Board
    ↓
Módulo em teste
```

A resposta percorre o caminho inverso.

---

## Arquitetura modular

Cada baby board possui sua própria camada lógica e eletrônica.

De forma geral, elas são compostas por:

### Camada lógica

Responsável por:

* interpretação dos comandos recebidos
* validação
* shadow RAM
* controle funcional

---

### Camada eletrônica

Responsável por:

* atuação física
* barramentos internos
* DACs
* multiplexadores
* drivers
* monitoramento elétrico

---

## Comunicação assíncrona

As baby boards podem sinalizar eventos ao gateway de forma assíncrona.

Isso permite:

* retorno de estados
* eventos de falha
* confirmação de atuação
* sinalização de término de processo

---

## Exemplo de board implementada

**A GSA (Gerador de Sinais Analógicos)** é atualmente a referência funcional mais madura dentro do projeto.

Ela representa o modelo oficial de integração entre gateway, backplane e baby board.

## Papel deste ramo na árvore

Nesta trilha da documentação, o aprofundamento segue pelo **aspecto físico e eletrônico** das boards atualmente documentadas.

Os detalhes de firmware, contratos SDH e roteamento lógico das mesmas boards continuam no ramo de firmware, preservando a separação entre:

* construção física da placa
* comportamento embarcado
* contrato de software


---

## Próximas camadas

### Construção física das boards

Visão física das boards já documentadas no acervo oficial, com foco em:

* função eletrônica da placa

* blocos internos principais

* interligação com backplane e gateway

* [Construção Física das Boards](05-boards-fisicas.md)
