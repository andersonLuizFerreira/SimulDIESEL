⬅ [Retornar para Boards de Firmware](../README.md)

# BPM

## Nome canônico

BPM

## Identificador SDH da board

BPM

## Responsabilidade principal no firmware

Ser o gateway físico entre host e baby boards, mantendo a sessão SDGW na serial e roteando o tráfego curto para as boards remotas.

## Papel atual no sistema

No estado atual do projeto, a BPM:

- recebe o bootstrap textual do host;
- entra em `Linked`;
- valida e processa frames SDGW;
- trata comandos locais da própria BPM;
- roteia comandos compactos para a GSA e demais boards;
- encaminha eventos assíncronos vindos das boards para o host.

## Papel da BPM na nova arquitetura da GSA

Com a arquitetura oficial nova da GSA, a BPM não arbitra mais BUSY/IDLE por polling.

O comportamento vigente é:

1. a BPM envia o comando TLV para a GSA no barramento físico;
2. recebe da GSA apenas o ACK lógico síncrono do comando;
3. aguarda a IRQ física da GSA;
4. quando a IRQ entra em `LOW`, lê um TLV assíncrono encapsulado da GSA;
5. encaminha esse TLV no fluxo reverso SDGW como evento.

## Pinagem oficial da BPM para a GSA

- I2C físico com a GSA: `D21` = SDA, `D22` = SCL
- entrada de IRQ da GSA: `D19`
- reset dedicado da GSA: `D23`

## IRQ da GSA

Definições vigentes na BPM:

- entrada de IRQ da GSA em `D19`
- ativo em `LOW`
- leitura por interrupção e drenagem no loop principal

A BPM não interpreta a regra de negócio da operação analógica. Ela apenas:

- detecta a IRQ;
- busca o evento;
- reencaminha o payload ao host.

## Sessão host/gateway

### Handshake

Estado inicial do firmware:

```text
WaitingBanner
```

Fluxo:

1. o host envia `SIMULDIESELAPI`
2. a BPM responde com o banner do dispositivo
3. o firmware desabilita o modo texto
4. a BPM entra em `Linked`

### Keepalive atual

A BPM continua alinhada ao host:

- qualquer frame SDGW válido renova a sessão;
- `PING 0x55` permanece suportado, mas não é a única prova de vida;
- timeout de atividade da sessão = `4000 ms`.

## Roteamento local

### Comandos locais

Exemplo:

- `BPM.gateway ping`

### Comandos roteados

Exemplos:

- `GSA.led set state=on|off`
- `GSA.channel.setpoint set channel=<1..16> value=<0..255>`
- `GSA.channel.enable set channel=<1..16> state=on|off`

No caso roteado, a BPM:

1. recebe o comando SDGW compacto do host;
2. identifica o endereço lógico de destino;
3. usa `GwRouter` para selecionar o barramento;
4. envia o TLV curto para a board remota;
5. devolve a resposta síncrona ao host;
6. em paralelo, encaminha eventos assíncronos quando a board sinaliza IRQ.

## Observações

- o wire format SDGW permanece compatível com o host atual;
- a BPM continua sendo dona do gateway físico e do roteamento;
- o fluxo ativo da GSA não usa mais o modelo BUSY/IDLE com retry ou polling semântico.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.

