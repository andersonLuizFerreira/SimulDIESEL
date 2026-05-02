# Mapa de Estados CAN RX - ETAPA 10

Repositorio: `C:\PROJETOS\SimulDIESEL`  
Branch analisada: `Construcao_CanService`  
Base: implementacao atual em `BpmSerialService`, `SdgwHostSession`, `UceClient`, `UceDispatcher`, `CanService` UCE, `CanRxTableManager`, `ApiCanService`, `CanRxMirrorManager` e `frmUCE_UI`.

Arquitetura CAN RX observada:

```text
CanDriver (UCE)
-> CanService (UCE)
-> UceServiceDispatcher
-> UceClient / UceDispatcher
-> ApiCanService
-> CanRxMirrorManager
-> frmUCE_UI
```

## 1. Lista de Estados Globais

Estados de transporte e link:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `TRANSPORT_DISCONNECTED` | `SdgwHostSession.SessionState.Disconnected`, `BpmSerialService.LinkState.Disconnected` | Transporte fechado ou sessao desconectada. |
| `TRANSPORT_CONNECTED` | `SdgwHostSession.SessionState.TransportConnected` | Transporte aberto, link SDGW ainda nao estabelecido. |
| `UCE_LINK_DRAINING` | `SdgwHostSession.SessionState.Draining` | Janela de drenagem antes do banner da API. |
| `UCE_BANNER_SENT` | `SdgwHostSession.SessionState.BannerSent` | Banner `SIMULDIESELAPI` enviado. |
| `UCE_LINKED` | `SdgwHostSession.SessionState.Linked`, `BpmSerialService.IsLinked` | Sessao SDGW estabelecida e supervisor ativo. |
| `UCE_LINK_FAILED` | `SdgwHostSession.SessionState.LinkFailed` | Handshake ou supervisao falhou; loop tenta nova conexao. |

Estados CAN:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `CAN_DISABLED` | `UceCanInterfaceState.Disabled`, `CAN_INTERFACE_DISABLED` | Porta CAN desabilitada. |
| `CAN_CONFIGURED` | `UceCanInterfaceState.Configured`, `CAN_INTERFACE_CONFIGURED` | Porta configurada, ainda nao aberta. |
| `CAN_OPEN` | `UceCanInterfaceState.Open`, `CAN_INTERFACE_OPEN` | Porta CAN aberta no driver. |
| `CAN_FAULT` | `UceCanInterfaceState.Fault` | Estado aceito no protocolo da API; no firmware atual o `CanService` retorna erro quando operacoes do driver falham. |

Estados do fluxo RX:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `RX_INACTIVE` | `CanService.collectRxFrames()` ignora controllers nao `CAN_INTERFACE_OPEN` | Sem coleta RX ativa. |
| `RX_POLLING_DRIVER` | `CanService.collectRxFrames()` chama `_driver.pollReceived(...)` | Coleta periodica no loop da UCE. |
| `RX_FRAME_QUEUED` | `enqueueRxFrame(...)` | Frame recebido tambem entra na fila legada/poll. |
| `RX_CRUD_PENDING` | `enqueueCrudEvent(...)` | Evento CRUD codificado aguarda publicacao assíncrona. |
| `RX_CRUD_PUBLISHED` | `publishNextCrudEvent()` | Evento entregue ao dispatcher UCE. |
| `RX_NO_CHANGE` | `CanRxTableManager::ProcessNoChange` | Frame repetido sem alteracao de DLC/dados. |
| `RX_TABLE_FULL` | `CanRxTableManager::ProcessTableFull` | Nova identidade sem linha livre na tabela UCE. |

Estados da tabela UCE:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `UCE_TABLE_EMPTY` | `CanRxTableManager::reset()` | 20 linhas invalidas. |
| `UCE_TABLE_PARTIALLY_FILLED` | `processFrame()` cria entradas enquanto ha linhas livres | Uma a 19 linhas validas. |
| `UCE_TABLE_FULL` | `findFirstFree()` nao encontra linha livre | 20 linhas validas; novas identidades sao ignoradas com `ProcessTableFull`. |
| `UCE_TABLE_UPDATING` | `ProcessCreate` ou `ProcessEdit` | Linha criada ou alterada por frame RX. |
| `UCE_TABLE_STABLE` | `ProcessNoChange` ou ausencia de frames | Tabela sem mutacao no ciclo atual. |
| `UCE_TABLE_SNAPSHOTTING` | `_readAllSnapshotActive` em `handleReadAll()`/`publishNextCrudEvent()` | Snapshot administrativo `CAN_READ_ALL`; durante ele `collectRxFrames()` retorna sem coletar. |

