# GSA — Contrato SDH/TLV

## Objetivo

Este documento formaliza o contrato vigente da GSA entre host, BPM e firmware da board.

## Arquitetura oficial

O fluxo atual da GSA é dividido em duas etapas distintas:

1. etapa lógica e síncrona:
   - host -> BPM -> GSA
   - TLV curto no barramento físico I2C
   - resposta imediata apenas confirma recepção e aceite do comando
2. etapa física e assíncrona:
   - GSA atua no barramento I2C lógico interno
   - acessa `TCA9548A` + `MCP4725`
   - sinaliza conclusão via IRQ
   - BPM busca e encaminha o evento `0x31`

## Barramentos

### Barramento físico

- BPM = `master`
- GSA = `slave`
- endereço da GSA = `0x23`
- BPM ESP32 `SDA=D21`, `SCL=D22`
- GSA Nano `SDA=A4`, `SCL=A5`

### Barramento lógico da GSA

- GSA = `master`
- `TCA9548A = 0x70`
- `MCP4725 = 0x60 / 0x61`
- GSA Nano `SDA=D2`, `SCL=D3`
- reset dedicado do `TCA9548A` em `D8`

### IRQ e reset

- IRQ GSA -> BPM: GSA Nano `D4` -> BPM ESP32 `D19`
- IRQ ativo em `LOW`, open-drain por software e pull-up externo `3,3 V`
- reset da GSA controlado pela BPM em `D23`

O firmware oficial não usa mais troca de papel `slave/master` no mesmo barramento com a BPM.

## Modelo funcional

### Canais

- `16` canais
- canais `1..8` em `0..5 V`
- canais `9..16` em `0..12 V`
- setpoint lógico em `0..255`
- shadow RAM por canal

### Status por canal

Payload:

```text
[channel][setpoint_raw][vout_raw][iread_raw][enabled][fault]
```

Semântica:

- `vout_raw` = bruto `0..255`
- `iread_raw` = bruto `0..255`
- `enabled` = saída efetivamente ativa
- `fault` = booleano latched

### Política de falha física

Se o `TCA9548A` ou o `MCP4725` não responderem:

- a resposta síncrona do comando original não passa a carregar esse erro;
- o shadow do canal é preservado;
- o enable é preservado;
- a saída não é zerada por política automática;
- não há retry automático;
- o resultado é comunicado pelo evento `0x31`.

## Mapeamento elétrico obrigatório

- canal `1`  -> `SC0` + `0x61`
- canal `2`  -> `SC0` + `0x60`
- canal `3`  -> `SC1` + `0x61`
- canal `4`  -> `SC1` + `0x60`
- canal `5`  -> `SC2` + `0x61`
- canal `6`  -> `SC2` + `0x60`
- canal `7`  -> `SC3` + `0x61`
- canal `8`  -> `SC3` + `0x60`
- canal `9`  -> `SC4` + `0x61`
- canal `10` -> `SC4` + `0x60`
- canal `11` -> `SC5` + `0x61`
- canal `12` -> `SC5` + `0x60`
- canal `13` -> `SC6` + `0x61`
- canal `14` -> `SC6` + `0x60`
- canal `15` -> `SC7` + `0x61`
- canal `16` -> `SC7` + `0x60`

## Contrato SDH vigente

### LED builtin

```text
sdh/1 GSA.led set state=on
sdh/1 GSA.led set state=off
```

### Setpoint por canal

```text
sdh/1 GSA.channel.setpoint set channel=6 value=128
```

### Enable por canal

```text
sdh/1 GSA.channel.enable set channel=6 state=on
sdh/1 GSA.channel.enable set channel=6 state=off
```

### Enable global

```text
sdh/1 GSA.channels.enable set state=on
sdh/1 GSA.channels.enable set state=off
```

### Status por canal

```text
sdh/1 GSA.channel.status get channel=6
```

### Fault reset

```text
sdh/1 GSA.channel.fault reset channel=6
```

### Offsets

```text
sdh/1 GSA.channel.offset set channel=6 kind=vout value=-500
sdh/1 GSA.channel.offset save channel=6
sdh/1 GSA.offset reset
```

## Contrato TLV binário

Formato base:

```text
[type][len][data...][crc]
```

