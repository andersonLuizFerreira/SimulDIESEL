# Introdução

## Estado atual

O SimulDIESEL é um ambiente de bancada para comunicação com módulos e periféricos eletrônicos por meio de um gateway embarcado e de um software local em C#. O que está efetivamente implementado no repositório hoje é um caminho operacional entre o host e um gateway ESP32, com roteamento para dispositivos mapeados em tabela e um periférico I2C concreto, o firmware `gerador-sinais-analogicos-GSA`, que expõe comandos de serviço simples.

A composição observada no código é a seguinte:

- a aplicação local usa `SerialTransport`, `SerialLinkService`, `SdGwLinkEngine` e `SdGgwClient` para abrir a serial, executar handshake textual, encapsular frames binários e monitorar a saúde do enlace;
- o gateway `esp32-api-bridge` implementa parser, link confiável com `ACK` e roteamento por endereço lógico;
- o periférico `GSA` processa TLVs curtos sobre I2C e delega o comportamento a um serviço local.

```
PC/WinForms -> Serial ASCII/Binário -> ESP32 Gateway -> I2C/SPI -> Dispositivo endereçado
```

## Funcionamento técnico

O fluxo inicia no host com a abertura da serial e a execução de um handshake de link. A classe `SerialLinkService` administra a máquina de estados `Disconnected -> SerialConnected -> Draining -> BannerSent -> Linked/LinkFailed`, drenando ruído inicial, enviando o banner `SIMULDIESELAPI` e aguardando a linha de identificação do firmware.

Após o estado `Linked`, a comunicação passa a usar frames binários delimitados por `0x00`, codificados em `COBS` e protegidos por `CRC8`. No gateway, `SggwLink` recebe o quadro, valida o enquadramento e entrega o conteúdo lógico para `GatewayApp`. Se o endereço do comando aponta para o próprio gateway, a resposta é tratada localmente; caso contrário, `GwRouter` consulta `GwDeviceTable` e escolhe `GwI2cBus` ou `GwSpiBus`.

O GSA recebe a transação como TLV curto em I2C slave, valida comprimento e `CRC`, resolve comandos em `Service` e devolve a resposta para o gateway, que a reencapsula no protocolo host.

## Limitações

O repositório ainda não apresenta uma cadeia completa de funcionalidades compatível com o nome amplo do projeto. Há evidência forte de infraestrutura de comunicação, mas poucos serviços de domínio concluídos. O firmware do GSA, por exemplo, implementa hoje apenas leitura e escrita de estado de LED, apesar do nome sugerir geração analógica mais rica. Também não há endpoints operacionais em `cloud/api-contracts/openapi.yaml`.

## Evolução prevista

O material legado de roadmap indica progressão por fases: primeiro o enlace serial confiável, depois o modelo de comandos do gateway e, por fim, a expansão de dispositivos, serviços e contratos. O estado atual do código confirma que as fundações de transporte e roteamento estão consolidadas; a próxima evolução natural é ampliar os serviços embarcados e documentar os dispositivos reais suportados pela tabela do gateway.

[Retornar ao README principal](../README.md)
