# GwDeviceTable --- Tabela de Dispositivos

## Objetivo

Definir como o Gateway mapeia **endereços lógicos** para dispositivos
físicos.

Formato:

ADDR lógico → BUS → endereço físico

Exemplo:

  ADDR   Board   BUS   Endereço
  ------ ------- ----- ----------
  0x1    GSA     I2C   0x23

## Responsabilidade

-   mapear destino lógico
-   selecionar barramento físico
-   permitir expansão futura de módulos
