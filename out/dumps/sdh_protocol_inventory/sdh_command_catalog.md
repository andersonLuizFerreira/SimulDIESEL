# Catalogo de comandos SDH encontrados

Formato base: `sdh/1 <target> <op> chave=valor ...`  
CommandCode abaixo e o comando compacto SDGW quando aplicavel: `GwProtocol.MakeCompactCommand(address, op)`. Para GSA/UCE, o `TLVMapping` identifica o type interno transportado no payload.

## BPM

### BPM.gateway ping

- CommandName: `BPM.gateway ping`
- CommandCode: `0x00` (`BpmAddress 0x0`, `BpmPingOp 0x0`)
- Board: BPM
- Service: gateway
- Direction: API -> Board
- RequestPayload: vazio
- ResponsePayload: ACK SDGW; DTO final `BpmCommandResult`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: nenhum TLV; payload `Array.Empty<byte>()`
- SourceFiles: `BLL/Boards/BPM/BpmClient.cs`, `DAL/Protocols/SDGW/SdhValidator.cs`, `DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
- UsedBy: `BpmClient.PingGatewayAsync()`
- Notes: timeout 150 ms, 1 retry no mapper.
- DatabaseRelevance: baixo para perfil de modulo; alto para diagnostico de conectividade da bancada.

## GSA

### GSA.led set

- CommandName: `GSA.led set`
- CommandCode: `0x10`
- Board: GSA
- Service: led
- Direction: API -> Board
- RequestPayload: `state=on|off`
- ResponsePayload: `GsaLedResponse.AcceptedState`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaSetLedType 0x12`, payload `[state: 0|1]`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`, `GwProtocol.cs`
- UsedBy: `GsaClient.SetBuiltinLedAsync()`, `FrmGsaLogic`
- Notes: comando operacional simples.
- DatabaseRelevance: baixo; util para teste de presenca/atuacao basica.

### GSA.channel.setpoint set

- CommandName: `GSA.channel.setpoint set`
- CommandCode: `0x10`
- Board: GSA
- Service: channel.setpoint
- Direction: API -> Board
- RequestPayload: `channel=1..16`, `value=0..255`
- ResponsePayload: `GsaChannelSetpointResponse.Channel`, `AcceptedValue`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelSetpointType 0x10`, payload `[channel, value]`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.SetChannelSetpointAsync()`, `FrmGsaLogic`
- Notes: controla setpoint analogico por canal.
- DatabaseRelevance: alto para sinais eletricos e perfis de modulo.

### GSA.channel.enable set

- CommandName: `GSA.channel.enable set`
- CommandCode: `0x10`
- Board: GSA
- Service: channel.enable
- Direction: API -> Board
- RequestPayload: `channel=1..16`, `state=on|off`
- ResponsePayload: `GsaChannelEnableResponse.Channel`, `AcceptedState`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelEnableType 0x11`, payload `[channel, state]`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.SetChannelEnableAsync()`
- Notes: habilita/desabilita canal individual.
- DatabaseRelevance: alto para configuracao de hardware/sinais.

### GSA.channels.enable set

- CommandName: `GSA.channels.enable set`
- CommandCode: `0x10`
- Board: GSA
- Service: channels.enable
- Direction: API -> Board
- RequestPayload: `state=on|off`
- ResponsePayload: `GsaChannelsEnableResponse.RequestedState`, `AffectedCount`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelsEnableType 0x14`, payload `[state]`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.SetChannelsEnableAsync()`
- Notes: enable global.
- DatabaseRelevance: medio/alto para aplicar perfil de bancada.

### GSA.channel.status get

- CommandName: `GSA.channel.status get`
- CommandCode: `0x10`
- Board: GSA
- Service: channel.status
- Direction: API -> Board
- RequestPayload: `channel=1..16`
- ResponsePayload: `GsaChannelStatusResponse`: `Channel`, `Setpoint`, `VoltageRead`, `CurrentRead`, `Enabled`, `Fault`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelStatusType 0x1B`, payload request `[channel]`, response len `0x06`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.GetChannelStatusAsync()`
- Notes: leitura por canal.
- DatabaseRelevance: alto para captura de estado, validacao e testes.

### GSA.channels.status get

- CommandName: `GSA.channels.status get`
- CommandCode: `0x10`
- Board: GSA
- Service: channels.status
- Direction: API -> Board
- RequestPayload: vazio
- ResponsePayload: `GsaChannelsStatusResponse.Channels`, 16 registros de 6 bytes no parser atual
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelsStatusType 0x13`, request vazio, response len `0x60`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.GetChannelsStatusAsync()`
- Notes: snapshot global GSA.
- DatabaseRelevance: alto para captura temporal e perfil de modulo.

### GSA.channel.fault reset

- CommandName: `GSA.channel.fault reset`
- CommandCode: `0x10`
- Board: GSA
- Service: channel.fault
- Direction: API -> Board
- RequestPayload: `channel=1..16`
- ResponsePayload: `GsaChannelFaultResetResponse.Channel`, `FaultState`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaChannelFaultResetType 0x15`, payload `[channel]`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.ResetChannelFaultAsync()`
- Notes: limpa latch de fault por canal.
- DatabaseRelevance: medio para rotinas de teste/manutencao.

### GSA.channel.offset set/get/save/reset

- CommandName: `GSA.channel.offset set|get|save|reset`
- CommandCode: `0x10`
- Board: GSA
- Service: channel.offset
- Direction: API -> Board
- RequestPayload: `channel=1..16`; `kind=vout|vread|iread` para set/get; `value=int16` para set
- ResponsePayload: set/get `GsaChannelOffsetResponse`; save/reset retornam o `Channel`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `0x16 set`, `0x17 get`, `0x18 save`, `0x19 reset`; kind `vout=0x01`, `vread=0x02`, `iread=0x03`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`, `GsaCommon.cs`
- UsedBy: `GsaClient.Set/Get/Save/ResetChannelOffsetAsync()`
- Notes: offset little-endian via `BitConverter.GetBytes(short)`.
- DatabaseRelevance: alto para calibracao, configuracao eletrica e perfis salvos.

