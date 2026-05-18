⬅ [Retornar para Arquitetura de Firmware](01-arquitetura-firmware.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Gerenciamento de Recursos em Firmware

Esta página descreve os limites, filas, watchdogs e buffers que o firmware realmente usa para manter a sessão e processar operações.

## BPM: limites e tempos reais

As constantes vivas estão em `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`.

| recurso | valor real | papel | status |
| --- | --- | --- | --- |
| `SDGW_MAX_LOGICAL_FRAME` | `250` | frame lógico antes de `COBS` | `IMPLEMENTADO` |
| `SDGW_MAX_PAYLOAD` | `247` | carga útil máxima do frame | `IMPLEMENTADO` |
| `SDGW_MAX_ENCODED_FRAME` | `384` | frame já codificado | `IMPLEMENTADO` |
| `SDGW_HANDSHAKE_BUFFER` | `64` | buffer para detectar `SIMULDIESELAPI` | `IMPLEMENTADO` |
| `SDGW_HANDSHAKE_TIMEOUT_MS` | `2000` | timeout de handshake | `IMPLEMENTADO` |
| `SDGW_LINK_ACTIVITY_TIMEOUT_MS` | `4000` | watchdog de sessão `Linked` | `IMPLEMENTADO` |
| `SDGW_GATEWAY_ROUTE_TIMEOUT_MS` | `100` | timeout do roteamento para board | `IMPLEMENTADO` |

## BPM: ownership e watchdog

`SdgwSessionOwner` garante exclusividade de endpoint. O link só processa bytes quando o endpoint atual continua dono da sessão.

Trecho real de `SdgwEndpointMux`:

```cpp
if (_sessionOwner.hasOwner()) {
    const SdgwEndpointKind currentOwner = _sessionOwner.owner();
    ISdgwEndpoint* ownerEndpoint = findByKind(currentOwner);
    if (ownerEndpoint && ownerEndpoint->isConnected())
        return currentOwner;

    _sessionOwner.release(currentOwner);
}
```

Esse bloco existe para evitar duas origens alimentando a mesma máquina de estados `SDGW`.

## GSA: filas e capacidade

As constantes vivas estão em `hardware/firmware/GSA - Gerador de sinais analógicos/include/config.h`.

| recurso | valor real | papel | status |
| --- | --- | --- | --- |
| `TLV_MAX_LEN` | `32` | limite do pacote `TLV + CRC` | `IMPLEMENTADO` |
| `GSA_PHYSICAL_OP_QUEUE_SIZE` | `24` | fila de operações físicas | `IMPLEMENTADO` |
| `GSA_EVENT_QUEUE_SIZE` | `24` | fila de eventos assíncronos | `IMPLEMENTADO` |
| `GSA_LOGICAL_I2C_DELAY_US` | `5` | temporização do `SoftwareWire` | `IMPLEMENTADO` |
| `GSA_TCA_RESET_PULSE_MS` | `1` | pulso de reset do `TCA9548A` | `IMPLEMENTADO` |
| `GSA_TCA_RESET_SETTLE_MS` | `1` | tempo de assentamento após reset | `IMPLEMENTADO` |

## GSA: aceite síncrono vs resultado físico

O firmware separa dois recursos lógicos:

1. resposta síncrona de aceite no `Transport`
2. conclusão física posterior via `BusArbiterService` e evento `0x31`

Trecho real de `AnalogService::processCompletedHardwareOperations()`:

```cpp
while (_busArbiter.popCompletedOperation(result)) {
  applyCompletedOperation(result);

  uint8_t payload[3] = {
    result.originType,
    result.channel,
    result.status
  };
  enqueueEvent(CMD_PHYSICAL_EVENT, payload, sizeof(payload));
}
```

Esse trecho existe para desacoplar o protocolo curto no barramento físico do tempo real necessário para a etapa elétrica.

## Retry, ACK e erro

- **IMPLEMENTADO na BPM**: `ACK`, `ERR`, deduplicação por `seq` e retransmissão da última resposta quando a sequência se repete.
- **IMPLEMENTADO na GSA**: códigos físicos `0x01`, `0x02`, `0x03` retornados em evento assíncrono.
- **NÃO IMPLEMENTADO no firmware**: retry automático da operação elétrica na GSA.
- **LEGADO**: modelo BUSY/IDLE como controle de concorrência da GSA.

## Glossário

- **Deduplicação**: retransmissão segura da última resposta quando o host repete a mesma sequência.
- **Watchdog**: temporizador usado para derrubar handshake ou sessão zumbi.
- **Fila física**: conjunto de operações pendentes para `TCA9548A` e `MCP4725`.
- **Evento assíncrono**: resposta publicada fora do caminho síncrono do comando.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
