⬅ [Retornar para BLL do Host](../05-bll-do-host.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Clients BPM, GSA e UCE

## Posição estrutural

Os clients ficam abaixo das fachadas e acima da DAL. Eles são o último degrau da BLL antes da semântica SDH/SDGW.

## Classes reais

| arquivo | classe | entrada superior | saída inferior | estado | entrega para cima |
| --- | --- | --- | --- | --- | --- |
| `BLL/Boards/BPM/BpmClient.cs` | `BpmClient` | `FrmBpmLogic` | `SdhClient` | `IMPLEMENTADO` | `BpmStatusDto`, `BpmCommandResult` |
| `BLL/Boards/GSA/GsaClient.cs` | `GsaClient` | `FrmGsaLogic`, `frmGSA_UI` | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | `GsaCommandResult`, `GsaOperationResult<T>`, eventos assíncronos |
| `BLL/Boards/UCE/UceClient.cs` | `UceClient` | `FrmUceLogic`, `frmUCE_UI` | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | `UceCommandResult`, `UceOperationResult<T>`, respostas tipadas da CAN |
| `BLL/Boards/BPM/Backplane/BackplaneService.cs` | `BackplaneService` | `BpmClient` | nenhum | `PARCIALMENTE IMPLEMENTADO` | status textual de expansão |
| `BLL/Boards/BPM/XConn/XConnService.cs` | `XConnService` | `BpmClient` | nenhum | `PARCIALMENTE IMPLEMENTADO` | status textual de expansão |
| `BLL/Boards/BPM/Comm/Network/BpmNetworkService.cs` | `BpmNetworkService` | `BpmSerialService.Network` | nenhum | `PLANEJADO` | mensagem de não implementado |

## Fluxo estrutural do client BPM

- `BpmClient.GetStatus()` não toca em protocolo; ele apenas projeta `BpmSerialService` em `BpmStatusDto`.
- `BpmClient.PingGatewayAsync()` cria `SdhCommand { Target = "BPM.gateway", Op = "ping" }` e entrega para `SdhClient`.
- `Backplane` e `XConn` ficam plugados na mesma classe, mas ainda não descem para a DAL.

## Fluxo estrutural do client GSA

- `GsaClient` assina `SdgwSession.FrameReceived` e `SdgwSession.EventReceived` no construtor.
- Cada operação pública cria um `SdhCommand` concreto, como `GSA.channel.setpoint` ou `GSA.channel.offset`.
- O client mantém um `_requestGate` e um `_pendingRequest`, então a correlação síncrona da GSA é serializada no host atual.

## Fluxo estrutural do client UCE

- `UceClient` assina `SdgwSession.FrameReceived` no construtor.
- Cada operação pública cria um `SdhCommand` concreto, hoje para `UCE.led`, `UCE.can.config`, `UCE.can.enable`, `UCE.can.status` e `UCE.can reset`.
- O client mantém `_requestGate` e `_pendingRequest`, então a correlação síncrona da UCE segue o mesmo padrão seguro da GSA, mas sem fluxo de evento assíncrono dedicado nesta entrega.
- Para CAN, o client usa `UceOperationResult<T>` e parsers tipados para `config`, `enable`, `status` e `reset`.

## Trecho comentado: BPM

Em `BpmClient.PingGatewayAsync()`, a BLL BPM confirma seu papel mínimo:

```csharp
var command = new SdhCommand
{
    Target = "BPM.gateway",
    Op = "ping"
};

SdGwLinkEngine.SendOutcome outcome = await _sdhClient
    .SendAsync(command, SdGwTxPriority.High, "BPM gateway ping");
```

O que esse trecho faz:

- monta o comando semântico mais simples do catálogo atual;
- envia com prioridade alta;
- traduz o `SendOutcome` para um `BpmCommandResult` mais legível para a UI.

## Trecho comentado: GSA

Em `GsaClient.ExecuteOperationAsync(...)`, a BLL da GSA prepara a correlação antes do envio:

```csharp
_pendingRequest = new PendingGsaRequest
{
    ResponseSource = new TaskCompletionSource<SdgwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
    MatchFrame = frame => MatchesExpectedResponse(frame, expectedType, expectedLen, expectedChannel)
};

SdGwLinkEngine.SendOutcome outcome = await _sdh.SendAsync(
    command,
    SdGwTxPriority.High,
    operationName).ConfigureAwait(false);
```

O que esse trecho faz:

- arma a espera pela resposta síncrona antes de transmitir;
- fixa qual TLV, tamanho e canal serão aceitos de volta;
- garante que um evento assíncrono ou uma resposta de outro TLV não seja confundido com o request atual.

## Trecho comentado: eventos assíncronos

Em `GsaClient.OnEventReceived(...)`, o client separa os dois eventos ativos da GSA:

```csharp
if (GsaParsers.TryReadPhysicalOperationEvent(frame, out physicalEvent, out physicalError))
{
    PhysicalOperationEventReceived?.Invoke(physicalEvent);
    return;
}

if (!GsaParsers.TryReadChannelFaultEvent(frame, out faultEvent, out faultError))
    return;
```

O que esse trecho faz:

- tenta primeiro o evento físico `0x31`;
- se não for ele, tenta o evento de fault `0x30`;
- sobe para a FormsLogic apenas eventos já tipados.

## Classificação dos blocos

- `IMPLEMENTADO`: `BpmClient`, `GsaClient`, `UceClient`, ping BPM, LED GSA, LED UCE, CAN da UCE, setpoint, enable, status, faults, offsets e eventos da GSA.
- `PARCIALMENTE IMPLEMENTADO`: `BackplaneService` e `XConnService` estão conectados à BLL, mas ainda não têm descida real.
- `PLANEJADO`: `BpmNetworkService`.

## Glossário

- **Correlacionar resposta**: associar um frame de volta ao request que o originou.
- **Request gate**: semáforo que impede mais de um request síncrono GSA por vez.
- **Evento físico**: retorno assíncrono que informa o resultado de execução real na board.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
