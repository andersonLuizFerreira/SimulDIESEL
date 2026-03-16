# CAN

## Estado atual

O protocolo `CAN` aparece no SimulDIESEL como tema arquitetural relevante para o domínio automotivo, mas não como transporte operacional implementado no código analisado. Não foram encontrados drivers, frames, controladores, contratos ativos nem classes de protocolo CAN exercidas pelo software local, pelo gateway ESP32 ou pelo firmware GSA.

O estado atual, portanto, é de preparação conceitual e não de implementação concluída.

## Funcionamento técnico

O que o repositório já oferece e pode sustentar uma futura camada CAN é a infraestrutura de abstração:

- no host, separação entre transporte físico e protocolo lógico;
- no gateway, separação entre link do host, roteamento e barramentos internos;
- em `legacy-docs`, contratos e diretrizes históricas de comunicação.

Em outras palavras, o sistema já possui um padrão para incorporar novos transportes:

```text
Comando lógico
  -> camada de enlace
  -> roteamento
  -> barramento específico
```

Hoje esse padrão está implementado para serial, `I2C` e `SPI`. Para CAN, ainda faltam no repositório:

- definição de frames e identificadores;
- driver ou HAL dedicado;
- política de timeout e retransmissão específica do meio;
- mapeamento entre comandos lógicos do SimulDIESEL e mensagens CAN.

## Limitações

Não é seguro documentar IDs CAN, layout de payload, taxa de bits ou estratégia de arbitragem como se fossem parte do produto atual, porque isso não está implementado nem especificado de forma operacional no repositório. O máximo que pode ser afirmado é que a arquitetura existente comporta esse tipo de expansão.

## Evolução prevista

Quando CAN entrar no código de fato, a documentação oficial deve passar a registrar:

- controlador e interface física utilizada;
- tabela de mensagens aceitas e emitidas;
- relação entre serviços do gateway e frames CAN;
- critérios de teste de bancada e captura de tráfego.

[Retornar ao README principal](../README.md)
