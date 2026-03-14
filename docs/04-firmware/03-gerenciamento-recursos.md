# Gerenciamento de Recursos em Firmware

## Objetivos

O firmware atual prioriza determinismo de comunicação. O gerenciamento de recursos cobre:

- limites de frame e buffers;
- controle de timeout;
- watchdog de link;
- cache de última resposta para deduplicação.

## Limites estruturais (Gateway)

Constantes em `hardware/firmware/esp32-api-bridge/include/Sggw.defs.h`:

- `SGGW_MAX_LOGICAL_FRAME = 250`
- `SGGW_MAX_PAYLOAD = 247`
- `SGGW_MAX_ENCODED_FRAME = 384`
- `SGGW_MAX_LAST_RESPONSE = 64`

Esses valores limitam frame bruto, payload e buffer de retransmissão.

## Controle de tempo

- `SggwLink` mantém estado de watchdog (`checkPingWatchdog`) para encerrar link em ausência prolongada de ping.
- Handshake e heartbeat são controlados via máquina de estados no PC.
- `SdGwHealthService` e `SdGwLinkEngine` mantêm timers para ping/ACK/retry.

## Tolerância a erro e deduplicação

- CRC de integridade:
  - COBS no fluxo serial;
  - `CRC8` com `poly 0x07`.
- Deduplicação de retransmissão:
  - `Gw`: resposta reenviada por sequência quando necessário.
  - `GSA`: `errCode` mantido até consulta de erro.

## Recursos de observabilidade

Eventos existentes:

- `Serial_ConnectionChanged`, `LinkStateChanged`, erros de transporte;
- `ProtocolError` no parser/link engine;
- status de saúde (`LinkHealthChanged`) no cliente local.

## Pontos ainda não implementados (dependente de validação)

- Métricas formais persistidas por sessão (contadores detalhados, histórico de falhas, latência média).
- Gerenciamento de memória por telemetria contínua no firmware.
- Estratégia de atualização OTA não documentada como recurso ativo.

[Retornar ao README principal](../README.md)
