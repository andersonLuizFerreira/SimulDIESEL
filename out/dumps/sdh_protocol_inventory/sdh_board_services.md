# Mapeamento por board e por servico

## BPM

Endereco: `GwProtocol.BpmAddress = 0x0`

| Servico | Comandos | Codigos | Classes |
|---|---|---|---|
| gateway | `BPM.gateway ping` | compact cmd `0x00`, op `BpmPingOp 0x0` | `BpmClient`, `SdhToSdgwMapper`, `SdhValidator` |
| serial/bluetooth/link | Nao exposto como SDH funcional, mas presente no host | NAO CONFIRMADO NO CODIGO como comando SDH | `BpmSerialService`, `BluetoothTransport`, `SerialTransport`, `SdgwHostSession` |
| backplane/xconn | Servicos BLL existem, comandos SDH nao confirmados | NAO CONFIRMADO NO CODIGO | `BackplaneService`, `XConnService` |

Observacao: BPM e tambem gateway SDGW. No host, `BpmClient.GetStatus()` le estado local de conexao/link/transporte, nao comando SDH.

## GSA

Endereco: `GwProtocol.GsaAddress = 0x1`; operacao compacta `GsaTlvTransactOp = 0x0`; command compact `0x10`.

| Servico | Comandos SDH | TLV type | Request | Response/event |
|---|---|---:|---|---|
| led | `GSA.led set` | `0x12` | `state` | `GsaLedResponse` |
| channel.setpoint | `GSA.channel.setpoint set` | `0x10` | `channel`, `value` | `GsaChannelSetpointResponse` |
| channel.enable | `GSA.channel.enable set` | `0x11` | `channel`, `state` | `GsaChannelEnableResponse` |
| channels.enable | `GSA.channels.enable set` | `0x14` | `state` | `GsaChannelsEnableResponse` |
| channel.status | `GSA.channel.status get` | `0x1B` | `channel` | `GsaChannelStatusResponse` |
| channels.status | `GSA.channels.status get` | `0x13` | vazio | `GsaChannelsStatusResponse` |
| channel.fault | `GSA.channel.fault reset` | `0x15` | `channel` | `GsaChannelFaultResetResponse` |
| channel.offset | `set|get|save|reset` | `0x16..0x19` | `channel`, `kind`, `value` conforme op | offset/save/reset DTOs |
| offset | `GSA.offset reset` | `0x1A` | vazio | `GsaOffsetResetResponse` |
| async fault | nao e comando SDH | `0x30` | Board -> API | `GsaChannelFaultEvent` |
| async physical op | nao e comando SDH | `0x31` | Board -> API | `GsaPhysicalOperationEvent` |
| functional error | resposta de erro | `0x7F` | Board -> API | `GsaFunctionalErrorResponse` |

Servicos internos host:

- `GsaClient`: monta `SdhCommand`, executa transacao e interpreta resposta.
- `GsaDispatcher`: fachada/event bridge.
- `GsaParsers`: interpreta TLV de resposta/evento.
- `FrmGsaLogic`: camada de forms logic para UI.

## UCE

Endereco: `GwProtocol.UceAddress = 0x2`; operacao compacta `UceTlvTransactOp = 0x0`; command compact `0x20`.

| Servico | Comandos SDH | TLV type | Request | Response/event |
|---|---|---:|---|---|
| led | `UCE.led set` | `0x12` | `state` | `UceLedResponse` |
| led event | nao e comando SDH | `0x13` | Board -> API | `UceLedEvent` |
| can.config | `UCE.can.config set` | `0x20` | `controller`, `bitrate`, `mode`, possivel `rxMode` | `UceCanConfigResponse` |
| can.enable | `UCE.can.enable set` | `0x21` | `controller`, `state` | `UceCanEnableResponse` |
| can.status | `UCE.can.status get` | `0x22` | `controller` | `UceCanStatusResponse` |
| can.reset | `UCE.can reset` | `0x23` | `controller` | `UceCanResetResponse` |
| can.rx poll | `UCE.can.rx poll` | `0x24` | `controller` | `UceCanRxPollResponse` |
| can.driverLog | `UCE.can.driverLog poll` | `0x25` | `controller` | `UceCanDriverLogPollResponse` |
| can.tx legado | `UCE.can.tx send` | `0x26` | frame + period | `UceCanTxResponse` |
| can.tx stop | `UCE.can.tx stop` | `0x27` | `controller`, `slot` | `UceCanTxStopResponse` |
| CAN_RX event | nao e comando SDH | `0x28` | Board -> API | `UceCanRxEvent` |
| CAN TX direct | `UCE.can.tx direct` | `0x50` | frame | `UceCanTxResponse` |
| CAN TX create | `UCE.can.tx create` | `0x51` | row completa | `UceCanTxResponse` |
| CAN TX edit | `UCE.can.tx edit` | `0x52` | row parcial por mask | `UceCanTxResponse` |
| CAN TX delete | `UCE.can.tx delete` | `0x53` | `index`, `reason` | `UceCanTxResponse` |
| CAN CRUD create | nao e comando SDH direto | `0x40` | Board -> API | `CanCreateDto` via processors |
| CAN CRUD edit | nao e comando SDH direto | `0x41` | Board -> API | `CanEditDto` |
| CAN CRUD delete | nao e comando SDH direto | `0x42` | Board -> API | `CanDeleteDto` |
| CAN read all | `UCE.can.rx readAll` no client/mapper | `0x43` | sem payload TLV no mapper | `UceCanReadAllResponse`, eventos `0x44/0x45` |
| CAN row | nao e comando SDH | `0x44` | Board -> API | `CanRowDto` |
| CAN read all done | nao e comando SDH | `0x45` | Board -> API | `CanReadAllResponseDto` |
| CAN tic | nao e comando SDH | `0x46` | Board -> API | `CanTicDto` |
| transport diag | nao e comando SDH | `0x7E` | Board -> API | `UceDispatcherOverflowDiagnostic` |
| functional error | resposta de erro | `0x7F` | Board -> API | mensagem funcional |

Servicos internos host:

- `UceClient`: monta comandos SDH e parseia respostas/eventos.
- `UceDispatcher`: fachada/event bridge.
- `UceParsers`: interpreta TLVs de UCE.
- `ApiCanService` e `SdctpApiService`: fronteiras de servico CAN/SDCTP sobre UCE.
- `CanEventProcessor`, `CanRxMirrorManager`, `SdctpEventProcessor`, `SdctpRxMirrorManager`: processam eventos CAN/SDCTP.
- `FrmUceLogic`: camada de forms logic para UI.

## Boards documentais sem suporte SDH host confirmado

| Board | Evidencia | Status |
|---|---|---|
| PSU | docs oficiais de boards/exemplos | NAO CONFIRMADO NO CODIGO host |
| UCO | docs oficiais de boards/exemplos | NAO CONFIRMADO NO CODIGO host |
| URL | docs oficiais de boards/exemplos | NAO CONFIRMADO NO CODIGO host |
| UIOD | docs oficiais de boards/exemplos | NAO CONFIRMADO NO CODIGO host |
| GSC | docs oficiais de boards/exemplos | NAO CONFIRMADO NO CODIGO host |

