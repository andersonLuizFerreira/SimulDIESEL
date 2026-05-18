# ETAPA 10 - SDCTP CAN Transport Protocol

## 1. Definicao do SDCTP

SDCTP significa SimulDIESEL CAN Transport Protocol.

O SDCTP e o protocolo de transporte CAN oficial do SimulDIESEL. Ele encapsula a logica RX/TX CAN validada, consome dados da camada inferior e oferece um servico CAN transparente para a camada superior, tanto na UCE quanto na API.

## 2. Objetivo

O objetivo do SDCTP e separar responsabilidades:

- abstrair CanDriver real/fake, SDGW, UceDispatcher e UceClient dos consumidores superiores;
- preservar os modos RX AUTO e DIRECT_ONLY;
- preservar compressao por tabela no RX;
- preservar reconstrucao por CAN_TIC;
- preservar TX_DIRECT e TX_TABLE;
- preservar CanRxOutputBuffer como saida oficial da API;
- manter diagnosticos operacionais do transporte CAN.

Esta etapa organiza o protocolo com wrappers SDCTP sobre as classes validadas, evitando renomeacao agressiva no caminho critico.

## 3. Camadas

### UCE

Camada inferior:

- CanDriver real/fake;
- buffers fisicos RX/TX.

Protocolo:

- SdctpService;
- SdctpCodec/SdctpProtocol;
- SdctpRxTableManager;
- SdctpTxTableManager;
- SdctpTypes;
- implementacao validada mantida em CanService, CanCrudProtocol, CanRxHub, CanRxTableManager e CanTxTableManager.

Camada superior:

- UceServiceDispatcher;
- comandos TLV vindos/indo para API.

### API

Camada inferior:

- SDGW;
- UceDispatcher;
- UceClient.

Protocolo:

- SdctpApiService;
- SdctpEventProcessor;
- SdctpRxMirrorManager;
- SdctpRxOutputBuffer;
- SdctpTxManager;
- SdctpProtocol;
- SdctpDiagnostics;
- SdctpTypes;
- implementacao validada mantida em ApiCanService, CanEventProcessor, CanRxMirrorManager, CanRxOutputBuffer e CanTxManager.

Camada superior:

- UI;
- consumidores futuros;
- service tools;
- modulos de diagnostico.

## 4. Fluxo RX

### UCE

CanDriver RX -> SDCTP RX Engine -> modo AUTO ou DIRECT_ONLY -> TLVs SDCTP para UceServiceDispatcher.

No codigo atual, SdctpService e a entrada oficial do protocolo e delega para o CanService validado.

### API

TLVs SDCTP vindos da UCE -> SdctpApiService/ApiCanService -> mirror/output buffer -> consumidores superiores.

O espelho RX e estado interno do protocolo. A saida oficial para consumo de frames e TryReadRxFrame.

## 5. Fluxo TX

### UCE

TLVs SDCTP vindos do UceServiceDispatcher -> SDCTP TX Engine -> CanDriver TX.

### API

Consumidor superior -> SdctpApiService/ApiCanService -> TLVs SDCTP -> UceDispatcher/SDGW -> UCE.

## 6. Modos

### AUTO

Modo com compressao por tabela RX:

- ID novo com slot livre: CAN_CREATE;
- repeticao sem alteracao: CAN_TIC;
- alteracao: CAN_EDIT com DATA_MASK;
- timeout: CAN_DELETE;
- tabela cheia: fallback CAN_RX_EVENT 0x28.

### DIRECT_ONLY

Modo direto:

- todos os frames RX sao enviados por CAN_RX_EVENT 0x28;
- nao usa CREATE/EDIT/TIC/DELETE para reconstruir a saida.

## 7. Comandos TLV

RX:

- CAN_RX_EVENT: 0x28;
- CAN_CREATE: 0x40;
- CAN_EDIT: 0x41;
- CAN_DELETE: 0x42;
- CAN_READ_ALL: 0x43;
- CAN_ROW: 0x44;
- CAN_READ_ALL_DONE: 0x45;
- CAN_TIC: 0x46.

