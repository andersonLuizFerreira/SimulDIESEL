# AGENTS.md - SimulDIESEL

Este arquivo orienta qualquer agente de IA que entre no projeto SimulDIESEL. Ele deve ser lido antes de qualquer analise, implementacao, validacao ou entrega.

## Visao geral

O SimulDIESEL e uma plataforma de bancada para simulacao, diagnostico e validacao de modulos diesel. O projeto combina:

- aplicacao local C# WinForms em `local-api/src/SimulDIESEL/SimulDIESEL`;
- camada host organizada em UI, BLL, DAL e DTL;
- gateway embarcado BPM;
- firmware UCE para execucao fisica CAN;
- firmware GSA para geracao de sinais analogicos;
- Banco de Modulos em `Data/Modules`;
- contratos de protocolo em SDH, SDGW, SDCTP e J1939.

A documentacao oficial viva fica em `docs/official/`. Dumps de ETAPAS e levantamentos ficam em `out/dumps/`.

## Arquitetura oficial

Use como modelo mental conservador:

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

Responsabilidades principais:

- `UI`: formularios WinForms, apresentacao e interacao do operador.
- `BLL`: casos de uso, FormsLogic, clients de boards, servicos de aplicacao e decoders de alto nivel.
- `DAL`: SDH, SDGW, sessao, framing, scheduler, supervisor e transportes.
- `DTL`: DTOs, enums e contratos compartilhados.
- `SDH`: contrato semantico de comandos de operacao/configuracao.
- `SDGW`: gateway/transporte confiavel, framing, COBS, CRC, ACK/ERR, sequencia, retry e eventos.
- `SDCTP`: protocolo de massa CAN RX/TX, mirror, buffers, compactacao e sincronizacao.
- `J1939`: camada de decodificacao automotiva sobre `CanFrameDto`; nao deve poluir SDGW.
- `BPM`: gateway embarcado entre host e boards.
- `UCE`: execucao fisica CAN, LED e servicos de hardware por SPI.
- `GSA`: geracao de sinais analogicos por I2C, TCA9548A, MCP4725 e EEPROM.
- `Banco de Modulos`: base futura para perfis de modulos, comandos SDH, sequencias de teste, capturas CAN/J1939 e configuracoes.

## Fontes de verdade

Quando houver divergencia, use a ordem:

1. codigo-fonte implementado;
2. contratos tecnicos efetivos;
3. documentacao oficial em `docs/official/`;
4. dumps recentes em `out/dumps/`;
5. documentacao legada em `docs/legacy/`.

Se a divergencia nao puder ser resolvida com leitura, registre como `pendente de confirmacao`. Nao escolha arbitrariamente.

## Regras de escopo

- Use sempre o termo `ETAPA`; nunca use `FASE`.
- Nao altere nada fora do escopo solicitado.
- Nao modifique contratos consolidados sem autorizacao explicita.
- Nao remova codigo legado sem pedido explicito.
- Nao altere firmware ao trabalhar na API, salvo pedido explicito.
- Nao altere UI ao trabalhar em BLL/DAL, salvo pedido explicito.
- Nao altere banco ao trabalhar em UI, salvo pedido explicito.
- Nao misture UI, BLL, DAL, DTL, firmware e protocolo em uma mesma ETAPA sem autorizacao.
- Nao invente comportamento inexistente; marque como `PLANEJADO`, `LEGADO`, `PARCIALMENTE IMPLEMENTADO` ou `pendente de confirmacao`.
- Nao use mocks/fakes sem deixar explicito.
- Nao esconda warnings, erros, falhas de build ou validacoes incompletas.

## Regras de nomenclatura

- `SDH`: contrato semantico de comandos.
- `SDGW`: SimulDIESEL Gateway, transporte/gateway.
- `SDCTP`: SimulDIESEL CAN Transport Protocol. Esta e a unica nomenclatura oficial para o protocolo de transporte CAN do projeto.
- `UCE`: Unidade de Comunicacao Externa.
- `BPM`: Backplane Manager Module.
- `GSA`: Gerador de Sinais Analogicos.
- `DTL`: camada de DTOs/contratos compartilhados.

## Regras de ETAPA

Toda ETAPA deve declarar:

- tema e objetivo;
- escopo permitido;
- fora de escopo;
- arquivos provaveis;
- regras de implementacao;
- validacao obrigatoria;
- necessidade de dump;
- entrega esperada;
- restricoes e rollback.

ETAPAS de congelamento devem registrar exatamente o que fica congelado, o que permanece legado, o que esta pendente e quais contratos nao podem ser alterados sem nova autorizacao.

## Validacao

Valide apenas o que se aplica ao escopo. Exemplos:

- API C# WinForms: build da solucao `local-api/src/SimulDIESEL/SimulDIESEL.sln`.
- Firmware BPM/UCE/GSA: `platformio run` na pasta correspondente, quando a ETAPA permitir.
- Protocolos CAN/SDCTP/J1939: scripts em `tools/testes/`, quando pertinentes.
- Banco de Modulos: validadores em `tools/dumps/module_database_model/`, quando pertinentes.
- Documentacao: conferir arquivos criados, links basicos e coerencia com `docs/DOCUMENTATION_RULES.md`.

Para ETAPA exclusivamente documental, nao execute build funcional se nao houver alteracao de codigo; registre que nao se aplica.

## Entrega

Toda entrega deve conter:

- lista de arquivos alterados/criados;
- resumo objetivo do que mudou;
- validacoes executadas e resultados;
- warnings, erros ou validacoes nao executadas;
- dump gerado, quando aplicavel;
- pontos pendentes;
- confirmacao de que rollback foi preservado.

## Commits, tags e rollback

- Nao faca commit sem autorizacao explicita.
- Nao crie branch sem autorizacao explicita.
- Nao crie tag sem autorizacao explicita.
- Preserve rollback: mantenha mudancas pequenas, rastreaveis e documentadas.
- Nunca use comandos destrutivos para limpar alteracoes existentes de outro autor.

## Documentacao

- A documentacao oficial viva fica em `docs/official/`.
- Documentacao de apoio para agentes fica em `docs/agents/`.
- Dumps de ETAPA ficam em `out/dumps/`.
- Ao documentar estado real, use `IMPLEMENTADO`, `PARCIALMENTE IMPLEMENTADO`, `PLANEJADO` e `LEGADO`.
- Se faltar informacao, escreva `pendente de confirmacao`.

## Skills do projeto

Skills reutilizaveis para agentes ficam em `docs/agents/skills/`. Antes de atuar em uma area, leia a skill correspondente e mantenha a ETAPA dentro do escopo permitido.

## Referencias rapidas

- `docs/agents/agents_overview.md`
- `docs/agents/project_conventions.md`
- `docs/agents/etapa_prompt_template.md`
- `docs/agents/validation_checklist.md`
- `docs/agents/freeze_checkpoint_template.md`
