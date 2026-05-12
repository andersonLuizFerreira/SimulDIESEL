# ETAPA 04 - SDCTP RX/TX e remoção do CAN_READ_ALL legado

## 1. Objetivo

Consolidar TX CAN como parte oficial do SDCTP, no mesmo paradigma de massa/sincronizacao ja aplicado ao RX CAN, e retirar `CAN_READ_ALL` do fluxo principal da API.

Esta ETAPA nao altera TLVs de fio, firmware, SDGW de transporte nem UI visual. As mudancas ficam na API C# e na documentacao arquitetural.

## 2. Decisões arquiteturais aplicadas

- TX CAN pertence ao SDCTP.
- RX CAN pertence ao SDCTP.
- CanService possui lado RX e lado TX.
- SDH permanece controle/operacao de hardware.
- SDGW permanece transporte puro.
- `CAN_READ_ALL` e legado.
- `CAN_READ_ALL_DONE` permanece apenas como compatibilidade tecnica de evento recebido.

## 3. Arquivos alterados

| Arquivo | Alteracao |
| --- | --- |
| `docs/architecture/sdh_sdctp_sdgw_contracts.md` | TX CAN congelado como SDCTP; `CAN_READ_ALL` registrado como legado. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs` | `RequestCanReadAllAsync` deixou de enviar TLV legado; eventos parseados antigos marcados como compatibilidade ETAPA 04. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs` | `RequestCanReadAllAsync` marcado como legado; eventos antigos mantidos com TODO ETAPA 04. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs` | Parser de resposta `CAN_READ_ALL` removido. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs` | Fluxo visual mantido; inscricao no evento CAN RX parseado antigo removida; wrapper legado de read all mantido temporariamente. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs` | `RequestReadAllAsync` nao envia mais `CAN_READ_ALL`; recuperacao automatica por `CAN_READ_ALL` removida do fluxo principal. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxMirrorManager.cs` | Removido `ReplaceAll(CanReadAllResponseDto)`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs` | Wrapper `RequestReadAllAsync` mantido temporariamente com TODO; TX permanece na fachada SDCTP. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs` | `CAN_READ_ALL_DONE` documentado como compatibilidade legada. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpProtocol.cs` | Removida constante de solicitacao `CAN_READ_ALL` da fachada SDCTP. |
| `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs` | Mapeamento SDH `UCE.can.rx readAll` passa a ser rejeitado. |
| `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhValidator.cs` | Validacao SDH nao aceita mais `readAll`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanReadAllResponseDto.cs` | Arquivo removido por nao haver mais consumo no fluxo da API. |
| `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj` | Removida inclusao do DTO legado removido. |

## 4. Antes

- TX CAN estava documentado como ambiguo.
- `CAN_READ_ALL` estava documentado como ambiguo.
- `ApiCanService.RequestReadAllAsync` enviava `CAN_READ_ALL` via `IUceDispatcher.RequestCanReadAllAsync`.
- `ApiCanService.HandleMirrorOutOfSyncAsync` tentava recuperar mirror table solicitando `CAN_READ_ALL` automatico.
- `UceParsers` ainda possuia parser de resposta para `CAN_READ_ALL`.
- Eventos antigos `CanRxEventReceived` e `CanCrudEventReceived` existiam para compatibilidade apos a ETAPA 03.

## 5. Depois

- TX CAN esta consolidado como SDCTP.
- `SendDirectAsync`, `SendFrameAsync`, `StartTxAsync`, `StopTxAsync`, `CreateTxRowAsync`, `EditTxRowAsync`, `DeleteTxRowAsync` e `GetTxSnapshot` permanecem na fachada SDCTP.
- `CAN_READ_ALL` saiu do fluxo principal: API/BLL nao envia mais o TLV `0x43`.
- O mapeador/validador SDH rejeita `readAll`.
- O DTO de snapshot legado foi removido.
- O Dispatcher continua roteando SDCTP por evento bruto.
- `CAN_READ_ALL_DONE` permanece processavel como evento SDCTP legado recebido, para nao quebrar compatibilidade com firmware existente.

## 6. Referências CAN_READ_ALL encontradas