Estados da tabela espelho API:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `MIRROR_EMPTY` | construtor/`ClearRows()` de `CanRxMirrorManager` | 20 linhas existem, todas invalidas. |
| `MIRROR_POPULATING` | `ApplyCreate(...)` | `CAN_CREATE 0x40` marca linha valida e copia dados. |
| `MIRROR_UPDATING` | `ApplyEdit(...)` | `CAN_EDIT 0x41` altera campos por mascara. |
| `MIRROR_STABLE` | snapshot sem alteracao ou evento ignorado | Estado persistente entre eventos. |
| `MIRROR_SYNCING_READ_ALL` | `StartReadAll()`/`IsSyncingReadAll` | Sincronizacao administrativa por `CAN_READ_ALL`; CREATE/EDIT sao ignorados enquanto ativa. |
| `MIRROR_OUT_OF_SYNC_POSSIBLE` | inferido de eventos perdidos, fila cheia, edit antes de create, ausencia de delete/timeout | Nao ha flag formal de out-of-sync; e um risco operacional observavel pelo comportamento. |

Estados da UI:

| Estado | Origem no codigo | Observacao |
|---|---|---|
| `UI_CLOSED` | formulario inexistente/fechado; `OnFormClosed` remove assinaturas | Nao renderiza nem controla a tabela espelho. |
| `UI_OPEN_INITIALIZING` | construtor e `FrmUCE_UI_Load` | Configura grid, assina eventos, consulta status CAN e snapshot. |
| `UI_OPEN_NO_DATA` | `RefreshCanRxGrid()` com linhas invalidas | Grid renderiza 20 linhas, sem dados validos. |
| `UI_OPEN_WITH_DATA` | `RefreshCanRxGrid()` com uma ou mais linhas validas | Grid mostra snapshot da tabela espelho. |
| `UI_UPDATING` | `Logic_CanRxTableChanged` | Atualizacao disparada pelo evento do `ApiCanService`; usa `BeginInvoke` se necessario. |
| `UI_STATUS_ERROR` | `RefreshCanStatusAsync()` sem resposta valida | Label mostra erro; a UI nao altera CAN por isso. |

## 2. Definicao de Cada Estado

