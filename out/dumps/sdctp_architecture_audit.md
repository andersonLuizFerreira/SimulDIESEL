# Auditoria SDCTP — Arquitetura CAN Transport

## 1. Resultado Geral

REPROVADO

Motivo: a maior parte da pilha SDCTP existe e o fluxo CAN validado esta encapsulado por wrappers/fachadas, mas o contrato conceitual ainda nao e totalmente verdadeiro no codigo atual. Ha pelo menos dois desvios reais:

- consumidores superiores ainda usam `ApiCanService` legado, nao `SdctpApiService`;
- `FrmUceLogic.SendCanAsync` ainda envia TX pelo `IUceDispatcher.SendCanAsync` legado (`CAN_TX 0x26`), bypassando o SDCTP/API oficial e os comandos TX SDCTP `0x50..0x53`.

## 2. Diagrama Real Encontrado

Fluxo UCE real encontrado:

```text
CanDriver/CanDriverFake
  <-> pollReceived/send e filas fisicas/fake RX/TX
CanService
  <-> RX AUTO/DIRECT_ONLY, CanRxHub, tabela RX, tabela TX, codec CRUD
SdctpService
  <-> wrapper oficial que delega ao CanService validado
UceServiceDispatcher
  <-> roteia TLVs CAN/SDCTP para SdctpService e publica eventos em FIFO
UceTransport/SpiLink
  <-> transporte fisico para BPM/SDGW
```

Fluxo API real encontrado:

```text
SDGW/SdhClient/UceClient
  <-> envia TLVs e decodifica eventos da UCE
UceDispatcher
  <-> repassa chamadas e eventos de UceClient
ApiCanService
  <-> processa eventos RX, mantem mirror, output buffer e TX manager
SdctpApiService
  <-> wrapper oficial que delega ao ApiCanService validado
FrmUceLogic/UI
  <-> usa ApiCanService para RX snapshot/diagnostico, mas TX ainda chama IUceDispatcher.SendCanAsync legado
```

Portanto, o desenho correto mais fiel ao codigo atual e:

```text
CanDriver UCE
<-> CanService validado
<-> SdctpService wrapper
<-> UceServiceDispatcher FIFO/TLV
<-> UceTransport/SPI/SDGW
<-> UceClient/UceDispatcher
<-> ApiCanService validado
<-> SdctpApiService wrapper ainda nao adotado pelos consumidores
<-> FrmUceLogic/UI parcialmente via ApiCanService, parcialmente via IUceDispatcher legado
```

## 3. Confirmação por Camada

