# Levantamento Arquitetural SDH / SDTPC / SDGW / UCE Dispatcher / CanService

Data do levantamento: 2026-05-11

Escopo: API C# em `local-api/src/SimulDIESEL/SimulDIESEL`, firmware UCE e firmware BPM relacionados a SDH, SDGW, Dispatcher UCE, CAN, SDCTP/SDTPC e J1939.

Observacao de nomenclatura: no codigo atual o nome formal encontrado e majoritariamente `SDCTP` (SimulDIESEL CAN Transport Protocol). O termo `SDTPC` aparece no enunciado arquitetural, mas nao foi encontrado como contrato formal no codigo analisado.

## 1. Resumo Executivo

Estado atual encontrado:

- A API ja possui uma camada SDH formal no host: `SdhCommand`, `SdhValidator`, `SdhClient`, `SdhToSdgwMapper`, parsers/serializers de texto e JSON.
- O fluxo de controle CAN da UCE passa por metodos de alto nivel da UI, `FrmUceLogic`, `SdctpApiService`/`IUceDispatcher`, `UceClient`, `BoardTlvDispatcher`, `SdhClient`, `SdhToSdgwMapper`, `SdgwSession` e finalmente SDGW/BPM/UCE.
- O SDGW da API (`SdGwLinkEngine`, `SdgwSession`) esta essencialmente puro como frame/transporte: COBS, CRC, ACK/ERR, flags, sequencia, eventos e entrega de frames.
- O firmware BPM tambem atua como gateway/roteador: recebe SDGW, roteia TLV para GSA/UCE, valida TLV e drena eventos da UCE por IRQ.
- A massa de dados CAN esta formalizada no codigo como `SDCTP`, mas a implementacao ainda e uma fachada sobre classes historicas: na API `SdctpApiService` envolve `ApiCanService`; no firmware UCE `SdctpService` envolve `CanService`; `SdctpCodec` e typedef de `CanCrudProtocol`.
- O Dispatcher UCE da API (`UceClient`/`UceDispatcher`) ainda faz parsing de eventos CAN_RX e CAN CRUD antes de repassar para o CanService API. Isso e parcialmente correto como roteamento, mas tambem concentra conhecimento de tipos TLV CAN no lado dispatcher.
- A UI nao consome TLV bruto nem SDGW diretamente para massa CAN. Ela consome `FrmUceLogic.TryReadRxFrame`, que drena o `CanRxOutputBuffer` via `SdctpApiService`/`ApiCanService`.

Principais desvios de responsabilidade:

- `SdctpApiService` tambem expoe comandos de controle CAN (`SetCanConfigAsync`, `SetCanEnabledAsync`, `GetCanStatusAsync`, `ResetCanAsync`, `PollCanDriverLogAsync`). Pelo modelo desejado, SDCTP deveria ficar restrito a massa/sincronizacao CAN, enquanto controle/operação deveria ser SDH.
- `SdhValidator` e `SdhToSdgwMapper` conhecem detalhes de CAN TX, flags, DATA_MASK, periodos, indices e payloads. Isso mistura a linguagem SDH de operacao com detalhes de contrato CAN/SDCTP.
- `UceClient.OnEventReceived` conhece e classifica `CAN_RX_EVENT`, `CAN_CREATE`, `CAN_EDIT`, `CAN_DELETE`, `CAN_ROW`, `CAN_READ_ALL_DONE`, `CAN_TIC` via `UceParsers`. E um roteamento funcional, mas o dispatcher ainda precisa conhecer a lista de eventos de massa CAN.
- `FrmUceLogic` contem servicos J1939 e decodificacao J1939 sobre frames CAN. Isso nao mistura SDGW, mas coloca protocolo de aplicacao automotiva na logica do formulario. Pode ser aceitavel como camada de aplicacao, mas nao e uma abstracao preparada para multiplos protocolos de forma neutra.
- A UI referencia `GwProtocol` para interpretar status de TX e logs de driver. Nao consome TLV bruto, mas conhece constantes de contrato de baixo nivel.

Riscos arquiteturais:

- Risco de confusao semantica: SDH, SDCTP e TLV usam os mesmos codigos definidos em `GwProtocol`, entao a separacao existe por classes, mas nao por contratos de namespace/domínio.
- Risco de crescimento acoplado: K-LINE/J1949/J1939 futuros podem acabar entrando em `GwProtocol`, `UceParsers`, `UceClient` e `FrmUceLogic` sem fronteira clara.
- Risco de manutencao: a camada chamada SDCTP e, em varios pontos, apenas uma fachada, o que pode esconder responsabilidades antigas em `ApiCanService`, `CanService` e `CanCrudProtocol`.
- Risco de duplicidade de comando CAN: controle CAN passa tanto por `IUceDispatcher` quanto por `SdctpApiService`, tornando ambigua a fronteira "controle SDH" versus "massa SDCTP".

## 2. Fluxo atual de controle/operação

Fluxo real encontrado para configuracao/operacao da UCE:

`UI UCE -> FrmUceLogic -> SdctpApiService ou IUceDispatcher -> UceDispatcher -> UceClient -> BoardTlvDispatcher -> SdhClient -> SdhValidator/SdhToSdgwMapper -> SdgwSession -> SdGwLinkEngine -> BPM Gateway -> UCE Transport -> UceServiceDispatcher -> SdctpService/CanService ou LedService -> Hardware`

