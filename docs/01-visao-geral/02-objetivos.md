# Objetivos

## Estado atual

Os objetivos efetivamente sustentados pelo repositório hoje podem ser agrupados em quatro frentes:

- manter um enlace local confiável entre host e BPM
- arbitrar o TX de forma previsível entre tráfego funcional e supervisão
- rotear comandos para recursos locais da BPM e baby boards
- criar uma base estável para expansão de comandos SDH e serviços embarcados

Esses objetivos aparecem diretamente no código atual do host e do firmware.

## Funcionamento técnico

### Objetivo 1: enlace confiável com o host

No host:

- `SerialTransport` faz a serial bruta
- `BpmSerialService` controla handshake e estado funcional do link
- `SdGwLinkEngine` garante framing, `ACK`, timeout e retry

Na BPM:

- `SggwLink` valida frames SDGW
- a sessão é mantida por atividade SDGW válida

### Objetivo 2: arbitragem previsível de transmissão

O host agora possui um caminho central de TX:

    SdgwSession -> SdGwTxScheduler -> SdGwLinkEngine

Isso permite:

- prioridade alta para comandos funcionais
- prioridade baixa para pings internos
- redução drástica da dependência de `Busy` como mecanismo normal de concorrência

### Objetivo 3: roteamento por endereço lógico

O protocolo compacto continua usando:

    CMD = [ADDR:4][OP:4]

Com isso:

- o host trata BPM e periféricos sob o mesmo contrato de transporte
- a BPM decide se o comando é local ou roteado
- o `GwRouter` escolhe o barramento e o destino físico

### Objetivo 4: suporte a bancada e diagnóstico

O software local atual oferece:

- controle de conexão serial
- supervisão lógica do link por `SdGwLinkSupervisor`
- clients funcionais por board
- caso funcional estabilizado para `GSA.led`

Esse conjunto atende ao objetivo de operação assistida de bancada sem depender de ping manual constante.

## Limitações

Nem todos os objetivos amplos do nome SimulDIESEL estão completos no repositório atual.

Ainda faltam, por exemplo:

- catálogo funcional mais amplo de boards
- maior cobertura de respostas/eventos SDH
- cenários mais ricos de sensores e protocolos automotivos

O foco real hoje é consolidar a espinha dorsal host <-> BPM <-> board.

## Evolução prevista

Os próximos passos coerentes com os objetivos atuais são:

- ampliar o catálogo SDH suportado no host
- ampliar a tabela de destinos e serviços na BPM
- formalizar testes de integração mais densos
- reduzir dependências transitórias de composição da UI

[Retornar ao README principal](../README.md)