| Camada | Esperado | Encontrado | Status | Evidência |
|---|---|---|---|---|
| CanDriver UCE | Camada inferior CAN, RX/TX por frames/buffers, sem conhecer API/SDGW/UI | `CanDriver` e `CanDriverFake` expõem `pollReceived` e `send`; fake possui filas RX/TX; seletor troca real/fake por macro | APROVADO | `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver.h:5,42,44`; `CanDriver_fake.h:5,42,44,90-96`; `CanDriverSelector.h:5,8` |
| SDCTP UCE | Protocolo entre CanDriver e dispatcher, com RX AUTO/DIRECT_ONLY, tabela RX, TX_DIRECT/TX_TABLE e TLVs SDCTP | Existe `SdctpService`, mas ele e wrapper sobre `CanService`; a logica real permanece em `CanService`, `CanRxHub`, `CanRxTableManager`, `CanTxTableManager` | APROVADO COM RESSALVA | `SdctpService.h:10`; `CanService.h:14,41-43,132-162`; `CanService.cpp:72-76,121,127-134,367,384,412-414,433-434,584,737,748,862,908,945,1032`; `CanRxTableManager.cpp:70,109,118,144`; `CanTxTableManager.cpp:103` |
| UceServiceDispatcher | Receber e encaminhar TLVs SDCTP, publicar eventos, ter FIFO, sem regra CAN e sem acesso direto ao CanDriver | Dispatcher referencia `SdctpService`, encaminha comandos CAN/SDCTP para `_sdctp.handleTlv`, publica eventos por FIFO; nao referencia CanDriver | APROVADO | `UceServiceDispatcher.h:6,9,14,19,40-47`; `UceServiceDispatcher.cpp:22,53-58,72,75,80,100,129,143-159` |
| UceDispatcher/API | Ponte TLV entre SDGW/UceClient e SDCTP API, sem regra CAN de alto nivel | `UceClient` monta TLVs e parseia eventos; `UceDispatcher` repassa chamadas/eventos. Ha parsers TLV aqui, mas isso e coerente com a camada de ponte | APROVADO COM RESSALVA | `UceClient.cs:32-34,114-181,303-314`; `UceDispatcher.cs:7-27,31,39-47,84-116,129-136`; `UceParsers.cs:352,434-450,571-593`; `GwProtocol.cs:45-56` |
| SDCTP API | Receber TLVs, reconstruir RX, manter mirror/output buffer, expor TryReadRxFrame, enviar TX via UceDispatcher | `ApiCanService` faz isso; `SdctpApiService` existe como wrapper, mas consumidores ainda usam `ApiCanService` diretamente | APROVADO COM RESSALVA | `ApiCanService.cs:16,21,36-47,179-211,250,268-273,340,367,412`; `CanEventProcessor.cs:22-76,144-247,256,291,308`; `SdctpApiService.cs:15,19,24,68-105` |
| Buffer RX/TX API | RX direto e reconstruido entram no OutputBuffer; TX superior vira TLV TX; UCE transmite localmente | RX direto/reconstruido entra no `CanRxOutputBuffer`; TX SDCTP existe via `CanTxManager`, mas fluxo superior atual chama TX legado em `FrmUceLogic.SendCanAsync` | REPROVADO PARCIAL | `CanRxOutputBuffer.cs:10,78`; `ApiCanService.cs:179-181,250,272-273,340,367`; `CanTxManager.cs:28-31,77-81,98-172,190-193`; `FrmUceLogic.cs:131-136`; `UceClient.cs:134-138` |
| Consumidores superiores | Consumir SDCTP/API, nao TLV nem UceDispatcher direto; UI pode exibir mirror para diagnostico | `FrmUceLogic` usa `ApiCanService` para snapshot/diagnostico, mas usa `IUceDispatcher` diretamente para TX legado; `TryReadRxFrame` nao tem consumidor real fora das classes SDCTP/API | REPROVADO | `FrmUceLogic.cs:31,36,40,54-55,120,131-136,149,154-167`; `frmUCE_UI.cs:211,460-473`; busca por `TryReadRxFrame(` retorna apenas declaracoes em `ApiCanService`, `SdctpApiService` e `SdctpRxOutputBuffer` |

## 4. Inconsistências Encontradas

1. `FrmUceLogic.SendCanAsync` bypassa SDCTP/API para TX.
   - Evidencia: `FrmUceLogic.cs:131-136` chama `_uceDispatcher.SendCanAsync(...)`.
   - Esse caminho usa `UceClient.SendCanAsync` com `GwProtocol.UceCanTxType` (`CAN_TX 0x26`), nao `CAN_TX_DIRECT 0x50` nem tabela TX SDCTP.
   - Evidencia: `UceClient.cs:134-138`.

2. Os wrappers SDCTP/API existem, mas nao estao adotados pelos consumidores.
   - Evidencia: `SdctpApiService` existe em `BLL/Services/CAN/SDCTP/SdctpApiService.cs`, mas a busca por `new SdctpApiService` nao encontrou uso.
   - `BpmSerialService` cria `ApiCanService` diretamente.
   - Evidencia: `BpmSerialService.cs:70-71,106-108`.

3. `CanRxOutputBuffer` e a saida oficial declarada, mas nao ha consumidor superior real lendo `TryReadRxFrame`.
   - Evidencia: busca por `TryReadRxFrame(` encontra apenas declaracoes/wrappers em `ApiCanService.cs:179`, `SdctpRxOutputBuffer.cs:34`, `SdctpApiService.cs:68`.

4. A camada superior ainda usa snapshot da tabela RX para exibicao principal.
   - Isso e permitido para UI/diagnostico, mas nao confirma consumo do fluxo oficial por `CanRxOutputBuffer`.
   - Evidencia: `FrmUceLogic.cs:149`; `frmUCE_UI.cs:460-473`.

## 5. Acoplamentos Indevidos

