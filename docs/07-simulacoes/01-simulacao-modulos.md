⬅ [Retornar para Arquitetura do Software Dashboard (Local API)](../05-software-dashboard/01-arquitetura-software.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Simulação de Módulos

## Estado atual

No estado atual do repositório, a simulação de módulos deve ser entendida como um **caso de uso lógico do software local** sustentado por uma arquitetura modular no gateway.

O componente mais concreto dessa estratégia é a capacidade do gateway de tratar boards e recursos como destinos lógicos independentes, permitindo ao host acionar um módulo sem conhecer diretamente o barramento físico.

Essa é a base da simulação modular: cada módulo pode ser representado como um endpoint roteável com contrato próprio e apresentado ao operador como uma ação de bancada.

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

## Papel das próximas camadas

As páginas inferiores deste ramo não introduzem novos módulos independentes. Elas separam a simulação atual em duas leituras complementares:

* **sensores**: quando o módulo simulado representa sinais ou estados de entrada
* **atuadores**: quando o módulo simulado representa a aplicação de uma ação física visível

Essa divisão existe para organizar a experiência de bancada já sustentada pelo software atual.

## Limitações

O repositório ainda não contém um catálogo amplo de módulos simulados com comportamentos de domínio ricos. A infraestrutura de simulação modular está à frente da quantidade de serviços embarcados disponíveis. Por isso, a documentação oficial deve descrever a plataforma de simulação e não prometer um conjunto maior de módulos do que o código entrega hoje.

## Evolução prevista

O próximo ganho arquitetural é adicionar módulos novos sem quebrar o fluxo existente. Para isso, o padrão atual já oferece a base necessária:

- nova entrada em `GwDeviceTable`;
- driver de barramento compatível;
- firmware de dispositivo com contrato bem definido;
- tela ou caso de uso correspondente no software local.

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.
- **TLV**: Type-Length-Value, formato interno de payload usado em transações específicas.

## Próximas camadas

- [Simulação de Sensores](02-simulacao-sensores.md)
- [Simulação de Atuadores](03-simulacao-atuadores.md)
