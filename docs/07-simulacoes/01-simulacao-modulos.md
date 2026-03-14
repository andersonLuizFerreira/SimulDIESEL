# Simulação de Módulos

## Estado atual

No estado atual do repositório, a simulação de módulos está apoiada principalmente na capacidade do gateway de tratar dispositivos como destinos lógicos independentes. O componente mais concreto dessa estratégia é o endereçamento por `CMD`, aliado à `GwDeviceTable`, que permite ao host acionar módulos sem conhecer diretamente o barramento físico.

Essa é a base da simulação modular: cada módulo pode ser representado como um endpoint roteável com contrato próprio.

## Funcionamento técnico

### Modelo de endereçamento

O host emite um comando com endereço lógico:

```text
CMD = [ADDR:4][OP:4]
```

O gateway interpreta o endereço:

- `ADDR = gateway`: processa localmente;
- `ADDR = periférico`: consulta a tabela de dispositivos e encaminha a transação.

### Papel do gateway na simulação

```text
Host
  -> comando lógico
Gateway
  -> resolve módulo
  -> escolhe barramento
  -> traduz para contrato interno
Módulo
  -> executa serviço
  -> responde ao gateway
```

Esse padrão significa que a simulação de módulo é, antes de tudo, uma questão de contrato de dispositivo. O gateway não precisa saber a semântica profunda do módulo; ele precisa saber como encontrá-lo e transportar a requisição.

### Exemplo concreto

O `GSA` já ocupa esse papel de módulo remoto. Embora seu serviço atual seja simples, ele prova o modelo de integração:

- endereço lógico definido no gateway;
- barramento `I2C` mapeado;
- contrato TLV curto no periférico;
- resposta correlacionada ao `SEQ` do host.

## Limitações

O repositório ainda não contém um catálogo amplo de módulos simulados com comportamentos de domínio ricos. A infraestrutura de simulação modular está à frente da quantidade de serviços embarcados disponíveis. Por isso, a documentação oficial deve descrever a plataforma de simulação e não prometer um conjunto maior de módulos do que o código entrega hoje.

## Evolução prevista

O próximo ganho arquitetural é adicionar módulos novos sem quebrar o fluxo existente. Para isso, o padrão atual já oferece a base necessária:

- nova entrada em `GwDeviceTable`;
- driver de barramento compatível;
- firmware de dispositivo com contrato bem definido;
- tela ou caso de uso correspondente no software local.

[Retornar ao README principal](../README.md)
