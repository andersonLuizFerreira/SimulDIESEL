# Auditoria SDCTP — Arquitetura CAN Transport

## 1. Resultado Geral

APROVADO

## 2. Diagrama Real Encontrado

Fluxo real consolidado no codigo:

`CanDriver real UCE`
`<-> buffers TX/RX fisicos ou fila RX loopback`
`<-> SdctpService/CanService UCE`
`<-> TLVs SDCTP`
`<-> UceServiceDispatcher`
`<-> SDGW/BPM/Serial-BT`
`<-> UceDispatcher/API`
`<-> SdctpApiService`
`<-> CanRxOutputBuffer + tabela local da UI/consumidores superiores`

No modo `LOOPBACK`, o `CanDriver` real nao consome barramento CAN fisico: `send(frame)` reenfileira o frame na fila RX local e `pollReceived` devolve esses frames ao SDCTP RX.

## 3. Confirmacao por Camada

| Camada | Esperado | Encontrado | Status | Evidencia |
|---|---|---|---|---|
| CanDriver UCE | Camada inferior CAN, RX/TX por frames/buffers, sem conhecer API/SDGW/UI | `CanDriver` expoe `send`, `pollReceived`, status/log; modo `LOOPBACK` usa fila RX interna; seletor oficial usa sempre `CanDriver` | APROVADO | `CanDriver.h:42,44,62-72`; `CanDriver.cpp:84-90,242-252,302-310,400-428`; `CanDriverSelector.h:3-4` |
| SDCTP UCE | Ficar entre driver e dispatcher, preservar RX AUTO/DIRECT_ONLY e TX_DIRECT/TX_TABLE | `SdctpService` encapsula `CanService`; dispatcher roteia TLVs SDCTP 0x40-0x46 e 0x50-0x53 | APROVADO | `sdctp/SdctpService.h`; `UceServiceDispatcher.cpp`; `CanService.cpp` |
| UceServiceDispatcher | Roteamento TLV/event FIFO, sem regra CAN nem acesso direto ao driver | Dispatcher conhece `SdctpService` e publica eventos; driver permanece encapsulado no SDCTP/CanService | APROVADO | `UceServiceDispatcher.h`; `UceServiceDispatcher.cpp` |
| UceDispatcher/API | Ponte SDGW/TLV para SDCTP API | `UceDispatcher` encaminha TLVs e eventos; `SendCanAsync 0x26` ficou legado/obsolete; fluxo CAN oficial passa por `SdctpApiService` | APROVADO | `UceDispatcher.cs`; `UceClient.cs`; `SdctpApiService.cs:89` |
| SDCTP API | Fronteira oficial CAN para consumidores superiores | `BpmSerialService` expoe `Sdctp`; `FrmUceLogic` recebe/usa `SdctpApiService`; TX manual usa `SendDirectAsync`/0x50 | APROVADO | `BpmSerialService.cs`; `FrmUceLogic.cs:35-48,137-164,175-177` |
| Buffer RX/TX API | RX por `TryReadRxFrame`, TX por metodos SDCTP | UI drena `_logic.TryReadRxFrame`; `CanRxFrameAvailable` apenas agenda drenagem; tabela visual local nao usa mirror | APROVADO | `frmUCE_UI.cs:30-32,40,291-294,516-518,530-550,764-817` |
| Consumidores superiores | Nao consumir TLV/mirror como fluxo oficial | UI CAN usa `UiCanMonitorRow` local e botao `LIMPAR`; mirror permanece apenas diagnostico/snapshot interno | APROVADO | `frmUCE_UI.cs:442-579,505-510`; `FrmUceLogic.cs:170-177` |

## 4. Inconsistencias Encontradas

Nenhuma inconsistencia bloqueante encontrada na arquitetura consolidada.

## 5. Acoplamentos Indevidos

- UI chamando TLV direto: nao encontrado no fluxo CAN normal.
- Dispatcher acessando `CanDriver`: nao encontrado.
- `CanDriver` conhecendo protocolo/API/UI: nao encontrado.
- Consumidor superior lendo tabela interna como fluxo oficial: corrigido; a UI usa `TryReadRxFrame` e tabela local.

## 6. Pontos Ainda Como Wrapper/Fachada

- `SdctpService` ainda encapsula `CanService`.
- `SdctpProtocol` ainda encapsula `CanCrudProtocol`.
- `SdctpEventProcessor`, `SdctpRxMirrorManager`, `SdctpRxOutputBuffer` e `SdctpTxManager` ainda delegam para as classes `Can*` validadas.
- `ApiCanService` permanece como implementacao interna encapsulada por `SdctpApiService`.

## 7. Riscos

- Nomes legados `CanService`, `CanCrudProtocol`, `ApiCanService` e gerenciadores `Can*` ainda existem por baixo dos wrappers SDCTP para reduzir risco de regressao.
- Arquivos `CanDriver_fake.*` permanecem no repositorio como legado descontinuado, mas foram removidos do seletor oficial e excluidos do build PlatformIO.
- Teste visual manual da UI em hardware nao foi automatizado neste workspace; validacao automatica cobriu build, harness RX/TX e consumidor oficial do output buffer.

## 8. Conclusao

- A arquitetura conceitual esta verdadeira? Sim.
- SDCTP esta realmente entre CanDriver/SDGW e consumidores? Sim.
- O buffer RX/TX da API espelha funcionalmente o CanDriver da UCE? Sim: RX sai por `TryReadRxFrame` e TX entra por `SdctpApiService`.
- O projeto esta pronto para checkpoint? Sim, com a ressalva de validacao manual de bancada da UI/porta serial quando houver UCE conectada.
