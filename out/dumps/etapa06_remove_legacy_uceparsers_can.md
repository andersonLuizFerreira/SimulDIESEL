# ETAPA 06 - Remoção dos parsers CAN legados do UceParsers

## 1. Objetivo

Remover de `UceParsers` o parsing legado de massa CAN que ja foi substituido pelo dominio SDCTP.

A partir desta ETAPA, `UceParsers` deixa de conter os parsers antigos de massa CAN RX/CRUD, e o parser oficial para eventos de massa CAN na API passa a ser o namespace SDCTP, especialmente:

- `SdctpEventParser.TryReadRawEvent`
- `SdctpEventParser.TryReadCanRxEvent`
- `SdctpEventProcessor.ProcessEvent`

## 2. Arquivos alterados

| Arquivo | Alteracao |
| --- | --- |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs` | Removidos `TryReadCanRxEvent` e `TryReadCanCrudEvent`. |

## 3. Antes

- `UceParsers` ainda possuia `TryReadCanRxEvent`.
- `UceParsers` ainda possuia `TryReadCanCrudEvent`.
- Esses metodos ja nao eram usados no fluxo oficial desde as ETAPAS 03 e 05.
- O fluxo oficial ja era `SdctpRawEventReceived`, com parsing no dominio SDCTP.

## 4. Depois

- Parsers legados de massa CAN foram removidos de `UceParsers`.
- `UceParsers` fica restrito a parsers de controle, respostas operacionais, erro funcional, erro de gateway, diagnostico de transporte e respostas de comandos UCE.
- Massa CAN fica no namespace SDCTP.
- `SdctpEventParser` permanece como parser oficial para evento bruto SDCTP e `CAN_RX_EVENT`.
- `SdctpEventProcessor` permanece como processador oficial de eventos SDCTP de mirror/buffer.

## 5. Buscas executadas

| Busca | Resultado | Classificacao |
| --- | --- | --- |
| `TryReadCanRxEvent` | Nao existe mais em `UceParsers`; existe em `SdctpEventParser` e `SdctpEventProcessor`. | Fluxo legado removido; fluxo oficial preservado. |
| `TryReadCanCrudEvent` | Nenhuma ocorrencia restante na API. | Fluxo legado removido. |
| `UceCanRxEvent` | Permanece em DTO, `SdctpEventParser`, `SdctpEventProcessor`, `CanEventProcessor` e `ApiCanService`. | DTO/contrato ainda necessario ao parsing SDCTP oficial. |
| `UceCanCrudEvent` | Nenhuma ocorrencia encontrada. | Nao ha contrato com esse nome. |
| `CanCrud` | Permanece em constantes `GwProtocol`, `CanEventProcessor` e `CanRxMirrorManager`. | Contrato/processing SDCTP ainda necessario para mirror RX. |
| `DataMask` | Permanece em `CanEditDto`, `CanEventProcessor`, `CanRxMirrorManager` e diagnostico SDCTP. | Compactacao SDCTP oficial preservada. |
| `UceCanReadAll` | Permanece em constantes legadas, wrappers obsoletos e tratamento de `CAN_READ_ALL_DONE`. | Compatibilidade legada preservada; `CAN_READ_ALL` nao voltou ao fluxo principal. |

## 6. Itens removidos

| Arquivo | Metodo/simbolo removido | Motivo |
| --- | --- | --- |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadCanRxEvent` | Parser legado de massa CAN RX; substituido por `SdctpEventParser.TryReadCanRxEvent`. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadCanCrudEvent` | Parser legado de eventos CRUD/mirror CAN; substituido por roteamento bruto SDCTP e `SdctpEventProcessor`. |

## 7. Itens preservados

| Arquivo | Metodo/simbolo preservado | Motivo |
| --- | --- | --- |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadBuiltinLedResponse`, `TryReadLedEvent` | Controle/evento de LED, fora de massa CAN. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadGatewayError`, `TryReadFunctionalError` | Diagnostico/envelope UCE. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadCanConfigResponse`, `TryReadCanEnableResponse`, `TryReadCanStatusResponse`, `TryReadCanResetResponse` | Controle/operacao de hardware via SDH. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadCanRxPollResponse` | Poll legado operacional ainda preservado; nao e evento bruto SDCTP. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadTransportDiagnosticEvent` | Diagnostico de transporte/dispatcher. |
| `BLL/Boards/UCE/UceParsers.cs` | Parsers de resposta TX CAN | Respostas transacionais de comandos TX preservadas; TX CAN continua classificado como SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpEventParser.cs` | `TryReadRawEvent`, `TryReadCanRxEvent` | Parser oficial SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs` | `ProcessEvent`, `TryReadCanRxEvent`, `ProcessCanRxEvent` | Processamento oficial SDCTP de massa CAN. |
| `DTL/Boards/UCE/UceCanResponses.cs` | `UceCanRxEvent`, `UceCanReadAllResponse` | DTOs ainda usados por SDCTP oficial e wrappers legados. |
| `DTL/Protocols/SDGW/GwProtocol.cs` | TLVs e constantes CAN/CRUD/READ_ALL_DONE | Contratos de fio preservados. |

## 8. Contratos preservados

- TLVs de fio nao alterados.
- Firmware UCE nao alterado.
- Firmware BPM nao alterado.
- SDGW nao alterado.
- UI visual nao alterada.
- `CanRxOutputBuffer` continua saida oficial de massa CAN.
- TX CAN continua SDCTP.
- `CAN_READ_ALL` nao foi reintroduzido.

## 9. Validação

Primeira tentativa:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug /p:OutDir='G:\PROJETOS\SIMULADORES\SimulDIESEL\out\build-etapa06\'
```

Resultado:

- Falhou por sandbox: acesso negado a `C:\Users\Escritorio\AppData\Local\Microsoft SDKs`.

Segunda tentativa:

- Mesmo comando, com permissao escalada.

Resultado:

- Build C# compilou com sucesso.
- Erros: 0.
- Avisos: 0.

## 10. Resultado

Compilou com sucesso.

Resultado tecnico:

- `UceParsers.TryReadCanRxEvent` removido.
- `UceParsers.TryReadCanCrudEvent` removido.
- `SdctpEventParser` consolidado como parser oficial de massa CAN SDCTP na API.
- Fluxo `SdctpRawEventReceived` preservado.
- Nao houve alteracao funcional intencional alem da remocao de codigo legado sem uso.

Pendencias reais:

- Avaliar em ETAPA futura se parsers transacionais de resposta TX devem migrar fisicamente para namespace SDCTP.
- Avaliar em ETAPA futura se wrappers obsoletos ligados a `CAN_READ_ALL` podem ser removidos por completo.
