# Documentação Oficial do SimulDIESEL

Esta pasta concentra a documentação oficial vigente do projeto.

Para a arquitetura atual do enlace host <-> BPM, a referência principal é:

- [Visão geral do host SDH/SDGW](../local-api/src/SimulDIESEL/SimulDIESEL/SDH_HOST_OVERVIEW.md)
- [Arquitetura SDH no host](./05-software-dashboard/04-sdh-host-architecture.md)
- [Camada de hardware do software](./05-software-dashboard/03-camada-hardware.md)
- [Fluxo de comunicação](./02-arquitetura/03-fluxo-de-comunicacao.md)
- [Contratos de software](./12-documentacao-tecnica/03-contratos-software.md)
- [Arquitetura de firmware](./04-firmware/01-arquitetura-firmware.md)
- [Board BPM](./04-firmware/boards/01-bpm.md)

## Navegação oficial

### Visão geral

- [00 - Índice](./00-INDICE.md)
- [Introdução](./01-visao-geral/01-introducao.md)
- [Objetivos](./01-visao-geral/02-objetivos.md)
- [Escopo](./01-visao-geral/03-escopo.md)

### Arquitetura

- [Visão arquitetural](./02-arquitetura/01-visao-arquitetural.md)
- [Camadas do sistema](./02-arquitetura/02-camadas-do-sistema.md)
- [Fluxo de comunicação](./02-arquitetura/03-fluxo-de-comunicacao.md)

### Firmware

- [Arquitetura de firmware](./04-firmware/01-arquitetura-firmware.md)
- [Board BPM](./04-firmware/boards/01-bpm.md)
- [Arquitetura SDH no gateway](./04-firmware/04-sdh-gateway-architecture.md)

### Software local

- [Arquitetura do software](./05-software-dashboard/01-arquitetura-software.md)
- [Camada de hardware do software](./05-software-dashboard/03-camada-hardware.md)
- [Arquitetura SDH no host](./05-software-dashboard/04-sdh-host-architecture.md)

### Casos de uso e testes

- [Diagnóstico](./08-casos-de-uso/02-diagnostico.md)
- [Testes de bancada](./08-casos-de-uso/03-testes-bancada.md)
- [Testes de integração](./10-testes/03-testes-integracao.md)

### Documentação técnica

- [Especificações](./12-documentacao-tecnica/01-especificacoes.md)
- [Diagramas](./12-documentacao-tecnica/02-diagramas.md)
- [Contratos de software](./12-documentacao-tecnica/03-contratos-software.md)

## Acervo histórico

Os diretórios abaixo não representam a arquitetura vigente e devem ser lidos apenas como histórico:

- `docs/legacy-docs/`
- `docs_estado_atual/`
- `docs_reconstruida/`

Quando houver divergência entre esses acervos e os documentos listados neste README, prevalece a documentação oficial em `docs/` e em `local-api/src/SimulDIESEL/SimulDIESEL/SDH_HOST_OVERVIEW.md`.