- UI chamando TLV direto: nao encontrado. A UI chama `FrmUceLogic`, nao `GwProtocol` para montar TLV CAN. Ela usa constantes `GwProtocol` para status/texto/limites de exibicao, o que e acoplamento de apresentacao/diagnostico, nao fluxo TLV direto.
- Dispatcher acessando CanDriver: nao encontrado. `UceServiceDispatcher` referencia `SdctpService`, nao `CanDriver`.
- CanDriver conhecendo protocolo/API/UI: nao encontrado. `CanDriver`/`CanDriverFake` operam em `Frame`, `pollReceived`, `send`, status/log e filas fake.
- Consumidor superior lendo tabela interna como fluxo oficial: parcialmente encontrado. `FrmUceLogic.GetCanRows` retorna `_apiCanService.GetSnapshot()` e a UI preenche grid de RX por snapshot. Isso parece ser exibicao/diagnostico, mas nao ha consumidor de `TryReadRxFrame`, entao o fluxo oficial ainda nao esta demonstrado na camada superior.
- Consumidor superior enviando TX fora do SDCTP/API: encontrado. `FrmUceLogic.SendCanAsync` chama `IUceDispatcher.SendCanAsync` legado.

## 6. Pontos Ainda Como Wrapper/Fachada

- `SdctpService` encapsula `CanService`; a logica RX/TX real continua em `CanService`.
- `SdctpCodec`/`SdctpProtocol` no firmware sao aliases de `CanCrudProtocol`.
- `SdctpRxTableManager` e alias de `CanRxTableManager`.
- `SdctpTxTableManager` e alias de `CanTxTableManager`.
- `SdctpApiService` encapsula `ApiCanService`.
- `SdctpEventProcessor` encapsula `CanEventProcessor`.
- `SdctpRxMirrorManager` encapsula `CanRxMirrorManager`.
- `SdctpRxOutputBuffer` encapsula `CanRxOutputBuffer`.
- `SdctpTxManager` encapsula `CanTxManager`.

## 7. Riscos

- Nomes antigos ainda sao os nomes realmente consumidos (`CanService`, `ApiCanService`, `CanTxManager`, `CanRxMirrorManager`).
- Wrappers SDCTP ainda delegam para classes legadas e nao provam, sozinhos, uma fronteira arquitetural forte.
- `SdctpApiService` nao e usado por `FrmUceLogic` nem por `BpmSerialService`.
- TX superior ainda possui caminho legado `CAN_TX 0x26`, fora do contrato SDCTP TX oficial `0x50..0x53`.
- `CanRxOutputBuffer` existe e e alimentado, mas falta consumidor superior real usando `TryReadRxFrame`.
- A UI exibe tabela espelho, o que e aceitavel para diagnostico, mas pode virar dependencia funcional se nenhum consumidor usar o buffer oficial.

## 8. Conclusão

- A arquitetura conceitual esta verdadeira? Parcialmente. A UCE esta bem alinhada; a API tem a implementacao validada e wrappers SDCTP, mas a adocao pelos consumidores superiores ainda nao fecha o contrato.
- SDCTP esta realmente entre CanDriver/SDGW e consumidores? Na UCE, sim. Na API, parcialmente: `ApiCanService` esta entre `UceDispatcher` e parte dos consumidores, mas `SdctpApiService` oficial ainda nao e usado e ha TX superior bypassando via `IUceDispatcher`.
- O buffer RX/TX da API espelha funcionalmente o CanDriver da UCE? Parcialmente. RX e reconstruido e enfileirado em `CanRxOutputBuffer`; TX tem `CanTxManager` para transformar comandos superiores em TLVs SDCTP. Porem nao ha consumidor superior lendo `TryReadRxFrame`, e ha TX legado ainda em uso.
- O projeto esta pronto para checkpoint? Nao para checkpoint arquitetural SDCTP estrito. Esta pronto apenas como checkpoint intermediario: "SDCTP wrappers/fachadas criados sobre caminho validado, com inconsistencias de adocao na API".

Validacao opcional: esta auditoria foi estatica e baseada no codigo atual. Builds/harness nao foram executados nesta etapa porque o objetivo principal foi confirmar a arquitetura e nao alterar codigo.