CRC:

- `CRC8-ATM` sobre `[type][len][data...]`

### Types vigentes

- `0x10` = setpoint de canal
- `0x11` = enable de canal
- `0x12` = LED builtin legado
- `0x14` = enable global
- `0x15` = fault reset de canal
- `0x16` = offset set
- `0x18` = offset save EEPROM
- `0x1A` = offset reset global
- `0x1B` = status de canal
- `0x30` = evento assíncrono de fault
- `0x31` = evento assíncrono de resultado físico
- `0x7F` = erro funcional

### 1. Channel Setpoint Set

Request:

```text
[0x10][0x02][channel][value][crc]
```

Response síncrona:

```text
[0x10][0x02][channel][acceptedValue][crc]
```

Semântica:

- a resposta indica apenas que o comando foi aceito para processamento;
- o resultado físico final vem depois no evento `0x31`.

### 2. Channel Enable Set

Request:

```text
[0x11][0x02][channel][state][crc]
```

Response síncrona:

```text
[0x11][0x02][channel][appliedState][crc]
```

### 3. LED Builtin

Request:

```text
[0x12][0x01][state][crc]
```

Response:

```text
[0x12][0x01][state_aplicado][crc]
```

### 4. Global Enable

Request:

```text
[0x14][0x01][state][crc]
```

Response síncrona:

```text
[0x14][0x01][state_aplicado][crc]
```

Para `enable global`, a GSA emite um evento `0x31` por canal efetivamente processado.

### 5. Channel Fault Reset

Request:

```text
[0x15][0x01][channel][crc]
```

Response:

```text
[0x15][0x02][channel][faultState][crc]
```

### 6. Channel Offset Set

Request:

```text
[0x16][0x04][channel][kind][offset_lo][offset_hi][crc]
```

Response:

```text
[0x16][0x04][channel][kind][offset_lo][offset_hi][crc]
```

### 7. Channel Offset Save

Request:

```text
[0x18][0x01][channel][crc]
```

Response:

```text
[0x18][0x02][channel][saveResult][crc]
```

### 8. Global Offset Reset

Request:

```text
[0x1A][0x00][crc]
```

Response:

```text
[0x1A][0x01][resetResult][crc]
```

### 9. Channel Status

Request:

```text
[0x1B][0x01][channel][crc]
```

Response:

```text
[0x1B][0x06][channel][setpoint][vout][iread][enabled][fault][crc]
```

### 10. Evento de fault

Payload:

```text
[0x30][0x06][channel][setpoint][vout][iread][enabled][fault][crc]
```

### 11. Evento de resultado físico

Payload:

```text
[0x31][0x03][origin_type][channel][status][crc]
```

Status:

- `0x01` = operação OK
- `0x02` = falha. `TCA9548A` não respondeu
- `0x03` = falha. `MCP4725` não respondeu

Regra:

- o `0x31` é emitido sempre, inclusive em sucesso.

### 12. Erro funcional

Response:

```text
[0x7F][0x03][requestType][channel][errorCode][crc]
```

Error codes:

- `0x01` = canal inválido
- `0x02` = valor inválido
- `0x03` = state inválido
- `0x04` = kind inválido
- `0x05` = fault latched
- `0x06` = falha EEPROM
- `0x07` = comando não suportado
- `0x08` = payload inválido
- `0x09` = CRC TLV inválido
- `0x0A` = condição física ainda em fault
- `0x0B` = operação não permitida no estado atual

## Papel de cada camada

### Host

- valida o comando SDH;
- serializa para SDGW compacto;
- consome a resposta síncrona;
- consome os eventos `0x30` e `0x31`.

### BPM

- roteia o TLV para a GSA;
- recebe IRQ;
- busca o evento assíncrono;
- reencaminha o evento no fluxo reverso SDGW.

### GSA

- valida o TLV recebido;
- mantém shadow RAM, offsets e EEPROM;
- executa a operação física no barramento interno;
- emite `0x31` com o resultado elétrico.

## Observação histórica

O modelo BUSY/IDLE com troca de papel `slave/master` no mesmo barramento I2C foi abandonado e não deve mais ser tratado como arquitetura oficial da GSA.

[Retornar ao README principal](../README.md)
