# Alimentação

## Estado atual

A alimentação do sistema é uma dimensão essencial da bancada, mas o repositório atual documenta esse tema de forma indireta. O que pode ser afirmado com segurança é que a solução envolve pelo menos três classes de consumidores:

- o controlador principal no gateway ESP32;
- os periféricos conectados por `I2C` ou `SPI`;
- os elementos de atuação local, como o LED controlado pelo firmware do GSA.

Os artefatos existentes mostram que a alimentação é parte do desenho físico, mas o código não explicita tensões, correntes máximas, proteção ou sequência formal de energização.

## Funcionamento técnico

Do ponto de vista de sistema, a alimentação precisa suportar o seguinte fluxo operacional:

```text
Fonte da bancada
  -> energiza gateway
  -> gateway inicializa barramentos
  -> periféricos entram em modo responsivo
  -> host estabelece comunicação serial
```

Esse encadeamento é coerente com o comportamento observado no firmware. O host só consegue atingir `Linked` se o gateway estiver energizado e executando o banner de inicialização. O gateway só consegue rotear para um dispositivo se o barramento e o periférico estiverem ativos. No GSA, a ação sobre `LED_PIN` também pressupõe estabilidade da placa filha.

## Limitações

Não há, no estado atual do repositório, especificação textual confiável de:

- tensão nominal do backplane;
- trilhas ou domínios de alimentação separados;
- proteção contra sobrecorrente, polaridade reversa ou transientes;
- orçamento de potência por slot ou por placa.

Por isso, este documento deve ser lido como descrição de dependências funcionais da energização, não como ficha elétrica completa.

## Evolução prevista

Para que a documentação de alimentação fique madura, é necessário consolidar:

- diagrama de distribuição de energia do backplane;
- relação entre fonte externa, reguladores e domínios locais;
- requisitos mínimos para a bancada de desenvolvimento;
- consequências de reset parcial ou queda de energia sobre a sessão de comunicação.

[Retornar ao README principal](../README.md)
