# CAN TX Loopback Validation

- Seed usada: `0x5D10E10`
- Total de IDs: `200`
- Frames por rodada: `5000`
- CAN: `EXT`, `DLC=8`, `RTR=false`
- Loopback: modelo SDCTP TX/RX sem dependencia de `CanDriver_fake`
- Rodada TX_DIRECT RX_MODE: `DIRECT_ONLY`
- Rodada TX_TABLE RX_MODE: `AUTO`

## Rodada TX_DIRECT
- Frames enviados: `5000`
- Frames recebidos: `5000`
- Matches: `5000`
- Mismatches: `0`
- Lost: `0`
- Extra: `0`

## Rodada TX_TABLE
- Linhas TX criadas: `50`
- Frames esperados: `5000`
- Frames recebidos: `5000`
- Matches: `5000`
- Mismatches: `0`
- Lost: `0`
- Extra: `0`
- Comparacao: `ordem deterministica por INDEX crescente no tick da UCE`

## Estatisticas
- TX_DIRECT enviados: `5000`
- TX_TABLE enviados: `5000`
- CAN_TX_CREATE: `50`
- CAN_TX_EDIT: `376`
- CAN_TX_DELETE: `50`
- CAN_RX_EVENT 0x28 retornados: `5000`
- CAN_CREATE RX: `50`
- CAN_EDIT RX: `376`
- CAN_TIC RX: `4574`
- CAN_DELETE RX: `50`
- FIFO overflow: `0`
- OutputBuffer overflow: `0`

## Massa de Dados
- IDs ciclicos fixos: `10`
- IDs ciclicos variaveis: `40`
- IDs unicos/esporadicos: `150`
- TX_DIRECT: massa deterministica de 200 IDs misturados, igual ao perfil RX validado.
- TX_TABLE: 50 linhas ciclicas deterministicas, 10 fixas e 40 variaveis; 100 ticks x 50 linhas.

TX_DIRECT LOOPBACK VALIDADO: 5000/5000
TX_TABLE LOOPBACK VALIDADO: 5000/5000
