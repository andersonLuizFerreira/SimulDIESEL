# ETAPA_003 - Relatorio de atualizacao docs <- codigo

## Status

Concluida em 2026-05-18.

## Escopo executado

Atualizacao de documentos existentes em `docs/` para refletir o estado atual comprovado no codigo versionado, conforme autorizacao humana explicita `codigo -> docs` desta etapa.

## Evidencias consultadas

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanControlApiService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDH/SdhValidator.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDH/SdhToSdgwMapper.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDCTP/`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/SpiLink.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/`

## Arquivos atualizados

- `docs/00-INDICE.md`
- `docs/01-visao-geral/01-visao-geral-projeto.md`
- `docs/02-arquitetura/04-api-e-host-local.md`
- `docs/02-arquitetura/05-bll-do-host.md`
- `docs/02-arquitetura/05-bll-do-host/01-formslogic-e-fachadas.md`
- `docs/02-arquitetura/06-dal-do-host.md`
- `docs/02-arquitetura/06-dal-do-host/01-sessao-sdh-e-sdgw.md`
- `docs/02-arquitetura/07-dtl-do-host.md`
- `docs/02-arquitetura/07-dtl-do-host/01-contratos-sdh-e-dtos.md`
- `docs/04-firmware/01-arquitetura-firmware.md`
- `docs/04-firmware/02-drivers.md`
- `docs/04-firmware/04-sdh-gateway-architecture.md`
- `docs/04-firmware/boards/UCE/11-uce.md`
- `docs/05-software-dashboard/01-arquitetura-software.md`
- `docs/05-software-dashboard/04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md`
- `docs/06-protocolos/07-uce-sdh-tlv.md`
- `docs/09-desenvolvimento/01-organizacao-repositorio.md`
- `docs/11-planejamento/01-planejamento.md`
- `docs/11-planejamento/02-proximas-funcionalidades.md`

## Principais divergencias corrigidas

- Estado geral do projeto deixou de tratar a UCE como proxima board apenas planejada e passou a refletir a rota `UCE.*` ja existente.
- Documentos de BLL, DAL e DTL passaram a incluir `UceClient`, `UceDispatcher`, `FrmUceLogic`, `CanControlApiService`, `SdctpApiService`, DTOs UCE, SDCTP e J1939.
- Documentos de firmware passaram a refletir a UCE real: `SpiLink`, `UceTransport`, `UceServiceDispatcher`, `LedService`, `SdctpService`, `CanService`, drivers e tabelas CAN.
- O gateway deixou de ser descrito como sem device `SPI`; a BPM publica a UCE por `GW_ADDR_UCE`.
- A organizacao do repositorio foi ajustada para remover referencias a `docs/archive`, `docs/generated` e pastas antigas de firmware que nao representam a arvore atual.
- O contrato UCE SDH/TLV passou a registrar comandos CAN/SDCTP e a restricao atual de `UCE.can.rx readAll` no mapper.

## Validacoes realizadas

- `git status --short`: alteracoes restritas a `docs/` antes da criacao deste relatorio.
- Verificacao de links Markdown em `docs/`: `broken_links=0`.
- Busca por referencias legadas removidas: `lib/app`, `lib/core/service`, `lib/protocol/tlv`, `lib/drivers/can`, `lib/diag/trace`, `Sam3xCanDriver`, `UceApp`, `docs/archive`, `docs/generated`, `esp32-api-bridge`.
- Conferencia da arvore real de firmware UCE com `rg --files`.
- Nenhum arquivo funcional de `local-api/`, `hardware/`, `cloud/`, `infra/`, `tools/` ou `tests/` foi alterado.

## Pendencias

- Validacao fisica de bancada ampla da UCE/CAN/SDCTP permanece fora desta etapa.
- Catalogo fisico completo de boards e detalhamento por netlist do backplane permanecem como evolucao futura.
- O hash do commit sera registrado no arquivo da ETAPA apos o primeiro commit desta execucao.

## Rollback

Como a etapa alterou apenas documentacao, relatorio e controle de execucao da propria ETAPA, o rollback pode ser feito revertendo o commit de documentacao correspondente.