TX:

- CAN_TX_DIRECT: 0x50;
- CAN_TX_CREATE: 0x51;
- CAN_TX_EDIT: 0x52;
- CAN_TX_DELETE: 0x53.

Diagnostico:

- CMD_TRANSPORT_DIAG: 0x7E.

## 8. Formatos de payload

### CAN_RX_EVENT 0x28

Payload:

- controller: 1 byte;
- frame_count: 1 byte;
- frames: ate 1 frame no evento assíncrono atual.

Frame CAN_RX_EVENT:

- id: uint32 little-endian;
- flags: bit0 EXT, bit1 RTR;
- dlc: 1 byte;
- data[8].

### CAN_CREATE 0x40 e CAN_ROW 0x44

Payload de 21 bytes:

- index: 1 byte;
- flags: 1 byte;
- can_id: uint32 little-endian;
- dlc: 1 byte;
- data[8];
- cycle_time: uint16 little-endian;
- message_order: uint32 little-endian.

### CAN_EDIT 0x41

Payload minimo de 6 bytes:

- index: 1 byte;
- mask: 1 byte;
- message_order: uint32 little-endian;
- campos variaveis conforme mask.

Mascara:

- 0x01 FLAGS;
- 0x02 CAN_ID;
- 0x04 DLC;
- 0x08 DATA;
- 0x10 CYCLE_TIME.

Quando DATA esta ativo, o payload validado usa DATA_MASK seguido apenas dos bytes alterados. A API tambem aceita o formato legado com DATA[8] quando o tamanho e compativel.

### CAN_TIC 0x46

Payload de 1 byte:

- index.

### CAN_DELETE 0x42

Payload de 6 bytes:

- index: 1 byte;
- reason: 1 byte;
- message_order: uint32 little-endian.

### CAN_READ_ALL 0x43

Payload vazio.

### CAN_READ_ALL_DONE 0x45

Payload de 5 bytes:

- count: 1 byte;
- message_order: uint32 little-endian.

### CAN_TX_DIRECT 0x50

Payload de 14 bytes:

- flags: 1 byte;
- can_id: uint32 little-endian;
- dlc: 1 byte;
- data[8].

### CAN_TX_CREATE 0x51

Payload de 18 bytes:

- index: 1 byte;
- flags: 1 byte;
- can_id: uint32 little-endian;
- dlc: 1 byte;
- data[8];
- period_ms: uint16 little-endian;
- enabled: 1 byte.

### CAN_TX_EDIT 0x52

Payload variavel:

- index: 1 byte;
- mask: 1 byte;
- campos variaveis conforme mask.

Mascara TX:

- 0x01 FLAGS;
- 0x02 CAN_ID;
- 0x04 DLC;
- 0x08 DATA, com DATA_MASK e bytes alterados;
- 0x10 PERIOD_MS;
- 0x20 ENABLED.

### CAN_TX_DELETE 0x53

Payload de 2 bytes:

- index: 1 byte;
- reason: 1 byte.

## 9. Tabela RX

A tabela RX pertence ao SDCTP. Ela comprime frames CAN repetitivos em eventos CREATE, EDIT, TIC e DELETE. A tabela espelho na API existe para reconstruir frames e para exibicao/diagnostico, mas nao substitui a saida oficial.

Regras preservadas:

- CREATE para ID novo com slot;
- TIC para repeticao sem alteracao;
- EDIT para alteracao de DLC/DATA/campos;
- DELETE por timeout;
- fallback direto por CAN_RX_EVENT quando a tabela esta cheia.

## 10. Tabela TX

A tabela TX pertence ao SDCTP UCE. A API envia CAN_TX_CREATE, CAN_TX_EDIT e CAN_TX_DELETE; a UCE transmite ciclicamente localmente.

Regras preservadas:

- TX_DIRECT transmite imediatamente;
- TX_TABLE cria/edita/remove linhas ciclicas;
- nao existe TIC_TX;
- o periodo e controlado localmente pela UCE.

## 11. CanRxOutputBuffer

CanRxOutputBuffer e a saida oficial do SDCTP API.

