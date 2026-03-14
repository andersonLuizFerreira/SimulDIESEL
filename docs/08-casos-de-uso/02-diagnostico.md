# Diagnóstico e Diagnóstico de Falhas

## Telemetria disponível

- Eventos de conexão (`ConnectionChanged`, `LinkStateChanged`).
- Estados de link: `SerialConnected`, `Draining`, `BannerSent`, `Linked`, `LinkFailed`.
- Eventos de frame e health no cliente SGGW.

## Rotas de diagnóstico

- UI: consulta visual dos ícones/labels de estado.
- Logs do código (console/exception output) para eventos de conexão e handshake.
- Tratamento de erro no firmware de transporte serial e no link engine.

## Verificações recomendadas

1. Confirmar banner do gateway (`SimulDIESEL ver`).
2. Enviar ping (`0x55`) e validar ACK.
3. Confirmar retorno do comando de aplicação (`LED`) e estado recebido.

## Limitações atuais

- Não há painel de logs central persistente estruturado no app.
- Falhas de negócio podem depender de observação manual.

[Retornar ao README principal](../README.md)