Evidencias principais:

- UI chama logica de formulario, nao SDGW diretamente:
  - `frmUCE_UI.ApplyCanConfigAsync` chama `_logic.SetCanConfigAsync(...)` em `local-api/.../UI/frmUCE_UI.cs:442`.
  - `frmUCE_UI.ApplyCanEnabledAsync` chama `_logic.SetCanEnabledAsync(...)` em `local-api/.../UI/frmUCE_UI.cs:466`.
  - `frmUCE_UI.RefreshCanStatusAsync` chama `_logic.GetCanStatusAsync()` em `local-api/.../UI/frmUCE_UI.cs:487`.
  - `frmUCE_UI.BtnEnable_Click` envia CAN TX pela logica em `local-api/.../UI/frmUCE_UI.cs:272`.

- `FrmUceLogic` decide se usa dispatcher direto ou `SdctpApiService`:
  - LED: `SetBuiltinLedAsync` chama `_uceDispatcher.SetBuiltinLedAsync(...)`.
  - CAN config/enable/status/reset/driverLog/readAll/TX: metodos chamam `_sdctp.*` em `local-api/.../BLL/FormsLogic/UCE/FrmUceLogic.cs`.

- `SdctpApiService` delega controle CAN para `IUceDispatcher`:
  - `SetCanConfigAsync`, `SetCanEnabledAsync`, `GetCanStatusAsync`, `ResetCanAsync`, `PollCanDriverLogAsync` chamam `RequireDispatcher().*` em `local-api/.../BLL/Services/CAN/SDCTP/SdctpApiService.cs`.
  - `RequestReadAllAsync`, `SendDirectAsync`, `StartTxAsync`, `StopTxAsync`, `CreateTxRowAsync`, `EditTxRowAsync`, `DeleteTxRowAsync` passam pelo `ApiCanService`/`CanTxManager`.

- `UceDispatcher` e um repassador sobre `UceClient`:
  - Interface `IUceDispatcher` em `local-api/.../BLL/Boards/UCE/UceDispatcher.cs:7`.
  - Classe `UceDispatcher` em `local-api/.../BLL/Boards/UCE/UceDispatcher.cs:32`.
  - Eventos e metodos sao repassados para `_client` em `UceDispatcher.cs:39` e seguintes.

- `UceClient` cria comandos SDH internamente:
  - `CreateCanConfigCommand` em `local-api/.../BLL/Boards/UCE/UceClient.cs:410`.
  - `CreateCanReadAllCommand` em `local-api/.../BLL/Boards/UCE/UceClient.cs:475`.
  - `ExecuteOperationAsync` chama `BoardTlvDispatcher.TransactAsync` em `UceClient.cs:222`.

- `BoardTlvDispatcher` envia via `SdhClient` e aguarda resposta TLV:
  - Classe em `local-api/.../BLL/Boards/BoardTlvDispatcher.cs:9`.
  - `TransactAsync` em `BoardTlvDispatcher.cs:36`.
  - `OnFrameReceived` filtra resposta por comando compacto e matcher em `BoardTlvDispatcher.cs:78`.

- `SdhClient` valida e mapeia SDH para SDGW:
  - Classe em `local-api/.../DAL/Protocols/SDGW/SdhClient.cs:12`.
  - `SendAsync(SdhCommand...)` valida com `SdhValidator` e mapeia com `SdhToSdgwMapper` em `SdhClient.cs:44`.

- `SdhToSdgwMapper` converte SDH em TLV:
  - Classe em `local-api/.../DAL/Protocols/SDGW/SdhToSdgwMapper.cs:15`.
  - `MapUce` em `SdhToSdgwMapper.cs:132`.
  - `BuildTlvPayload` em `SdhToSdgwMapper.cs:292`.

- `SdgwSession` e `SdGwLinkEngine` transportam:
  - `SdgwSession.OnAppFrameReceived` converte AppFrame em `SdgwFrame` e separa evento por flag em `local-api/.../DAL/Protocols/SDGW/SdgwSession.cs:27`.
  - `SdGwLinkEngine.OnBytesReceived` faz RX COBS/CRC/ACK e entrega AppFrame em `local-api/.../DAL/Protocols/SDGW/SdgwLinkEngine.cs:146`.
  - `SdGwLinkEngine.SendWithSeq` faz TX com ACK/retry/timeout em `SdgwLinkEngine.cs:256`.

- Firmware BPM roteia:
  - `GatewayApp::onCommand` recebe comando SDGW e chama `_router.route(...)` em `hardware/.../BPM.../lib/Gateway/GatewayApp.cpp:34`.
  - `GwRouter::route` escolhe bus e faz transact para a board em `hardware/.../BPM.../lib/GwRouter/GwRouter.cpp:15`.

- Firmware UCE executa:
  - `UceTransport::poll` recebe pacote SPI, parseia TLV e chama `_dispatcher.dispatch(...)` em `hardware/.../UCE.../lib/core/transport/UceTransport.cpp:11`.
  - `UceServiceDispatcher::dispatch` roteia `CMD_CAN_*` para `_sdctp.handleTlv(...)` em `hardware/.../UCE.../lib/core/services/UceServiceDispatcher.cpp:22`.
  - `CanService::handleTlv` executa comandos CAN em `hardware/.../UCE.../lib/services/can/service/CanService.cpp:100`.

Diagnostico do fluxo de controle:

