# ETAPA SPI IRQ UCE -> BPM - Levantamento

Data: 2026-04-26

Escopo: verificar se o contrato SPI atual ja suporta transmissao iniciada pela UCE para a BPM via IRQ, antes de criar CAN_RX_EVENT.

## 1. Resumo executivo

O caminho UCE -> BPM via IRQ esta **parcialmente implementado no nivel fisico/enlace da UCE**, mas esta **ausente como fluxo assíncrono completo**.

Conclusao objetiva:

- `SpiLink.write(...)` na UCE carrega `_txBuf` e gera pulso no pino `UCE_SPI_IRQ_PIN`.
- Esse mecanismo e generico e, eletricamente, poderia ser chamado fora de uma resposta de comando.
- Porem `UceTransport` hoje so chama `_link.write(...)` dentro de `poll()`, apos receber e despachar um comando SPI vindo da BPM.
- Nao existe metodo publico em `UceTransport` para publicar evento espontaneo.
- Nao existe fila de eventos nem arbitragem entre resposta sincrona e evento espontaneo na UCE.
- Na BPM, `GwDeviceTable` conhece `BPM_UCE_IRQ_PIN`, e `GwSpiBus::transact(...)` usa IRQ da UCE durante transacao sincrona.
- Mas `GatewayApp` so instala `attachInterrupt` para `BPM_GSA_IRQ_PIN`.
- Nao ha `pollUceEvent`, `drainPendingUceEvents` ou transacao SPI espontanea disparada por IRQ da UCE.
- A API Windows ja possui infraestrutura generica para evento SDGW (`SdgwSession.EventReceived`), mas `UceClient/UceDispatcher` nao assinam eventos da UCE.

Portanto, antes de implementar `CAN_RX_EVENT`, a proxima etapa recomendada e implementar e validar primeiro um caminho generico simples:

UCE publica evento diagnostico pequeno -> `SpiLink.write()` pulsa IRQ -> BPM detecta IRQ da UCE -> BPM faz leitura SPI dummy -> BPM envia `SDGW_FLAG_IS_EVENT` para API -> `UceClient` recebe evento.

## 2. Fluxo atual de resposta sincrona LED

Fluxo atual:

1. API chama `UceClient.SetBuiltinLedAsync(...)`.
   - Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
   - O comando SDH criado e `Target = "UCE.led"`, `Op = "set"`.

2. `SdhToSdgwMapper.MapUce(...)` transforma isso em TLV UCE.
   - Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
   - Tipo TLV: `GwProtocol.UceSetLedType = 0x12`.
   - Payload: `[0x12][0x01][state][crc]`.

3. `BoardTlvDispatcher.TransactAsync(...)` envia o comando e aguarda resposta nao-evento.
   - Arquivo: `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BoardTlvDispatcher.cs`
   - `OnFrameReceived(...)` ignora frames com `frame.Flags & 0x02`.
   - Isso separa resposta sincrona de evento SDGW no lado API.

4. BPM recebe comando SDGW e chama `GatewayApp::onCommand(...)`.
   - Arquivo: `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.cpp`
   - Comando de UCE e roteado por `_router.route(...)`.

