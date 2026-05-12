# Catalogo de eventos assincronos SDH/SDGW/board

Eventos no host sao publicados por `SdgwSession.EventReceived` quando `frame.Flags & 0x02 != 0`. Os clients de board filtram pelo command compacto da board e interpretam TLVs especificos.

## GSA

### GSA Channel Fault Event

- EventName: `GsaChannelFaultEvent`
- EventCode/TLV: `GsaChannelFaultEventType 0x30`
- Board: GSA
- Direction: Assincrono Board -> API
- Payload: len `0x06`: `channel`, `setpoint`, `voltageRead`, `currentRead`, `enabled`, `fault`
- DTO: `GsaChannelFaultEvent : GsaChannelSnapshot`
- Parser: `GsaParsers.TryReadChannelFaultEvent`
- SourceFiles: `GwProtocol.cs`, `GsaClient.cs`, `GsaParsers.cs`, `GsaCommon.cs`
- UsedBy: `GsaDispatcher.ChannelFaultEventReceived`, `FrmGsaLogic`
- DatabaseRelevance: alto para historico de falhas, validacao de teste e diagnostico.

### GSA Physical Operation Event

- EventName: `GsaPhysicalOperationEvent`
- EventCode/TLV: `GsaPhysicalOperationEventType 0x31`
- Board: GSA
- Direction: Assincrono Board -> API
- Payload: len `0x03`: `originType`, `channel`, `status`
- DTO: `GsaPhysicalOperationEvent`
- Parser: `GsaParsers.TryReadPhysicalOperationEvent`
- Status: `Ok 0x01`, `TcaNoAck 0x02`, `McpNoAck 0x03`
- SourceFiles: `GwProtocol.cs`, `GsaClient.cs`, `GsaParsers.cs`, `GsaCommon.cs`
- UsedBy: `GsaDispatcher.PhysicalOperationEventReceived`, `FrmGsaLogic`
- DatabaseRelevance: alto para auditoria fisica e rastreabilidade de execucao.

## UCE

### UCE LED Event

- EventName: `UceLedEvent`
- EventCode/TLV: `UceLedEventType 0x13`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: len `0x04`: `ledState`, `eventCode`, `counterLo`, `counterHi`
- DTO: `UceLedEvent`
- Parser: `UceParsers.TryReadLedEvent`
- SourceFiles: `GwProtocol.cs`, `UceClient.cs`, `UceParsers.cs`, `UceLedResponse.cs`
- UsedBy: `UceDispatcher.LedEventReceived`, `FrmUceLogic`, `frmUCE_UI`
- DatabaseRelevance: baixo/medio para diagnostico de board.

### UCE CAN_RX Event

- EventName: `UceCanRxEvent`
- EventCode/TLV: `UceCanRxEventType 0x28`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: header len `0x02`: `controller`, `count`; seguido de ate `UceCanRxEventMaxFrames = 1` frame com len `0x0E`
- Frame payload: `id32`, `flags`, `dlc`, `data[8]`
- DTO: `UceCanRxEvent` com lista de `UceCanFrame`
- Parser: `UceParsers.TryReadCanRxEvent`
- SourceFiles: `GwProtocol.cs`, `UceClient.cs`, `UceParsers.cs`, `ApiCanService.cs`, `SdctpApiService.cs`
- UsedBy: `CanEventProcessor.ProcessCanRxEvent`, output buffers CAN/SDCTP, UI UCE
- DatabaseRelevance: muito alto para captura temporal CAN/J1939.

### CAN_CREATE

- EventName: `CAN_CREATE`
- EventCode/TLV: `UceCanCreateType 0x40`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: len `0x15` conforme `CanCreateDto`
- DTO: `CanCreateDto`: `Index`, `Valid`, `Flags`, `CanId`, `Dlc`, `Data[8]`, `CycleTime`, `MessageOrder`
- Parser/Processor: `UceParsers.TryReadCanCrudEvent`, `CanEventProcessor`, `SdctpEventProcessor`
- SourceFiles: `GwProtocol.cs`, `UceParsers.cs`, `CanEventProcessor.cs`, `SdctpProtocol.cs`
- DatabaseRelevance: muito alto para mirror CAN e perfis detectados.

