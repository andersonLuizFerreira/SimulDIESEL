# SimulDIESEL - Levantamento CAN TX/RX e caminho assincrono UCE -> API

Data do levantamento: 2026-04-27

Escopo: API Windows <-> BPM <-> SPI <-> UCE <-> CAN fisico Arduino Due.

## 1. Resumo executivo

O fluxo CAN atual e misto, mas o caso CAN RX da UCE para a API ainda e baseado em polling.

- API -> UCE -> CAN fisico: sincrono por comando/resposta SDGW/TLV. O TX one-shot e o start/stop periodico sao iniciados pela API.
- CAN fisico -> UCE -> API: funcional, mas por polling explicito da tela UCE a cada 500 ms via `CMD_CAN_RX_POLL` (`0x24`).
- UCE -> BPM por IRQ: existe no nivel `SpiLink.write()`, mas hoje e usado para sinalizar resposta a uma transacao SPI ja iniciada pelo master.
- BPM -> API assinc: existe no protocolo SDGW (`SDGW_FLAG_IS_EVENT`, `SdgwSession.EventReceived`) e ja e usado pela GSA.
- UCE CAN RX assinc: nao existe contrato, fila, evento de dispatcher, nem monitoramento de IRQ da UCE na BPM para eventos espontaneos.

Conclusao curta: TX CAN esta separado e operacional; RX CAN preserva STD/EXT/RTR/DLC/ID 29 bits/data[8], mas e drenado por poll. O caminho ideal UCE CAN RX -> IRQ -> BPM -> evento SDGW -> `UceDispatcher` ainda precisa ser implementado.

## 2. Fluxo CAN TX atual

### API Windows

1. A tela UCE recebe o clique em `btnEnable` em `frmUCE_UI.BtnEnable_Click`.
2. `frmUCE_UI.TryReadCanTxInput` valida:
   - tipo CAN 1.0/STD ou CAN 2.0/EXT;
   - ID hexadecimal;
   - limite STD `0x7FF`;
   - limite EXT `0x1FFFFFFF`;
   - `LEN`/DLC em `0..8`;
   - `D0..D(LEN-1)` em `00..FF`;
   - periodo `0..65535`.
3. `frmUCE_UI` chama `FrmUceLogic.SendCanAsync`.
4. `FrmUceLogic.SendCanAsync` fixa controller atual em `can0` e chama `IUceDispatcher.SendCanAsync`.
5. `UceDispatcher.SendCanAsync` apenas repassa para `UceClient.SendCanAsync`.
6. `UceClient.SendCanAsync` monta comando semantico SDH `Target="UCE.can.tx"`, `Op="send"` e aguarda resposta sincrona por `BoardTlvDispatcher.TransactAsync`.
7. `SdhToSdgwMapper.MapUce` converte para comando compacto SDGW `GwProtocol.MakeCompactCommand(UceAddress, UceTlvTransactOp)` e payload TLV `0x26`.

Arquivos envolvidos:

- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`

### API -> BPM

1. `SdhClient.SendAsync` valida o SDH e chama `SdgwSession.SendAsync`.
2. `SdGwTxScheduler` enfileira o envio com prioridade alta.
3. `SdGwLinkEngine.SendWithSeq` gera frame SDGW COBS/CRC com `ACK_REQUIRED`.
4. `SdgwHostSession.WriteRaw` escreve no transporte ativo (Serial ou Bluetooth).
5. A BPM responde ACK de transporte; depois envia resposta da aplicacao com o TLV retornado da UCE.

Arquivos envolvidos:

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwSession.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdGwTxScheduler.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwLinkEngine.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/SdgwHostSession.cs`

### BPM -> SPI -> UCE

1. `SdgwLink.poll()` na BPM recebe frame da API.
2. `SdgwLink.handleFrameOk` envia ACK se solicitado e chama `GatewayApp.onCommand`.
3. `GatewayApp.onCommand` chama `GwRouter.route`.
4. `GwRouter.route` resolve a UCE como `GW_BUS_SPI`.
5. `GwSpiBus.transact`:
   - monta frame SPI COBS/CRC de burst fixo;
   - aguarda IRQ da UCE em nivel baixo antes do primeiro burst;
   - transfere comando;
   - aguarda pulso/IRQ da resposta;
   - faz burst de leitura;
   - valida TLV retornado.
