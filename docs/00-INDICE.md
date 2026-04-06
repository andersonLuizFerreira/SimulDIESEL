⬅ [Retornar para SimulDIESEL — Documentação Oficial](README.md)
⬅ [Retornar para Índice Geral](00-INDICE.md)

# Índice Geral da Navegação

Este documento é o **mapa completo clicável** da documentação oficial do SimulDIESEL.

Fora desta página, a navegação estrutural continua restrita à relação pai -> filhos imediatos.

## Acesso rápido

* [Página inicial da documentação](README.md)
* [Regras Oficiais da Documentação](DOCUMENTATION_RULES.md)
* [Acervo legado](legacy/00-INDICE-LEGACY.md)

## Árvore oficial completa

- [Visão Geral do Projeto](official/01-visao-geral/01-visao-geral-projeto.md)
  - [Visão Arquitetural](official/02-arquitetura/01-visao-arquitetural.md)
    - [Visão Física do Projeto](official/02-arquitetura/02-visao-fisica.md)
      - [API e Host Local](official/02-arquitetura/04-api-e-host-local.md)
        - [Interface de Usuário](official/05-software-dashboard/02-interface-usuario.md)
        - [BLL do Host](official/02-arquitetura/05-bll-do-host.md)
          - [FormsLogic e Fachadas do Host](official/02-arquitetura/05-bll-do-host/01-formslogic-e-fachadas.md)
          - [Clients BPM e GSA](official/02-arquitetura/05-bll-do-host/02-clients-bpm-e-gsa.md)
        - [DAL do Host](official/02-arquitetura/06-dal-do-host.md)
          - [Sessão, SDH e SDGW na DAL](official/02-arquitetura/06-dal-do-host/01-sessao-sdh-e-sdgw.md)
          - [Framing, Scheduler e Supervisor](official/02-arquitetura/06-dal-do-host/02-framing-scheduler-e-supervisor.md)
        - [DTL do Host](official/02-arquitetura/07-dtl-do-host.md)
          - [Contratos SDH, SDGW e DTOs](official/02-arquitetura/07-dtl-do-host/01-contratos-sdh-e-dtos.md)
        - [Transporte do Host](official/02-arquitetura/08-transporte-do-host.md)
          - [SwitchableTransport e Contratos de Transporte](official/02-arquitetura/08-transporte-do-host/01-switchable-transport.md)
          - [Serial e Bluetooth no Host](official/02-arquitetura/09-serial-e-bluetooth.md)
            - [Catálogo Bluetooth e Portas no Windows](official/02-arquitetura/09-serial-e-bluetooth/01-catalogo-e-portas-bluetooth.md)
      - [Hardware da Bancada](official/02-arquitetura/10-hardware-da-bancada.md)
        - [Backplane](official/03-hardware/01-backplane.md)
          - [Baby Boards](official/03-hardware/02-baby-boards.md)
            - [Construção Física das Boards](official/03-hardware/05-boards-fisicas.md)
              - [GSA — Gerador de Sinais Analógicos](official/03-hardware/boards/03-gsa/README.md)
                - [Funcionamento Eletrônico](official/03-hardware/boards/03-gsa/01-funcionamento-eletronico.md)
          - [Barramentos](official/03-hardware/03-barramentos.md)
          - [Alimentação](official/03-hardware/04-alimentacao.md)
      - [Módulo em Teste e X-CONN](official/02-arquitetura/11-modulo-em-teste-e-xconn.md)
    - [Visão Lógica do Projeto](official/02-arquitetura/03-visao-logica.md)
      - [Camadas do Sistema](official/02-arquitetura/02-camadas-do-sistema.md)
        - [Fluxo de Comunicação](official/02-arquitetura/03-fluxo-de-comunicacao.md)
          - [Protocolos e Contratos](official/06-protocolos/README.md)
            - [Onboarding — Arquitetura de Comandos (SDH)](official/06-protocolos/00-onboarding-comandos.md)
              - [SDH Command Model](official/06-protocolos/01-sdh-command-model.md)
                - [SDH Response Model](official/06-protocolos/02-sdh-response-model.md)
                  - [SDH Examples](official/06-protocolos/03-sdh-examples.md)
                    - [GSA — Contrato SDH/TLV](official/06-protocolos/06-gsa-sdh-tlv.md)
            - [CAN](official/06-protocolos/04-can.md)
            - [J1939](official/06-protocolos/05-j1939.md)
      - [Arquitetura de Firmware](official/04-firmware/01-arquitetura-firmware.md)
        - [Drivers de Firmware](official/04-firmware/02-drivers.md)
        - [Gerenciamento de Recursos em Firmware](official/04-firmware/03-gerenciamento-recursos.md)
        - [Arquitetura SDH no Gateway](official/04-firmware/04-sdh-gateway-architecture.md)
          - [Catálogo de Baby Boards e Targets SDH](official/04-firmware/05-catalogo-baby-boards.md)
            - [Boards de Firmware](official/04-firmware/boards/README.md)
              - [BPM](official/04-firmware/boards/BPM/01-bpm.md)
              - [PSU](official/04-firmware/boards/PSU/02-psu.md)
              - [GSA](official/04-firmware/boards/GSA/03-gsa.md)
              - [GSC](official/04-firmware/boards/04-gsc.md)
              - [URL](official/04-firmware/boards/05-url.md)
              - [SLU](official/04-firmware/boards/06-slu.md)
              - [UCO](official/04-firmware/boards/07-uco.md)
              - [UCS](official/04-firmware/boards/08-ucs.md)
              - [UIOD](official/04-firmware/boards/09-uiod.md)
              - [UHM](official/04-firmware/boards/10-uhm.md)
          - [Tabela Mestra de Binding Lógico-Físico do Gateway](official/04-firmware/06-gateway-binding-logico-fisico.md)
          - [Resolver Engine do Gateway](official/04-firmware/07-resolver-engine-gateway.md)
      - [Arquitetura do Software Dashboard (Local API)](official/05-software-dashboard/01-arquitetura-software.md)
        - [Camada Hardware do Software](official/05-software-dashboard/03-camada-hardware.md)
          - [Arquitetura SDH no Host](official/05-software-dashboard/04-sdh-host-architecture.md)
            - [Handshake e Estados da Sessão](official/05-software-dashboard/04-sdh-host-architecture/01-handshake-e-estados-da-sessao.md)
            - [Scheduler, Retry e Supervisão do Link](official/05-software-dashboard/04-sdh-host-architecture/02-scheduler-retry-e-supervisao.md)
            - [Fluxo GSA do Comando ao Evento](official/05-software-dashboard/04-sdh-host-architecture/03-fluxo-gsa-do-comando-ao-evento.md)
            - [Parsing e Tratamento de Respostas](official/05-software-dashboard/04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md)
        - [Simulação de Módulos](official/07-simulacoes/01-simulacao-modulos.md)
          - [Simulação de Sensores](official/07-simulacoes/02-simulacao-sensores.md)
          - [Simulação de Atuadores](official/07-simulacoes/03-simulacao-atuadores.md)
        - [Manutenção de Módulos](official/08-casos-de-uso/01-manutencao-modulos.md)
          - [Diagnóstico de Falhas](official/08-casos-de-uso/02-diagnostico.md)
            - [Testes de Bancada](official/08-casos-de-uso/03-testes-bancada.md)
              - [Testes de Hardware](official/10-testes/01-testes-hardware.md)
              - [Testes de Firmware](official/10-testes/02-testes-firmware.md)
              - [Testes de Integração](official/10-testes/03-testes-integracao.md)
- [Organização do Repositório](official/09-desenvolvimento/01-organizacao-repositorio.md)
  - [Padrões de Código](official/09-desenvolvimento/02-padroes-codigo.md)
    - [Fluxo Git](official/09-desenvolvimento/03-fluxo-git.md)
- [Planejamento](official/11-planejamento/01-planejamento.md)
  - [Próximas Funcionalidades](official/11-planejamento/02-proximas-funcionalidades.md)
- [Especificações](official/12-documentacao-tecnica/01-especificacoes.md)
  - [Diagramas](official/12-documentacao-tecnica/02-diagramas.md)
    - [Contratos de Software](official/12-documentacao-tecnica/03-contratos-software.md)

## Glossário

- **Índice geral**: única página autorizada a apontar para qualquer nó da árvore viva.
- **Ramo**: sequência de páginas pertencentes ao mesmo aprofundamento temático.
- **Pai imediato**: documento estrutural acima da página atual na árvore.
- **ONDE**: trilha física de pilha, arquivos, conectores e posição das classes.
- **COMO**: trilha lógica de função, estados, fluxo, scheduler, retry e eventos.
