# Diagnóstico e Diagnóstico de Falhas

## Telemetria disponível

- eventos de conexão do transporte serial
- eventos de transição de `BpmSerialService.LinkState`
- nome da interface identificada pela BPM
- confirmação de frames SDGW válidos no engine
- mudança de saúde lógica informada pelo `SdGwLinkSupervisor`

Estados de link hoje expostos no host:

- `Disconnected`
- `SerialConnected`
- `Draining`
- `BannerSent`
- `Linked`
- `LinkFailed`

## Rotas de diagnóstico

- UI: leitura visual dos indicadores de serial e link
- logs da aplicação para conexão, handshake e transições de estado
- análise do fluxo funcional por client (`GsaClient`, `BpmClient`)
- análise do comportamento da BPM no enlace e no router

## Verificações recomendadas

1. confirmar abertura da porta serial
2. confirmar que o bootstrap textual chegou a `Linked`
3. confirmar se comandos funcionais estão passando pelo fluxo esperado de TX
4. observar se há RX SDGW válido recente mantendo o link vivo
5. confirmar se a BPM continua respondendo no prazo esperado

## Leitura correta do keepalive

O diagnóstico atual não deve mais assumir ping manual como prova central de vida do link.

O comportamento correto é:

- RX SDGW válido mantém o link vivo no host
- o supervisor só agenda ping sob silêncio
- a BPM também mantém a sessão por atividade SDGW válida

Se o link cair, as perguntas corretas são:

- houve silêncio real de RX válido?
- o comando ficou preso antes do scheduler, no engine ou na BPM?
- houve resposta tardia que ainda deveria ter sido entregue ao engine?

## Casos práticos de falha

### Link cai para `LinkFailed`

Verificar:

- se o bootstrap textual ocorreu corretamente
- se houve silêncio de RX acima do timeout lógico do host
- se a porta permaneceu aberta
- se a BPM ainda estava dentro da janela de atividade de `4000 ms`

### Timeout aguardando ACK

Verificar:

- se o comando realmente saiu pelo `SdGwTxScheduler`
- se o engine estava aguardando `ACK`
- se a BPM recebeu e validou o frame
- se a resposta veio tarde ou foi perdida no caminho serial

### Instabilidade em comando funcional repetitivo

No caso da GSA LED, considerar:

- timeout funcional do LED em `400 ms`
- `2` retries no mapeamento
- correlação reforçada da resposta no `GsaClient`
- router da BPM com timeout de `100 ms`

## Limitações atuais

- ainda não existe painel persistente de logs estruturados no app
- parte do diagnóstico continua dependendo de observação manual e correlação por fluxo

[Retornar ao README principal](../README.md)