6. `GatewayApp` envia a resposta da UCE para a API via `SdgwLink.sendResponse`.

Arquivos envolvidos:

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwLink/SdgwLink.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.cpp`

### UCE -> CAN fisico

1. `SpiLink` recebe o burst e marca `_rxReady`.
2. `UceTransport.poll()` le o frame com `_link.read`, decodifica COBS/CRC e chama `UceServiceDispatcher.dispatch`.
3. `UceServiceDispatcher.dispatch` roteia `CMD_CAN_TX` (`0x26`) para `CanService.handleTlv`.
4. `CanService.handleTx` valida payload, estado do controller e DLC.
5. Se `periodMs == 0`, chama `CanDriver.send` imediatamente.
6. Se `periodMs > 0`, grava o slot `_periodicTx`, envia uma vez e depois `CanService.loop` repete com `millis()`.
7. `CanDriver.send` monta mailbox TX `CAN_TX_MAILBOX = 2`, codifica ID STD/EXT e dispara `can_global_send_transfer_cmd`.

Arquivos envolvidos:

- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/SpiLink.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/CanService.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/CanDriver.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`

## 3. Fluxo CAN RX atual

### CAN fisico -> UCE

1. A recepcao fisica ocorre nos mailboxes configurados por `CanDriver.configureRxMailboxes`.
2. Existem dois mailboxes RX:
   - `CAN_RX_MAILBOX_STD = 0`
   - `CAN_RX_MAILBOX_EXT = 1`
3. Os mailboxes usam modo `CAN_MB_RX_OVER_WR_MODE`, separando STD/EXT pelo bit `MIDE`.
4. Nao ha ISR CAN propria no projeto e nao ha fila de frames CAN em RAM no `CanService`.
5. O primeiro codigo do projeto que efetivamente pega a mensagem e `CanDriver.pollReceived`, chamado apenas quando a API solicita `CMD_CAN_RX_POLL`.
6. `CanDriver.pollReceived` chama `readRxMailbox` para STD e EXT; se `CAN_MSR_MRDY` estiver ativo, le o mailbox com `can_mailbox_read`.
7. `CanService.handleRxPoll` empacota ate `CAN_RX_MAX_FRAMES_PER_RESPONSE = 3`.

Resposta RX atual:

```text
V[0] = controller
V[1] = frameCount
repeat frameCount:
  id[31:24]
  id[23:16]
  id[15:8]
  id[7:0]
  flags bit0=EXT, bit1=RTR
  dlc
  data[0..7]
```

Observacao importante: o ID de RX e big-endian no payload de resposta (`id[31:24]` primeiro), enquanto o TX request usa ID little-endian.

### UCE -> BPM -> API

O caminho atual de RX nao e espontaneo. Ele acontece assim:

1. `frmUCE_UI` inicia `_canRxTimer` no load.
2. A cada 500 ms, `CanRxTimer_Tick` chama `PollCanRxAsync` se a aba `tabDados` estiver ativa.
3. `FrmUceLogic.PollCanRxAsync` chama `IUceDispatcher.PollCanRxAsync`.
4. `UceClient.PollCanRxAsync` envia `Target="UCE.can.rx"`, `Op="poll"`.
5. `SdhToSdgwMapper.MapUce` converte para TLV `0x24`.
6. BPM roteia por SPI como transacao sincrona.
7. UCE responde com frames encontrados nos mailboxes.
8. `UceParsers.TryReadCanRxPollResponse` converte para `UceCanRxPollResponse`.
9. `frmUCE_UI.PollCanRxAsync` adiciona cada `UceCanFrame` em `lstRX`.

Nao ha evento C# de CAN RX na UCE. Nao ha subscribe da UCE em `SdgwSession.EventReceived`.

## 4. Contratos TLV atuais

### 0x20 - CAN_CONFIG

Request `L=3`:

```text
V[0] controller: 0=CAN0, 1=CAN1
V[1] bitrateCode: 0=5k, 1=10k, 2=25k, 3=50k, 4=125k, 5=250k, 6=500k, 7=800k, 8=1000k
V[2] modeCode: 0=normal, 1=listen
```

Response `L=3`:

