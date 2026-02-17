# SdGwHealthService (Serviço de Saúde do Link)

## Propósito
Executar pings periódicos para verificar a saúde funcional do link SGGW e detectar falhas de transporte/timeout, notificando a camada superior.

## Escopo
Classe: `SimulDIESEL.BLL.SdGwHealthService`

## Responsabilidades
- Periodicamente enviar um comando `Ping` (`PingCmd`, padrão `0x55`) usando `SdGwLinkEngine.SendAsync` com `RequireAck = true`.
- Avaliar `SendOutcome` retornado pelo engine e traduzir em estado de saúde (`alive`).
- Detectar falha de transporte e sinalizar `TransportDownDetected` (uma vez por ocorrência) e desabilitar o serviço para evitar spam.
- Expor evento `LinkHealthChanged(bool alive)` para informar consumidores.

## Configuração (Config)
- `PingCmd` (byte) — comando a ser usado (padrão `0x55`).
- `PingIntervalMs` — intervalo entre pings (padrão 1000 ms).
- `PingTimeoutMs` — timeout para cada tentativa de ping (padrão 150 ms).
- `PingRetries` — número de retransmissões permitidas pelo engine (padrão 2).

## Operação
- `SetEnabled(true)` inicializa um `Timer` que executa `Tick` a cada `PingIntervalMs`.
- `Tick`:
  - Protegido contra reentrância por `Interlocked.Exchange` em `_inTick`.
  - Monta `SendOptions` com `RequireAck = true`, `TimeoutMs = PingTimeoutMs`, `MaxRetries = PingRetries`.
  - Aguarda `engine.SendAsync(PingCmd, empty, opt)`.
  - Se `SendOutcome.Acked` => marca alive (`LinkHealthChanged(true)`).
  - Se `SendOutcome.TransportDown` => registra log, marca dead, se ainda não latched (`_transportDownLatched == false`):
    - seta `_transportDownLatched = true`, para o timer (`StopTimer()`), desabilita service (`_enabled = false`) e dispara `TransportDownDetected`.
  - Para outros outcomes (Timeout, Nacked, Busy) => marca dead e emite log (`HealthLog`).

## Tratamento de falhas
- Em caso de exceção durante ping, escreve log em `HealthLog` e marca `alive = false`.
- `TransportDownDetected` é disparado somente uma vez por ocorrência (latched) para evitar flooding de eventos quando transporte falha repetidamente.

## Thread safety / concorrência
- `_inTick` utilizado com `Interlocked.Exchange` para garantir que não haja execução concorrente de `Tick`.
- `_enabled` e `_alive` são variáveis voláteis/imutáveis de leitura concorrente; alterações disparando eventos são seguras para uso por consumidores.

## Eventos públicos
- `LinkHealthChanged(bool alive)` — notifica mudança no estado de saúde do link.
- `HealthLog(string)` — logs operacionais do serviço de health.
- `TransportDownDetected()` — notificação de falha física detectada via tentativa de ping.

## Not implemented / limitações
- Não realiza recuperação automática do transporte; detecta e notifica para que camada superior tome ação.