### GSA.offset reset

- CommandName: `GSA.offset reset`
- CommandCode: `0x10`
- Board: GSA
- Service: offset
- Direction: API -> Board
- RequestPayload: vazio
- ResponsePayload: `GsaOffsetResetResponse.AffectedChannels`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `GsaOffsetResetType 0x1A`
- SourceFiles: `GsaClient.cs`, `GsaParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `GsaClient.ResetOffsetsAsync()`
- Notes: reset global.
- DatabaseRelevance: medio para restauracao/limpeza de perfil.

## UCE

### UCE.led set

- CommandName: `UCE.led set`
- CommandCode: `0x20`
- Board: UCE
- Service: led
- Direction: API -> Board
- RequestPayload: `state=on|off`
- ResponsePayload: `UceLedResponse.AcceptedState`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceSetLedType 0x12`, payload `[state]`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.SetBuiltinLedAsync()`, `FrmUceLogic`
- Notes: equivalente ao LED GSA para UCE.
- DatabaseRelevance: baixo; diagnostico de board.

### UCE.can.config set

- CommandName: `UCE.can.config set`
- CommandCode: `0x20`
- Board: UCE
- Service: can.config
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`, `bitrate=5|10|25|50|125|250|500|800|1000`, `mode=normal|listen|loopback`; `rxMode=auto|directOnly` aparece no mapper/client, mas NAO e aceito pelo validator atual
- ResponsePayload: `UceCanConfigResponse.Controller`, `AcceptedBitrateKbps`, `AcceptedMode`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanConfigType 0x20`, payload `[controller, bitrateCode, mode]` ou `[controller, bitrateCode, mode, rxMode]`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `UceCanProtocol.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.SetCanConfigAsync()`, `SdctpApiService`, `FrmUceLogic`
- Notes: divergencia validator vs mapper para `rxMode`.
- DatabaseRelevance: alto para CAN/J1939 e configuracao de hardware.

### UCE.can.enable set

- CommandName: `UCE.can.enable set`
- CommandCode: `0x20`
- Board: UCE
- Service: can.enable
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`, `state=on|off`
- ResponsePayload: `UceCanEnableResponse.Controller`, `EffectiveEnabled`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanEnableType 0x21`, payload `[controller, state]`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.SetCanEnabledAsync()`, `SdctpApiService`, `FrmUceLogic`
- Notes: abre/fecha porta CAN via UCE.
- DatabaseRelevance: alto para execucao de testes e perfis CAN.

### UCE.can.status get