```text
V[0] controller
V[1] acceptedBitrateCode
V[2] acceptedModeCode
```

### 0x21 - CAN_ENABLE

Request `L=2`:

```text
V[0] controller
V[1] state: 0=off, 1=on
```

Response `L=2`:

```text
V[0] controller
V[1] effectiveState
```

### 0x22 - CAN_STATUS

Request `L=1`:

```text
V[0] controller
```

Response `L=4`:

```text
V[0] controller
V[1] interfaceState: 0=disabled, 1=configured, 2=open, 3=fault
V[2] bitrateCode
V[3] modeCode
```

### 0x23 - CAN_RESET

Request `L=1`:

```text
V[0] controller
```

Response `L=2`:

```text
V[0] controller
V[1] resetStatus: 1=ok
```

### 0x24 - CAN_RX_POLL

Request `L=1`:

```text
V[0] controller
```

Response `L=2 + N*14`, `N <= 3`:

```text
V[0] controller
V[1] count
frame:
  id big-endian u32
  flags bit0=EXT, bit1=RTR
  dlc
  data[8]
```

Este nao e evento; e resposta de comando.

### 0x25 - CAN_DRIVER_LOG_POLL

Request `L=1`:

```text
V[0] controller
```

Response `L=2 + N*8`, `N <= 6`:

```text
V[0] controller
V[1] count
entry:
  timestampLow
  eventCode
  interfaceState
  bitrateCode
  modeCode
  detail0
  detail1
  detail2
```

### 0x26 - CAN_TX

Request `L=17`:

```text
V[0]  controller
V[1]  flags bit0=EXT, bit1=RTR
V[2]  dlc
V[3]  periodLow
V[4]  periodHigh
V[5]  id byte 0
V[6]  id byte 1
V[7]  id byte 2
V[8]  id byte 3
V[9]  data[0]
V[10] data[1]
V[11] data[2]
V[12] data[3]
V[13] data[4]
V[14] data[5]
V[15] data[6]
V[16] data[7]
```

Endianness:

- `periodMs`: uint16 little-endian.
- `id`: uint32 little-endian.
- STD mascara `0x7FF`.
- EXT mascara `0x1FFFFFFF`.

Response `L=3`:

```text
V[0] controller
V[1] txStatus
V[2] sequenceOrSlot
```

Status:

```text
0x00 accepted/sent
0x01 invalid payload
0x02 controller disabled
0x03 tx failed
0x04 periodic started
0x05 periodic stopped
0x06 no free periodic slot
```

Observacao: atualmente ha um slot periodico simples; novo start substitui o anterior.

### 0x27 - CAN_TX_STOP

Request `L=2`:

```text
V[0] controller
V[1] slotOrAll: 0x00=slot unico, 0xFF=todos
```

Response `L=2`:

```text
V[0] controller
V[1] txStatus: 0x05=periodic stopped
```

### Contrato ausente

Nao existe contrato TLV separado para evento espontaneo de frame CAN recebido. O contrato atual de RX (`0x24`) e resposta de poll.

## 5. Eventos e polling

### Timers

- API UCE:
  - `_canRxTimer.Interval = 500`
  - `_canDriverLogTimer.Interval = 500`
  - ambos em `frmUCE_UI`.
- API host:
  - `SdgwHostSession` usa timer de handshake/link.
  - `SdGwLinkEngine` usa timer de ACK.
  - `SdGwTxScheduler` usa fila/pump por `Task.Run`.
- UCE:
  - `CanService.loop` usa `millis()` para TX periodico.
- BPM:
  - `loop()` chama `sdgwLink.poll()` e `app.tick()`.

### Polling

- UCE CAN RX: `frmUCE_UI` -> `PollCanRxAsync` -> `CMD_CAN_RX_POLL`.
- UCE driver log: `frmUCE_UI` -> `PollCanDriverLogAsync` -> `CMD_CAN_DRIVER_LOG_POLL`.
- UCE firmware: `UceTransport.poll` depende de `_link.available()` para comandos SPI.
- BPM: `SdgwLink.poll` le serial/bluetooth.
- BPM GSA async: `GatewayApp.tick` drena eventos GSA apos IRQ latched.

### Callbacks/eventos existentes

