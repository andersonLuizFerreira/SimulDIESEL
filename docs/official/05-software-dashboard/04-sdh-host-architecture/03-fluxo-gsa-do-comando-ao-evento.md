⬅ [Retornar para Arquitetura SDH no Host](../04-sdh-host-architecture.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# Fluxo GSA do Comando ao Evento

## Função lógica

Este fluxo mostra como o host transforma uma intenção da UI em comando GSA, aguarda a resposta síncrona e ainda consome eventos assíncronos posteriores.

## Fluxo ativo

```text
frmGSA_UI
  -> FrmGsaLogic
  -> GsaClient
  -> SdhClient
  -> SdhToSdgwMapper
  -> SdgwSession
  -> resposta síncrona correlacionada
  -> GsaParsers
  -> frmGSA_UI

evento assíncrono:
SdgwSession.EventReceived
  -> GsaClient.OnEventReceived
  -> FrmGsaLogic
  -> frmGSA_UI
```

## Operações confirmadas

- `IMPLEMENTADO`: LED builtin
- `IMPLEMENTADO`: `channel.setpoint`
- `IMPLEMENTADO`: `channel.enable`
- `IMPLEMENTADO`: `channels.enable`
- `IMPLEMENTADO`: `channel.status`
- `IMPLEMENTADO`: `channels.status`
- `IMPLEMENTADO`: `channel.fault reset`
- `IMPLEMENTADO`: offsets por canal e reset global
- `PARCIALMENTE IMPLEMENTADO`: botão de configuração por canal da UI ainda só exibe mensagem placeholder

## Trecho comentado: guarda de link na FormsLogic

Em `FrmGsaLogic.SetChannelSetpointAsync(...)`, a primeira decisão é proteger a operação:

```csharp
if (!_isLinked())
    return FailWhenNotLinked<GsaChannelSetpointResponse>();
```

O que esse trecho faz:

- impede request GSA fora do estado `Linked`;
- falha cedo na BLL, antes de abrir qualquer tráfego SDGW;
- mantém a UI da GSA dependente do estado real da sessão host.

## Trecho comentado: correlação síncrona

Em `GsaClient.ExecuteOperationAsync(...)`, o client prepara o casamento da resposta:

```csharp
_pendingRequest = new PendingGsaRequest
{
    ResponseSource = new TaskCompletionSource<SdgwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
    MatchFrame = frame => MatchesExpectedResponse(frame, expectedType, expectedLen, expectedChannel)
};
```

O que esse trecho faz:

- arma a espera da resposta antes do envio;
- define o TLV, o tamanho e o canal esperados;
- garante que erro funcional e erro de gateway também possam ser correlacionados ao request atual.

## Trecho comentado: UI aguarda confirmação física

Em `frmGSA_UI.GsaControls_OutputEnabledChanged(...)`, o form separa aceite lógico de execução física:

```csharp
state.OutputEnabled = result.Response.AcceptedState;
ApplyChannelState(channel);
SetPhysicalResultMessage(
    "Comando aceito pela GSA. Aguardando IRQ e evento 0x31 para confirmar a execução física.",
    true);
```

Isso mostra um ponto importante do comportamento atual:

- a resposta síncrona confirma que a GSA aceitou a operação;
- a confirmação do que aconteceu no hardware vem depois, via evento `0x31`.

## Trecho comentado: evento físico na UI

Em `Logic_PhysicalOperationEventReceived(...)`, o form fecha o ciclo:

```csharp
SetPhysicalResultMessage(BuildPhysicalResultText(physicalEvent), physicalEvent.IsSuccess);

if (physicalEvent.Channel >= 1 && physicalEvent.Channel <= _channels.Length)
{
    _ = RefreshChannelAsync(physicalEvent.Channel, false);
}
```

O que esse trecho faz:

- traduz o evento físico para uma mensagem legível na interface;
- agenda um refresh do canal atingido;
- fecha o caminho completo comando -> aceite -> evento -> refresh.

## Eventos confirmados

- `0x30`: snapshot assíncrono de fault do canal.
- `0x31`: resultado físico da operação, com `Ok`, `TcaNoAck` ou `McpNoAck`.

## Glossário

- **Resposta síncrona correlacionada**: resposta aceita por `MatchesExpectedResponse(...)`.
- **Erro funcional**: TLV `0x7F` devolvido pela GSA.
- **Evento físico**: retorno assíncrono que informa o resultado real da execução após o aceite inicial.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
