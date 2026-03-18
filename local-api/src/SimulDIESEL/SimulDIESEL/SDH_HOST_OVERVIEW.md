# SDH/SDGW no host SimulDIESEL

## Visão geral

O host atual do SimulDIESEL usa a BPM como dona funcional do link serial e organiza o envio em duas camadas distintas:

- camada técnica de transporte SDGW: `SdGwLinkEngine`
- camada de arbitragem de TX: `SdGwTxScheduler`
- camada de sessão e consumo de frames: `SdgwSession`
- camada semântica de comando: `SdhClient`
- clients funcionais por board: `GsaClient`, `BpmClient`
- supervisão lógica de link: `SdGwLinkSupervisor`
- composição central do host: `BpmSerialService`

O ponto global transitório atualmente usado pela UI é:

    BpmSerialService.Shared

Esse ponto substitui o acesso global legado baseado em `SerialLink` e `SerialLinkService`, que não são mais a arquitetura vigente.

## Fluxo de transmissão atual

O caminho central de transmissão no host é:

    UI / FormsLogic
        -> BpmSerialService.Shared
        -> Client funcional por board
        -> SdhClient
        -> SdhToSdgwMapper
        -> SdgwSession
        -> SdGwTxScheduler
        -> SdGwLinkEngine
        -> SerialTransport

O scheduler é o único caminho normal de TX do link.

Ele organiza a fila com três prioridades:

- `High`: comandos funcionais da aplicação
- `Normal`: uso interno sem urgência especial
- `Low`: ping de supervisão

Na prática:

- `GsaClient.SetLedAsync(...)` envia em `High`
- `BpmClient.PingGatewayAsync()` envia em `High`
- o `SdGwLinkSupervisor` agenda ping em `Low`

Com isso, a arbitragem de envio deixou de depender de `Busy` como mecanismo principal para concorrência interna. O `Busy` continua existindo no engine como proteção técnica, mas não é mais o comportamento esperado no uso normal.

## Papel de cada componente

### `BpmSerialService`

É a fachada funcional do link serial da BPM.

Responsabilidades principais:

- controlar conexão/desconexão serial
- executar o handshake textual inicial
- compor `SdGwLinkEngine`, `SdGwTxScheduler`, `SdgwSession`, `SdhClient`, `SdGwLinkSupervisor`, `GsaClient` e `BpmClient`
- refletir estado lógico do link para a UI
- manter o parser SDGW ativo após o primeiro `Linked` bem-sucedido da conexão atual

### `SdGwLinkEngine`

É a camada técnica de framing SDGW:

- delimitação por `0x00`
- `COBS`
- `CRC-8/ATM`
- `ACK` / `ERR`
- timeout/retry
- stop-and-wait com uma transação pendente por vez

Ele continua simples e técnico. A fila e a prioridade não ficam nele.

### `SdGwTxScheduler`

É o agendador central de transmissão do host.

Responsabilidades:

- enfileirar requisições
- respeitar prioridade `High -> Normal -> Low`
- manter FIFO dentro da mesma prioridade
- despachar um item por vez para o `SdGwLinkEngine`
- completar as tarefas pendentes com o resultado final do envio
- encerrar pendências com `TransportDown` quando o transporte cai

### `SdgwSession`

É a sessão de alto nível do SDGW sobre o scheduler.

Responsabilidades:

- expor `SendAsync(...)` sem acesso direto ao engine
- publicar `FrameReceived`
- publicar `EventReceived`

### `SdhClient`

É a camada semântica do host acima do SDGW.

Responsabilidades:

- validar `SdhCommand`
- mapear o comando semântico para SDGW compacto
- delegar o envio ao `SdgwSession`

O host ainda resolve SDH para SDGW compacto antes do envio. O binding lógico-físico completo continua fora do host.

### `SdGwLinkSupervisor`

É o supervisor lógico de saúde do link.

Ele não gera ping periódico fixo.

A estratégia atual é:

- atividade SDGW válida recebida mantém o link vivo
- o supervisor mede silêncio de RX válido
- ping só é agendado quando há ociosidade
- timeout lógico de link ocorre apenas se o silêncio ultrapassar o limite configurado

Configuração atualmente usada no host:

- `IdleBeforePingMs = 1500`
- `LinkTimeoutMs = 3000`
- `PingTimeoutMs = 150`
- `PingRetries = 2`
- `TickPeriodMs = 50`

## Keepalive atual

O host não depende mais de ping periódico fixo para manter a sessão.

O que prova vida agora é:

- qualquer frame SDGW estruturalmente válido recebido pelo `SdGwLinkEngine`

Isso inclui:

- `ACK`
- `ERR`
- frame normal
- evento

Quando um frame válido chega:

- o engine dispara `ValidFrameReceived`
- o `BpmSerialService` encaminha isso para `SdGwLinkSupervisor.OnValidFrameReceived()`
- o watchdog lógico é renovado

O ping do supervisor é apenas estímulo de verificação sob silêncio. Ele não é mais a única prova de vida do link.

## Comportamento atual de RX após o primeiro `Linked`

Depois que a conexão serial atinge `Linked` uma vez, o `BpmSerialService` marca a sessão SDGW da conexão atual como estabelecida.

A partir desse ponto:

- bytes binários SDGW continuam podendo ser entregues ao `SdGwLinkEngine`
- isso vale mesmo se o estado lógico cair temporariamente para `LinkFailed`
- a condição é a porta serial continuar aberta

Esse ajuste evita que `ACK`s ou respostas tardias sejam tratados como texto de handshake e descartados indevidamente.

O handshake textual inicial continua existindo apenas antes do primeiro `Linked` da conexão atual.

## Caso funcional GSA LED

O fluxo atual do LED embutido da GSA é:

    GsaClient.SetBuiltinLedAsync(bool)
        -> GsaClient.SetLedAsync(bool)
        -> SdhClient.SendAsync(...)
        -> SdhToSdgwMapper.MapGsaLed(...)
        -> SdgwSession.SendAsync(...)
        -> SdGwTxScheduler (High)
        -> SdGwLinkEngine

Mapeamento atual:

- target SDH: `GSA.led`
- operação: `set`
- transporte SDGW: comando compacto `GW_ADDR_GSA / GW_OP_GSA_TLV_TRANSACT`
- payload: TLV curto com CRC interno da transação para a GSA

Correções já incorporadas para estabilizar esse fluxo:

- `TimeoutMs = 400`
- `Retries = 2`
- correlação de resposta reforçada no `GsaClient`
- validação da resposta antes de completar a requisição pendente
- conferência do estado aplicado esperado para reduzir aceitação de resposta tardia errada

Isso reduziu a instabilidade em clique repetido no `LED_BUILTIN`.

## Limitações atuais

- o host ainda trabalha com uma sessão SDGW por vez
- a recepção funcional ainda entrega `SggwFrame`, não um envelope SDH completo
- o catálogo SDH suportado continua pequeno
- `BpmSerialService.Shared` ainda é um ponto global transitório, mantido por compatibilidade de composição com a UI atual

## Legado removido

Os seguintes componentes não são mais a fonte de verdade do host atual:

- `SerialLink`
- `SerialLinkService`
- `SdGgwClient`
- `SdgwHealthService`

Qualquer menção a eles deve ser tratada como legado removido ou documentação histórica.
