# Arquitetura de Firmware do SimulDIESEL

## Visão geral

O repositório possui dois firmwares principais:

- Gateway de integração: `hardware/firmware/esp32-api-bridge`
- Módulo de borda (GSA): `hardware/firmware/gerador-sinais-analogicos-GSA`

Os dois firmwares compartilham uma ideia comum de arquitetura em camadas, com separação entre transporte, protocolo/encaminhamento e serviços de aplicação.

## Camada de transporte

No ESP32 Gateway:

- `SggwTransport` encapsula `HardwareSerial` (`115200`, `SERIAL_8N1`).
- `GwI2cBus` usa `Wire` para barramento I2C.
- `GwSpiBus` usa `SPI` para barramentos SPI.

No GSA:

- `Transport` atua como escravo I2C via `TwoWire`, recebendo/entregando buffers TLV.

## Camada de protocolo e parsing

No Gateway:

- `SggwParser` decodifica frames COBS e valida CRC8.
- `SggwLink` gerencia estado de link, handshake por banner, ACK/ERR, retransmissão de resposta e emissão de eventos.

No GSA:

- `Link` valida `[T][L][V...][CRC]`, mantém `errCode` interno e encaminha para `Service`.
- `Tlv` encapsula a estrutura dos TLVs (`t`, `l`, `v`).

## Camada de roteamento e serviços

- `GwRouter` é responsável por `CMD = [ADDR:4][OP:4]`, consulta `GwDeviceTable` e despacha para I2C/SPI.
- `GatewayApp` é o ponto único de decisão do gateway para cada comando (`onCommand`).
- `Service` (GSA) implementa operação de `SET/GET` de LED e coordena erros (`GET_ERR`, `CLR_ERR`) via `LedService`.

## Fluxo resumido de execução

1. ESP32 recebe frame SGGW.
2. `SggwLink` valida e entrega para `GatewayApp`.
3. Para `addr != 0x0`, o gateway consulta `GwDeviceTable` e roteia para o barramento.
4. `GwI2cBus`/`GwSpiBus` fazem transação com a baby board.
5. Resposta TLV retorna ao gateway.
6. `GatewayApp` envia evento SGGW para a API com o TLV da resposta.

No GSA:

1. `Transport` recebe frame TLV completo.
2. `Link` valida estrutura e CRC.
3. `Service` executa ação de domínio (`LedService`) e monta resposta.
4. `Transport` publica resposta para o próximo `requestFrom()`.

## Observações de integridade

- Existe separação explícita de responsabilidades por camada.
- O fluxo atual é funcional para o caminho Gateway ↔ GSA e já inclui watchdog e handshake no caminho ESP32.
- A expansão para novos módulos passa pela expansão do `GwDeviceTable` e dos serviços no nível de app.

## Fontes internas usadas

- `hardware/firmware/esp32-api-bridge/src/main.cpp`
- `hardware/firmware/esp32-api-bridge/lib/*`
- `hardware/firmware/gerador-sinais-analogicos-GSA/src/main.cpp`
- `hardware/firmware/gerador-sinais-analogicos-GSA/lib/*`

[Retornar ao README principal](../README.md)