| Estado | Camada responsavel | Condicao de entrada | Condicao de saida | Ativo no estado | Eventos possiveis |
|---|---|---|---|---|---|
| `TRANSPORT_DISCONNECTED` | API/Transporte | Estado inicial, `Disconnect()`, queda do transporte | `Connect(...)` serial/bluetooth bem-sucedido | Transporte fechado, scheduler indisponivel | `ConnectTransport` |
| `TRANSPORT_CONNECTED` | API/Transporte | `OnTransportConnectionChanged(true)` | Tick do link inicia drenagem ou `Disconnect()` | Transporte aberto, scheduler disponivel | `StartLinkLoop`, `DisconnectTransport` |
| `UCE_LINK_DRAINING` | API/Sessao SDGW | Timer inicia tentativa de handshake | Fim da janela de drenagem | Buffer RX limpo, aguardando janela | `DrainElapsed`, `DisconnectTransport`, timeout posterior |
| `UCE_BANNER_SENT` | API/Sessao SDGW | Banner API escrito no transporte | Parser identifica interface ou timeout | Aguardando resposta textual da interface | `InterfaceInfoReceived`, `HandshakeTimeout`, `DisconnectTransport` |
| `UCE_LINKED` | API/Sessao SDGW | `BpmParsers.TryParseInterfaceInfo(...)` com sucesso | `Disconnect()`, transporte cai, supervisor falha | `SdgwSession`, `SdhClient`, `UceClient`, `UceDispatcher`, `ApiCanService` | `OpenCAN`, `CloseCAN`, `CAN_STATUS`, eventos SDGW |
| `UCE_LINK_FAILED` | API/Sessao SDGW | Timeout de handshake ou supervisor indica link morto | Proxima tentativa de handshake ou disconnect | Loop de reconexao habilitado | `RetryLink`, `DisconnectTransport` |
| `CAN_DISABLED` | UCE/CanService | `begin()`, `resetPort()`, `handleEnable(off)`, `handleReset()` | `handleConfig()` ou `handleEnable(on)` | Driver fechado/desabilitado, tabela UCE resetada nos fluxos enable/reset | `ConfigureCAN`, `OpenCAN`, `ResetCAN`, `StatusCAN` |
| `CAN_CONFIGURED` | UCE/CanService | `handleConfig()` com sucesso quando nao esta aberta | `handleEnable(on)`, reset, enable off | Bitrate e modo definidos, CAN nao aberta | `OpenCAN`, `ConfigureCAN`, `ResetCAN`, `StatusCAN` |
| `CAN_OPEN` | UCE/CanDriver/CanService | `handleEnable(on)` e `_driver.open()` com sucesso | `handleEnable(off)`, reset, falha operacional | Driver aberto; `collectRxFrames()` passa a consultar RX | `RX_FRAME`, `CloseCAN`, `ResetCAN`, `CAN_TX`, `StatusCAN` |
| `CAN_FAULT` | Protocolo API/UCE | Codigo 0x03 aceito por parser de status | Novo status/config/reset conforme UCE responder | UI consegue exibir "falha" se status retornar 0x03 | `StatusCAN`, `ResetCAN`, `ConfigureCAN` |
| `RX_INACTIVE` | UCE/CanService | CAN nao esta `CAN_INTERFACE_OPEN` ou snapshot `READ_ALL` ativo | CAN aberta e snapshot inativo | Loop roda, mas nao coleta frames para controllers fechados | `OpenCAN`, `ReadAllDone` |
| `RX_POLLING_DRIVER` | UCE/CanService | `loop()` chama `collectRxFrames()` com CAN aberta | Fim do ciclo de poll | Driver consultado por ate 3 frames por ciclo | `RX_FRAME`, `NoFrames`, `DriverPollFail` |
| `RX_FRAME_QUEUED` | UCE/CanService | `_driver.pollReceived()` retorna frame | Consumo por `PollCanRx` legado ou permanencia na fila | Fila RX interna | `RX_POLL_REQUEST`, overflow de fila |
| `RX_CRUD_PENDING` | UCE/CanService | `CanRxTableManager` gera create/edit e `enqueueCrudEvent()` aceita | `publishNextCrudEvent()` entrega | Fila CRUD interna | `PublishAsyncEvent`, overflow de fila |
| `RX_CRUD_PUBLISHED` | UCE/Dispatcher/API | `publishAsyncEvent(...)` aceita evento | API processa `CanCrudEventReceived` | Evento assíncrono em transito para API | `CAN_CREATE`, `CAN_EDIT`, `CAN_ROW`, `CAN_READ_ALL_DONE` |
| `UCE_TABLE_EMPTY` | UCE/CanRxTableManager | Construtor, `reset()`, enable on/off, reset CAN | Primeiro frame novo valido | 20 entradas invalidas, `messageOrder` reiniciado | `RX_FRAME_NEW_ID` |
| `UCE_TABLE_PARTIALLY_FILLED` | UCE/CanRxTableManager | `ProcessCreate` com 1 a 19 linhas validas | Nova identidade ate 20, reset, edit/no-change | Entradas validas e livres | `CAN_CREATE`, `CAN_EDIT`, `RX_NO_CHANGE` |
| `UCE_TABLE_FULL` | UCE/CanRxTableManager | 20 entradas validas | Reset/enable/reset CAN | Todas as linhas ocupadas | `ProcessTableFull`, `CAN_EDIT` para identidades existentes |
| `UCE_TABLE_UPDATING` | UCE/CanRxTableManager | Frame novo ou alterado | Evento CRUD codificado | Linha sendo criada/editada | `CAN_CREATE`, `CAN_EDIT` |
| `UCE_TABLE_STABLE` | UCE/CanRxTableManager | Sem frame novo/alterado ou `ProcessNoChange` | Novo frame que cria/edita | Tabela preservada | `RX_FRAME`, `ResetCAN`, `ReadAll` |
| `UCE_TABLE_SNAPSHOTTING` | UCE/CanService | `CMD_CAN_READ_ALL` | Fila snapshot esvazia e `CAN_READ_ALL_DONE` publica | Coleta RX pausada; eventos `CAN_ROW`/DONE pendentes | `CAN_ROW 0x44`, `CAN_READ_ALL_DONE 0x45` |
| `MIRROR_EMPTY` | API/CanRxMirrorManager | Construtor ou `StartReadAll()`/`ClearRows()` | `ApplyCreate()` ou `ApplyRow()` | 20 DTOs, `Valid=false` | `CAN_CREATE`, `CAN_ROW` |
| `MIRROR_POPULATING` | API/CanRxMirrorManager | `ApplyCreate()` valido | Retorno para estavel | Linha marcada valida | `CanRxTableChanged` |
| `MIRROR_UPDATING` | API/CanRxMirrorManager | `ApplyEdit()` valido em linha ja criada | Retorno para estavel | Campos alterados por mascara | `CanRxTableChanged` |
| `MIRROR_STABLE` | API/CanRxMirrorManager | Apos create/edit/row/done ou ausencia de evento | Proximo evento | Snapshot consultavel por copia | `GetSnapshot`, `CAN_CREATE`, `CAN_EDIT`, `UI_OPEN` |
| `MIRROR_SYNCING_READ_ALL` | API/CanRxMirrorManager | `ApiCanService.RequestReadAllAsync()` chama `StartReadAll()` | `ApplyReadAllDone()` ou `CancelReadAll()` | CREATE/EDIT ignorados; rows de snapshot podem ser aplicadas | `CAN_ROW`, `CAN_READ_ALL_DONE`, falha de request |
| `MIRROR_OUT_OF_SYNC_POSSIBLE` | API/Mirror | Evento perdido, CRUD queue cheia, edit antes de create, delete ausente | `READ_ALL` administrativo ou reinicio/novo fluxo | Nao ha indicador formal | `RequestReadAllAsync` administrativo |
| `UI_CLOSED` | UI | Antes de abrir ou apos `OnFormClosed` | `frmUCE_UI.Instance` cria formulario | Sem assinatura da tela; servico API permanece ativo | `UI_OPEN`, eventos API sem consumidor UI |
| `UI_OPEN_INITIALIZING` | UI | Construtor e Load | Fim de `RefreshCanStatusAsync()`/`RefreshCanRxGrid()` | Grid configurado, logic criada, eventos assinados | `StatusCAN`, `GetSnapshot` |
| `UI_OPEN_NO_DATA` | UI | Snapshot sem linhas validas | Mirror recebe create/row | Grid com 20 linhas invalidas | `CanRxTableChanged`, `UI_CLOSE`, `StatusCAN` |
| `UI_OPEN_WITH_DATA` | UI | Snapshot com linhas validas | Proxima alteracao ou fechamento | Grid mostra dados da tabela espelho | `CanRxTableChanged`, `UI_CLOSE`, `StatusCAN` |
| `UI_UPDATING` | UI | Evento `CanRxTableChanged` | Fim de `RefreshCanRxGrid()` | `BeginInvoke` se necessario; re-render do snapshot | `GetSnapshot` |
| `UI_STATUS_ERROR` | UI | `GetCanStatusAsync()` falha | Nova consulta de status com sucesso ou fechamento | Label de erro; grid pode continuar mostrando snapshot | `StatusCAN`, `UI_CLOSE` |

