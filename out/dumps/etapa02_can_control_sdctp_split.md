# ETAPA 02 - Separação CanControlApiService / SdctpApiService

Data: 2026-05-11

## 1. Objetivo

Separar a fachada de controle/operação CAN da fachada SDCTP na API, conforme o congelamento arquitetural da ETAPA 01.

O ajuste cria `CanControlApiService` como fachada de aplicacao para comandos CAN de controle via `IUceDispatcher`/SDH, e altera `FrmUceLogic` para deixar de chamar controle CAN por `SdctpApiService`.

Nao houve alteracao intencional de comportamento funcional, contrato TLV, firmware ou SDGW.

## 2. Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanControlApiService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`
- `out/dumps/etapa02_can_control_sdctp_split.md`

## 3. Antes

`SdctpApiService` expunha diretamente metodos de controle/operação CAN:

- `SetCanConfigAsync`
- `SetCanEnabledAsync`
- `GetCanStatusAsync`
- `ResetCanAsync`
- `PollCanDriverLogAsync`

`FrmUceLogic` chamava esses metodos pela instancia `_sdctp`, misturando controle CAN com a fachada SDCTP.

## 4. Depois

Foi criado `CanControlApiService` em `BLL/Services/CAN/CanControlApiService.cs`.

Esse servico delega para `IUceDispatcher` os metodos de controle/operação CAN:

- `SetCanConfigAsync`
- `SetCanEnabledAsync`
- `GetCanStatusAsync`
- `ResetCanAsync`
- `PollCanDriverLogAsync`

`BpmSerialService` agora cria e expoe:

- `CanControlApiService CanControl`
- `SdctpApiService Sdctp`

`FrmUceLogic.CreateDefault` passa a receber `service.CanControl` e `service.Sdctp`.

`FrmUceLogic` agora usa:

- `_canControl` para `SetCanConfigAsync`
- `_canControl` para `SetCanEnabledAsync`
- `_canControl` para `GetCanStatusAsync`
- `_canControl` para `ResetCanAsync`
- `_canControl` para `PollCanDriverLogAsync`
- `_sdctp` para leitura/sincronizacao/massa CAN

## 5. Métodos que permaneceram no SDCTP

Permaneceram em `SdctpApiService` os metodos ligados a massa, sincronizacao e estado CAN RX/API:

- `GetSnapshot`
- `GetRxSnapshot`
- `TryReadRxFrame`
- `RequestReadAllAsync`
- `SendDirectAsync`
- `SendFrameAsync`
- `StartTxAsync`
- `StopTxAsync`
- `CreateTxRowAsync`
- `EditTxRowAsync`
- `DeleteTxRowAsync`
- `GetTxSnapshot`
- Eventos `CanRxTableChanged`, `CanDiagnosticStateChanged`, `CanRxFrameAvailable`
- Propriedades de mirror/diagnostico/output buffer como `IsMirrorOutOfSync`, `IsSyncingReadAll`, `OutputBufferCount` e `OutputBufferOverflowCount`

Por compatibilidade temporaria, os metodos antigos de controle ainda existem em `SdctpApiService`, mas foram marcados com comentario:

```csharp
// TODO ETAPA 02: metodo mantido apenas para compatibilidade temporaria. Preferir CanControlApiService.
```

`FrmUceLogic` nao usa mais esses wrappers.

## 6. Métodos ambíguos mantidos sem decisão

Sem reclassificacao nesta ETAPA:

- `SendDirectAsync`
- `StartTxAsync`
- `StopTxAsync`
- `CreateTxRowAsync`
- `EditTxRowAsync`
- `DeleteTxRowAsync`

TX CAN continua com fronteira oficial pendente.

`CAN_READ_ALL` tambem permanece ambiguo: `RequestReadAllAsync` continua em `SdctpApiService`, porque participa da sincronizacao da mirror table e do `CanRxOutputBuffer`, mas a fronteira oficial do comando ainda nao foi decidida.

## 7. Contratos preservados

- TLVs nao alterados.
- Firmware UCE nao alterado.
- Firmware BPM nao alterado.
- SDGW nao alterado.
- UI continua sem consumir TLV bruto.
- CanRxOutputBuffer continua sendo a saida oficial de massa CAN para aplicacao/UI.
- SDCTP nao foi renomeado.
- TX CAN nao foi reclassificado.
- CAN_READ_ALL nao foi reclassificado.

## 8. Validação

Comando de build padrao executado:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug
```

Resultado do build padrao:

- `CoreCompile` executou sem erros de compilacao.
- O build falhou no passo `CopyFilesToOutputDirectory`, porque `bin\Debug\SimulDIESEL.exe` estava bloqueado pelos processos `Visual Studio 2022 Remote Debugger (8504)` e `SimulDIESEL (13736)`.

Comando de build de validacao com saida alternativa:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug /p:OutDir='G:\PROJETOS\SIMULADORES\SimulDIESEL\out\build-etapa02\'
```

Resultado do build de validacao:

- Compilacao com exito.
- 0 avisos.
- 0 erros.
- Saida gerada em `out/build-etapa02/`.

## 9. Resultado

Compilou: sim, usando `OutDir` alternativo em `out/build-etapa02`.

Pendencias:

- Remover wrappers temporarios de controle em `SdctpApiService` em ETAPA futura, depois que todos os consumidores externos forem migrados.
- Decidir fronteira oficial de TX CAN.
- Decidir fronteira oficial de CAN_READ_ALL.
- Avaliar em ETAPA futura a reducao do conhecimento SDCTP em `UceClient`/`UceParsers`.

Alteracao funcional intencional:

- Nenhuma. A alteracao foi organizacional/conceitual na API, preservando as chamadas existentes ao `IUceDispatcher` e ao `ApiCanService`.