- O fluxo passa por SDH no host/API, mas a UI nao gera objeto/texto SDH diretamente. Ela chama servicos de alto nivel; `UceClient` gera SDH.
- A conversao SDH -> TLV esta centralizada em `SdhToSdgwMapper`.
- Ha ambiguidade: `SdctpApiService` tambem oferece operacoes de controle, entao o nome SDCTP aparece no caminho de controle.

## 3. Fluxo atual de massa de dados CAN

Fluxo real encontrado:

`UCE CAN RX -> CanDriver/CanRxHub/CanRxTableManager -> CanService -> CanCrudProtocol ou CAN_RX_EVENT -> UceServiceDispatcher event queue -> UceTransport -> BPM Gateway pollUceEvent/drainPendingUceEvents -> SDGW event -> SdgwSession.EventReceived -> UceClient.OnEventReceived -> UceParsers -> UceDispatcher events -> ApiCanService.OnCanRxEventReceived/OnCanCrudEventReceived -> CanEventProcessor/CanRxMirrorManager/CanRxOutputBuffer -> FrmUceLogic.TryReadRxFrame -> UI/J1939`

Evidencias principais:

- Firmware UCE coleta massa CAN:
  - `CanService::collectRxFrames` em `hardware/.../UCE.../lib/services/can/service/CanService.cpp:368`.
  - Usa `_rxHub.process(...)`, `_rxTable` e gera eventos CRUD.
  - `CanRxTableManager::processFrame` decide `CMD_CAN_CREATE`, `CMD_CAN_EDIT`, `CMD_CAN_TIC` em `hardware/.../UCE.../lib/services/can/table/CanRxTableManager.cpp:46`.
  - `CanService::checkRxTimeouts` gera `CMD_CAN_DELETE` por timeout.

- Firmware UCE codifica SDCTP/CanCrud:
  - `CanCrudProtocol::encodeCreate`, `encodeEdit`, `encodeTic`, `encodeDelete`, `encodeReadAllDone` em `hardware/.../UCE.../lib/services/can/protocol/CanCrudProtocol.cpp:7`.
  - `SdctpCodec.h` define `typedef CanCrudProtocol SdctpCodec` e `SdctpProtocol` em `hardware/.../UCE.../lib/services/can/sdctp/SdctpCodec.h:7`.

- Firmware UCE publica eventos:
  - `CanService::publishNextCrudEvent` em `hardware/.../UCE.../lib/services/can/service/CanService.cpp:543`.
  - `CanService::publishNextRxEvent` em `CanService.cpp:570`.
  - `UceServiceDispatcher::enqueueEvent` monta evento TLV `[type,len,value]` em `UceServiceDispatcher.cpp`.
  - `UceTransport::drainPendingEvent` monta pacote TLV para SPI.

- BPM drena eventos UCE:
  - `GwRouter::pollUceEvent` em `hardware/.../BPM.../lib/GwRouter/GwRouter.cpp:121`.
  - `GatewayApp::drainPendingUceEvents` envia `SDGW_CMD_UCE_TLV` como evento SDGW em `hardware/.../BPM.../lib/Gateway/GatewayApp.cpp:132`.

- API recebe eventos:
  - `SdgwSession.OnAppFrameReceived` dispara `EventReceived` se flag evento estiver setada em `local-api/.../DAL/Protocols/SDGW/SdgwSession.cs:27`.
  - `UceClient.OnEventReceived` filtra comando UCE, tenta `TryReadLedEvent`, `TryReadCanRxEvent`, `TryReadCanCrudEvent`, `TryReadTransportDiagnosticEvent` em `local-api/.../BLL/Boards/UCE/UceClient.cs:286`.
  - `UceParsers.TryReadCanRxEvent` em `local-api/.../BLL/Boards/UCE/UceParsers.cs:352`.
  - `UceParsers.TryReadCanCrudEvent` em `UceParsers.cs:434`.

- API CanService atualiza massa CAN:
  - `ApiCanService` se inscreve em `IUceDispatcher.CanRxEventReceived` e `CanCrudEventReceived` em `local-api/.../BLL/Services/CAN/ApiCanService.cs:51`.
  - `ApiCanService.OnCanRxEventReceived` processa evento direto e enfileira frames em `ApiCanService.cs:243`.
  - `ApiCanService.OnCanCrudEventReceived` chama `_eventProcessor.ProcessEvent(...)` e, se aplicavel, reconstrói frame da mirror table em `ApiCanService.cs:253`.
  - `CanEventProcessor.ProcessEvent` decodifica `CAN_CREATE`, `CAN_EDIT`, `CAN_DELETE`, `CAN_ROW`, `CAN_READ_ALL_DONE`, `CAN_TIC` em `local-api/.../BLL/Services/CAN/CanEventProcessor.cs:22`.
  - `CanRxMirrorManager.ApplyCreate`, `ApplyEdit`, `ApplyReadAllDone` ficam em `local-api/.../BLL/Services/CAN/CanRxMirrorManager.cs:75`, `:101`, `:247`.
  - `CanRxOutputBuffer.Enqueue` em `local-api/.../BLL/Services/CAN/CanRxOutputBuffer.cs:60`.

