# Arquitetura de Firmware

## Visão geral

O firmware atual da BPM continua baseado em SDGW/SGGW como camada binária de transporte entre host e gateway.

A arquitetura ativa hoje está organizada em torno de:

- transporte serial
- link SDGW confiável
- aplicação local do gateway
- roteamento para barramentos internos
- contratos TLV curtos para as baby boards

O firmware não depende mais de ping periódico fixo como única forma de manter a sessão host/gateway.

## Camadas internas do firmware

### Transporte físico

Responsável por:

- UART
- I2C
- SPI
- GPIO e temporização auxiliar

Essa camada apenas movimenta bytes.

### Link SDGW

Implementado principalmente em `SggwLink`.

Responsabilidades:

- handshake textual inicial
- transição para modo binário
- delimitação de frames
- `COBS`
- `CRC-8/ATM`
- tratamento de `ACK` / `ERR`
- cache da última resposta para retransmissão
- watchdog de atividade da sessão

### Aplicação local do gateway

Implementada em `GatewayApp`.

Responsabilidades:

- tratar comandos locais da BPM
- distinguir endereço local BPM de comandos roteados
- encaminhar comandos compactos para o router
- transformar erros do gateway em eventos SDGW

### Router e barramentos

O `GwRouter` escolhe o destino físico e executa a transação:

- I2C
- SPI

O payload interno continua sendo TLV curto com CRC próprio da transação para a baby board.

## Sessão host/gateway

### Handshake inicial

O bootstrap continua textual:

1. a BPM inicia em `WaitingBanner`
2. o host envia `SIMULDIESELAPI`
3. a BPM responde com seu banner
4. a BPM entra em `Linked`
5. o transporte em texto é desabilitado

Esse comportamento continua necessário para o primeiro estabelecimento da conexão serial.

### Operação binária

Depois do handshake, o link usa frames SDGW:

    CMD | FLAGS | SEQ | PAYLOAD | CRC8

Nada foi alterado em:

- wire format
- `COBS`
- delimitador `0x00`
- `ACK`
- `ERR`
- flags do protocolo

## Keepalive atual da BPM

O firmware da BPM foi alinhado ao host novo.

### Lógica antiga

Antes, a sessão era renovada na prática por `PING 0x55`.

Isso criava divergência com o host quando havia tráfego funcional válido, mas sem ping explícito recente.

### Lógica atual

Agora:

- qualquer frame SDGW estruturalmente válido recebido renova a atividade da sessão
- a renovação ocorre logo após a validação estrutural do frame
- `PING 0x55` continua suportado, mas não é a única prova de vida

Em termos práticos:

- comandos funcionais válidos mantêm a sessão ativa
- `ACK`s e demais frames válidos também contam como atividade
- a BPM não deve mais se auto-deslogar no meio de tráfego funcional só por falta de ping explícito

### Timeout atual da sessão

O watchdog da BPM mede silêncio de atividade SDGW válida.

Valor atual:

- timeout de atividade do link: `4000 ms`

Quando esse silêncio expira:

- a BPM faz logout da sessão binária
- volta para `WaitingBanner`

Esse comportamento continua existindo, mas agora está baseado em ausência de atividade válida, não em ausência específica de `PING`.

## Router do gateway

O `GatewayApp` continua tratando:

- comandos locais da BPM
- comandos compactos roteados para as baby boards

O timeout interno do roteamento foi tornado mais realista para uso interativo.

Valor atual:

- timeout do router/gateway: `100 ms`

Isso reduz falhas artificiais em operações repetitivas como o fluxo de LED da GSA.

## Papel atual do SDH no firmware

No host, o SDH já existe como camada semântica.

No firmware da BPM, a situação atual é mais conservadora:

- o host resolve SDH para comandos SDGW compactos antes do envio
- a BPM continua operando sobre o contrato compacto `[ADDR:4][OP:4]`
- o gateway trata endereço local BPM ou roteia para a baby board correspondente

Ou seja:

- o firmware atual ainda não usa SDH textual como parser de entrada do gateway
- a compatibilidade real vigente é host SDH semântico -> SDGW compacto -> BPM/Gateway -> TLV interno

## Exemplo atual de fluxo funcional

Caso de uso: `GSA.led set state=on`

Fluxo:

1. o host monta o comando SDH
2. o host o converte para SDGW compacto do endereço GSA
3. a BPM recebe o frame SDGW válido
4. a atividade da sessão é renovada
5. o `GatewayApp` encaminha a transação ao `GwRouter`
6. o router envia o TLV curto para a GSA
7. a resposta volta ao gateway
8. a BPM publica a resposta como evento SDGW para o host

## Conclusão

A arquitetura atual do firmware da BPM pode ser resumida assim:

- handshake textual inicial preservado
- operação binária SDGW preservada
- keepalive baseado em atividade SDGW válida
- watchdog de sessão em `4000 ms`
- router com timeout de `100 ms`
- gateway ainda consumindo comandos compactos resolvidos pelo host

[Retornar ao README principal](../README.md)
