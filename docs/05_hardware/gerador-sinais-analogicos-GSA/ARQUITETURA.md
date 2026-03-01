# GSA — Arquitetura do Firmware

## Objetivo

Firmware do módulo GSA, operando como dispositivo I2C (slave), recebendo comandos TLV e executando ações (ex.: controle de LED), com validação por CRC-8.

## Organização interna (alto nível)

Entrypoint:
- `src/main.cpp` (contém `setup()` e `loop()`)

Módulos identificados:
- `Transport`: gerencia comunicação I2C
- `LedService`: controla o LED
- `Service`: processa comandos TLV relacionados ao LED
- `Link` / `LinkService`: gerencia estado de conexão e valida frames TLV
- `Tlv` (`TlvFrame`, `TlvBuilder`): estrutura e construção de mensagens TLV
- `Crc8`: cálculo de CRC-8 para validação das mensagens

## Fluxo de execução

### setup()
- Inicializa LED
- Inicializa transporte I2C
- Inicializa/ativa componentes de link/estado

### loop()
- Recebe frames TLV via I2C
- Valida frame (tamanho / CRC)
- Processa comando TLV
- Atualiza estado de link e LED

## Interfaces externas

### I2C
- Biblioteca: `Wire.h`
- Modo: slave
- Endereço: `0x23` (`I2C_GSA_ADDR`)
- Eventos:
  - `Wire.onReceive`
  - `Wire.onRequest`

## Estados e erros (Link)

Estados (identificados):
- `WAIT_PING`
- `LINKED`
- `FAULT`

Erros (identificados):
- `LINK_ERR_NONE`
- `LINK_ERR_BAD_LEN`
- `LINK_ERR_BAD_CRC`

> Demais estados, critérios exatos de transição e timeouts completos: **não identificados no resumo**.