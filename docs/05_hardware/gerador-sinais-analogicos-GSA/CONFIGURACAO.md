# GSA — Configuração do Firmware

## Constantes e definições relevantes (defs.h)

- `I2C_GSA_ADDR = 0x23`
- `TLV_MAX_LEN = 32`

### Comandos TLV
- `CMD_GET_ERR = 0x01`
- `CMD_CLR_ERR = 0x02`
- `CMD_GET_LED = 0x11`
- `CMD_SET_LED = 0x12`

### Pinos
- `LED_PIN = LED_BUILTIN`

## Itens configuráveis

- Timeout do ping na classe `LinkService` (parâmetro configurável mencionado no resumo)

> Valor exato e local de configuração: **não identificados no resumo**.

## Itens fixos

- Endereço I2C do dispositivo (`0x23`)
- Tamanho máximo do frame TLV (`32` bytes)