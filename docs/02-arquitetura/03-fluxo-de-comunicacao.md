# Fluxo de Comunicação

## Estado atual

O fluxo de comunicação implementado no repositório é híbrido: textual no bootstrap e binário durante a operação normal. Essa escolha aparece em ambos os lados do enlace e explica por que o host primeiro procura um banner legível antes de passar a tratar frames delimitados por `0x00`.

## Funcionamento técnico

### Fase 1: estabelecimento de link

Máquina de estados do host (`SerialLinkService`):

    Disconnected
      -> SerialConnected
      -> Draining
      -> BannerSent
      -> Linked
      -> LinkFailed

Máquina de estados do gateway (`SggwLink`):

    WaitingBanner
      -> Linked
      -> WaitingBanner (se resetar ou expirar)

O host drena a serial, envia o banner de ativação e aguarda uma linha iniciada por `SimulDIESEL ver`. Depois disso, ativa o motor de frames.

### Fase 2: frame lógico host/gateway

Estrutura lógica observada em `Sggw.defs.h`:

    CMD | FLAGS | SEQ | PAYLOAD... | CRC8

- `CMD`: 4 bits de endereço e 4 bits de operação.
- `FLAGS`: inclui pedido de `ACK` e marcação de evento.
- `SEQ`: número de sequência para deduplicação.
- `CRC8`: validação do quadro lógico.

Esse bloco é codificado em `COBS` e finalizado com delimitador `0x00`.

### Exemplo de ida e volta

Exemplo conceitual de `PING` para o endereço do gateway:

    Host
      Frame lógico: CMD=0x01 FLAGS=0x01 SEQ=0x10 PAYLOAD=[]
      -> COBS(...)
      -> 00

    Gateway
      valida frame
      gera ACK/RESP
      devolve frame com mesmo SEQ

Exemplo de operação roteada para o GSA:

    Host -> Gateway:
      CMD = [ADDR_GSA | OP_GSA_TLV]
      PAYLOAD = TLV do dispositivo

    Gateway -> GSA (I2C):
      T | L | V... | CRC8

    GSA -> Gateway:
      T | L | V... | CRC8

### Fase 3: roteamento interno

`GwRouter` consulta `GwDeviceTable` para determinar:

- tipo do barramento (`I2C` ou `SPI`);
- endereço físico ou pino de seleção;
- parâmetros necessários para a transação.

`GwI2cBus::transact` e `GwSpiBus::transact` usam a mesma ideia de request/resposta curta e validada, preservando um contrato uniforme para o restante do firmware.

## Integração com o modelo SDH

A arquitetura de evolução do SimulDIESEL deve convergir para o padrão SDH (SimulDiesel Hardware Command) como contrato oficial da camada Hardware.

Nesse modelo, todo comando é composto por:

- `version`
- `target`
- `op`
- `args`
- `meta`

O fluxo lógico de processamento esperado passa a ser:

1. recepção do comando;
2. validação da versão;
3. resolução do `target`;
4. resolução da `op`;
5. validação dos `args`;
6. execução no recurso correspondente;
7. montagem da resposta padronizada.

Esse padrão permite desacoplamento entre:

- transporte físico;
- parser do protocolo;
- roteamento por board;
- lógica funcional do recurso;
- representação textual e JSON.

## Limitações

O protocolo do host está bem definido no código, mas o repertório funcional ainda é enxuto. O canal existe, a confiabilidade existe e o roteamento existe; o número de comandos de alto nível e a quantidade de dispositivos funcionais ainda são limitados. Também não há evidência, no fluxo atual, de multiplexação avançada de sessões ou fila concorrente de requisições.

## Evolução prevista

O fluxo atual já suporta a ampliação natural do sistema:

- novos `OP codes` por endereço;
- novos dispositivos na tabela do gateway;
- novos tipos de evento gerados do firmware para a UI;
- maior densidade de payloads TLV, desde que respeitados os limites de tamanho do enlace;
- convergência gradual para o envelope semântico SDH como contrato único da camada Hardware.

## Referências

- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/02-sdh-response-model.md`
- `docs/06-protocolos/03-sdh-examples.md`

[Retornar ao README principal](../README.md)
