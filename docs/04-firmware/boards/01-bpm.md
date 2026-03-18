# BPM

## Nome canônico

BPM

## Identificador SDH da board

BPM

## Responsabilidade principal no firmware

A BPM é a board principal do enlace host/gateway e concentra:

- handshake inicial da conexão serial
- sessão binária SDGW com o host
- aplicação local do gateway
- roteamento para as baby boards
- recursos locais da própria BPM

## Papel atual no sistema

No estado atual do projeto, a BPM é o ponto de terminação física e lógica do link usado pelo host C#.

Ela:

- recebe o bootstrap textual
- entra em `Linked`
- valida frames SDGW
- trata comandos locais da BPM
- roteia comandos compactos para GSA e demais boards

## Sessão host/gateway

### Handshake

Estado inicial do firmware:

    WaitingBanner

Fluxo:

1. o host envia `SIMULDIESELAPI`
2. a BPM responde com o banner do dispositivo
3. o firmware desabilita o modo texto
4. a BPM entra em `Linked`

### Keepalive atual

A BPM não depende mais exclusivamente de `PING 0x55`.

Comportamento atual:

- qualquer frame SDGW válido recebido renova a atividade da sessão
- essa renovação acontece logo após a validação estrutural do frame
- `PING` continua suportado, mas é apenas mais um frame válido possível

Timeout atual:

- atividade do link: `4000 ms`

Se não houver atividade SDGW válida dentro dessa janela:

- a BPM encerra a sessão binária
- volta para `WaitingBanner`

## Papel do gateway local

A BPM trata dois grupos de comandos:

### Comandos locais

Exemplo atual:

- `BPM.gateway ping`

O host resolve esse comando para o formato SDGW compacto local da BPM antes do envio.

### Comandos roteados

Exemplo atual:

- `GSA.led set state=on|off`

Nesse caso, a BPM:

1. recebe o comando SDGW compacto do host
2. identifica o endereço lógico de destino
3. usa `GwRouter` para selecionar o barramento
4. envia TLV curto para a baby board
5. devolve a resposta ao host como evento SDGW

## Timeouts operacionais atuais

Valores relevantes da BPM atual:

- timeout de atividade da sessão SDGW: `4000 ms`
- timeout do router/gateway para a baby board: `100 ms`

Esses dois tempos são parte importante do alinhamento recente com o host.

## Alinhamento com o host atual

O host atual usa:

- `BpmSerialService`
- `SdGwTxScheduler`
- `SdGwLinkSupervisor`

Esse host considera o link vivo por RX SDGW válido e agenda ping apenas sob silêncio.

A BPM foi ajustada para o mesmo conceito:

- atividade válida mantém a sessão
- tráfego funcional não deve provocar logout artificial por falta de ping explícito

## Casos observados no código atual

Targets semânticos atualmente exercitados no host:

- `BPM.gateway`
- `GSA.led`

Do ponto de vista do firmware, isso chega como:

- comando compacto local da BPM
- comando compacto roteado para a GSA

## Observações

- o wire format SDGW permanece compatível com o host atual
- a BPM continua sendo a dona do gateway físico e do roteamento
- o parser SDH textual ainda não é a interface de entrada ativa do firmware; o host resolve SDH para SDGW compacto antes do envio

[Retornar ao README principal](../../README.md)
