# Baby Boards

## Estado atual

O hardware foi concebido de forma modular, com a BPM atuando como gateway e as baby boards encapsulando funções específicas de simulação e interface física.

O exemplo mais completo hoje é a GSA.

## Funcionamento técnico

O padrão de integração observado no projeto é:

1. o host fala com a BPM pela serial;
2. a BPM resolve o endereço lógico de destino;
3. a BPM entrega uma mensagem curta para a baby board;
4. a baby board responde com um contrato simples e validado;
5. eventos assíncronos retornam pelo caminho reverso via BPM.

## Caso oficial da GSA

A GSA hoje é a referência concreta do modelo de baby board com duas camadas internas:

### Pinagem oficial BPM <-> GSA

- I2C físico BPM ESP32 `D21/D22` <-> GSA Nano `A4/A5`
- I2C lógico interno da GSA em `D2/D3`
- IRQ físico GSA -> BPM em `D4` -> `D19`
- reset do `TCA9548A` em `D8` na GSA
- reset da GSA controlado pela BPM em `D23`

### Camada lógica

- `Transport`
- `Link`
- `Service`
- `LedService`
- `AnalogService`
- `EepromService`

Essa camada recebe os comandos TLV da BPM e mantém o shadow RAM dos canais.

### Camada eletrônica

- `BusArbiterService`
- `Tca9548Service`
- `Mcp4725Service`

Essa camada executa a atuação física real no barramento I2C interno da própria GSA.

### Sinalização assíncrona

Quando a etapa física termina, a GSA:

- gera um TLV assíncrono `0x31`;
- sinaliza a BPM por IRQ físico;
- deixa a BPM buscar e encaminhar o evento ao host.

## Observações

- a baby board não precisa conhecer o protocolo textual do host;
- a BPM continua concentrando o protocolo de sessão e o roteamento;
- no caso da GSA, o modelo BUSY/IDLE anterior foi abandonado em favor de dois barramentos independentes + IRQ.

[Retornar ao README principal](../README.md)