| Arquivo | Simbolo/metodo | Acao tomada | Observacao |
| --- | --- | --- | --- |
| `DTL/Protocols/SDGW/GwProtocol.cs` | `UceCanReadAllType`, `UceCanReadAllPayloadLength` | Mantido temporariamente | Constantes TLV preservadas para nao alterar contrato de fio nesta ETAPA. |
| `DTL/Protocols/SDGW/GwProtocol.cs` | `UceCanReadAllDoneType`, `UceCanReadAllDonePayloadLength` | Mantido | Evento recebido ainda e aceito como compatibilidade SDCTP. |
| `BLL/Boards/UCE/UceClient.cs` | `RequestCanReadAllAsync` | Substituido no fluxo principal | Metodo legado retorna falha local e nao envia TLV. |
| `BLL/Boards/UCE/UceClient.cs` | `CreateCanReadAllCommand` | Removido | Comando SDH legado nao e mais criado. |
| `BLL/Boards/UCE/UceDispatcher.cs` | `RequestCanReadAllAsync` | Mantido temporariamente | Compatibilidade binaria; marcado como legado. |
| `BLL/Boards/UCE/UceParsers.cs` | `TryReadCanReadAllResponse` | Removido | Parser de resposta da solicitacao legada nao e mais usado. |
| `BLL/Services/CAN/ApiCanService.cs` | `RequestReadAllAsync` | Substituido | Nao envia TLV; retorna sucesso local de compatibilidade com mensagem de legado. |
| `BLL/Services/CAN/ApiCanService.cs` | `HandleMirrorOutOfSyncAsync` | Substituido | Nao solicita mais `CAN_READ_ALL`; mantem diagnostico e aguarda eventos SDCTP. |
| `BLL/Services/CAN/ApiCanService.cs` | `FinishReadAllSync` | Mantido temporariamente | Usado apenas quando `CAN_READ_ALL_DONE` legado for recebido. |
| `BLL/Services/CAN/CanEventProcessor.cs` | `UceCanReadAllDoneType`, `TryParseReadAllDone`, `ProcessReadAllDone` | Mantido temporariamente | Necessario enquanto `CAN_READ_ALL_DONE` ainda puder chegar do firmware. |
| `BLL/Services/CAN/CanRxMirrorManager.cs` | `ReplaceAll(CanReadAllResponseDto)` | Removido | Snapshot legado por DTO saiu da API. |
| `BLL/Services/CAN/CanRxMirrorManager.cs` | `StartReadAll`, `CancelReadAll`, `ApplyReadAllDone`, `IsSyncingReadAll` | Mantido temporariamente | Estado legado ainda existe para compatibilidade com `CAN_READ_ALL_DONE`; nao e iniciado pelo fluxo principal. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `RequestReadAllAsync` | Mantido temporariamente | Wrapper com TODO; consumidores devem usar `GetRxSnapshot`/`TryReadRxFrame`. |
| `BLL/Services/CAN/SDCTP/SdctpProtocol.cs` | `CanReadAll` | Removido | Fachada SDCTP nao expoe mais solicitacao legada. |
| `BLL/Services/CAN/SDCTP/SdctpEventParser.cs` | `UceCanReadAllDoneType` | Mantido | Roteamento bruto ainda aceita evento legado recebido. |
| `BLL/FormsLogic/UCE/FrmUceLogic.cs` | `RequestCanReadAllAsync` | Mantido temporariamente | Wrapper legado; UI deve consumir snapshot/buffer. |
| `DAL/Protocols/SDGW/SdhToSdgwMapper.cs` | `UCE.can.rx readAll` | Removido/substituido | Mapeamento rejeita o comando legado. |
| `DAL/Protocols/SDGW/SdhValidator.cs` | `readAll` em `UCE.can.rx` | Removido | SDH nao valida mais o comando legado. |
| `DTL/Boards/UCE/Can/CanReadAllResponseDto.cs` | DTO de snapshot legado | Removido | Nao havia consumidor remanescente. |

## 7. Referências TX CAN classificadas como SDCTP

