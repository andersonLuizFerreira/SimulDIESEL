# I2C Bus – TLV+CRC (Gateway)
**Status:** Rascunho inicial (alinhado ao CONTRATO_CENTRAL v1.0)

## Regra do barramento
No barramento I2C, o Gateway sempre envia/recebe:

`[T=OP][L][V...][CRC]`

CRC-8/ATM em `[T L V]`.

## Leitura recomendada
Para evitar problemas de resposta fragmentada em múltiplos `requestFrom()`, recomenda-se:
- ler a resposta em uma única chamada `requestFrom(rxMax)`
- recortar pelo campo L (2+L+1)
