# I2C --- TLV + CRC

## Estrutura

\[T\]\[L\]\[V...\]\[CRC\]

Onde:

T = tipo/operação\
L = tamanho do payload\
V = dados\
CRC = CRC‑8/ATM

## Fluxo

Gateway → Device

1.  montar TLV
2.  calcular CRC
3.  enviar via I2C

Device → Gateway

1.  receber TLV
2.  validar CRC
3.  executar comando
