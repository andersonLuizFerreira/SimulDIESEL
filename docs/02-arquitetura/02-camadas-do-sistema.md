# Camadas do Sistema

## Camada de Apresentação (UI)

**Arquivos principais:** `DashBoard`, `frmPortaSerial_UI`, `frmLedGw`.

Responsabilidades observadas:
- exibir estado de serial/link;
- acionar conexão/desconexão;
- iniciar testes de comando simples (ex.: LED no GSA).

A UI responde por eventos, sem manipular diretamente framing/protocolo.

## Camada de Aplicação/BLL

**Arquivos principais:**
- `SerialLinkService`
- `SdGwLinkEngine`
- `SdGwHealthService`
- `SdGgwClient`
- `LedGwTest_BLL`

Responsabilidades:
- manter estado do link;
- orquestrar handshake de link;
- parser/encode de frames SGGW;
- reintentos e timeout de mensagens críticas;
- manter watchdog de conexão.

## Camada de Acesso a Dados (DAL)

**Arquivo:** `SerialTransport`.

Responsável por I/O serial bruto: listar portas, abrir/fechar, escrever bytes e repassar bytes recebidos.

## Camada de Gateway embarcada

**Arquivos:** `SggwTransport`, `SggwLink`, `GatewayApp`, `GwRouter`, `GwI2cBus`, `GwSpiBus`.

Responsabilidades:
- handshake de validação textual com ESP32;
- parsing e roteamento de comandos lógicos (`ADDR/OP`);
- tradução para barramentos internos (TLV+CRC para módulos).

## Camada de módulos

No estado atual, a implementação de módulo ativa no repositório é `gerador-sinais-analógicos` com comunicação TLV no barramento I2C.

## Dependências entre camadas

A integridade da arquitetura depende da ordem de inicialização:
1. UI disponível;
2. DAL aberta;
3. handshake;
4. ativação do protocolo binário.

Qualquer variação fora dessa ordem aumenta risco de frames inválidos no primeiro minuto de conexão.

[Retornar ao README principal](../README.md)
