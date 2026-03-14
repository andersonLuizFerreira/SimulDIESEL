# Escopo

## Estado atual

O escopo efetivo do repositório inclui firmware, software local, artefatos de hardware e documentação técnica. O que está dentro do escopo operacional atual é o fluxo de bancada local, sem dependência obrigatória de serviços externos.

Estão dentro do escopo comprovado pelo código:

- comunicação host <-> gateway por serial;
- protocolo binário com enquadramento `COBS + CRC8`;
- roteamento interno do gateway para I2C e SPI;
- tabela de dispositivos no firmware do ESP32;
- periférico I2C `GSA` com serviço real de LED;
- interface local WinForms para abertura de porta, health check e teste de LED.

## Funcionamento técnico

O escopo do projeto pode ser lido como uma cadeia de responsabilidades:

```
Operação de bancada
  -> software local
  -> link serial confiável
  -> gateway embarcado
  -> barramentos internos
  -> periféricos endereçados
```

O software local cobre a sessão com o gateway. O gateway cobre encapsulamento, roteamento e resposta. Cada periférico cobre sua lógica de serviço específica. A documentação oficial, por sua vez, cobre o comportamento observado nessas camadas e usa `legacy-docs` apenas como apoio histórico.

## Limitações

Os itens abaixo não aparecem como escopo implementado no estado atual do repositório:

- backend de nuvem com contratos ativos;
- malha completa de módulos diesel e tabelas de sensores/atuadores em produção;
- pilhas CAN e J1939 operacionais no código analisado;
- especificação textual consolidada de todas as placas físicas em `hardware/boards`.

Há sinais de intenção arquitetural para esses pontos, mas a base objetiva disponível hoje está concentrada no enlace local, no gateway e no primeiro periférico.

## Evolução prevista

O escopo tende a crescer em duas direções compatíveis com o material legado e com o código atual:

- horizontalmente, com novos periféricos ou serviços adicionados à tabela do gateway;
- verticalmente, com contratos mais ricos, testes de integração e eventual expansão para protocolos automotivos e integração remota.

[Retornar ao README principal](../README.md)
