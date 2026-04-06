⬅ [Retornar para Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Resolver Engine do Gateway

O nome histórico desta página foi preservado para manter a navegação, mas o código atual mostra que **não existe um resolver engine semântico no firmware da BPM**.

## Classificação correta

- **PLANEJADO**: resolver engine amplo de `SDH` dentro do gateway.
- **IMPLEMENTADO**: pipeline compacto `SdgwLink -> GatewayApp -> GwRouter -> barramento`.
- **LEGADO**: qualquer leitura que trate a BPM atual como parser completo de `SDH`.

## Pipeline real que ocupa esse lugar hoje

```text
Frame SDGW
  -> SdgwLink valida handshake / framing / seq
  -> GatewayApp extrai ADDR/OP
  -> BPM local ou GwRouter
  -> GwI2cBus ou GwSpiBus
  -> resposta TLV
  -> sendResponse / sendEvent
```

## Comentário orientado a código

### 1. `SdgwLink::handleFrameOk(...)`

```cpp
if (f.cmd == (uint8_t)SDGW_CMD_PING)
{
    if (ackReq) {
        sendAck(f.seq);
    }
    return;
}
```

Esse bloco existe para tratar o ping do gateway sem encaminhá-lo a uma board.

### 2. `GatewayApp::onCommand(...)`

```cpp
GwErr r = _router.route(cmd, data, dataLen, resp, sizeof(resp), respLen, timeoutMs);
if (r != GWERR_OK) {
    sendGatewayErrAsResponse(cmd, r);
    return;
}
```

Esse trecho é o despachante real. Ele tenta a rota física e, em caso de falha, converte o erro do gateway em resposta funcional para o host.

### 3. `Link::poll()` na GSA

```cpp
if (!_svc.handleOneTlv(tlv, txTlv, txTlvLen)) {
  uint8_t payload[3] = { tlv.t, 0, GSA_ERROR_COMMAND_NOT_SUPPORTED };
  txTlvLen = TlvBuilder::build(CMD_FUNCTIONAL_ERROR, payload, sizeof(payload), txTlv, TLV_MAX_LEN);
}
```

Esse bloco existe para garantir que a GSA sempre responda algo sintático quando o `TLV` é válido, mesmo que o comando não seja suportado.

### 4. `AnalogService::handleEnableChannel(...)`

```cpp
if (state.effectiveEnable != desiredEnable) {
    GsaPhysicalOperation operation = {};
    operation.originType = CMD_ENABLE_CHANNEL;
    operation.channel = channel;
    ...
    if (!queuePhysicalOperation(operation)) {
      return buildFunctionalError(..., GSA_ERROR_OPERATION_NOT_ALLOWED, ...);
    }
}
```

Esse trecho existe para separar a aceitação lógica do comando da execução elétrica, que pode ser enfileirada e concluída depois.

## O que ainda falta para existir um resolver engine de verdade

- parser `SDH` dentro da BPM
- binding por target lógico, não só por `ADDR`
- mapeadores por board/resource
- resposta semântica padronizada saindo do firmware

Hoje esses papéis ainda ficam majoritariamente no host local.

## Glossário

- **Resolver engine**: camada que traduziria comando lógico amplo em ação física detalhada.
- **Despachante compacto**: pipeline atual da BPM, que opera com `ADDR/OP`.
- **Erro funcional**: erro retornado pela GSA quando o `TLV` é válido, mas a intenção não é suportada.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
