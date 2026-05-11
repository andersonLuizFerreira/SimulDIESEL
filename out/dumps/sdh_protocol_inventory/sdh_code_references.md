# Referencias de codigo SDH

## Contratos e DAL

| Arquivo | Classes/metodos | Conteudo relevante |
|---|---|---|
| `DTL/Protocols/SDGW/SdhCommand.cs` | `SdhCommand` | DTO de comando SDH; `Version`, `Target`, `Op`, `Args`, `Meta`. |
| `DTL/Protocols/SDGW/SdhResponse.cs` | `SdhResponse` | DTO de resposta SDH documental; nao e usado como resposta principal de board. |
| `DTL/Protocols/SDGW/SdhTarget.cs` | `SdhTarget.Parse` | Divide `Board.resource.subresource`. |
| `DTL/Protocols/SDGW/GwProtocol.cs` | constantes | Enderecos, ops, TLV types, payload lengths, status e masks. |
| `DTL/Protocols/SDGW/SdgwCommand.cs` | enum | `Ping 0x55`, `Ack 0xF1`, `Err 0xF2`. |
| `DTL/Protocols/SDGW/SdgwFrame.cs` | `SdgwFrame` | Frame logico sem CRC/COBS. |
| `DAL/Protocols/SDGW/SdhTextParser.cs` | `Parse` | Parser textual `sdh/1 target op chave=valor`. |
| `DAL/Protocols/SDGW/SdhTextSerializer.cs` | `Serialize` | Serializador textual; args ordenados por chave. |
| `DAL/Protocols/SDGW/SdhValidator.cs` | `Validate*` | Catalogo aceito e validacao de argumentos. |
| `DAL/Protocols/SDGW/SdhToSdgwMapper.cs` | `Map*`, `BuildTlvPayload` | Traducao SDH -> SDGW/TLV. |
| `DAL/Protocols/SDGW/SdhClient.cs` | `SendAsync`, `SendTextAsync`, `ToText` | Entrada semantica SDH. |
| `DAL/Protocols/SDGW/SdgwSession.cs` | `SendAsync`, `FrameReceived`, `EventReceived` | Sessao SDGW e eventos por flag. |
| `DAL/Protocols/SDGW/SdGwTxScheduler.cs` | `EnqueueAsync` | Agendamento TX. |
| `DAL/Protocols/SDGW/SdgwLinkEngine.cs` | framing/app frames | Link engine e estados de transporte. |

## BPM

| Arquivo | Classes/metodos | Conteudo relevante |
|---|---|---|
| `BLL/Boards/BPM/BpmClient.cs` | `PingGatewayAsync` | Cria `SdhCommand Target=BPM.gateway Op=ping`. |
| `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs` | `Shared`, `BoardDispatcher`, `Sdctp` | Monta servicos compartilhados, transporte serial/Bluetooth e dispatchers. |
| `BLL/Boards/BPM/Comm/SdgwHostSession.cs` | sessao host | Conecta engine/sessao SDGW no host. |
| `DTL/Boards/BPM/BpmStatusDto.cs` | `BpmStatusDto` | Status local de link/transporte. |

## GSA

| Arquivo | Classes/metodos | Conteudo relevante |
|---|---|---|
| `BLL/Boards/GSA/GsaClient.cs` | `SetBuiltinLedAsync`, `SetChannelSetpointAsync`, `SetChannelEnableAsync`, `SetChannelsEnableAsync`, `GetChannelStatusAsync`, `GetChannelsStatusAsync`, `ResetChannelFaultAsync`, offsets | Cria comandos SDH GSA e executa transacoes. |
| `BLL/Boards/GSA/GsaParsers.cs` | `TryRead*` | Parse de TLVs GSA, erros e eventos. |
| `BLL/Boards/GSA/GsaDispatcher.cs` | eventos e fachada | Encaminha eventos e metodos publicos. |
| `BLL/FormsLogic/GSA/FrmGsaLogic.cs` | forms logic | Consome dispatcher GSA para UI. |
| `DTL/Boards/GSA/GsaRequests.cs` | request DTOs | Requisicoes tipadas BLL/UI. |
| `DTL/Boards/GSA/GsaResponses.cs` | response DTOs | Respostas tipadas. |
| `DTL/Boards/GSA/GsaCommon.cs` | enums/event DTOs | `GsaOffsetKind`, erros, eventos. |