## 3. Transicoes de Estado

### 3.1 Transporte e UCE

```text
TRANSPORT_DISCONNECTED -> ConnectTransport -> TRANSPORT_CONNECTED
TRANSPORT_CONNECTED -> StartLinkLoop -> UCE_LINK_DRAINING
UCE_LINK_DRAINING -> DrainElapsed / SendApiBanner -> UCE_BANNER_SENT
UCE_BANNER_SENT -> InterfaceInfoReceived -> UCE_LINKED
UCE_BANNER_SENT -> HandshakeTimeout -> UCE_LINK_FAILED
UCE_LINK_DRAINING -> HandshakeTimeout -> UCE_LINK_FAILED
UCE_LINK_FAILED -> RetryIntervalElapsed -> UCE_LINK_DRAINING
UCE_LINKED -> LinkSupervisorDead -> UCE_LINK_FAILED
UCE_LINKED -> DisconnectTransport -> TRANSPORT_DISCONNECTED
UCE_LINK_FAILED -> DisconnectTransport -> TRANSPORT_DISCONNECTED
TRANSPORT_CONNECTED -> DisconnectTransport -> TRANSPORT_DISCONNECTED
```

### 3.2 CAN

```text
UCE_LINKED -> CAN_STATUS -> CAN_DISABLED | CAN_CONFIGURED | CAN_OPEN | CAN_FAULT
CAN_DISABLED -> ConfigureCAN -> CAN_CONFIGURED
CAN_CONFIGURED -> ConfigureCAN -> CAN_CONFIGURED
CAN_OPEN -> ConfigureCAN -> CAN_OPEN
CAN_DISABLED -> OpenCAN / handleEnable(on) -> CAN_OPEN
CAN_CONFIGURED -> OpenCAN / handleEnable(on) -> CAN_OPEN
CAN_OPEN -> CloseCAN / handleEnable(off) -> CAN_DISABLED
CAN_OPEN -> ResetCAN -> CAN_DISABLED
CAN_CONFIGURED -> ResetCAN -> CAN_DISABLED
CAN_DISABLED -> ResetCAN -> CAN_DISABLED
CAN_DISABLED | CAN_CONFIGURED | CAN_OPEN -> DriverOperationFail -> operacao retorna erro; estado efetivo depende do ultimo status aceito
```

Observacao: o protocolo C# reconhece `CAN_FAULT`, mas no firmware analisado as falhas de driver em config/enable/status/reset retornam erro funcional em vez de setar explicitamente `CAN_INTERFACE_FAULT` no `PortState`.

### 3.3 Fluxo RX UCE

```text
CAN_DISABLED -> LoopCanService -> RX_INACTIVE
CAN_CONFIGURED -> LoopCanService -> RX_INACTIVE
CAN_OPEN -> LoopCanService -> RX_POLLING_DRIVER
RX_POLLING_DRIVER -> NoFrames -> UCE_TABLE_STABLE
RX_POLLING_DRIVER -> RX_FRAME(new identity) -> UCE_TABLE_UPDATING -> RX_CRUD_PENDING
RX_POLLING_DRIVER -> RX_FRAME(existing changed) -> UCE_TABLE_UPDATING -> RX_CRUD_PENDING
RX_POLLING_DRIVER -> RX_FRAME(existing unchanged) -> RX_NO_CHANGE -> UCE_TABLE_STABLE
RX_POLLING_DRIVER -> RX_FRAME(new identity, table full) -> RX_TABLE_FULL -> UCE_TABLE_FULL
RX_CRUD_PENDING -> publishNextCrudEvent succeeds -> RX_CRUD_PUBLISHED
RX_CRUD_PENDING -> publishNextCrudEvent fails because dispatcher has pending event -> RX_CRUD_PENDING
RX_CRUD_PUBLISHED -> takePendingEvent / SDGW event -> API receives CRUD event
```