- API transporte:
  - `IByteTransport.BytesReceived`
  - `SwitchableTransport.BytesReceived`
  - `SdgwHostSession` alimenta `SdGwLinkEngine.OnBytesReceived`.
- API SDGW:
  - `SdGwLinkEngine.AppFrameReceived`
  - `SdgwSession.FrameReceived`
  - `SdgwSession.EventReceived`
- API GSA:
  - `GsaClient` assina `SdgwSession.EventReceived`.
  - `GsaDispatcher` expoe `ChannelFaultEventReceived` e `PhysicalOperationEventReceived`.
  - `FrmGsaLogic` repassa eventos.
- API UCE:
  - nao ha evento CAN RX.
  - `UceClient` nao assina `SdgwSession.EventReceived`.
  - `IUceDispatcher` nao expoe `CanFrameReceived`.

### IRQ

- UCE:
  - `SpiLink.write()` chama `pulseAttention()`.
  - `pulseAttention()` alterna o pino IRQ idle -> active -> idle.
  - `onCsFalling()` coloca IRQ ready durante transacao.
- BPM:
  - `GwSpiBus.transact` espera IRQ da UCE durante uma transacao iniciada pelo master.
  - `GatewayApp` so da `attachInterrupt` para `BPM_GSA_IRQ_PIN`, nao para `BPM_UCE_IRQ_PIN`.
  - `GwDeviceTable` conhece `BPM_UCE_IRQ_PIN`, mas nao ha rotina de drenagem espontanea da UCE.

## 6. Gaps arquiteturais

O fluxo ideal desejado e:

```text
UCE CAN RX -> fila -> evento TLV -> SpiLink.write/pulse IRQ -> BPM detecta IRQ -> SPI read event -> SDGW event -> API EventReceived -> UceDispatcher -> Form
```

Hoje faltam:

1. Contrato TLV de evento CAN RX separado de `CAN_RX_POLL`.
2. Fila CAN RX em RAM na UCE.
3. Producao de evento TLV espontaneo pela UCE fora de resposta de comando.
4. API publica em `UceTransport` ou equivalente para enfileirar resposta/evento espontaneo.
5. Arbitragem entre resposta sincrona de comando e evento espontaneo no buffer TX unico do `SpiLink`.
6. Monitoramento de `BPM_UCE_IRQ_PIN` na BPM fora de `GwSpiBus.transact`.
7. Metodo `GwRouter.pollUceEvent` ou equivalente.
8. Uso de `SdgwLink.sendEvent(SDGW_CMD_UCE_TLV, ...)` para eventos UCE.
9. `UceClient` assinando `SdgwSession.EventReceived`.
10. `IUceDispatcher` expondo evento C# de `UceCanFrame`.
11. `FrmUceLogic` repassando evento para tela.
12. `frmUCE_UI` atualizando `lstRX` por evento no thread da UI.
13. Remocao do timer de polling CAN RX apos validacao do caminho assinc.

## 7. Riscos atuais

### Perda de frame CAN

Risco alto em trafego intenso. O RX usa mailboxes `CAN_MB_RX_OVER_WR_MODE` e nao ha fila de frames em RAM. Se outro frame chegar antes do poll, o mailbox pode ser sobrescrito.

### Limite de resposta

`CAN_RX_POLL` retorna ate 3 frames por chamada, mas a origem real sao dois mailboxes. Na pratica, por ciclo atual, o driver tenta ler STD e EXT uma vez cada. O limite de 3 no contrato fica acima do que o driver hoje consegue entregar nessa configuracao.

### Dependencia de polling de UI

Se a aba de dados nao estiver ativa, `CanRxTimer_Tick` nao chama `PollCanRxAsync`. Frames podem ficar nos mailboxes ate serem sobrescritos.

### Mistura de resposta com evento

Hoje nao ha evento CAN RX espontaneo, entao nao ha mistura real no CAN. Mas quando for implementado, existe risco se o mesmo `SpiLink._txBuf` for usado para resposta de comando e evento sem fila/arbitragem.

### Confusao entre resposta e evento na API

O `BoardTlvDispatcher.OnFrameReceived` ignora frames com `flags & 0x02`, entao a infraestrutura ja separa evento SDGW de resposta. O risco e menor na API se o evento usar `SDGW_FLAG_IS_EVENT`. O risco volta se o evento CAN RX reutilizar `0x24` como resposta normal sem flag de evento.