## UCE

| Arquivo | Classes/metodos | Conteudo relevante |
|---|---|---|
| `BLL/Boards/UCE/UceClient.cs` | LED, CAN config/enable/status/reset/rx/driverLog/tx | Cria comandos SDH UCE e executa transacoes. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryRead*` | Parse de TLVs UCE, erros e eventos. |
| `BLL/Boards/UCE/UceDispatcher.cs` | eventos e fachada | Encaminha eventos e metodos publicos. |
| `BLL/Boards/UCE/UceGatewayDiagnosticLog.cs` | diagnosticos | Log de erros gateway/CAN/transport. |
| `BLL/FormsLogic/UCE/FrmUceLogic.cs` | forms logic | Usa `UceDispatcher`/`SdctpApiService`; CAN/J1939 acima da UCE. |
| `DTL/Boards/UCE/UceCanProtocol.cs` | enums/conversoes | Controller, mode, bitrate, status. |
| `DTL/Boards/UCE/UceCanResponses.cs` | response/event DTOs | CAN/LED responses e eventos. |
| `DTL/Boards/UCE/Can/*.cs` | CAN DTOs | `CanCreateDto`, `CanEditDto`, `CanDeleteDto`, `CanRowDto`, `CanFrameDto`, `CanReadAllResponseDto`, `CanTicDto`, `CanTxRowDto`. |

## CAN/SDCTP sobre UCE

| Arquivo | Conteudo relevante |
|---|---|
| `BLL/Services/CAN/ApiCanService.cs` | Servico CAN legado/alto nivel; solicita readAll, processa eventos UCE. |
| `BLL/Services/CAN/CanEventProcessor.cs` | Decodifica CAN_CREATE/EDIT/DELETE/ROW/READ_ALL_DONE/TIC. |
| `BLL/Services/CAN/CanRxMirrorManager.cs` | Tabela espelho CAN RX. |
| `BLL/Services/CAN/CanRxOutputBuffer.cs` | Buffer de frames RX para consumidores. |
| `BLL/Services/CAN/CanTxManager.cs` | Gerencia TX CAN. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | Fronteira atual recomendada para CAN/SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpProtocol.cs` | Alias dos TLVs UCE CAN/SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs` | Facade para eventos SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpRxMirrorManager.cs` | Mirror SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpRxOutputBuffer.cs` | Output buffer SDCTP. |

## Firmware e documentacao correlata

| Caminho | Conteudo |
|---|---|
| `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h` | Defines UCE/TLV como `CMD_CAN_READ_ALL 0x43`, `CMD_CAN_CREATE 0x40`, etc. |
| `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp` | Despacho interno UCE. |
| `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp` | CAN_READ_ALL, publish RX events, CAN_READ_ALL_DONE. |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/*` | SDGW transport/router/link no firmware BPM. |
| `docs/official/06-protocolos/01-sdh-command-model.md` | Modelo documental SDH. |
| `docs/official/06-protocolos/02-sdh-response-model.md` | Modelo documental de resposta/JSON. |
| `docs/official/06-protocolos/06-gsa-sdh-tlv.md` | Contrato GSA SDH/TLV. |
| `docs/official/06-protocolos/07-uce-sdh-tlv.md` | Contrato UCE SDH/TLV. |

## Exemplos encontrados

Confirmados em codigo:

```text
sdh/1 BPM.gateway ping
sdh/1 GSA.led set state=on
sdh/1 GSA.channel.setpoint set channel=6 value=128
sdh/1 UCE.can.config set controller=can0 bitrate=250 mode=normal
sdh/1 UCE.can.tx direct controller=can0 extended=1 rtr=0 id=419364352 dlc=8 d0=0 ...
```

Somente documentais / NAO CONFIRMADO NO CODIGO host:

```text
sdh/1 PSU.power.main set state=on
sdh/1 UCO.can1 cfg bitrate=250000 mode=normal
sdh/1 URL.relay3 status
sdh/1 UIOD.di1 read
sdh/1 GSC.signal1 cfg mode=pulse freq=1000 duty=50
```

