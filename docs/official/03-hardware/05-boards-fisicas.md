⬅ [Retornar para Baby Boards](02-baby-boards.md)

# Construção Física das Boards

Esta seção reúne a documentação relacionada à **engenharia física e eletrônica das baby boards** do SimulDIESEL que já possuem material oficial conectado à árvore viva.

O objetivo desta camada não é esgotar toda a documentação de hardware possível, mas apresentar a forma como uma board física é descrita no acervo oficial atual.

No estado atual da árvore, isso significa:

* visão física da board
* blocos eletrônicos principais
* caminho funcional entre gateway, lógica local e circuito analógico
* pontos de interligação mais relevantes

---

## Escopo desta camada

Nesta área são documentados os aspectos físicos e eletrônicos das boards, incluindo:

* descrição detalhada do funcionamento eletrônico
* esquemas elétricos
* topologia dos circuitos
* placas de circuito impresso (PCI)
* tabelas de interligação
* pinagens
* níveis de tensão
* proteção elétrica
* circuitos analógicos e digitais

---

## Estrutura de engenharia

A construção física de uma board normalmente é dividida nas seguintes áreas:

```text id="7y4m9p"
Alimentação
    ↓
Proteção elétrica
    ↓
Processamento eletrônico
    ↓
Entradas e saídas
    ↓
Interligação física
```

Cada board pode possuir variações conforme sua função.

---

## Estado atual da árvore

Hoje a árvore oficial possui aprofundamento físico conectado para a **GSA**, usada como board de referência neste ramo.

Isso significa que as próximas páginas devem ser lidas como:

* exemplo concreto de board física atualmente documentada
* referência de como este ramo deve evoluir quando outras boards tiverem material equivalente

Não há, neste momento, garantia de que todas as categorias listadas acima já estejam disponíveis para cada board.

---

## Elementos normalmente documentados

Cada board física deverá conter, quando aplicável:

### Alimentação

* tensões de entrada
* tensões reguladas
* separação de domínios
* proteção contra curto

---

### Circuitos de processamento

* DACs
* multiplexadores
* HUBs I2C
* drivers
* buffers
* amplificadores operacionais
* condicionamento de sinais

---

### Interface física

* conectores
* chicotes
* pinagem
* tabelas de interligação
* interface com X-CONN e backplane

---

### Placa de circuito impresso

* layout
* camadas
* roteamento crítico
* dissipação térmica

---

## Próximas camadas

### GSA — Gerador de Sinais Analógicos

Visão física da board mais madura hoje no acervo oficial.

* [Abrir documentação física da GSA](./boards/03-gsa/README.md)