| Arquivo | Metodo/simbolo | Classificacao | Observacao |
| --- | --- | --- | --- |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `SendDirectAsync` | SDCTP / TX CAN | Fachada oficial de envio direto. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `SendFrameAsync` | SDCTP / TX CAN | Alias de envio direto. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `StartTxAsync` | SDCTP / TX CAN | Envio periodico/ciclico via tabela TX. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `StopTxAsync` | SDCTP / TX CAN | Parada de TX periodico. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `CreateTxRowAsync` | SDCTP / TX CAN | Criacao de linha TX. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `EditTxRowAsync` | SDCTP / TX CAN | Edicao compactada de linha TX. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `DeleteTxRowAsync` | SDCTP / TX CAN | Remocao de linha TX. |
| `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | `GetTxSnapshot` | SDCTP / TX CAN | Snapshot local da tabela TX. |
| `BLL/Services/CAN/SDCTP/SdctpTxManager.cs` | `SendFrameAsync`, `CreateTxRowAsync`, `EditTxRowAsync`, `DeleteTxRowAsync`, `GetTxSnapshot` | SDCTP / TX CAN | Adapter SDCTP para o manager validado. |
| `BLL/Services/CAN/ApiCanService.cs` | TX methods | SDCTP / TX CAN | Mantidos como implementacao interna da fachada SDCTP. |
| `BLL/FormsLogic/UCE/FrmUceLogic.cs` | `SendCanAsync`, `StopCanTxAsync`, J1939 requests via `SendDirectAsync` | SDCTP / TX CAN | Consumidores continuam chamando a fachada SDCTP. |
| `BLL/Services/CAN/SDCTP/SdctpProtocol.cs` | `CanTxDirect`, `CanTxCreate`, `CanTxEdit`, `CanTxDelete` | SDCTP / TX CAN | Constantes oficiais da fachada SDCTP. |
| `DTL/Protocols/SDGW/GwProtocol.cs` | `UceCanTxType`, `UceCanTxStopType`, `UceCanTxDirectType`, `UceCanTxCreateType`, `UceCanTxEditType`, `UceCanTxDeleteType` | SDCTP / TX CAN | TLVs preservados; classificacao arquitetural atualizada. |

## 8. Compatibilidades temporárias

- `IUceDispatcher.RequestCanReadAllAsync`, `UceDispatcher.RequestCanReadAllAsync` e `UceClient.RequestCanReadAllAsync` permanecem por compatibilidade, mas nao enviam o fluxo principal.
- `SdctpApiService.RequestReadAllAsync` e `FrmUceLogic.RequestCanReadAllAsync` permanecem para nao quebrar chamadas existentes; ambos indicam que o uso correto e snapshot/buffer SDCTP.
- `CanRxMirrorManager.StartReadAll`, `CancelReadAll`, `ApplyReadAllDone` e `IsSyncingReadAll` permanecem enquanto `CAN_READ_ALL_DONE` ainda puder existir como evento legado.
- `CanEventProcessor` ainda processa `CAN_READ_ALL_DONE` como evento legado recebido.
- `UceClient.CanRxEventReceived` e `UceClient.CanCrudEventReceived` permanecem temporariamente; o build mostra que nao sao mais usados no fluxo principal.
- Constantes `0x43` permanecem em `GwProtocol` para preservar contratos de fio e diagnostico, mas nao sao expostas pela fachada SDCTP nem enviadas pela API.

## 9. Contratos preservados

- SDGW nao alterado.
- Firmware UCE nao alterado.
- Firmware BPM nao alterado.
- TLVs de fio nao alterados.
- UI visual nao alterada.
- SDCTP nao renomeado.
- SDH continua controle/operacao.
- Controle CAN permanece em `CanControlApiService`.
- TX CAN nao foi movido para `CanControlApiService`.

## 10. Validação

Primeira tentativa:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug /p:OutDir='G:\PROJETOS\SIMULADORES\SimulDIESEL\out\build-etapa04\'
```

Resultado da primeira tentativa:

- Falhou por sandbox: acesso negado a `C:\Users\Escritorio\AppData\Local\Microsoft SDKs`.

Segunda tentativa:

- Mesmo comando, com permissao escalada.

Resultado:

- Build C# compilou com sucesso.
- Avisos: 2.
- Erros: 0.

Avisos:

| Arquivo | Aviso |
| --- | --- |
| `BLL/Boards/UCE/UceClient.cs` | `CS0067`: evento `UceClient.CanRxEventReceived` nunca e usado. |
| `BLL/Boards/UCE/UceClient.cs` | `CS0067`: evento `UceClient.CanCrudEventReceived` nunca e usado. |

## 11. Resultado

Compilou com sucesso.

Houve alteracao funcional intencional: `CAN_READ_ALL` nao e mais enviado pela API C# no fluxo principal. Chamadas legadas agora recebem resultado local ou rejeicao antes de gerar TLV, conforme a camada.

Pendencias reais:

- Definir mecanismo futuro de snapshot SDCTP sem `CAN_READ_ALL`, caso seja necessaria recuperacao completa apos perda de mirror table.
- Remover eventos parseados antigos do Dispatcher quando nao houver mais consumidores externos.
- Renomear propriedades internas `IsSyncingReadAll`/metodos correlatos em ETAPA futura, se a compatibilidade permitir.
- Avaliar remocao futura das constantes `0x43` quando o contrato de fio legado puder ser eliminado.
