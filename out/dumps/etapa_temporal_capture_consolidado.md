# ETAPA - Captura Temporal de Massa J1939 para Engenharia Reversa

Data: 2026-05-13

## Objetivo

Criar captura temporal de trafego J1939 para engenharia reversa assistida por IA, com reducao de frames repetitivos e exportacao legivel em `.txt`/`.md`.

## Branch

- `feature/j1939-reference-catalogs`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939TemporalCaptureService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939CaptureExportService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CapturedEventDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939TemporalTickDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CaptureSessionDto.cs`
- `out/dumps/etapa_temporal_capture_service.md`
- `out/dumps/etapa_temporal_capture_export.md`
- `out/dumps/etapa_temporal_capture_ui.md`
- `out/dumps/etapa_temporal_capture_validation.md`
- `out/dumps/etapa_temporal_capture_consolidado.md`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `docs/official/06-protocolos/05-j1939.md`
- `docs/official/06-protocolos/README.md`
- `docs/official/05-software-dashboard/02-interface-usuario.md`

Observacao: a worktree ja continha alteracoes anteriores da ETAPA de catalogos/Rede CAN. Elas foram preservadas e nao revertidas.

## Cadeia final

```text
CAN RX consolidado
  -> TryReadRxFrame()
  -> FrmUceLogic.TryDecodeJ1939Frame()
  -> J1939DataMonitorMessageDto
  -> UpdateJ1939DataRow()
  -> FrmUceLogic.RegisterJ1939TemporalCaptureMessage()
  -> J1939TemporalCaptureService
  -> J1939CaptureSessionDto
  -> J1939CaptureExportService
  -> .md/.txt
```

## Comportamento implementado

- Inicio de captura limpa sessao anterior e cria nova sessao temporal.
- Finalizacao para a sessao, abre `SaveFileDialog` e exporta `.md` ou `.txt`.
- Frames identicos consecutivos sao agrupados.
- Primeira ocorrencia vira `NEW_FRAME`.
- Repeticoes agrupadas viram `PERIODIC_TICK`.
- Mudancas apos uma assinatura anterior viram `TRANSITION`.
- Address Claim (`PGN 0x00EE00`) vira `ADDRESS_CLAIM` com `NameHex`, `ClaimedSA` e `RawCanId` quando disponivel.
- Exportador inclui resumo por PGN, Source Address, Address Claims detectados, periodicidade e top talkers.

## Preservacoes

- Parser J1939 preservado.
- Address Registry preservado.
- Fluxo CAN RX consolidado preservado.
- Mirror CAN preservado.
- SDGW preservado.
- SDCTP preservado.
- Firmware preservado.
- Banco/SQLite nao foi alterado por esta ETAPA.

## Validacao

Build final:

- 0 erros;
- 0 avisos.

Smoke test:

- 20 frames identicos simulados;
- 2 eventos exportaveis;
- `PERIODIC_TICK` detectado;
- intervalo medio de 100 ms.

Smoke test adicional de Address Claim:

- entrada `PGN 0x00EE00`, SA `0xC0`, Raw CAN ID `0x18EEFFC0`;
- payload `00 00 C0 0B 00 13 0E 20`;
- exportacao gerou `Evento: ADDRESS_CLAIM`;
- exportacao gerou `NameHex: 200E13000BC00000`;
- exportacao gerou `ClaimedSA: 0xC0`.

## Validacao nao executada

Nao foi possivel executar a validacao real solicitada com a EST:

- abrir `Status do Controlador`;
- capturar trafego;
- mudar para `Monitor`;
- finalizar captura;
- confirmar PGN `0x00DA00` em barramento real.

Estado: `pendente de confirmacao` em bancada.

## Documentacao

Atualizada documentacao oficial:

- `docs/official/06-protocolos/05-j1939.md`;
- `docs/official/06-protocolos/README.md`;
- `docs/official/05-software-dashboard/02-interface-usuario.md`.

## Criterios de aceite

Atendidos:

- servico temporal criado;
- reducao de spam implementada;
- exportacao TXT/MD implementada;
- integracao na aba `Dados J1939`;
- build final com 0 erros;
- fluxo CAN RX consolidado preservado;
- parsers J1939 preservados;
- firmware nao alterado;
- SDGW nao alterado;
- SDCTP nao alterado;
- arquivos gerados legiveis para analise humana e IA.
- Address Claim aparece destacado no arquivo exportado para facilitar analise temporal.

Parcial/pendente:

- `SaveFileDialog` compilado e integrado, mas abertura visual nao validada automaticamente;
- teste real com EST nao executado neste ambiente;
- responsividade sob trafego continuo real pendente de confirmacao.

## Rollback

Rollback preservado. Nenhum commit, tag ou branch foi criado.