Consumidores superiores devem usar TryReadRxFrame para receber frames CAN. A tabela espelho e estado interno do protocolo e pode ser exibida pela UI, mas nao e a fila oficial de consumo.

## 12. Diagnosticos

Diagnosticos preservados:

- DISPATCHER FIFO OVERFLOW;
- MIRROR_OUT_OF_SYNC;
- CAN_TIC invalido;
- CAN_EDIT truncado;
- DATA_MASK invalido;
- TABLE_FULL, quando aplicavel;
- OutputBuffer overflow.

## 13. Validacoes realizadas

Resultados validados antes da organizacao SDCTP:

RX:

- DIRECT_ONLY: 5000;
- AUTO: 5000;
- matches: 5000;
- mismatches: 0;
- lost: 0;
- extra: 0.

TX:

- TX_DIRECT loopback: 5000/5000;
- TX_TABLE loopback: 5000/5000;
- mismatches: 0;
- lost: 0;
- extra: 0;
- FIFO overflow: 0;
- OutputBuffer overflow: 0.

## 14. Limitacoes pendentes

- Renomeacao interna completa ainda nao foi feita para evitar risco no caminho validado.
- CanService, CanCrudProtocol, CanEventProcessor, CanRxMirrorManager, CanRxOutputBuffer e CanTxManager continuam existindo como implementacoes validadas.
- O modo LOOPBACK valida o fluxo SDCTP sem barramento fisico, mas teste eletrico em rede CAN real continua sendo uma validacao de bancada separada.

## 15. Adocao arquitetural na API

A camada API passou a adotar `SdctpApiService` como fronteira oficial entre `UceDispatcher`/SDGW e os consumidores superiores.

Regras de adocao:

- `BpmSerialService` expoe `SdctpApiService` pela propriedade `Sdctp`.
- `ApiCanService` continua existindo como implementacao interna validada, encapsulada por `SdctpApiService`.
- `FrmUceLogic` consome `SdctpApiService` para operacoes CAN de RX, TX, snapshot e diagnostico.
- O envio manual one-shot usa `CAN_TX_DIRECT 0x50` por `SdctpApiService.SendDirectAsync`.
- O envio periodico da UI usa tabela TX SDCTP por `CAN_TX_CREATE 0x51`.
- A parada do envio periodico usa remocao da linha TX SDCTP por `CAN_TX_DELETE 0x53`.
- `CAN_TX 0x26` permanece no codigo apenas como compatibilidade/legado.
- Consumidores superiores devem ler frames recebidos por `TryReadRxFrame(out CanFrameDto frame)`, que consome o `CanRxOutputBuffer` oficial.
- A UI de monitoramento CAN consome `TryReadRxFrame` e mantem tabela local propria, sem usar o mirror SDCTP como fonte visual principal.
- A tabela espelho RX permanece como estado interno do protocolo e fonte de diagnostico/sincronismo, mas nao e a saida oficial do fluxo CAN.

## 16. Loopback de producao no CanDriver real

O modo `LOOPBACK` foi incorporado ao `CanDriver` real como modo de operacao controlado por configuracao CAN da UCE.

Fluxo:

- API/UI envia configuracao CAN com modo `loopback`.
- SDCTP UCE configura o `CanDriver` real com modo `0x02`.
- No modo `LOOPBACK`, o driver nao consome o barramento CAN fisico.
- Cada `send(frame)` aceito e reenfileirado na fila RX local do proprio driver.
- O polling RX do SDCTP consome essa fila e publica o frame pelo fluxo normal SDCTP RX.

Esse caminho preserva o fluxo completo:

`API TX -> SDCTP API -> TLV SDCTP -> SDCTP UCE -> CanDriver.send -> RX local -> SDCTP RX -> CanRxOutputBuffer -> TryReadRxFrame`.

O `CanDriver_fake` nao faz parte do fluxo principal nem dos ambientes PlatformIO de producao. Os arquivos antigos podem permanecer no repositorio como legado descontinuado, mas o seletor oficial usa sempre `CanDriver`.
