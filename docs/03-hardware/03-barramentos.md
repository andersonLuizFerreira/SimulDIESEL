# Barramentos

## Estado atual

O repositório implementa três camadas de transporte claramente distintas:

- serial entre host e gateway;
- `I2C` entre gateway e periféricos como o GSA;
- `SPI` entre gateway e dispositivos mapeados na tabela.

Esses barramentos não são equivalentes. A serial transporta o protocolo completo de sessão com `COBS`, `CRC8`, `ACK` e sequência. `I2C` e `SPI` transportam requisições curtas já resolvidas pelo gateway para os dispositivos.

## Funcionamento técnico

### Serial host/gateway

Responsabilidades:

- transporte bruto em `SerialTransport`;
- sincronização textual em `SerialLinkService`;
- confiabilidade e enquadramento em `SdGwLinkEngine` e `SggwLink`.

Características observadas:

- delimitador `0x00`;
- codificação `COBS`;
- `CRC8 ATM`;
- sequência para deduplicação;
- `ACK` e resposta de erro específicos do protocolo.

### I2C gateway/dispositivo

No GSA, o dispositivo é escravo `I2C` em `0x23`. O gateway monta uma carga TLV curta, escreve no periférico e depois solicita a resposta preparada pelo callback de leitura. O contrato do periférico é do tipo:

```text
T | L | V... | CRC8
```

Esse desenho privilegia simplicidade de firmware e permite que o gateway concentre a inteligência de sessão.

### SPI gateway/dispositivo

`GwSpiBus` indica suporte a dispositivos selecionados por `chip select`, inclusive com metadados adicionais como pino de interrupção em entradas da tabela. O padrão do código sugere o mesmo ciclo request/resposta usado em `I2C`, mas com a mecânica física do `SPI`.

## Limitações

O repositório não apresenta, na parte oficial de documentação, uma tabela elétrica consolidada com clock, resistores de pull-up, níveis lógicos ou restrições temporais dos barramentos. Também não há implementação operacional de `CAN` ou `J1939` no código analisado, embora esses protocolos apareçam como temas documentais e de evolução futura.

## Evolução prevista

Os barramentos existentes já permitem expansão modular. Os próximos passos coerentes com a arquitetura atual são:

- ampliar a tabela de dispositivos por barramento;
- padronizar documentação de payloads internos por dispositivo;
- consolidar critérios de timeout e tratamento de erro por tipo de barramento;
- documentar, quando implementados, os transportes automotivos ainda ausentes no código atual.

[Retornar ao README principal](../README.md)
