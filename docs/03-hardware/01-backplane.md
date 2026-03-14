# Backplane

## Estado atual

O repositório contém artefatos de hardware em `hardware/boards/` que apontam para uma arquitetura física com backplane e placas satélites. A documentação textual dessa parte ainda é menos detalhada que o firmware, mas a estrutura do código embarcado deixa claro o papel do backplane: servir como base de interconexão entre o gateway ESP32, barramentos internos e dispositivos conectados.

Do ponto de vista arquitetural, o backplane sustenta:

- distribuição de conectividade entre gateway e periféricos;
- exposição de barramentos `I2C` e `SPI`;
- organização modular compatível com tabela de dispositivos;
- montagem de bancada com placas substituíveis.

## Funcionamento técnico

Mesmo sem uma pinagem textual consolidada, o comportamento do firmware mostra o que o backplane precisa viabilizar:

```text
ESP32 Gateway
  -> linhas seriais para o host
  -> barramento I2C para periféricos endereçados
  -> barramento SPI para dispositivos selecionados por chip-select
```

`GwDeviceTable` materializa essa visão ao separar dispositivos por barramento e por identidade lógica. Isso indica que o backplane não é apenas suporte mecânico; ele é o ponto de integração física que torna a tabela roteável na prática.

Há também evidência de que a solução foi pensada para expansão incremental: a inclusão de um dispositivo novo tende a exigir entrada na tabela do gateway e suporte físico correspondente no conjunto de placas.

## Limitações

O repositório atual não apresenta, na documentação oficial ou nos arquivos textuais do hardware, uma descrição consolidada de conectores, tensões, proteção elétrica, topologia mecânica ou revisões de placa do backplane. Por isso, este documento descreve com segurança apenas o papel sistêmico sustentado pelo código e pelos artefatos de placa existentes.

## Evolução prevista

A evolução natural da documentação de backplane é consolidar, em texto rastreável:

- quais slots ou conectores correspondem a cada classe de dispositivo;
- quais sinais são compartilhados entre placas;
- como a alimentação e o aterramento são distribuídos;
- como a tabela lógica do gateway se relaciona com a montagem física da bancada.

[Retornar ao README principal](../README.md)