### 3.4 Tabela UCE

```text
CanService.begin -> UCE_TABLE_EMPTY
CAN_DISABLED -> OpenCAN successful -> UCE_TABLE_EMPTY
CAN_OPEN -> CloseCAN successful -> UCE_TABLE_EMPTY
Any CAN state -> ResetCAN successful -> UCE_TABLE_EMPTY
UCE_TABLE_EMPTY -> first new RX identity -> UCE_TABLE_PARTIALLY_FILLED
UCE_TABLE_PARTIALLY_FILLED -> new RX identity and free slot -> UCE_TABLE_PARTIALLY_FILLED
UCE_TABLE_PARTIALLY_FILLED -> twentieth valid identity -> UCE_TABLE_FULL
UCE_TABLE_PARTIALLY_FILLED | UCE_TABLE_FULL -> existing identity changed -> UCE_TABLE_UPDATING -> previous occupancy state
UCE_TABLE_PARTIALLY_FILLED | UCE_TABLE_FULL -> existing identity unchanged -> UCE_TABLE_STABLE
Any UCE table state -> CAN_READ_ALL -> UCE_TABLE_SNAPSHOTTING -> previous table contents preserved
```

### 3.5 Espelho API

```text
ApiCanService constructed -> MIRROR_EMPTY
MIRROR_EMPTY -> CAN_CREATE 0x40 -> MIRROR_POPULATING -> MIRROR_STABLE
MIRROR_STABLE -> CAN_CREATE 0x40 -> MIRROR_POPULATING -> MIRROR_STABLE
MIRROR_STABLE -> CAN_EDIT 0x41 for valid row -> MIRROR_UPDATING -> MIRROR_STABLE
MIRROR_STABLE -> CAN_EDIT 0x41 for invalid row -> MIRROR_STABLE (evento ignorado)
MIRROR_STABLE -> RequestReadAllAsync -> MIRROR_SYNCING_READ_ALL
MIRROR_SYNCING_READ_ALL -> CAN_ROW 0x44 -> MIRROR_SYNCING_READ_ALL
MIRROR_SYNCING_READ_ALL -> CAN_READ_ALL_DONE 0x45 -> MIRROR_STABLE
MIRROR_SYNCING_READ_ALL -> request failure -> MIRROR_STABLE
MIRROR_STABLE -> lost event / queue overflow / edit before create -> MIRROR_OUT_OF_SYNC_POSSIBLE
```

### 3.6 UI

```text
UI_CLOSED -> UI_OPEN -> UI_OPEN_INITIALIZING
UI_OPEN_INITIALIZING -> GetSnapshot with no valid rows -> UI_OPEN_NO_DATA
UI_OPEN_INITIALIZING -> GetSnapshot with valid rows -> UI_OPEN_WITH_DATA
UI_OPEN_NO_DATA -> CanRxTableChanged with valid row -> UI_UPDATING -> UI_OPEN_WITH_DATA
UI_OPEN_WITH_DATA -> CanRxTableChanged -> UI_UPDATING -> UI_OPEN_WITH_DATA
UI_OPEN_* -> GetCanStatusAsync failure -> UI_STATUS_ERROR
UI_STATUS_ERROR -> GetCanStatusAsync success -> UI_OPEN_NO_DATA | UI_OPEN_WITH_DATA
UI_OPEN_* -> UI_CLOSE -> UI_CLOSED
```

## 4. Fluxo Completo do Ciclo CAN

1. Conexao com a UCE  
   `FrmBpmLogic`/UI de porta chama `BpmSerialService.Connect(...)` ou fluxo Bluetooth. O `SwitchableTransport` abre o transporte e `SdgwHostSession.OnTransportConnectionChanged(true)` entra em `TRANSPORT_CONNECTED`.

2. Estabelecimento do link  
   `SdgwHostSession` inicia timer de handshake, passa por `UCE_LINK_DRAINING`, envia o banner `SIMULDIESELAPI`, aguarda informacao de interface e entra em `UCE_LINKED`. Nesse estado `BpmSerialService.IsLinked` passa a verdadeiro.

3. Abertura da CAN  
   `frmUCE_UI` altera `chkCanEnabled`; `FrmUceLogic.SetCanEnabledAsync()` chama `UceDispatcher.SetCanEnabledAsync()`, que chama `UceClient`. O comando SDH vira TLV `CMD_CAN_ENABLE`. Na UCE, `CanService.handleEnable(on)` chama `_driver.open(controller)`, marca `interfaceState = CAN_INTERFACE_OPEN` e reseta tabela/fila.

