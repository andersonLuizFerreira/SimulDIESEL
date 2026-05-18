⬅ [Retornar para DTL do Host](../07-dtl-do-host.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Contratos SDH, SDGW e DTOs

## Posição estrutural

Esta página aprofunda a DTL como trilha `ONDE`: quais tipos existem, em que arquivos estão e quais camadas os consomem.

## Estrutura real dos contratos

| arquivo | tipo | camada consumidora | estado | observação |
| --- | --- | --- | --- | --- |
| `DTL/Protocols/SDGW/SdhCommand.cs` | `SdhCommand` | BLL e DAL | `IMPLEMENTADO` | comando semântico de entrada do host |
| `DTL/Protocols/SDGW/SdhTarget.cs` | `SdhTarget` | DAL | `IMPLEMENTADO` | parser estrutural do target |
| `DTL/Protocols/SDGW/SdhResponse.cs` | `SdhResponse` | nenhum fluxo quente ativo | `PARCIALMENTE IMPLEMENTADO` | contrato pronto, ainda sem integração direta |
| `DTL/Protocols/SDGW/SdgwFrame.cs` | `SdgwFrame` | DAL e BLL | `IMPLEMENTADO` | frame lógico já sem COBS e CRC |
| `DTL/Protocols/SDGW/SdgwCommand.cs` | `SdgwCommand` | DAL | `IMPLEMENTADO` | enum reservado para `Ping`, `Ack`, `Err` |
| `DTL/Protocols/SDGW/GwProtocol.cs` | `GwProtocol` | DAL e BLL GSA | `IMPLEMENTADO` | catálogo de endereços, ops e TLVs |
| `DTL/Boards/BPM/BpmStatusDto.cs` | `BpmStatusDto` | UI e FormsLogic | `IMPLEMENTADO` | espelha estado resumido do host |
| `DTL/Boards/BPM/BluetoothDeviceDto.cs` | `BluetoothDeviceDto` | UI e BLL Bluetooth | `IMPLEMENTADO` | descreve dispositivo pareado/usável |
| `DTL/Boards/GSA/GsaRequests.cs` | requests GSA | FormsLogic e BLL GSA | `IMPLEMENTADO` | requests fortemente tipados |
| `DTL/Boards/GSA/GsaResponses.cs` | responses GSA | BLL GSA e UI | `IMPLEMENTADO` | respostas síncronas da GSA |
| `DTL/Boards/GSA/GsaCommon.cs` | enums, snapshots e eventos | BLL GSA e UI | `IMPLEMENTADO` | estado, faults, operação física e erros |
| `DTL/Boards/UCE/UceLedResponse.cs` | response LED UCE | BLL UCE e UI | `IMPLEMENTADO` | resposta funcional simples da UCE |
| `DTL/Boards/UCE/UceCanProtocol.cs` | enums/protocolo CAN UCE | BLL UCE, DAL e UI | `IMPLEMENTADO` | encoding/decoding de controller, bitrate, modo e modo RX |
| `DTL/Boards/UCE/UceCanResponses.cs` | responses CAN UCE | BLL UCE e UI | `IMPLEMENTADO` | config, enable, status, RX, TX, driver log e reset |
| `DTL/Boards/UCE/Can/*.cs` | DTOs CAN RX/TX | SDCTP, UI e J1939 | `IMPLEMENTADO` | `CanFrameDto`, linhas RX/TX, create/edit/delete/tic |
| `DTL/Protocols/SDCTP/SdctpRawEventDto.cs` | evento SDCTP bruto | BLL CAN/SDCTP | `IMPLEMENTADO` | transporte de eventos CAN de massa |
| `DTL/Protocols/J1939/**/*.cs` | DTOs J1939 | BLL J1939 e UI UCE | `IMPLEMENTADO` | aplicação, data link, diagnósticos, network management, captura e catálogos |
| `DTL/Common/DeviceInfo.cs` | `DeviceInfo` | bootstrap textual | `IMPLEMENTADO` | materializa a linha textual identificada pelo parser BPM |
| `DTL/Common/OperationStatusDto.cs` | `OperationStatusDto` | apoio comum | `PARCIALMENTE IMPLEMENTADO` | presente, mas pouco usado no fluxo principal |

## Trecho comentado: shape do comando SDH

Em `SdhCommand`, a DTL define a forma mínima exigida pela DAL:

```csharp
public string Version { get; set; } = "sdh/1";
public string Target { get; set; }
public string Op { get; set; }
public Dictionary<string, string> Args { get; set; }
public Dictionary<string, string> Meta { get; set; }
```

O que esse trecho faz:

- fixa a forma de entrada comum entre UI, BLL e DAL;
- permite que validação e serialização trabalhem sem depender de formulários ou de tipos de board;
- deixa `Meta` preparada para contexto extra, mesmo que o host atual use pouco esse campo.

## Trecho comentado: parser do target

Em `SdhTarget.Parse(...)`, a DTL reduz `target` a três segmentos estruturais:

```csharp
string[] parts = target.Trim().Split('.');
if (parts.Length < 2 || parts.Length > 3)
    throw new ArgumentException(...);
```

Esse bloco é importante porque obriga o formato `Board.resource` ou `Board.resource.subresource`, que é exatamente o formato usado por `SdhValidator`.

## Trecho comentado: catálogo TLV ativo

Em `GwProtocol.cs`, o contrato vivo da GSA e da UCE está materializado em constantes:

```csharp
public const byte GsaChannelSetpointType = 0x10;
public const byte GsaChannelEnableType = 0x11;
public const byte GsaChannelStatusType = 0x1B;
public const byte GsaPhysicalOperationEventType = 0x31;
public const byte UceCanConfigType = 0x20;
public const byte UceCanTxDirectType = 0x55;
public const byte GatewayErrorType = 0xFE;
```

O que esse trecho faz:

- fixa os TLVs aceitos pela DAL e pelos parsers da GSA;
- fixa também os TLVs usados pela UCE para LED, CAN, SDCTP e diagnósticos;
- remove ambiguidade documental sobre o status por canal, que no host atual está em `0x1B`;
- mantém host e firmware alinhados por nomes estáveis de constantes.

## Trecho comentado: frame lógico

Em `SdgwFrame`, a DTL expõe apenas o que sobra depois do engine:

```csharp
public byte Cmd { get; }
public byte Seq { get; }
public byte Flags { get; }
public byte[] Payload { get; }
```

Por que isso importa:

- `SdgwFrame` já não carrega delimitador `0x00`, COBS ou CRC;
- ele é o contrato que a BLL GSA e a BLL UCE recebem em `OnFrameReceived(...)` e, quando aplicável, `OnEventReceived(...)`.

## Glossário

- **Meta**: dicionário opcional de contexto adicional em `SdhCommand` e `SdhResponse`.
- **Snapshot**: DTO que representa o estado instantâneo de um canal GSA.
- **Frame lógico**: `SdgwFrame` pronto para consumo acima do enlace.
- **SDCTP**: contrato de massa CAN RX/TX que usa DTOs CAN da UCE e eventos próprios acima de SDGW.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