### Ausencia de contrato de evento

Sem um TLV proprio, a UCE nao consegue diferenciar "resposta ao poll" de "frame CAN chegou".

### Bloqueio no loop

TX periodico usa `millis()` e nao bloqueia. `GwSpiBus.transact` na BPM usa waits curtos de IRQ dentro da chamada de roteamento, mas isso e parte da transacao sincrona atual.

### SPI durante resposta de comando

`SpiLink.write()` tem buffer TX unico. Um evento espontaneo durante resposta de comando poderia sobrescrever ou ser sobrescrito se nao houver uma fila/event manager.

## 8. Respostas objetivas aos pontos criticos

1. Quando uma mensagem CAN chega fisicamente na UCE, qual funcao e chamada primeiro?
   - Nao ha funcao de callback/ISR do projeto. O frame fica no mailbox CAN configurado. A primeira funcao do projeto que o le e `CanDriver.pollReceived`, chamada por `CanService.handleRxPoll`.

2. Essa mensagem e armazenada onde?
   - No mailbox fisico do controlador CAN0 do Arduino Due, mailbox 0 para STD e mailbox 1 para EXT.

3. Existe fila de recepcao CAN na UCE?
   - Nao. Existe apenas leitura de mailboxes sob demanda.

4. Existe risco de perda se chegarem muitas mensagens CAN?
   - Sim. O modo RX overwrite e a ausencia de fila tornam possivel perda/sobrescrita entre polls.

5. A UCE ja consegue montar uma mensagem TLV espontanea para a API?
   - Nao no fluxo CAN. `UceTransport` so monta resposta dentro de `poll()` apos receber comando.

6. A UCE ja consegue chamar a BPM via IRQ quando possui dados?
   - O pino IRQ pulsa em `SpiLink.write()`, mas nao ha produtor CAN chamando `SpiLink.write()` espontaneamente. Portanto, a capacidade eletrica existe, mas o fluxo CAN nao usa isso.

7. A BPM reage a IRQ da UCE sem solicitacao da API?
   - Nao. A BPM so espera `BPM_UCE_IRQ_PIN` dentro de `GwSpiBus.transact`. O unico `attachInterrupt` assinc atual e para GSA.

8. A BPM possui fila para eventos vindos da UCE?
   - Nao. Existe drenagem de eventos GSA (`drainPendingGsaEvents`), mas nao equivalente UCE.

9. A API recebe eventos espontaneos vindos da BPM?
   - Sim, genericamente. `SdgwSession.EventReceived` dispara quando `SDGW_FLAG_IS_EVENT` esta setado.

10. A tela da UCE esta usando polling, timer ou evento?
    - Timer/polling. `_canRxTimer` chama `PollCanRxAsync` a cada 500 ms na aba de dados.

11. O fluxo atual mistura resposta de comando com evento CAN espontaneo?
    - Nao, porque nao ha evento CAN espontaneo atual.

12. Existe risco de uma resposta de comando ser confundida com uma mensagem CAN recebida?
    - Hoje nao, pois CAN RX recebido pela API vem como resposta ao comando `0x24`. Futuramente, sim, se evento CAN reutilizar o mesmo contrato sem flag/event type separado.

13. Existe contrato TLV separado para resposta de comando CAN e evento de frame CAN recebido?
    - Existe resposta de comando (`0x24 CAN_RX_POLL`), mas nao existe evento de frame CAN recebido.

14. O contrato TLV atual preserva controller, STD/EXT, RTR, DLC, ID 29 bits, data[8]?
    - Sim para `CAN_RX_POLL` e `CAN_TX`. RX usa flags EXT/RTR, DLC, ID 29 bits e data[8]. TX idem, mas ID em little-endian.

15. O caminho TX CAN esta corretamente separado do caminho RX CAN?
    - Sim. TX usa `0x26/0x27`; RX poll usa `0x24`; log usa `0x25`.

## 9. Arquitetura recomendada

### UCE