4. Geracao de RX  
   No loop da UCE, `UceServiceDispatcher.loop()` chama `CanService.loop()`. Se a porta esta aberta e nao existe snapshot `READ_ALL` ativo, `collectRxFrames()` consulta `_driver.pollReceived(...)`.

5. CREATE/EDIT  
   Cada frame recebido vira `ObservedFrame` e passa por `CanRxTableManager.processFrame(...)`. Uma identidade nova gera `CMD_CAN_CREATE 0x40`; uma identidade existente com DLC/dados alterados gera `CMD_CAN_EDIT 0x41`; frame sem alteracao gera `ProcessNoChange`; tabela cheia para nova identidade gera `ProcessTableFull`.

6. Espelhamento na API  
   O evento CRUD e codificado por `CanCrudProtocol`, entra em `_crudEventQueue` e e publicado por `publishNextCrudEvent()`. `UceServiceDispatcher.publishAsyncEvent()` guarda um evento pendente. No lado API, `UceClient.OnEventReceived()` parseia via `TryReadCanCrudEvent()` e dispara `CanCrudEventReceived`; `ApiCanService.OnCanCrudEventReceived()` processa no `CanEventProcessor`; `CanRxMirrorManager.ApplyCreate/ApplyEdit` atualiza o espelho.

7. Renderizacao na UI  
   `frmUCE_UI` cria/configura `dgCanRx`, consulta `FrmUceLogic.GetCanRxMirrorRows()`, que retorna snapshot do `ApiCanService`. Em `CanRxTableChanged`, a tela usa `BeginInvoke` se necessario e chama `RefreshCanRxGrid()`.

8. Fechamento da UI  
   `frmUCE_UI.OnFormClosed()` remove assinaturas da tela, para timers e descarta `FrmUceLogic`. A instancia persistente de `ApiCanService` pertence a `BpmSerialService`, portanto nao e fechada pela UI.

9. Continuidade do servico  
   Com a UI fechada, `UceDispatcher` e `ApiCanService` continuam assinados aos eventos da sessao enquanto `BpmSerialService` estiver vivo. Eventos `CAN_CREATE`/`CAN_EDIT` continuam atualizando `CanRxMirrorManager`.

10. Fechamento da CAN  
    `SetCanEnabledAsync(false)` envia `CMD_CAN_ENABLE` com estado off. Na UCE, `CanService.handleEnable(off)` chama `_driver.close(controller)`, marca `CAN_INTERFACE_DISABLED` e reseta tabela/fila UCE. Nao ha limpeza automatica correspondente da tabela espelho API nesse fluxo.

11. Desconexao total  
    `BpmSerialService.Disconnect()` chama `_session.Disconnect()`, que desabilita supervisor, limpa handshake, derruba engine/scheduler e desconecta transporte. O estado volta para `TRANSPORT_DISCONNECTED`.

## 5. Estado da Tabela UCE

| Estado | Quando ocorre | Comportamento |
|---|---|---|
| `EMPTY` | `CanRxTableManager::reset()` no construtor, `CanService.begin()`, enable on/off, reset CAN | Todas as 20 linhas ficam invalidas; `messageOrder` reinicia em 1. |
| `PARTIALLY_FILLED` | Apos um ou mais `ProcessCreate`, antes de ocupar 20 linhas | Novas identidades ocupam o primeiro slot livre. |
| `FULL` | 20 entradas validas | Nova identidade nao encontra slot e retorna `ProcessTableFull`; identidades existentes ainda podem gerar edit. |
| `UPDATING` | Durante `processFrame()` quando cria ou edita entrada | Gera `CrudEvent` com `CMD_CAN_CREATE` ou `CMD_CAN_EDIT`. |
| `STABLE` | Sem frames, frames sem alteracao, ou apos aplicar a mutacao | Nenhum evento novo e gerado. |
| `SNAPSHOTTING` | `CAN_READ_ALL` ativo | `collectRxFrames()` retorna sem coletar; linhas validas sao publicadas como `CAN_ROW` e depois `CAN_READ_ALL_DONE`. |

## 6. Estado da Tabela Espelho API

