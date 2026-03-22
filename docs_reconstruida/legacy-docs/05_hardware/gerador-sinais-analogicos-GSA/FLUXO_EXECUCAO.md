⚠️ Documento histórico. Pode não refletir a arquitetura atual do SimulDIESEL.

# GSA — Fluxo de Execução

## Sequência geral

### setup
- inicializa LED
- inicializa transporte I2C
- inicializa componentes de link

### loop
- recebe frame TLV
- valida comprimento e CRC
- processa comando
- atualiza estado interno

## Observação

Detalhes exatos dependem da versão do firmware do GSA auditada.

[Retornar ao README principal](..\..\..\README.md)

