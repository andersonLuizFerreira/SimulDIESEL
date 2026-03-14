# Drivers de Firmware

## Objetivo dos drivers

Este documento descreve os drivers concretos usados hoje para I/O físico e controle de barramento.

## Drivers no gateway (ESP32)

### Serial
- **Arquivo:** `SggwTransport`
- **Implementação:** `HardwareSerial` (serial USB do ESP32)
- **Funções reais:** `begin()`, `available()`, `readByte()`, `writeBytes()`, `flushTx()`
- **Configuração:** `SGGW_UART_BAUDRATE` definido em `Sggw.defs.h` (`115200`).

### I2C
- **Arquivo:** `GwI2cBus`
- **Driver:** `TwoWire` (`Wire` por padrão)
- **Uso:** transação síncrona: escrita do frame TLV, leitura em uma única janela e corte por campo `L`.
- **Observação:** o próprio firmware de teste do link de handshake pode emitir texto no boot; por isso a camada de link trata janelas de sincronização.

### SPI
- **Arquivo:** `GwSpiBus`
- **Driver:** `SPI`
- **Uso:** `beginTransaction` com CS por dispositivo, escrita + aguardar IRQ opcional + leitura do retorno.

## Drivers no GSA

### I2C (modo escravo)
- **Arquivo:** `Transport` do GSA
- **Eventos:** `onReceiveThunk` e `onRequestThunk`.
- `Wire.begin(i2cAddr)` inicia endereço fixo (macro `I2C_GSA_ADDR`).

### GPIO
- **Arquivo:** `LedService`
- **Uso:** controle de pino de LED (`LED_PIN`) com nível digital.

## Drivers de software (camada de aplicação)

- `SerialPort` em `local-api` para transporte físico.
- Timers de handshake e heartbeat na camada BLL.

## Lacunas e controle de dependência

- Não há camada de abstração dedicada para CAN/LIN neste repositório atual.
- `HAL` de rede/cloud está reservado, sem implementação ativa.

[Retornar ao README principal](../README.md)
