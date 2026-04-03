# Barramentos

## Estado atual

O repositório implementa três classes principais de transporte:

- serial entre host e gateway;
- `I2C` físico entre gateway e baby boards;
- `SPI` para dispositivos mapeados diretamente pelo gateway.

No caso da GSA, a arquitetura oficial passou a usar dois barramentos `I2C` independentes.

## Funcionamento técnico

### Serial host/gateway

Responsabilidades:

- transporte bruto em `SerialTransport`;
- sincronização textual inicial;
- enquadramento SDGW com `COBS`, `CRC8`, sequência e `ACK`.

### I2C físico gateway/GSA

No barramento físico:

- BPM = `master`
- GSA = `slave`
- endereço da GSA = `0x23`
- BPM ESP32 `SDA=D21`, `SCL=D22`
- GSA Nano `SDA=A4`, `SCL=A5`

Esse barramento transporta o TLV curto:

```text
T | L | V... | CRC8
```

A resposta síncrona da GSA agora significa apenas:

- comando recebido;
- payload válido;
- operação aceita para processamento.

### I2C lógico interno da GSA

Além do barramento físico, a GSA possui um barramento I2C eletrônico independente:

- GSA = `master`
- `TCA9548A = 0x70`
- `MCP4725 = 0x60 / 0x61`
- GSA Nano `SDA=D2`, `SCL=D3`
- reset dedicado do `TCA9548A` em `D8`

Esse barramento não é compartilhado com a BPM. Por isso:

- a GSA não precisa mais trocar de papel `slave/master` no mesmo fio;
- o modelo BUSY/IDLE anterior deixou de ser necessário para arbitragem do barramento físico.

### IRQ físico GSA -> BPM

A conclusão da operação física da GSA é sinalizada por uma linha dedicada:

- GSA Nano `D4`
- BPM ESP32 `D19`
- ativo em `LOW`
- pull-up externo em `3,3 V`
- open-drain por software na GSA

### Reset dedicado

- BPM ESP32 `D23` controla o reset físico da GSA
- o reset local do `TCA9548A` fica em `D8` na GSA

O IRQ substitui o modelo antigo de polling semântico para BUSY/IDLE.

### SPI gateway/dispositivo

`GwSpiBus` continua representando o mesmo papel para dispositivos SPI, com seleção por `chip select` e transação request/response curta.

## Observações

- a documentação oficial da GSA deve sempre distinguir `I2C físico` de `I2C lógico`;
- o evento assíncrono associado à conclusão da etapa elétrica da GSA é `0x31`;
- o evento de `fault` legado `0x30` continua existindo para fault de canal, mas não substitui o resultado físico do `0x31`.

[Retornar ao README principal](../README.md)