- UI consome buffer:
  - `frmUCE_UI.DrainCanRxOutputBuffer` chama `_logic.TryReadRxFrame(out frame)` em `local-api/.../UI/frmUCE_UI.cs:846`.
  - `FrmUceLogic.TryReadRxFrame` chama `_sdctp.TryReadRxFrame(out frame)`.
  - `ApiCanService.TryReadRxFrame` chama `_rxOutputBuffer.TryDequeue(out frame)` em `ApiCanService.cs:181`.
  - `frmUCE_UI.ProcessJ1939Frame` decodifica J1939 a partir de `CanFrameDto` em `frmUCE_UI.cs:860`.

Diagnostico do fluxo de massa:

- A UI nao recebe TLV bruto nem SDGW bruto para massa CAN.
- A massa passa pelo dispatcher, mas e desviada por eventos tipados (`CanRxEventReceived`, `CanCrudEventReceived`) para o `ApiCanService`.
- A logica equivalente ao SDCTP esta formalmente em `BLL/Services/CAN/SDCTP`, mas a implementacao real ainda esta em `ApiCanService`, `CanEventProcessor`, `CanRxMirrorManager`, `CanRxOutputBuffer` e `CanTxManager`.

## 4. Mapa de responsabilidades atual

| Componente | Responsabilidade esperada | Responsabilidade atual encontrada | Esta correto? | Observacao |
|---|---|---|---|---|
| UI UCE | Acionar casos de uso e consumir dados tratados | Chama `FrmUceLogic`; consome `CanFrameDto` via buffer; tambem interpreta alguns status `GwProtocol` e executa exibicao/decodificacao J1939 | Parcial | Nao consome TLV bruto, mas conhece constantes de contrato baixo nivel e concentra parte de exibicao J1939 |
| UceDispatcher API | Roteador/decodificador de comandos/eventos da UCE | `UceDispatcher` repassa `UceClient`; `UceClient` cria SDH, espera respostas e classifica eventos CAN/LED/diag | Parcial | Roteia bem, mas ainda conhece lista detalhada de eventos de massa CAN |
| SDH | Linguagem de configuracao/operacao do hardware | Existe como `SdhCommand`, validator, mapper e client; gerado internamente por `UceClient` | Parcial | Central para comandos, mas contem detalhes CAN TX/SDCTP e nao e gerado pela UI diretamente |
| SDGW | Transporte/gateway API <-> BPM <-> UCE | API: COBS/CRC/ACK/eventos. BPM: roteia TLV para UCE/GSA e retorna eventos | Sim | Nao foi encontrada regra CAN/J1939 na engine SDGW da API; BPM conhece apenas enderecos/bus/TLV |
| CanService API | Dono da massa CAN, buffers, mirror table e protocolo SDCTP | `ApiCanService` recebe eventos do dispatcher, atualiza mirror e `CanRxOutputBuffer`, controla TX | Parcial | Dono real da massa, mas nome oficial SDCTP e fachada; tambem dispara READ_ALL via dispatcher |
| CanEventProcessor | Decodificar eventos SDCTP/CanCrud e aplicar na mirror | Processa `CREATE/EDIT/DELETE/TIC/ROW/READ_ALL_DONE` e aplica em `CanRxMirrorManager` | Sim | Responsabilidade coerente com massa CAN |
| CanRxMirrorManager | Estado interno da tabela espelho CAN RX | Aplica create/edit/delete/tic/row/done, detecta out-of-sync | Sim | Bom isolamento de mirror table |
| CanRxOutputBuffer | Saida oficial de frames CAN para aplicacao/UI | Fila clonada e limitada; recebe frames diretos e reconstruidos | Sim | UI consome por `TryReadRxFrame` |
| UCE firmware dispatcher | Roteador de TLV para servicos da UCE | `UceServiceDispatcher` roteia LED para `LedService` e CAN para `SdctpService` | Parcial | CAN config/enable/status/readAll/TX todos entram no servico SDCTP/CanService |
| UCE CanService | Operar CAN fisico e massa SDCTP | Faz config/enable/status/reset, RX hub, CRUD, direct RX events, TX, readAll | Parcial | Agrega controle de hardware e massa CAN no mesmo servico |
| CanCrudProtocol / equivalente | Codec de compactacao/sincronizacao de massa CAN | Implementa payloads `CREATE/EDIT/TIC/DELETE/ROW/READ_ALL_DONE`; `SdctpCodec` e typedef | Sim | Contrato de massa bem localizado no firmware |
| J1939 services | Decodificar J1939 sobre frames CAN | `J1939ProtocolService` e servicos de diagnostico/rede sao usados por `FrmUceLogic` e UI | Parcial | Nao esta no SDGW; esta na camada de aplicacao/UI logic. Para multiplos protocolos, faltam fronteiras mais genericas |

## 5. Pontos de mistura indevida

