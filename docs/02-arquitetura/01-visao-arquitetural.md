# Visão Arquitetural

## Estado atual

A arquitetura do SimulDIESEL é multicamada e orientada a gateways. O host não fala diretamente com cada periférico; ele fala com um gateway ESP32 que centraliza enquadramento, confiabilidade de link e roteamento para barramentos internos. Esse desenho já está visível tanto no software local em C# quanto no firmware do `esp32-api-bridge`.

```
+--------------------+       +----------------------+       +------------------+
| UI / BLL / DAL     |       | ESP32 API Bridge     |       | Dispositivos     |
| local-api (C#)     |<----->| Link / Router / Buses|<----->| I2C / SPI        |
+--------------------+       +----------------------+       +------------------+
```

## Funcionamento técnico

### Camada host

- `SerialTransport`: leitura e escrita de bytes crus.
- `SerialLinkService`: sincronização inicial, controle de estado e descarte de ruído textual.
- `SdGwLinkEngine`: montagem e análise de frames, `ACK`, retransmissão e deduplicação por sequência.
- `SdGgwClient`: API de envio para a camada superior.
- `SdGwHealthService`: supervisão periódica com `ping`.

### Camada gateway

- `SggwTransport`: abstração do meio serial no ESP32.
- `SggwParser`: validação de `COBS`, tamanho e `CRC8`.
- `SggwLink`: sessão host/gateway, tratamento de `ACK`, respostas em cache e watchdog.
- `GatewayApp`: tratamento de comandos do endereço do gateway.
- `GwRouter`: resolução do destino lógico.
- `GwDeviceTable`: catálogo local de dispositivos e barramentos.
- `GwI2cBus` e `GwSpiBus`: execução da transação física.

### Camada de dispositivo

No `GSA`, a organização é explícita:

- `Transport`: integra callbacks do barramento.
- `Link`: valida frame TLV e administra erros de protocolo.
- `Service`: interpreta comando funcional.
- `LedService`: atua sobre o pino físico.

### Fluxo arquitetural real

1. A UI solicita uma operação.
2. O cliente C# monta um frame lógico.
3. O gateway valida e resolve o endereço.
4. O barramento selecionado entrega o comando ao dispositivo.
5. O dispositivo responde em TLV.
6. O gateway reconstrói a resposta do protocolo host.
7. O host confirma `ACK` e atualiza a interface.

## Limitações

A arquitetura está mais madura nos mecanismos de transporte do que nos serviços de domínio. O mapeamento de dispositivos existe, mas a tabela ainda é curta e parte dos endereços aponta para dispositivos cuja documentação textual não está consolidada no repositório. Também não há, no estado atual, uma camada de integração em nuvem equivalente ao nível de detalhe do enlace local.

## Evolução prevista

As decisões já presentes no código favorecem expansão sem ruptura:

- inclusão de novos dispositivos por tabela, sem alterar o protocolo host;
- reaproveitamento da pilha `Transport/Link/Service` em novos periféricos;
- enriquecimento do dashboard com mais casos de uso sem reescrever o enlace;
- evolução dos contratos formais quando os serviços embarcados deixarem de ser apenas infraestrutura básica.

[Retornar ao README principal](../README.md)
