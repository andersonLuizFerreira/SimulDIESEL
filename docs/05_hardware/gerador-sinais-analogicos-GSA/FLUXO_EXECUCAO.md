# GSA — Fluxo de Execução

## Sequência geral

### 1) Inicialização (setup)
- Inicializa o LED (`LedService`)
- Inicializa o transporte I2C (`Transport`)
- Inicializa o link/estado (`Link` / `LinkService`)

### 2) Loop principal (loop)
- Recepção de dados via I2C
  - `Wire.onReceive` para receber bytes
  - Frame TLV é montado/interpretado (via `TlvFrame`)
- Validação de integridade
  - Valida comprimento (erro: `LINK_ERR_BAD_LEN`)
  - Valida CRC-8 (erro: `LINK_ERR_BAD_CRC`)
- Processamento de comandos
  - Comandos TLV relacionados ao LED são tratados por `Service` / `LedService`
- Atualização de estados
  - Estados: `WAIT_PING`, `LINKED`, `FAULT`
- Temporizações
  - Uso de `millis()` para controle de LED e watchdog (mencionado no resumo)

## Erros e diagnóstico

Erros identificados:
- `LINK_ERR_NONE`
- `LINK_ERR_BAD_LEN`
- `LINK_ERR_BAD_CRC`

> Mensagens de log, telemetria e forma exata de “GET_ERR”: **não identificados no resumo**.