| Estado | Quando ocorre | Relacao com eventos |
|---|---|---|
| `EMPTY` | Construtor ou `StartReadAll()` | Existem 20 DTOs invalidos; a UI ainda renderiza 20 linhas. |
| `POPULATING` | `CAN_CREATE 0x40` valido | `ApplyCreate()` marca `Valid=true`, copia flags, id, dlc, dados, cycle time e message order. |
| `UPDATING` | `CAN_EDIT 0x41` valido para linha ja criada | `ApplyEdit()` altera apenas campos indicados pela mascara e atualiza `MessageOrder`. |
| `STABLE` | Apos evento aplicado ou ausencia de eventos | `GetSnapshot()` retorna copia das 20 linhas. |
| `SYNCING_READ_ALL` | `ApiCanService.RequestReadAllAsync()` | Limpa espelho, ignora CREATE/EDIT enquanto `_isSyncingReadAll` esta ativo, aplica `CAN_ROW`, encerra em `CAN_READ_ALL_DONE` ou cancelamento. |
| `OUT_OF_SYNC_POSSIBLE` | Evento perdido, fila CRUD cheia, `CAN_EDIT` antes do `CAN_CREATE`, ausencia de delete/timeout | Nao existe estado/flag formal; e uma condicao possivel pela falta de reconciliacao automatica. |

## 7. Estado da UI

| Estado | Quando ocorre | Regra observada |
|---|---|---|
| `UI_CLOSED` | Antes de abrir ou apos `OnFormClosed()` | Nao influencia CAN, nao limpa espelho, nao fecha CAN automaticamente. |
| `UI_OPEN_NO_DATA` | Snapshot sem linhas validas | Renderiza 20 linhas fixas com `VALID=False`. |
| `UI_OPEN_WITH_DATA` | Snapshot possui linhas validas | Renderiza os dados atuais do espelho. |
| `UI_UPDATING` | Evento `CanRxTableChanged` | Reconsulta snapshot e redesenha `dgCanRx`. |
| `UI_STATUS_ERROR` | Falha em `GetCanStatusAsync()` | Atualiza label de erro; nao altera a tabela espelho. |

Regras atuais:

- A UI nao e dona da tabela CAN RX.
- A UI consome snapshot por `GetCanRxMirrorRows()` e eventos `CanRxTableChanged`.
- A UI nao dispara `CAN_READ_ALL` automaticamente no carregamento normal.
- A UI influencia CAN somente por acoes explicitas de configuracao, enable/disable e TX.

## 8. Eventos do Sistema

| Evento | Origem | Destino/efeito |
|---|---|---|
| `ConnectTransport` | UI/BPM logic | Abre Serial/Bluetooth e inicia `SdgwHostSession`. |
| `DisconnectTransport` | UI/BPM logic | Fecha sessao e transporte. |
| `LinkDrainElapsed` | Timer de `SdgwHostSession` | Envia banner API. |
| `LinkEstablished` | Parser de info da interface | Entra em `UCE_LINKED`. |
| `LinkFailed` | Timeout ou supervisor | Entra em `UCE_LINK_FAILED`. |
| `ConfigureCAN` | UI/API para UCE | Ajusta bitrate/modo e pode entrar em `CAN_CONFIGURED`. |
| `OpenCAN` | `CMD_CAN_ENABLE` estado on | Driver abre, `CAN_OPEN`, tabela UCE resetada. |
| `CloseCAN` | `CMD_CAN_ENABLE` estado off | Driver fecha, `CAN_DISABLED`, tabela UCE resetada. |
| `ResetCAN` | `CMD_CAN_RESET` | Driver reset, porta volta a disabled, filas/tabela UCE resetadas. |
| `StatusCAN` | `CMD_CAN_STATUS` | API/UI consultam estado, bitrate e modo. |
| `RX_FRAME` | CanDriver | `CanService.collectRxFrames()` processa frame. |
| `CAN_CREATE (0x40)` | UCE `CanRxTableManager` | API popula linha espelho. |
| `CAN_EDIT (0x41)` | UCE `CanRxTableManager` | API atualiza linha espelho. |
| `CAN_DELETE (0x42)` | Constante/protocolo API | Stub/nao implementado na etapa atual. |
| `CAN_READ_ALL (0x43)` | API administrativa | Solicita snapshot; nao e fluxo automatico da UI. |
| `CAN_ROW (0x44)` | UCE snapshot READ_ALL | API aplica linha de snapshot. |
| `CAN_READ_ALL_DONE (0x45)` | UCE snapshot READ_ALL | API encerra estado de sync. |
| `CanRxTableChanged` | `ApiCanService` | UI aberta re-renderiza snapshot. |
| `UI_OPEN` | Usuario abre `frmUCE_UI` | Tela assina eventos e renderiza snapshot. |
| `UI_CLOSE` | Usuario fecha `frmUCE_UI` | Tela remove assinaturas; servico continua. |

## 9. Diagrama Textual

### 9.1 Visao Geral

```text
[TRANSPORT_DISCONNECTED]
   |
   | ConnectTransport
   v
[TRANSPORT_CONNECTED]
   |
   | StartLinkLoop
   v
[UCE_LINK_DRAINING]
   |
   | SendApiBanner
   v
[UCE_BANNER_SENT]
   |
   | LinkEstablished
   v
[UCE_LINKED]
   |
   | OpenCAN
   v
[CAN_OPEN]
   |
   | RX_FRAME
   v
[RX_POLLING_DRIVER]
   |
   | CREATE/EDIT
   v
[RX_CRUD_PENDING]
   |
   | publishAsyncEvent
   v
[MIRROR_POPULATING / MIRROR_UPDATING]
   |
   | CanRxTableChanged
   v
[UI_UPDATING]
```