1. Criar contrato TLV de evento CAN RX, por exemplo `CMD_CAN_RX_EVENT = 0x28`, sem substituir `0x24`.
2. Adicionar fila circular RX no `CanDriver` ou `CanService`.
3. Criar rotina leve de drenagem CAN em `CanService.loop()`:
   - ler mailboxes;
   - copiar frames para fila;
   - sinalizar que ha evento pendente.
4. Criar produtor de evento TLV fora de ISR:
   - `CanService.tryBuildRxEvent(...)`;
   - `UceTransport` ou novo `UceEventPump` pega evento e chama `SpiLink.write()`.
5. Nunca chamar CAN/SPI pesado dentro de ISR.
6. Criar arbitragem do buffer TX:
   - prioridade para resposta de comando em andamento;
   - fila de eventos pendentes;
   - nao sobrescrever `_txBuf`.

### BPM

1. Registrar `BPM_UCE_IRQ_PIN` com `attachInterrupt`, semelhante a GSA.
2. Adicionar `_uceIrqLatched`.
3. Em `GatewayApp.tick`, drenar eventos UCE com limite por ciclo.
4. Adicionar `GwRouter.pollUceEvent`.
5. Adicionar metodo no `GwSpiBus` para leitura de evento sem comando funcional, ou definir comando NOP/event-poll fisico no SPI.
6. Enviar para a API com `SdgwLink.sendEvent(SDGW_CMD_UCE_TLV, eventPacket, eventLen)`.
7. Manter separacao entre `sendResponse` de transacao e `sendEvent` espontaneo.

### API

1. Em `UceClient`, assinar `SdgwSession.EventReceived`.
2. Filtrar `frame.Cmd == MakeCompactCommand(UceAddress, UceTlvTransactOp)`.
3. Parsear `CMD_CAN_RX_EVENT`.
4. Expor `event Action<UceCanFrame> CanFrameReceived` ou batch `Action<UceCanRxEvent>`.
5. Em `UceDispatcher`, repassar evento.
6. Em `FrmUceLogic`, repassar evento para UI.
7. Em `frmUCE_UI`, atualizar `lstRX` via `BeginInvoke`/`InvokeRequired`.
8. Manter `CAN_RX_POLL` temporariamente como fallback/diagnostico.
9. Remover ou desabilitar `_canRxTimer` somente apos validacao de bancada.

## 10. Proposta de etapas

### ETAPA A - Contrato TLV de evento CAN RX

- Criar `CMD_CAN_RX_EVENT`.
- Reusar formato de frame do `0x24` para preservar controller, STD/EXT, RTR, DLC, ID e data[8].
- Documentar endianess e limites.

### ETAPA B - Fila RX CAN na UCE

- Implementar fila circular simples.
- Drenar mailboxes no `CanService.loop()`.
- Medir overflow e expor contador diagnostico.

### ETAPA C - Evento/IRQ UCE -> BPM

- Criar caminho seguro para `UceTransport` publicar evento pendente.
- Usar `SpiLink.write()` apenas fora de ISR.
- Garantir que resposta de comando nao seja sobrescrita por evento.

### ETAPA D - BPM monitora IRQ UCE

- Adicionar latch e drain UCE no `GatewayApp`.
- Criar `GwRouter.pollUceEvent` e suporte em `GwSpiBus`.
- Enviar evento para API com `SDGW_FLAG_IS_EVENT`.

### ETAPA E - Evento C# no UceDispatcher/API

- `UceClient` assina `SdgwSession.EventReceived`.
- Parser de evento CAN RX.
- `IUceDispatcher` expoe evento.

### ETAPA F - Tela UCE sem polling

- UI assina evento.
- Atualiza `lstRX` com marshal para thread WinForms.
- Mantem polling como fallback ate bancada confirmar.

## 11. Recomendacao final

A proxima etapa deve ser a ETAPA A: criar o contrato separado de evento CAN RX, sem ainda mexer no SPI. Justificativa: hoje a maior ambiguidade arquitetural e semantica. Enquanto `0x24` representar apenas resposta de polling, nao existe forma limpa de fazer UCE -> BPM -> API espontaneo sem risco de confundir resposta e evento.

Depois do contrato, implementar a fila RX na UCE. So entao vale ligar IRQ assinc UCE -> BPM, porque sem fila a IRQ apenas avisaria que um mailbox possivelmente sobrescrito tem dado.

