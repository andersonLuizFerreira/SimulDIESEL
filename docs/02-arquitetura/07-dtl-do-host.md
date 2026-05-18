⬅ [Retornar para API e Host Local](04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# DTL do Host

## Posição na pilha

A DTL define os tipos que circulam entre UI, BLL e DAL. Ela não abre conexão, não agenda fila e não faz retry; sua função é fixar a forma dos dados que o host realmente manipula.

## Contratos reais da camada

| grupo | arquivos | tipos principais | estado | papel |
| --- | --- | --- | --- | --- |
| Estado da BPM | `DTL/Boards/BPM/BpmStatusDto.cs`, `BluetoothDeviceDto.cs` | `BpmStatusDto`, `BluetoothDeviceDto` | `IMPLEMENTADO` | DTOs de estado de interface e descoberta Bluetooth |
| Requests da GSA | `DTL/Boards/GSA/GsaRequests.cs` | requests de setpoint, enable, status, fault e offset | `IMPLEMENTADO` | envelopes de entrada usados por `FrmGsaLogic` e `GsaClient` |
| Responses e eventos da GSA | `DTL/Boards/GSA/GsaResponses.cs`, `GsaLedResponse.cs`, `GsaCommon.cs` | responses síncronas, enums e eventos assíncronos | `IMPLEMENTADO` | tipos finais entregues à UI e à BLL |
| Contratos da UCE | `DTL/Boards/UCE/*.cs`, `DTL/Boards/UCE/Can/*.cs` | LED, CAN config/status/RX/TX e DTOs de tabela | `IMPLEMENTADO` | tipos consumidos por `FrmUceLogic`, `UceClient`, SDCTP e J1939 |
| Contratos J1939 | `DTL/Protocols/J1939/**/*.cs` | mensagens data link, aplicação, diagnósticos, network management, captura e catálogos | `IMPLEMENTADO` | contratos consumidos pelos serviços J1939 e UI UCE |
| Contrato SDCTP | `DTL/Protocols/SDCTP/SdctpRawEventDto.cs` | evento bruto SDCTP | `IMPLEMENTADO` | apoio ao transporte CAN de massa |
| Tipos comuns | `DTL/Common/DeviceInfo.cs`, `OperationStatusDto.cs` | `DeviceInfo`, `OperationStatusDto` | `IMPLEMENTADO` | apoio geral para bootstrap textual e retornos simples |
| Contratos SDH/SDGW | `DTL/Protocols/SDGW/*.cs` | `SdhCommand`, `SdhResponse`, `SdhTarget`, `SdgwFrame`, `SdgwCommand`, `GwProtocol` | `IMPLEMENTADO` / `PARCIALMENTE IMPLEMENTADO` | contratos semânticos e de enlace do host |

## Pontos de fidelidade importantes

- `GwProtocol.GsaChannelStatusType` está em `0x1B`; o host atual não usa `0x12` para `channel.status`.
- A nomenclatura ativa do código é `Sdgw`, não `Sggw`.
- `SdhResponse` existe como contrato, mas não participa do hot path atual do host.
- `OperationStatusDto` existe como tipo comum, porém os caminhos ativos da BPM, GSA e UCE preferem resultados específicos como `BpmCommandResult`, `GsaOperationResult<T>` e `UceOperationResult<T>`.

## Trecho âncora

O contrato base do comando semântico do host continua curto e objetivo:

```csharp
public string Version { get; set; } = "sdh/1";
public string Target { get; set; }
public string Op { get; set; }
public Dictionary<string, string> Args { get; set; }
```

Isso explica por que o host consegue separar bem as responsabilidades: a DTL carrega o comando em uma forma estável e a DAL fica livre para validar, mapear e serializar.

## Classificação de estado

- `IMPLEMENTADO`: DTOs BPM, DTOs e enums GSA, DTOs UCE/CAN, DTOs J1939, `SdhCommand`, `SdhTarget`, `SdgwFrame`, `SdgwCommand`, `GwProtocol` e `SdctpRawEventDto`.
- `PARCIALMENTE IMPLEMENTADO`: `SdhResponse` e `OperationStatusDto` existem como contratos, mas não estruturam o fluxo quente desta aplicação.
- `LEGADO`: nomes `Sggw*` pertencem ao histórico documental; eles não são os tipos ativos do host auditado.

## Glossário

- **DTL**: conjunto de DTOs, enums e contratos compartilhados entre camadas.
- **Target SDH**: identificador semântico como `GSA.channel.status`.
- **TLV**: formato compacto de tipo, comprimento e valor usado dentro dos payloads GSA e UCE.
- **CanFrameDto**: DTO de frame CAN consumido por SDCTP e pela pilha J1939.

## Próximas camadas

- [Contratos SDH, SDGW e DTOs](07-dtl-do-host/01-contratos-sdh-e-dtos.md)