### 9.2 Por Camadas

```text
UCE:
  [CAN_DISABLED]
      | ConfigureCAN
      v
  [CAN_CONFIGURED]
      | OpenCAN
      v
  [CAN_OPEN]
      | pollReceived -> processFrame
      v
  [UCE_TABLE_UPDATING]
      | encode CAN_CREATE/CAN_EDIT
      v
  [RX_CRUD_PENDING]

API:
  [UCE_LINKED]
      | CanCrudEventReceived
      v
  [ApiCanService]
      | ProcessEvent
      v
  [MIRROR_POPULATING / MIRROR_UPDATING]
      | CanRxTableChanged
      v
  [MIRROR_STABLE]

UI:
  [UI_CLOSED]
      | UI_OPEN
      v
  [UI_OPEN_INITIALIZING]
      | GetSnapshot
      v
  [UI_OPEN_NO_DATA / UI_OPEN_WITH_DATA]
      | CanRxTableChanged
      v
  [UI_UPDATING]
      | RefreshCanRxGrid
      v
  [UI_OPEN_WITH_DATA]
```

## 10. Analise de Consistencia

| Item | Resultado |
|---|---|
| Estados orfaos | `CAN_FAULT` existe no protocolo C# e pode ser exibido pela UI, mas o firmware atual analisado nao atribui explicitamente `CAN_INTERFACE_FAULT` ao `PortState`; falhas retornam erro operacional. |
| Transicoes invalidas | `CAN_EDIT` para linha invalida no espelho e ignorado por `CanRxMirrorManager`; isso evita criar linha por edit, mas pode deixar o espelho sem atualizacao se o `CREATE` foi perdido. |
| Estados nao trataveis pela UI | A UI trata status CAN conhecido e falha de consulta. Nao ha UI especifica para fila CRUD cheia, `ProcessTableFull`, perda de evento ou out-of-sync. |
| Risco UCE/API | Existe risco se evento CRUD nao for publicado porque o dispatcher tem evento pendente, se a fila CRUD encher, se evento se perder no transporte, ou se `CAN_EDIT` chegar antes do `CAN_CREATE`. |
| Continuidade com UI fechada | Consistente: `ApiCanService` esta no servico compartilhado, e `frmUCE_UI` apenas remove assinaturas proprias no fechamento. |
| Influencia da UI sobre CAN RX | A UI consulta status e snapshot; nao e dona da tabela. Enable/disable/config sao acoes explicitas do usuario. |
| READ_ALL | Preservado como fluxo administrativo. Nao pertence ao carregamento normal da UI. |

## 11. Limitacoes Atuais

- `CAN_DELETE` nao esta implementado; o processamento retorna falso/stub.
- Nao ha politica implementada de timeout/expiracao de linhas na tabela espelho API.
- Nao ha reconciliacao automatica periodica entre tabela UCE e espelho API.
- `MIRROR_OUT_OF_SYNC` nao existe como flag formal; e apenas uma condicao possivel.
- `CanRxMirrorManager.ApplyEdit()` ignora edit se a linha ainda nao foi criada.
- `CanRxTableManager` UCE nao remove entradas antigas por tempo; a tabela so limpa em reset/enable/read-all conforme fluxo atual.
- Nova identidade CAN com tabela UCE cheia retorna `ProcessTableFull` e nao gera create.
- `UceServiceDispatcher` mantem apenas um evento pendente; se ja houver evento pendente, `publishAsyncEvent()` falha e o CRUD permanece na fila para nova tentativa.
- O fluxo legado `CAN_RX_EVENT` direto esta desabilitado por padrao via `CAN_LEGACY_RX_EVENT_DIRECT 0`; o caminho principal e CRUD `CREATE/EDIT`.
- `CAN_READ_ALL` pausa a coleta RX na UCE enquanto o snapshot esta ativo.
- A UI nao exibe estados internos como fila cheia, snapshot ativo, read-all syncing ou out-of-sync.

## 12. Referencias de Codigo

| Area | Arquivo |
|---|---|
| Estados de sessao/link | `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/SdgwHostSession.cs` |
| Estado compartilhado BPM/UCE/API | `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs` |
| Protocolo/estado CAN API | `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/UceCanProtocol.cs` |
| Cliente/dispatcher de eventos UCE | `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`, `UceDispatcher.cs` |
| Servico CAN UCE | `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp` |
| Tabela CAN RX UCE | `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanRxTableManager.cpp` |
| Dispatcher UCE | `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp` |
| Servico CAN API | `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs` |
| Tabela espelho API | `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxMirrorManager.cs` |
| UI consumidora | `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs` |