### CAN_EDIT

- EventName: `CAN_EDIT`
- EventCode/TLV: `UceCanEditType 0x41`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: variavel, minimo `0x06`, max `0x17`; campos condicionais por mask
- DTO: `CanEditDto`
- Parser/Processor: `CanEventProcessor`; diagnosticos `CAN_EDIT_TRUNCATED`, `CAN_EDIT_INVALID_DATA_MASK`, `CAN_EDIT_INVALID_PAYLOAD`
- SourceFiles: `GwProtocol.cs`, `CanEventProcessor.cs`, `SdctpDiagnostics.cs`
- DatabaseRelevance: muito alto para rastrear mudancas em mensagens detectadas.

### CAN_DELETE

- EventName: `CAN_DELETE`
- EventCode/TLV: `UceCanDeleteType 0x42`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: len `0x06`; `CanDeleteDto.Index`, `Reason`, `MessageOrder`
- Reasons: `Timeout 0x01`, `Reset 0x02`, `TableClear 0x03`, `ManualDelete 0x04`
- Parser/Processor: `CanEventProcessor`, `SdctpEventProcessor`
- DatabaseRelevance: alto para expiracao/limpeza de mensagens e perfil temporal.

### CAN_READ_ALL / CAN_ROW / CAN_READ_ALL_DONE

- EventName: `CAN_READ_ALL`
- EventCode/TLV request: `UceCanReadAllType 0x43`
- Direction: API -> Board para solicitacao; Board -> API para resposta/eventos relacionados
- RequestPayload: len `0x00` no mapper
- ResponsePayload: `UceCanReadAllResponse.Accepted`
- Events:
  - `CAN_ROW 0x44`: payload igual a `CanRowDto`, len `0x15`
  - `CAN_READ_ALL_DONE 0x45`: payload len `0x05`, processado como fim de snapshot
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `CanEventProcessor.cs`, `SdctpProtocol.cs`, firmware `CanService.cpp`
- Notes: `readAll` existe no client/mapper; `SdhValidator` atual nao aceita `Op=readAll` para `UCE.can.rx`.
- DatabaseRelevance: muito alto para snapshot inicial e sincronizacao de tabela.

### CAN_TIC

- EventName: `CAN_TIC`
- EventCode/TLV: `UceCanTicType 0x46`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: len `0x01`; `CanTicDto.Index`
- Parser/Processor: `CanEventProcessor`, `SdctpEventProcessor`
- DatabaseRelevance: medio/alto para atividade temporal de linha CAN.

### UCE Transport Diagnostic

- EventName: `UceDispatcherOverflowDiagnostic`
- EventCode/TLV: `UceTransportDiagType 0x7E`
- Board: UCE
- Direction: Assincrono Board -> API
- Payload: len `0x07`: diagnostic type, overflow count uint32 LE, queue size, max event size
- Diagnostic subtype: `UceTransportDiagDispatcherFifoOverflow 0x01`
- Parser: `UceParsers.TryReadTransportDiagnosticEvent`
- UsedBy: `UceGatewayDiagnosticLog`, `FrmUceLogic`
- DatabaseRelevance: medio para confiabilidade da captura.

## Erros

| Nome | TLV | Board | Payload | Parser |
|---|---:|---|---|---|
| GSA functional error | `0x7F` | GSA | requestType, channel, errorCode | `GsaParsers.TryReadFunctionalError` |
| UCE functional error | `0x7F` | UCE | requestType, ?, errorCode | `UceParsers.TryReadFunctionalError` |
| Gateway error | `0xFE` | BPM/Gateway | codigo e dados opcionais | `GsaParsers.TryReadGatewayError`, `UceParsers.TryReadGatewayError` |

## Eventos mencionados mas nao confirmados como SDH host

- Eventos especificos de SPI/BPM firmware existem em docs/firmware, mas nao aparecem como `SdhCommand` host.
- J1939 e processado acima do CAN no host; nao ha evento SDH J1939 dedicado encontrado. A origem J1939 atual e `UceCanRxEvent`/frames CAN.

