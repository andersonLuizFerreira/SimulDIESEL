# Documentação Oficial do SimulDIESEL

Esta pasta concentra a documentação oficial do estado atual do projeto SimulDIESEL. O conteúdo foi reorganizado para refletir o que está implementado no repositório hoje: hardware embarcado, firmware de gateway, firmware de periféricos, software local em C# e contratos/protocolos preservados em material legado.

## Visão rápida

O repositório mostra uma arquitetura de bancada composta por:

- `local-api/`: aplicação local em C# com camadas `UI`, `BLL`, `DAL` e `DTL`.
- `hardware/firmware/esp32-api-bridge/`: gateway embarcado baseado em ESP32 que faz o enlace com o host e roteia requisições para barramentos internos.
- `hardware/firmware/gerador-sinais-analogicos-GSA/`: firmware de periférico I2C com pilha `Transport -> Link -> Service`.
- `hardware/boards/`: artefatos de placas, backplane e conectores.
- `cloud/`: área reservada para contratos e evolução de integração remota; no estado atual, os contratos OpenAPI ainda não descrevem endpoints ativos.

## Navegação

### Visão geral

- [00 - Índice](./00-INDICE.md)
- [Introdução](./01-visao-geral/01-introducao.md)
- [Objetivos](./01-visao-geral/02-objetivos.md)
- [Escopo](./01-visao-geral/03-escopo.md)

### Arquitetura

- [Visão arquitetural](./02-arquitetura/01-visao-arquitetural.md)
- [Camadas do sistema](./02-arquitetura/02-camadas-do-sistema.md)
- [Fluxo de comunicação](./02-arquitetura/03-fluxo-de-comunicacao.md)

### Hardware e firmware

- [Backplane](./03-hardware/01-backplane.md)
- [Baby boards](./03-hardware/02-baby-boards.md)
- [Barramentos](./03-hardware/03-barramentos.md)
- [Alimentação](./03-hardware/04-alimentacao.md)
- [Arquitetura de firmware](./04-firmware/01-arquitetura-firmware.md)
- [Drivers](./04-firmware/02-drivers.md)
- [Gerenciamento de recursos](./04-firmware/03-gerenciamento-recursos.md)

### Software local e protocolos

- [Arquitetura do software](./05-software-dashboard/01-arquitetura-software.md)
- [Interface de usuário](./05-software-dashboard/02-interface-usuario.md)
- [Camada de hardware do software](./05-software-dashboard/03-camada-hardware.md)
- [SDH Command Model](./06-protocolos/01-sdh-command-model.md)
- [CAN](./06-protocolos/02-can.md)
- [J1939](./06-protocolos/03-j1939.md)

### Simulações, uso e desenvolvimento

- [Simulação de módulos](./07-simulacoes/01-simulacao-modulos.md)
- [Simulação de sensores](./07-simulacoes/02-simulacao-sensores.md)
- [Simulação de atuadores](./07-simulacoes/03-simulacao-atuadores.md)
- [Manutenção de módulos](./08-casos-de-uso/01-manutencao-modulos.md)
- [Diagnóstico](./08-casos-de-uso/02-diagnostico.md)
- [Testes de bancada](./08-casos-de-uso/03-testes-bancada.md)
- [Organização do repositório](./09-desenvolvimento/01-organizacao-repositorio.md)
- [Padrões de código](./09-desenvolvimento/02-padroes-codigo.md)
- [Fluxo Git](./09-desenvolvimento/03-fluxo-git.md)

### Testes, planejamento e documentação técnica

- [Testes de hardware](./10-testes/01-testes-hardware.md)
- [Testes de firmware](./10-testes/02-testes-firmware.md)
- [Testes de integração](./10-testes/03-testes-integracao.md)
- [Planejamento](./11-planejamento/01-planejamento.md)
- [Próximas funcionalidades](./11-planejamento/02-proximas-funcionalidades.md)
- [Especificações](./12-documentacao-tecnica/01-especificacoes.md)
- [Diagramas](./12-documentacao-tecnica/02-diagramas.md)
- [Contratos de software](./12-documentacao-tecnica/03-contratos-software.md)

## Material legado preservado

A pasta `docs/legacy-docs/` continua sendo a principal fonte histórica para contratos, decisões arquiteturais e roadmap técnico. Ela não substitui a documentação oficial atual, mas sustenta diversas descrições aqui consolidadas, especialmente sobre o protocolo `SGGW`, decisões de enquadramento `COBS + CRC8` e o planejamento incremental do gateway.
