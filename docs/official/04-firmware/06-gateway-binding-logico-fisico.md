⬅ [Retornar para Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)

# Tabela Mestra de Binding Lógico-Físico do Gateway

## Objetivo

Este documento define a estrutura oficial de documentação do binding lógico-físico no gateway do SimulDIESEL.

O binding lógico-físico é a etapa responsável por converter o domínio lógico do SDH em uma rota física concreta dentro da bancada.

## Tabela mestra

| Board | Nome da Board (Extenso) | TargetBase | Resource | Subresource | GatewayLegacyCode | BusType | PhysicalAddress | Mapper | FirmwareHandler | Status |
|------|-------------------------|------------|----------|-------------|-------------------|---------|-----------------|--------|-----------------|--------|
| BPM  | BACKPLANE MANAGER MODULE | BPM.gateway | gateway | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | INTERNAL | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| BPM  | BACKPLANE MANAGER MODULE | BPM.gateway.serial | gateway | serial | PENDENTE DE DEFINIÇÃO OFICIAL | INTERNAL | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| BPM  | BACKPLANE MANAGER MODULE | BPM.xconn | xconn | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | INTERNAL | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| PSU  | POWER SUPPLY UNIT | PSU.power.main | power | main | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.led | led | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | GsaLedMapper | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channel.setpoint | channel | setpoint | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channel.enable | channel | enable | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channels.enable | channels | enable | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channel.status | channel | status | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channels.status | channels | status | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channel.fault | channel | fault | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.channel.offset | channel | offset | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |
| GSA  | GERADOR DE SINAIS ANALÓGICOS | GSA.offset | offset | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | I2C | PENDENTE | PENDENTE | PENDENTE | PARCIAL |

| GSC  | GERADOR DE SINAIS DE CONTROLE | GSC.signal1 | signal1 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| URL  | UNIDADE DE RELÉS LÓGICOS | URL.relay1 | relay1 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| URL  | UNIDADE DE RELÉS LÓGICOS | URL.relay2 | relay2 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| URL  | UNIDADE DE RELÉS LÓGICOS | URL.relay3 | relay3 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| SLU  | SENSOR LOAD UNIT | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE |

| UCO  | UNIDADE DE COMUNICAÇÃO CAN | UCO.can1 | can1 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | CAN | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| UCS  | UNIDADE DE CONTROLE DE SENSORES | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE |

| UIOD | UNIDADE DE I/O DIGITAL | UIOD.do1 | do1 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| UIOD | UNIDADE DE I/O DIGITAL | UIOD.do5 | do5 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |
| UIOD | UNIDADE DE I/O DIGITAL | UIOD.di1 | di1 | N/A | PENDENTE DE DEFINIÇÃO OFICIAL | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PLANEJADO |

| UHM  | UNIDADE DE HEALTH MONITOR | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE | PENDENTE |

## Observação

Esta tabela deve evoluir conforme:

- definição dos códigos legados do gateway
- definição dos barramentos físicos
- implementação dos mappers
- criação dos handlers de firmware
- validação em bancada

O primeiro binding prioritário continua sendo:

    GSA.led

O binding lógico da GSA já foi expandido na documentação oficial do host, mas os mapeamentos físicos detalhados e handlers de firmware ainda exigem consolidação oficial.

Para o contrato funcional da GSA, consultar:

    docs/official/06-protocolos/06-gsa-sdh-tlv.md

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.