5. `GwRouter::route(...)` seleciona barramento SPI para UCE e chama `GwSpiBus::transact(...)`.
   - Arquivos:
     - `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
     - `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`

6. `GwSpiBus::transact(...)` faz duas sessoes SPI:
   - primeira sessao: envia frame COBS/TLV para a UCE;
   - aguarda pulso/estado de IRQ indicando resposta pronta;
   - segunda sessao: envia 64 bytes zero/dummy e le os 64 bytes de resposta da UCE.

7. Na UCE, `SpiLink` recebe os 64 bytes. Ao fim da transferencia (`SPI_SR_NSSR`), marca `_rxReady = (_index == BufferSize)`.
   - Arquivo: `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/SpiLink.cpp`

8. No `loop()` da UCE:
   - `g_transport.poll();`
   - `g_dispatcher.loop();`
   - Arquivo: `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`

9. `UceTransport::poll()` le o buffer RX, decodifica COBS/CRC/TLV, chama dispatcher e monta resposta.
   - Arquivo: `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.cpp`

10. `UceServiceDispatcher::dispatch(...)` roteia `CMD_LED_BUILTIN = 0x12` para `LedService::handleTlv(...)`.
    - Arquivos:
      - `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp`
      - `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/led/LedService.cpp`

11. `LedService::handleTlv(...)` altera o LED e retorna `responseValue[0] = currentState`.

12. `UceTransport::poll()` chama `_link.write(tx, txLen)`.

13. `SpiLink::write(...)`:
    - zera `_txBuf`;
    - copia a resposta em `_txBuf`;
    - chama `pulseAttention()`;
    - retorna `true`.

14. `pulseAttention()` gera pulso no IRQ da UCE:
    - `IDLE`;
    - `delayMicroseconds(2)`;
    - `ACTIVE`;
    - `delayMicroseconds(20)`;
    - `IDLE`.

15. BPM detecta esse pulso dentro de `GwSpiBus::transact(...)`, abre a segunda sessao SPI e busca a resposta.

Observacao importante: no fluxo LED, o pulso IRQ existe, mas ele e usado como parte da resposta sincrona a um comando ja iniciado pela BPM, nao como evento espontaneo UCE -> BPM.

## 3. Fluxo potencial UCE espontaneo

O fluxo desejado seria:

1. Algum servico da UCE detecta dado espontaneo, por exemplo CAN RX.
2. O servico monta TLV de evento.
3. UCE empacota TLV com COBS/CRC.
4. UCE chama `SpiLink.write(...)`.
5. `SpiLink.write(...)` carrega `_txBuf` e pulsa IRQ.
6. BPM detecta IRQ da UCE sem comando pendente.
7. BPM inicia sessao SPI dummy de 64 bytes.
8. UCE transmite `_txBuf`.
9. BPM valida COBS/CRC/TLV.
10. BPM encapsula em SDGW com `SDGW_FLAG_IS_EVENT`.
11. API recebe `SdgwSession.EventReceived`.
12. `UceClient/UceDispatcher` repassa evento para servico/tela.

Estado atual:

- Passos 4 e 5 sao fisicamente possiveis, porque `SpiLink.write(...)` e publico e pulsa IRQ.
- Passos 1, 2 e 3 nao existem como API publica do `UceTransport`.
- Passos 6, 7, 8 e 9 nao existem para UCE na BPM.
- Passos 10 e 11 existem genericamente no SDGW.
- Passo 12 nao existe para UCE.

Ponto critico: se algum codigo chamasse `SpiLink.write(...)` diretamente fora do `UceTransport`, ele conseguiria carregar o TX buffer e pulsar IRQ, mas furaria a camada de transporte e nao resolveria fila, CRC/TLV padronizado, colisao com resposta sincrona nem roteamento de evento na BPM.

## 4. Comparacao com GSA

A GSA ja possui fluxo assíncrono funcional como modelo arquitetural.

Na BPM:

- `GatewayApp::begin()` configura `BPM_GSA_IRQ_PIN` como entrada e instala `attachInterrupt(..., FALLING)`.
- A ISR apenas marca `_gsaIrqLatched = true`.
- `GatewayApp::tick()` chama `drainPendingGsaEvents()` quando ha IRQ latched ou linha baixa.
- `drainPendingGsaEvents()` chama `_router.pollGsaEvent(...)` em loop limitado por `MaxEventsPerDrain = 24`.
- Cada evento valido e enviado para a API por `_link.sendEvent(SDGW_CMD_GSA_TLV, ...)`.
- `SdgwLink::sendEvent(...)` seta `SDGW_FLAG_IS_EVENT`.

No barramento da GSA:

- `GwRouter::pollGsaEvent(...)` chama `GwI2cBus::pollEvent(...)`.
- `GwI2cBus::pollEvent(...)` faz leitura I2C espontanea do slave e valida TLV.

Na API:

- `SdgwSession.EventReceived` e disparado para frames com flag `0x02`.
- `GsaClient` assina `_sdgwSession.EventReceived += OnEventReceived`.
- `GsaDispatcher` expoe eventos C# de dominio.
- `FrmGsaLogic` e `frmGSA_UI` assinam os eventos e atualizam UI com `BeginInvoke(...)`.

Esse e o melhor modelo para a UCE, mas a camada de coleta do evento muda: para UCE deve existir uma leitura SPI dummy disparada por IRQ, equivalente conceitual ao `GwI2cBus::pollEvent(...)` da GSA.

## 5. Analise da BPM

### IRQ UCE configurado?

Parcialmente.

- `GwDeviceTable` mapeia a UCE com `BPM_UCE_IRQ_PIN` e `spiUseIrq = true`.
- `GwSpiBus::transact(...)` configura o pino IRQ da board SPI com `pinMode(irq, INPUT_PULLUP)` durante transacao.
- Isso serve para transacao sincrona.

Ausente:

- `GatewayApp::begin()` nao configura `BPM_UCE_IRQ_PIN`.
- Nao ha `attachInterrupt(digitalPinToInterrupt(BPM_UCE_IRQ_PIN), ...)`.
- Nao ha latch equivalente a `_gsaIrqLatched` para UCE.

### Interrupcao ou polling?

Para UCE, nao ha interrupcao espontanea nem polling periodico de IRQ no `GatewayApp::tick()`.

O unico uso de IRQ UCE acontece dentro de `GwSpiBus::transact(...)`, quando a API ja mandou um comando.

### Leitura espontanea SPI?

Nao existe.

`GwSpiBus::transact(...)` exige `tx` valido, constroi frame de comando, envia esse frame e depois faz leitura da resposta. Nao ha metodo dedicado como `pollEvent(...)` para:

- selecionar CS da UCE;
- enviar 64 bytes dummy;
- capturar `_txBuf` ja carregado pela UCE;
- decodificar COBS/CRC/TLV;
- limpar/confirmar evento.

### Envio de evento para API?

Existe genericamente.

- `SdgwLink::sendEvent(...)` esta implementado.
- `GatewayApp::drainPendingGsaEvents()` usa esse metodo para GSA.

Ausente para UCE:

- chamada `_link.sendEvent(SDGW_CMD_UCE_TLV, eventPacket, eventLen)` ou equivalente.
- constante/nome de rotina dedicada ao evento UCE.
- dreno assíncrono da UCE.

## 6. Analise da API

### EventReceived existe?

Sim.

`SdgwSession` expoe:

- `FrameReceived`
- `EventReceived`

`OnAppFrameReceived(...)` dispara `EventReceived` quando `frame.Flags & 0x02` e diferente de zero.

### BoardTlvDispatcher mistura evento com resposta?

Nao, no lado API ele filtra.

`BoardTlvDispatcher.OnFrameReceived(...)` retorna imediatamente se `frame.Flags & 0x02` estiver setado. Assim, eventos SDGW nao completam requisicoes sincronas pendentes.

### UCE assina eventos?

Nao.

`UceClient` recebe `SdgwSession` no construtor, mas so o usa para criar `BoardTlvDispatcher`. Diferente de `GsaClient`, ele nao guarda `_sdgwSession` e nao assina `EventReceived`.

`IUceDispatcher` tambem nao expoe eventos de UCE.

### Tela/servico usa evento?

Nao para UCE.

A tela UCE usa polling CAN RX via timer:

- `frmUCE_UI` possui timer de CAN RX.
- `FrmUceLogic.PollCanRxAsync(...)` chama `IUceDispatcher.PollCanRxAsync(...)`.
- `UceDispatcher.PollCanRxAsync(...)` chama `UceClient.PollCanRxAsync(...)`.
- O comando vai para `CMD_CAN_RX_POLL = 0x24`.

### LED tem fluxo assíncrono util?

Como esta hoje, nao.

O LED UCE e apenas comando/resposta. Ele e util como referencia de transacao sincrona SPI, COBS, CRC e pulso IRQ de resposta, mas nao como evento espontaneo.

Para testar o caminho assíncrono UCE -> BPM, seria melhor criar futuramente um evento diagnostico simples e isolado, por exemplo `UCE_DIAG_EVENT`, gerado por comando de arm/disparo ou por contador, antes de ligar no CAN RX.

## 7. Analise de riscos

### TX buffer unico na UCE

`SpiLink` possui apenas um `_txBuf[64]`.

`SpiLink::write(...)` sempre zera e sobrescreve `_txBuf`. Nao ha:

- flag explicita de `txPending`;
- fila;
- contador de sequencia;
- lock semantico entre resposta e evento;
- confirmacao de consumo pelo master.

### Colisao resposta/evento

Se um evento espontaneo for escrito em `_txBuf` enquanto uma resposta sincrona estiver pendente, o ultimo `write(...)` vence e sobrescreve o anterior.

Se uma resposta sincrona for escrita enquanto um evento espontaneo aguarda coleta, o evento pode ser perdido.

### Evento perdido

O pulso de IRQ em `pulseAttention()` e curto. A BPM hoje nao monitora IRQ da UCE em ISR fora da transacao sincrona. Portanto, qualquer pulso espontaneo da UCE seria perdido no estado atual.

Mesmo com ISR futura, sem `txPending`/fila/linha mantida ativa ate coleta, ha risco de evento perdido se o pulso nao for latched corretamente no firmware BPM.

### IRQ perdido ou nivel ambiguo

`pulseAttention()` termina retornando a linha para `IDLE`. Durante `onCsFalling()`, `SpiLink` chama `setIrqReady(true)`, e ao fim da sessao (`NSSR`) chama `setIrqReady(false)`.

Esse desenho funciona no handshake sincrono atual, mas para evento espontaneo precisa ficar claro se o contrato e:

- pulso curto de atencao; ou
- nivel ativo enquanto houver TX pendente.

Hoje nao ha variavel de TX pendente para sustentar nivel.

### Mistura de resposta com evento

No SDGW/API existe separacao por flag `SDGW_FLAG_IS_EVENT`.

No TLV interno UCE ainda nao existe separacao entre:

- resposta de comando UCE;
- evento espontaneo UCE.

Sem tipo TLV dedicado e sem empacotamento SDGW com flag de evento na BPM, ha risco de confundir evento com resposta, principalmente se o payload usar o mesmo tipo de uma resposta.

### Bloqueio e timing SPI

`SpiLink::write(...)` desabilita interrupcoes enquanto limpa/copia 64 bytes e chama `pulseAttention()` com pequenos delays. Isso ja existe no fluxo validado.

Para eventos frequentes, nao se deve chamar `write(...)` em ISR de CAN nem em trecho critico longo. O caminho correto deve enfileirar evento no servico e publicar no `loop()`.

## 8. Revisao little-endian

Decisao de contrato para daqui em diante: campos multibyte de payloads TLV da UCE devem ser **little-endian**.

Estado observado:

### Contratos UCE sem multibyte

- `CMD_LED_BUILTIN = 0x12`: apenas `state`, sem endianess.
- `CMD_CAN_CONFIG = 0x20`: `controller`, `bitrate`, `mode`, sem multibyte.
- `CMD_CAN_ENABLE = 0x21`: `controller`, `state`, sem multibyte.
- `CMD_CAN_STATUS = 0x22`: `controller`, `state`, `bitrate`, `mode`, sem multibyte.
- `CMD_CAN_RESET = 0x23`: `controller`, `status`, sem multibyte.
- `CMD_CAN_DRIVER_LOG_POLL = 0x25`: campos de 1 byte; `timestampLow` e apenas byte baixo.
- `CMD_CAN_TX_STOP = 0x27`: `controller`, `slot`, sem multibyte.

### CAN_TX usa little-endian

`CMD_CAN_TX = 0x26`, request `V[17]`:

- `periodMs`: `V[3] low`, `V[4] high`.
- `id`: `V[5] least significant byte`, depois `V[6]`, `V[7]`, `V[8] most significant byte`.

Na API:

- `SdhToSdgwMapper.BuildUceCanTxPayload(...)` monta `period` e `id` em little-endian.

Na UCE:

- `CanService::handleTx(...)` reconstrói `periodMs` e `id` em little-endian.

### CAN_RX_POLL usa big-endian para ID

`CMD_CAN_RX_POLL = 0x24`, response:

- `id`: `id[31:24]`, `id[23:16]`, `id[15:8]`, `id[7:0]`.

Na UCE:

- `CanService::handleRxPoll(...)` escreve o ID com shifts `>> 24`, `>> 16`, `>> 8`, `& 0xFF`.

Na API:

- `UceParsers.TryReadCanRxPollResponse(...)` reconstrói `rawId` com o mesmo formato big-endian.

Esse contrato esta funcional e validado em bancada. Nao deve ser mudado nesta etapa. Deve ser registrado como inconsistencia historica e corrigido apenas em etapa planejada, com compatibilidade ou versao de contrato.

### Recomendacao de endianess

Para novos contratos, inclusive futuro `CAN_RX_EVENT`, usar little-endian:

- `id`: byte 0 LSB, byte 3 MSB.
- `timestamp`, se existir: little-endian.
- `sequence`, se maior que 1 byte: little-endian.
- `period`, `counter`, `lostCount`: little-endian.

Para `CAN_RX_POLL`, manter como esta ate existir plano explicito de migracao. O futuro `CAN_RX_EVENT` pode nascer little-endian e documentar a diferenca em relacao ao poll legado.

## 9. Respostas objetivas

1. No firmware UCE, `SpiLink.write()` gera pulso de IRQ?
   - Sim. Depois de copiar para `_txBuf`, chama `pulseAttention()`.

2. Esse pulso de IRQ funciona fora do fluxo de resposta de comando?
   - Ele e tecnicamente chamavel fora do fluxo de resposta, pois `write(...)` e publico. Mas nao ha contrato, fila, publicador de evento ou consumidor BPM para usar isso com seguranca.

3. A UCE consegue carregar um TLV no TX buffer sem ter recebido comando?
   - Em baixo nivel, sim, se algum codigo montar o frame e chamar `SpiLink.write(...)`. Em nivel arquitetural, nao: `UceTransport` nao expoe metodo de publish/event e seus builders sao privados.

4. A UCE possui alguma fila ou apenas um TX buffer unico?
   - Apenas um TX buffer unico `_txBuf[64]`.

5. Se a UCE publicar evento enquanto ha resposta pendente, o que acontece?
   - O buffer pode ser sobrescrito. Nao ha arbitragem; o ultimo `write(...)` vence.

6. A BPM detecta IRQ da UCE de forma espontanea?
   - Nao. Ela so espera IRQ da UCE dentro de `GwSpiBus::transact(...)`.

7. A BPM inicia transacao SPI apenas por IRQ da UCE?
   - Nao. Nao existe rotina de dreno UCE acionada por IRQ.

8. A BPM consegue ler 64 bytes dummy/COBS para buscar evento da UCE?
   - O mecanismo de transferencia dummy existe dentro da segunda fase de `GwSpiBus::transact(...)`, mas nao esta exposto como `pollEvent` para UCE.

9. A BPM consegue distinguir resposta de comando versus evento espontaneo?
   - No nivel SDGW sim, via `SDGW_FLAG_IS_EVENT`. Para UCE, falta produzir eventos nessa flag. No TLV interno UCE ainda falta contrato separado de evento.

10. A BPM consegue encaminhar evento espontaneo para API?
    - Sim genericamente, com `SdgwLink::sendEvent(...)`. Hoje isso e usado para GSA, nao para UCE.

11. A API ja possui infraestrutura para receber evento espontaneo?
    - Sim. `SdgwSession.EventReceived`.

12. `UceClient/UceDispatcher` ja assinam eventos?
    - Nao.

13. O servico LED tem algum fluxo assíncrono util para testar o caminho?
    - Nao como esta. Ele serve como referencia de comando/resposta. Para teste assíncrono, criar futuro evento diagnostico simples e isolado e mais adequado.

14. O que falta para usar o mesmo mecanismo com `CAN_RX_EVENT`?
    - Publicador de evento na UCE, fila/arbitragem de TX, IRQ UCE monitorado na BPM, `GwSpiBus::pollEvent`, `GatewayApp::drainPendingUceEvents`, envio SDGW com flag de evento, assinatura em `UceClient/UceDispatcher`, parser/evento de dominio e UI assinante.

15. O contrato little-endian ja e consistente no fluxo CAN?
    - Nao. `CAN_TX` usa little-endian para `periodMs` e `id`; `CAN_RX_POLL` usa big-endian para `id`. Novos contratos devem usar little-endian, mas o poll atual deve permanecer intocado ate migracao planejada.

## 10. Proxima etapa recomendada

Como IRQ UCE -> BPM ainda nao esta pronto de ponta a ponta, a proxima etapa nao deve ser `CAN_RX_EVENT` diretamente.

Recomendacao:

1. Criar um evento diagnostico simples da UCE, sem CAN, para validar o caminho generico.
2. Adicionar publicacao controlada no `UceTransport`, sem chamar `SpiLink.write(...)` diretamente de servicos.
3. Adicionar uma fila pequena ou pelo menos um slot `txPending` com protecao contra sobrescrever resposta sincrona.
4. Na BPM, adicionar IRQ/latch da UCE e `drainPendingUceEvents()`.
5. Adicionar `GwSpiBus::pollEvent(...)` ou equivalente para leitura dummy de 64 bytes sem comando de API.
6. Encaminhar o pacote para API com `SdgwLink::sendEvent(...)` usando comando compacto da UCE.
7. Na API, fazer `UceClient` assinar `SdgwSession.EventReceived`.
8. So depois disso ligar CAN RX nesse mecanismo, com `CAN_RX_EVENT` little-endian e fila CAN RX propria.

Essa ordem preserva SPI, COBS, CRC, LED, GSA e CAN funcional, e separa a validacao do enlace assíncrono da complexidade do CAN.
