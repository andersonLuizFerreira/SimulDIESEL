# GwRouter – Roteamento (Gateway)
**Status:** Rascunho inicial (alinhado ao CONTRATO_CENTRAL v1.0)

## Objetivo
Documentar o roteamento do Gateway a partir do `cmd` SGGW:

- extrair ADDR/OP
- consultar DeviceTable
- selecionar BUS (I2C/SPI)
- enviar TLV+CRC para a baby board

> Este arquivo descreve *o conceito*. A implementação deve seguir `GatewayApp::onCommand()` como ponto único de decisão.
