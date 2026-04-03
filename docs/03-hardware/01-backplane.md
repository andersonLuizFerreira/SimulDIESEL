# Backplane

## Estado atual

O repositório contém artefatos de hardware em `hardware/boards/` que apontam para uma arquitetura física com backplane e placas satélites. A documentação textual dessa parte ainda é menos detalhada que o firmware, mas a estrutura do código embarcado deixa claro o papel do backplane: servir como base de interconexão entre o gateway ESP32, barramentos internos e dispositivos conectados.

Do ponto de vista arquitetural, o backplane sustenta:

- distribuição de conectividade entre gateway e periféricos;
- exposição de barramentos `I2C` e `SPI`;
- organização modular compatível com tabela de dispositivos;
- montagem de bancada com placas substituíveis.

## Funcionamento técnico

Com a pinagem BPM <-> GSA agora consolidada no repositório, o backplane precisa viabilizar explicitamente:

```text
ESP32 Gateway
  -> linhas seriais para o host
  -> I2C fisico em D21/D22 para a GSA
  -> IRQ GSA -> BPM em D4 -> D19
  -> reset dedicado da GSA em D23
  -> barramento SPI para dispositivos selecionados por chip-select
```

`GwDeviceTable` materializa essa visão ao separar dispositivos por barramento e por identidade lógica. Isso indica que o backplane não é apenas suporte mecânico; ele é o ponto de integração física que torna a tabela roteável na prática.

Há também evidência de que a solução foi pensada para expansão incremental: a inclusão de um dispositivo novo tende a exigir entrada na tabela do gateway e suporte físico correspondente no conjunto de placas.

## Limitações

O repositório ainda não apresenta, na documentação oficial ou nos arquivos textuais do hardware, uma descrição consolidada de conectores, tensões, proteção elétrica, topologia mecânica ou revisões de placa do backplane. Por isso, este documento descreve com segurança o papel sistêmico e a pinagem crítica BPM <-> GSA sustentados pelo código, mas não substitui um esquemático elétrico final do backplane.

## Evolução prevista

A evolução natural da documentação de backplane é consolidar, em texto rastreável:

- quais slots ou conectores correspondem a cada classe de dispositivo;
- quais sinais são compartilhados entre placas;
- como a alimentação e o aterramento são distribuídos;
- como a tabela lógica do gateway se relaciona com a montagem física da bancada.

[Retornar ao README principal](../README.md)