- CommandName: `UCE.can.status get`
- CommandCode: `0x20`
- Board: UCE
- Service: can.status
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`
- ResponsePayload: `UceCanStatusResponse.Controller`, `State`, `BitrateKbps`, `Mode`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanStatusType 0x22`, request `[controller]`, response len `0x04`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.GetCanStatusAsync()`, `FrmUceLogic`
- Notes: status operacional CAN.
- DatabaseRelevance: alto para validacao e auditoria de execucao.

### UCE.can reset

- CommandName: `UCE.can reset`
- CommandCode: `0x20`
- Board: UCE
- Service: can
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`
- ResponsePayload: `UceCanResetResponse.Controller`, `ResetSucceeded`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanResetType 0x23`, payload `[controller]`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.ResetCanAsync()`
- Notes: reset da interface CAN.
- DatabaseRelevance: medio para procedimentos de teste.

### UCE.can.rx poll

- CommandName: `UCE.can.rx poll`
- CommandCode: `0x20`
- Board: UCE
- Service: can.rx
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`
- ResponsePayload: `UceCanRxPollResponse.Controller`, lista `UceCanFrame`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanRxPollType 0x24`, payload `[controller]`; frame `[id32, flags, dlc, data8]`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `UceClient.PollCanRxAsync()`
- Notes: parser aceita ate `UceCanRxMaxFramesPerResponse = 3`.
- DatabaseRelevance: alto para captura temporal e monitor CAN/J1939.

### UCE.can.rx readAll

- CommandName: `UCE.can.rx readAll`
- CommandCode: `0x20`
- Board: UCE
- Service: can.rx
- Direction: API -> Board
- RequestPayload: `controller=can0|can1` no `UceClient`; mapper envia TLV sem payload
- ResponsePayload: ACK/`UceCanReadAllResponse.Accepted`, eventos posteriores `CAN_ROW`/`CAN_READ_ALL_DONE`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanReadAllType 0x43`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhToSdgwMapper.cs`, `SdhValidator.cs`
- UsedBy: `UceClient.RequestCanReadAllAsync()`, `ApiCanService`/`SdctpApiService`
- Notes: NAO CONFIRMADO COMO ACEITO PELO VALIDATOR atual; `SdhValidator` aceita apenas `poll` para `UCE.can.rx`.
- DatabaseRelevance: alto para snapshot de tabela CAN e recuperacao de mirror.

### UCE.can.driverLog poll

- CommandName: `UCE.can.driverLog poll`
- CommandCode: `0x20`
- Board: UCE
- Service: can.driverLog
- Direction: API -> Board
- RequestPayload: `controller=can0|can1`
- ResponsePayload: `UceCanDriverLogPollResponse.Controller`, lista `UceCanDriverLogEntry`
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `UceCanDriverLogPollType 0x25`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`
- UsedBy: `FrmUceLogic.PollCanDriverLogAsync()`
- Notes: diagnostico de driver CAN.
- DatabaseRelevance: medio para auditoria de testes e falhas.

### UCE.can.tx send/direct/create/edit/delete/stop

- CommandName: `UCE.can.tx send|direct|create|edit|delete|stop`
- CommandCode: `0x20`
- Board: UCE
- Service: can.tx
- Direction: API -> Board
- RequestPayload: ver detalhes abaixo
- ResponsePayload: `UceCanTxResponse` para send/direct/create/edit/delete; `UceCanTxStopResponse` para stop
- JsonContract: NAO CONFIRMADO NO CODIGO
- TLVMapping: `0x26 send legado`, `0x50 direct`, `0x51 create`, `0x52 edit`, `0x53 delete`, `0x27 stop`
- SourceFiles: `UceClient.cs`, `UceParsers.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`, `UceCanProtocol.cs`
- UsedBy: `UceClient`, `SdctpApiService`, `FrmUceLogic.SendCanAsync()`
- Notes: `send` legado esta marcado `[Obsolete]`; preferencia atual para `direct` e tabela TX SDCTP.
- DatabaseRelevance: muito alto para simulacao/reproducao de perfis CAN/J1939.

Detalhes de payload:

| Op | Args obrigatorios |
|---|---|
| `send` | `controller`, `extended`, `id`, `dlc`, `period`, `d0..d7` |
| `direct` | `controller`, `extended`, `rtr`, `id`, `dlc`, `d0..d7` |
| `create` | `controller`, `index`, `extended`, `rtr`, `id`, `dlc`, `d0..d7`, `period`, `enabled` |
| `edit` | `controller`, `index`, `mask`, e campos condicionais por mask: `flags`, `id`, `dlc`, `dataMask`, `d0..d7`, `period`, `enabled` |
| `delete` | `controller`, `index`, `reason=1..4` |
| `stop` | `controller`, `slot=0|255` |

