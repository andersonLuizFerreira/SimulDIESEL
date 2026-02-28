# GSA – Estrutura interna (Firmware)
**Status:** Rascunho inicial

## Pastas/Componentes
- Transport: I2C slave, buffer RX/TX
- Link: valida TLV+CRC, mantém buffer de erro
- Service: dispatcher (SET/GET/CONFIG) + subcomandos (LED/ERR/PORT/CONFIG...)