### 5.1 SDCTP usado como fachada tambem para comandos de controle CAN

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs`
- Classe: `SdctpApiService`
- Metodo: `SetCanConfigAsync`, `SetCanEnabledAsync`, `GetCanStatusAsync`, `ResetCanAsync`, `PollCanDriverLogAsync`
- Descricao do problema: a classe nomeada como entrypoint SDCTP expoe controle de hardware CAN e delega para `IUceDispatcher`.
- Impacto: embaralha a fronteira conceitual entre SDH (controle/operação) e SDCTP (massa/sincronizacao CAN).
- Sugestao conceitual: manter SDCTP apenas para RX/TX massa, mirror, output buffer, readAll/sync; expor controle CAN por facade de aplicacao/SDH com nome separado.

### 5.2 SDH conhece detalhes de frame CAN TX e DATA_MASK

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhValidator.cs`
- Classe: `SdhValidator`
- Metodo: `ValidateUceCanTx`, `ValidateCanFrameArgs`, `ValidateUceCanTxEdit`
- Descricao do problema: valida flags, DLC, ID, DATA_MASK, periodos, indices e bytes `d0..d7`.
- Impacto: SDH deixa de ser apenas linguagem de operacao e passa a conhecer detalhes de contrato CAN/SDCTP.
- Sugestao conceitual: decidir se TX e comando operacional SDH ou parte do dominio CanService; se for SDH, documentar explicitamente como "controle TX"; se for massa/SDCTP, mover regras de baixo nivel para contrato SDCTP.

### 5.3 Mapper SDH monta TLVs de SDCTP/TX diretamente

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
- Classe: `SdhToSdgwMapper`
- Metodo: `MapUce`, `BuildUceCanTxDirectPayload`, `BuildUceCanTxCreatePayload`, `BuildUceCanTxEditPayload`
- Descricao do problema: mapeador SDH monta payloads CAN TX direct/create/edit/delete e `CAN_READ_ALL`.
- Impacto: acopla SDH ao layout binario CAN/SDCTP.
- Sugestao conceitual: separar comandos de operacao de hardware de payloads de transporte de massa; usar builder/codec dedicado ao dominio CAN quando a montagem for inevitavel.

### 5.4 Dispatcher API classifica eventos de massa CAN

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- Classe: `UceClient`
- Metodo: `OnEventReceived`
- Descricao do problema: o client/dispatcher tenta `TryReadCanRxEvent` e `TryReadCanCrudEvent`, depois publica eventos especificos.
- Impacto: o dispatcher precisa conhecer todos os tipos de massa CAN para desviar corretamente.
- Sugestao conceitual: manter no dispatcher apenas classificacao minima por dominio/protocolo, por exemplo "TLV pertence ao dominio SDCTP", e entregar payload bruto desse dominio ao CanService/SDCTP.

