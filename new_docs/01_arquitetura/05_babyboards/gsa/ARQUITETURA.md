# Arquitetura do Firmware GSA

Organização interna recomendada:

Transport → Link → Service

## Transport

Interface com barramento I2C.

## Link

Validação do frame TLV + CRC.

## Service

Interpretação das operações.