### 5.5 Parser UCE contem parsers detalhados de massa CAN

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs`
- Classe: `UceParsers`
- Metodo: `TryReadCanRxEvent`, `TryReadCanCrudEvent`, `TryReadCanReadAllResponse`
- Descricao do problema: parser de board UCE conhece layout de eventos de massa CAN.
- Impacto: parser do dispatcher cresce junto com protocolos de massa.
- Sugestao conceitual: mover parsers de massa para namespace SDCTP/CanService e deixar `UceParsers` apenas para respostas de controle ou envelope minimo.

### 5.6 UI conhece constantes `GwProtocol`

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- Classe: `frmUCE_UI`
- Metodo: `BtnEnable_Click`, `StopCanTxAsync`, `DescribeDriverEvent`, `FormatDriverEventDetail`
- Descricao do problema: UI compara status de TX e eventos de driver por constantes `GwProtocol`.
- Impacto: UI nao consome TLV bruto, mas conhece contrato de baixo nivel.
- Sugestao conceitual: mover traducao de status/eventos para DTOs ou servico de apresentacao da BLL.

### 5.7 Firmware UCE `CanService` agrega controle e massa

- Arquivo: `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp`
- Classe: `CanService`
- Metodo: `handleTlv`, `collectRxFrames`, `handleReadAll`, `handleTx*`
- Descricao do problema: o mesmo servico trata config/enable/status/reset, RX massa, CRUD, TX direct/table e readAll.
- Impacto: no firmware, a separacao SDH x SDCTP ainda e interna/nominal; a classe concentra controle e massa.
- Sugestao conceitual: em etapa futura, separar "CAN control service" de "SDCTP RX/TX stream/sync service", mantendo os mesmos codigos ate estabilizar.

### 5.8 J1939 acoplado a logica de formulario

- Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- Classe: `FrmUceLogic`
- Metodo: `TryDecodeJ1939Frame`, `TryProcessJ1939NetworkFrame`, `RequestJ1939DiagnosticCodesAsync`
- Descricao do problema: servicos J1939 sao instanciados diretamente na logica da tela UCE.
- Impacto: novos protocolos (K-LINE/J1949) podem entrar na mesma classe e misturar casos de uso.
- Sugestao conceitual: criar uma camada de aplicacao/protocolos automotivos acima do `CanRxOutputBuffer`, sem colocar isso no SDGW nem no dispatcher.

Nao foram encontradas evidencias fortes de:

- UI consumindo TLV bruto diretamente para massa CAN.
- SDGW API contendo regra CAN/J1939.
- CanService API dependendo da UI.

## 6. Contratos e codigos TLV envolvidos

Fonte principal API: `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/GwProtocol.cs`.

Fonte firmware UCE: `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`.

| Codigo | Nome simbolico API | Nome firmware UCE | Quem envia | Quem recebe | Dominio diagnosticado |
|---|---|---|---|---|---|
| `0x12` | `UceSetLedType` | `CMD_LED_BUILTIN` | API/UceClient via SDH | UCE `LedService` | SDH/controle |
| `0x13` | `UceLedEventType` | `CMD_LED_EVENT` | UCE | API/UceClient/UI | Evento controle |
| `0x20` | `UceCanConfigType` | `CMD_CAN_CONFIG` | API via SDH | UCE `CanService::handleConfig` | SDH/controle CAN |
| `0x21` | `UceCanEnableType` | `CMD_CAN_ENABLE` | API via SDH | UCE `CanService::handleEnable` | SDH/controle CAN |
| `0x22` | `UceCanStatusType` | `CMD_CAN_STATUS` | API via SDH | UCE `CanService::handleStatus` | SDH/controle CAN |
| `0x23` | `UceCanResetType` | `CMD_CAN_RESET` | API via SDH | UCE `CanService::handleReset` | SDH/controle CAN |
| `0x24` | `UceCanRxPollType` | `CMD_CAN_RX_POLL` | API via SDH | UCE `CanService::handleRxPoll` | Ambiguo: controle/poll legado de RX |
| `0x25` | `UceCanDriverLogPollType` | `CMD_CAN_DRIVER_LOG_POLL` | API via SDH | UCE `CanService::handleDriverLogPoll` | Diagnostico/controle |
| `0x26` | `UceCanTxType` | `CMD_CAN_TX` | API via SDH | UCE `CanService::handleTx` | Ambiguo/legado TX |
| `0x27` | `UceCanTxStopType` | `CMD_CAN_TX_STOP` | API via SDH | UCE `CanService::handleTxStop` | Controle TX |
| `0x28` | `UceCanRxEventType` | `CMD_CAN_RX_EVENT` | UCE `CanService::publishNextRxEvent` | API `UceClient` -> `ApiCanService` | SDCTP/massa CAN direta |
| `0x40` | `UceCanCreateType` | `CMD_CAN_CREATE` | UCE `CanService`/`CanRxTableManager` | API `CanEventProcessor` | SDCTP/massa CAN |
| `0x41` | `UceCanEditType` | `CMD_CAN_EDIT` | UCE `CanService`/`CanRxTableManager` | API `CanEventProcessor` | SDCTP/massa CAN |
| `0x42` | `UceCanDeleteType` | `CMD_CAN_DELETE` | UCE timeout/table | API `CanEventProcessor` | SDCTP/massa CAN |
| `0x43` | `UceCanReadAllType` | `CMD_CAN_READ_ALL` | API `ApiCanService.RequestReadAllAsync` via dispatcher/SDH | UCE `CanService::handleReadAll` | Ambiguo: comando SDH que dispara sync SDCTP |
| `0x44` | `UceCanRowType` | `CMD_CAN_ROW` | UCE `handleReadAll` | API `CanEventProcessor.ApplyRow` | SDCTP/massa CAN |
| `0x45` | `UceCanReadAllDoneType` | `CMD_CAN_READ_ALL_DONE` | UCE `handleReadAll` | API `CanEventProcessor.ApplyReadAllDone` | SDCTP/massa CAN |
| `0x46` | `UceCanTicType` | `CMD_CAN_TIC` | UCE `CanRxTableManager` | API `CanEventProcessor.ApplyTic` | SDCTP/massa CAN |
| `0x50` | `UceCanTxDirectType` | `CMD_CAN_TX_DIRECT` | API `CanTxManager`/SDH | UCE `CanService::handleTxDirect` | Ambiguo: operacao TX via CAN service |
| `0x51` | `UceCanTxCreateType` | `CMD_CAN_TX_CREATE` | API `CanTxManager`/SDH | UCE `CanService::handleTxCreate` | Ambiguo: tabela TX, controle/SDCTP |
| `0x52` | `UceCanTxEditType` | `CMD_CAN_TX_EDIT` | API `CanTxManager`/SDH | UCE `CanService::handleTxEdit` | Ambiguo: tabela TX, controle/SDCTP |
| `0x53` | `UceCanTxDeleteType` | `CMD_CAN_TX_DELETE` | API `CanTxManager`/SDH | UCE `CanService::handleTxDelete` | Ambiguo: tabela TX, controle/SDCTP |
| `0x7E` | `UceTransportDiagType` | `CMD_TRANSPORT_DIAG` | UCE dispatcher | API `UceClient`/`ApiCanService` | Diagnostico transporte/dispatcher |
| `0x7F` | `UceErrorType` | `CMD_FUNCTIONAL_ERROR` | UCE | API parser | Erro funcional |
| `0xFE` | `GatewayErrorType` | `SDGW_TLV_GATEWAY_ERR` no BPM | BPM gateway | API `UceParsers.TryReadGatewayError` | SDGW/erro gateway |

J1939:

- Nao ha TLV UCE especifico `0x81` encontrado no codigo analisado.
- J1939 e tratado como protocolo decodificado sobre `CanFrameDto` pela API:
  - `J1939ProtocolService.ProcessCanFrame` em `local-api/.../BLL/Protocols/J1939/J1939ProtocolService.cs:23`.
  - `FrmUceLogic.TryDecodeJ1939Frame` chama `_j1939Protocol.ProcessCanFrame(frame)` em `FrmUceLogic.cs:253`.
  - `frmUCE_UI.ProcessJ1939Frame` consome `CanFrameDto`, nao TLV, em `frmUCE_UI.cs:860`.

## 7. Diagnostico SDH

- SDH esta realmente centralizado?
  - Parcialmente. O modelo `SdhCommand`, validacao e mapeamento estao centralizados em `DTL/DAL/Protocols/SDGW`. Entretanto, a geracao dos comandos UCE esta dentro de `UceClient` e os detalhes CAN TX estao no validator/mapper.

- SDH e usado como linguagem de operacao do hardware?
  - Sim para LED, CAN config, enable, status, reset, driverLog, TX e readAll no caminho de comandos da API.

- A UI gera SDH ou pula essa camada?
  - A UI nao gera SDH diretamente. Ela chama `FrmUceLogic`, que chama `SdctpApiService` ou `IUceDispatcher`. O SDH e criado dentro de `UceClient` pelos metodos `Create*Command`.

- O Dispatcher converte SDH para TLV de forma clara?
  - A conversao clara esta em `SdhToSdgwMapper`, nao no `UceDispatcher` em si. O `BoardTlvDispatcher` envia o `SdhCommand` ao `SdhClient`, que valida e mapeia.

- Existem comandos de controle fora do SDH?
  - No caminho API analisado, comandos UCE passam por `UceClient`/SDH. A ambiguidade e de nomenclatura: chamadas de controle passam por `SdctpApiService` antes de chegar ao dispatcher.

## 8. Diagnostico SDTPC

- Existe protocolo formal para massa CAN?
  - Sim. O nome formal encontrado e `SDCTP`, nao `SDTPC`.

- O nome usado no codigo e SDTPC, SDCTP, CanCrudProtocol ou outro?
  - API: `BLL/Services/CAN/SDCTP/*`, `SdctpApiService`, `SdctpProtocol`, `SdctpEventProcessor`, `SdctpRxMirrorManager`.
  - Firmware UCE: `services/can/sdctp/*`, `SdctpService`, `SdctpCodec`, `SdctpProtocol`.
  - Implementacao historica/subjacente: `ApiCanService`, `CanEventProcessor`, `CanRxMirrorManager`, `CanRxOutputBuffer`, `CanTxManager`, `CanCrudProtocol`, `CanService`.

- O protocolo esta isolado no CanService?
  - Parcialmente. A massa RX esta no `ApiCanService`/`CanEventProcessor`/`CanRxMirrorManager`/`CanRxOutputBuffer`; a fachada oficial `SdctpApiService` envolve isso. No firmware, `SdctpService` e wrapper de `CanService`.

- Eventos CAN sao desviados corretamente pelo Dispatcher para o CanService?
  - Sim em termos funcionais: `UceClient` identifica eventos e `UceDispatcher` repassa; `ApiCanService` assina eventos do dispatcher. Mas o dispatcher ainda conhece tipos e parsing de massa CAN.

- A aplicacao consome buffer RX ou consome TLV bruto?
  - Consome buffer RX. `frmUCE_UI.DrainCanRxOutputBuffer` chama `_logic.TryReadRxFrame`, que chega ao `CanRxOutputBuffer.TryDequeue`.

## 9. Diagnostico SDGW

- SDGW esta puro como gateway/transporte?
  - API: sim. `SdGwLinkEngine` trata framing, COBS, CRC, ACK/ERR, sequencia, retries e entrega de frames.
  - BPM: majoritariamente sim. `GatewayApp` e `GwRouter` roteiam por endereco/bus e validam TLV; nao foi encontrada regra CAN/J1939 no gateway.

- SDGW tem conhecimento indevido de CAN/J1939?
  - Nao na API. No BPM, ha conhecimento de endereco UCE/GSA e diagnostico de erro SPI, mas nao de semantica CAN/J1939.

- SDGW depende de SDH ou SDCTP alem do transporte bruto?
  - `SdGwLinkEngine` nao. `SdgwSession` tambem nao. A dependencia semantica esta acima, em `SdhClient`, `UceClient` e `BoardTlvDispatcher`.

## 10. Preparacao para protocolos futuros

K-LINE:

- A arquitetura de transporte SDGW poderia carregar TLVs de K-LINE sem mudancas conceituais no link.
- O risco esta em adicionar K-LINE em `GwProtocol`, `UceParsers`, `UceClient`, `FrmUceLogic` e `CanService` sem criar um servico/protocolo separado.

J1939:

- Ja existe decodificacao J1939 sobre `CanFrameDto`, e isso e positivo porque nao polui SDGW.
- A decodificacao esta em `FrmUceLogic`/UI logic; para escalar, deveria ser um consumidor separado do `CanRxOutputBuffer`.

J1949:

- Nao foi encontrado contrato especifico J1949 no codigo analisado.
- Se for sobre CAN, deve consumir `CanFrameDto` do buffer como J1939. Se exigir transporte proprio, precisa de servico separado.

Outros protocolos automotivos:

- O SDGW esta preparado como transporte generico.
- A camada de aplicacao ainda nao tem uma abstracao generica clara para "protocolos consumidores de fluxo CAN" versus "servicos de controle de hardware".
- Recomendacao conceitual: manter quatro eixos separados: comando/controle (SDH), transporte/gateway (SDGW), massa CAN (SDCTP), decoders automotivos (J1939/K-LINE/J1949/etc.).

## 11. Proposta de ETAPAS de correcao

Nao implementar nesta etapa.

ETAPA 01 - Congelar contratos atuais e documentar nomes oficiais

- Decidir oficialmente entre `SDCTP` e `SDTPC`.
- Documentar que `CanCrudProtocol` e codec interno do protocolo oficial, se essa for a decisao.
- Documentar quais codigos TLV pertencem a SDH/controle, SDCTP/massa, SDGW/erro.

ETAPA 02 - Formalizar separacao SDH x SDCTP x SDGW

- SDH: comandos operacionais.
- SDCTP: massa CAN RX/TX/sync.
- SDGW: transporte.
- Evitar nomes de fachada que misturem controle com massa.

ETAPA 03 - Corrigir fluxo de comandos da UI para SDH

- Manter UI chamando casos de uso, mas garantir que comandos de controle aparecam em facade de aplicacao/SDH, nao em `SdctpApiService`.

ETAPA 04 - Corrigir desvio de massa CAN para CanService

- Reduzir conhecimento de massa CAN no `UceClient`/`UceParsers`.
- Dispatcher deve rotear por dominio; CanService/SDCTP deve decodificar payloads de massa.

ETAPA 05 - Isolar SDCTP dentro do CanService

- Mover parsers/constantes de massa para namespace SDCTP.
- Transformar `ApiCanService` em implementacao interna ou renomear apenas em etapa planejada.

ETAPA 06 - Garantir que UI consuma apenas CanRxOutputBuffer

- Remover dependencias diretas da UI para `GwProtocol` quando forem apenas display/status.
- Fornecer DTOs de apresentacao para TX status e driver log.

ETAPA 07 - Preparar abstracao para K-LINE/J1939/J1949

- Criar consumidores de frames/eventos separados da tela.
- J1939 deve continuar sobre `CanFrameDto`, sem passar por SDGW/TLV.

ETAPA 08 - Criar testes/harness de validacao arquitetural

- Testes de mapeamento SDH -> TLV.
- Testes de roteamento de eventos SDCTP -> mirror/output buffer.
- Testes de que UI nao referencia SDGW/TLV bruto.
- Testes de que SDGW nao referencia protocolos de aplicacao.

## 12. Arquivos analisados

API C#:

- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/GwProtocol.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdhCommand.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhValidator.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwSession.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwLinkEngine.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwFrameCodec.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BoardTlvDispatcher.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanEventProcessor.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxMirrorManager.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxOutputBuffer.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanTxManager.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpProtocol.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpRxMirrorManager.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpRxOutputBuffer.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/ProtocolDecoderGateway.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/J1939ProtocolService.cs`
- Arquivos J1939 sob `BLL/Protocols/J1939/*` e DTOs J1939 sob `DTL/Protocols/J1939/*` por busca/simbolos.
- DTOs CAN sob `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/*`.
- DTOs UCE sob `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/*`.

Firmware UCE:

- `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/protocol/CanCrudProtocol.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/protocol/CanCrudProtocol.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanRxTableManager.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanRxTableManager.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanTxTableManager.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanTxTableManager.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/rxhub/CanRxHub.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/rxhub/CanRxHub.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpService.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpCodec.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpRxTableManager.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpTxTableManager.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpTypes.h`

Firmware BPM:

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwLink/SdgwLink.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwLink/SdgwLink.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwParser/SdgwParser.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwParser/SdgwParser.h`

## 13. Conclusao

O que esta correto:

- SDGW na API esta bem separado como transporte/frame/ACK/evento.
- SDH existe como camada formal de comando no host/API.
- Massa CAN chega na UI por `CanRxOutputBuffer`, nao por TLV bruto.
- `CanEventProcessor`, `CanRxMirrorManager` e `CanRxOutputBuffer` formam um nucleo coerente para massa CAN na API.
- Firmware BPM atua majoritariamente como gateway, sem regra CAN/J1939 aparente.
- J1939 e decodificado a partir de `CanFrameDto`, nao dentro do SDGW.

O que esta errado ou desalinhado com o paradigma desejado:

- `SdctpApiService` esta sendo usado tambem como facade de controle CAN.
- `SdhValidator` e `SdhToSdgwMapper` conhecem detalhes profundos de payload CAN/SDCTP.
- `UceClient`/`UceParsers` conhecem e parseiam eventos de massa CAN em vez de apenas rotear para o dominio SDCTP/CanService.
- Firmware UCE `CanService` mistura controle de hardware CAN e massa SDCTP na mesma classe.
- UI conhece constantes `GwProtocol` em alguns pontos de status/log.

O que esta ambiguo:

- Se `CAN_TX_DIRECT`, `CAN_TX_CREATE`, `CAN_TX_EDIT`, `CAN_TX_DELETE` devem ser SDH/controle operacional ou SDCTP/TX table. O codigo atual trata como comando SDH que passa por CanService.
- Se `CAN_READ_ALL` deve ser considerado comando SDH de solicitacao ou parte integral do protocolo SDCTP. Atualmente e um comando SDH que dispara eventos SDCTP.
- Se o nome oficial deve ser `SDCTP` ou `SDTPC`.
- Se a UI logic deve continuar sendo dona da decodificacao J1939 ou se deve existir uma camada de consumidores de protocolo acima do buffer CAN.

O que precisa ser decidido antes de alterar codigo:

- Nome oficial do protocolo de massa CAN: `SDCTP` versus `SDTPC`.
- Lista oficial de TLVs pertencentes a SDH, SDCTP e SDGW.
- Fronteira de TX CAN: comando de operacao SDH ou parte de SDCTP.
- Fronteira do dispatcher: parser detalhado de eventos CAN ou roteador por dominio.
- Fronteira de protocolos futuros: onde entram K-LINE, J1939 e J1949 sem acoplar UI, dispatcher e transporte.
