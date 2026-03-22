# GSA — Contrato SDH/TLV

## Objetivo

Este documento formaliza o contrato vigente da GSA na documentação oficial do projeto.

Ele descreve:

- o papel do SDH no host;
- o subconjunto efetivamente implementado hoje para a GSA;
- o contrato TLV binário usado entre gateway e board;
- as regras funcionais observadas e assumidas pelo host;
- o conflito histórico do type `0x12` e sua resolução pela migração de `channel.status` para `0x1B`.

## Escopo e papel de cada camada

No estado atual do projeto:

- o `SDH` continua sendo o envelope semântico do host;
- o host converte `SDH -> SDGW compacto` antes do envio;
- a BPM/gateway roteia a transação para a GSA;
- a GSA consome e responde por TLV binário curto com `CRC8-ATM`.

O caminho operacional vigente para a GSA é:

    UI / FormsLogic
        -> GsaClient
        -> SdhClient
        -> SdhToSdgwMapper
        -> SdgwSession
        -> SDGW compacto
        -> BPM / gateway
        -> TLV GSA

## Histórico resumido

### Suporte já existente antes da expansão

Antes da expansão atual, o único comando GSA suportado de forma inequívoca no host era:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

Além disso, o host já suportava:

    sdh/1 BPM.gateway ping

### Expansão implementada agora no host

O host C# passou a suportar também:

    sdh/1 GSA.channel.setpoint set channel=<1..16> value=<0..255>
    sdh/1 GSA.channel.enable set channel=<1..16> state=on|off
    sdh/1 GSA.channels.enable set state=on|off
    sdh/1 GSA.channel.status get channel=<1..16>
    sdh/1 GSA.channels.status get
    sdh/1 GSA.channel.fault reset channel=<1..16>
    sdh/1 GSA.channel.offset set channel=<1..16> kind=vout|vread|iread value=<int16>
    sdh/1 GSA.channel.offset get channel=<1..16> kind=vout|vread|iread
    sdh/1 GSA.channel.offset save channel=<1..16>
    sdh/1 GSA.channel.offset reset channel=<1..16>
    sdh/1 GSA.offset reset

## Modelo funcional da GSA

### Canais

- existem `16` canais;
- canais `1..8` operam na faixa `0..5 V`;
- canais `9..16` operam na faixa `0..12 V`;
- o setpoint transportado no protocolo é sempre `0..255` em `1 byte`;
- a conversão de setpoint lógico para tensão real é responsabilidade da board.

### Status

O status por canal devolve:

- `setpoint`
- `vout`
- `iread`
- `enabled`
- `fault`

Regras:

- status deve responder mesmo com o canal em `OFF`;
- status deve retornar valores reais lidos;
- o host não deve zerar artificialmente leituras só porque o canal está desligado.

### Enable e fault

Regras funcionais:

- `setpoint set` é permitido com canal `OFF`;
- `setpoint set` é permitido com `fault latched`;
- `enable on` por canal deve falhar se houver `fault latched`;
- `channels.enable on` liga apenas canais sem fault;
- `channels.enable off` desliga todos;
- desligar globalmente não limpa fault;
- o fault é latched;
- existe `fault reset` por canal;
- existe evento assíncrono apenas para `fault`.

### Offsets

Kinds suportados:

- `vout`
- `vread`
- `iread`

Regras:

- offset é `int16` com sinal;
- serialização binária é little-endian;
- unidades:
  - `vout` = `mV`
  - `vread` = `mV`
  - `iread` = `mA`
- `offset set` altera RAM;
- `offset save` grava EEPROM;
- `offset reset` por canal restaura default e já grava EEPROM;
- `GSA.offset reset` restaura defaults globais e já grava EEPROM.

## Contrato SDH da GSA

### LED builtin

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

### Setpoint por canal

    sdh/1 GSA.channel.setpoint set channel=6 value=128

### Enable por canal

    sdh/1 GSA.channel.enable set channel=6 state=on
    sdh/1 GSA.channel.enable set channel=6 state=off

### Enable global

    sdh/1 GSA.channels.enable set state=on
    sdh/1 GSA.channels.enable set state=off

### Status por canal

    sdh/1 GSA.channel.status get channel=6

### Status global

    sdh/1 GSA.channels.status get

### Fault reset por canal

    sdh/1 GSA.channel.fault reset channel=6

### Offsets por canal

    sdh/1 GSA.channel.offset set channel=6 kind=vout value=-500
    sdh/1 GSA.channel.offset get channel=6 kind=vout
    sdh/1 GSA.channel.offset save channel=6
    sdh/1 GSA.channel.offset reset channel=6

### Reset global de offsets

    sdh/1 GSA.offset reset

## Contrato TLV binário da GSA

Todos os comandos GSA usam:

    Cmd = GwProtocol.MakeCompactCommand(GwProtocol.GsaAddress, GwProtocol.GsaTlvTransactOp)

Formato base:

    [type][len][data...][crc]

CRC:

- `CRC8-ATM` sobre `[type][len][data...]`

### Types

- `GwProtocol.GsaSetLedType` = LED builtin
- `0x10` = `GsaChannelSetpointType`
- `0x11` = `GsaChannelEnableType`
- `0x13` = `GsaChannelsStatusType`
- `0x14` = `GsaChannelsEnableType`
- `0x15` = `GsaChannelFaultResetType`
- `0x16` = `GsaChannelOffsetSetType`
- `0x17` = `GsaChannelOffsetGetType`
- `0x18` = `GsaChannelOffsetSaveType`
- `0x19` = `GsaChannelOffsetResetType`
- `0x1A` = `GsaOffsetResetType`
- `0x1B` = `GsaChannelStatusType`
- `0x30` = `GsaChannelFaultEventType`
- `0x7F` = `GsaErrorType`

### Offset kinds

- `0x01` = `vout`
- `0x02` = `vread`
- `0x03` = `iread`

### 1. Channel Setpoint Set

Request:

    [0x10][0x02][channel][value][crc]

Response:

    [0x10][0x02][channel][appliedValue][crc]

### 2. Channel Enable Set

Request:

    [0x11][0x02][channel][state][crc]

Response:

    [0x11][0x02][channel][appliedState][crc]

### 3. Channel Status Get

Request:

    [0x1B][0x01][channel][crc]

Response:

    [0x1B][0x06][channel][setpoint][vout][iread][enabled][fault][crc]

### 4. Channels Status Get

Request:

    [0x13][0x00][crc]

Response:

    [0x13][0x60][16 blocos de 6 bytes][crc]

Bloco:

    [channel][setpoint][vout][iread][enabled][fault]

### 5. Channels Enable Set

Request:

    [0x14][0x01][state][crc]

Response:

    [0x14][0x02][requestedState][affectedCount][crc]

### 6. Channel Fault Reset

Request:

    [0x15][0x01][channel][crc]

Response:

    [0x15][0x02][channel][faultState][crc]

### 7. Channel Offset Set

Request:

    [0x16][0x04][channel][kind][offset_lo][offset_hi][crc]

Response:

    [0x16][0x04][channel][kind][offset_lo][offset_hi][crc]

### 8. Channel Offset Get

Request:

    [0x17][0x02][channel][kind][crc]

Response:

    [0x17][0x04][channel][kind][offset_lo][offset_hi][crc]

### 9. Channel Offset Save

Request:

    [0x18][0x01][channel][crc]

Response:

    [0x18][0x01][channel][crc]

### 10. Channel Offset Reset

Request:

    [0x19][0x01][channel][crc]

Response:

    [0x19][0x01][channel][crc]

### 11. Global Offset Reset

Request:

    [0x1A][0x00][crc]

Response:

    [0x1A][0x01][affectedChannels][crc]

### 12. Functional Error

Response:

    [0x7F][0x03][requestType][channel][errorCode][crc]

Error codes:

- `0x01` = canal inválido
- `0x02` = valor inválido
- `0x03` = state inválido
- `0x04` = kind inválido
- `0x05` = fault latched
- `0x06` = EEPROM write failed
- `0x07` = comando não suportado
- `0x08` = payload inválido
- `0x09` = CRC TLV inválido
- `0x0A` = condição física ainda em fault
- `0x0B` = operação não permitida no estado atual

### 13. Fault Event

Payload:

    [0x30][0x06][channel][setpoint][vout][iread][enabled][fault][crc]

## Conflito histórico do type `0x12`

Houve um conflito histórico importante no contrato da GSA:

- o host já preservava `GwProtocol.GsaSetLedType = 0x12` para o LED builtin;
- uma fase intermediária da expansão da GSA também chegou a documentar `0x12` para `GsaChannelStatusType`.

Esse conflito foi resolvido no contrato oficial atual:

- `0x12` permanece dedicado ao LED builtin legado;
- `0x1B` passa a ser o type oficial de `GSA.channel.status`.

### Consequência para host e firmware

Com a migração de `channel.status` para `0x1B`:

- o host não precisa mais resolver `channel.status` por `len`;
- o parser do LED builtin continua preservado sem regressão;
- o firmware da GSA passa a operar com contrato TLV sem ambiguidade para status por canal.

### Regra documental vigente

A documentação oficial deve tratar como contrato atual:

- `0x12` = LED builtin legado
- `0x1B` = `GSA.channel.status`

Referências antigas que associem `channel.status` ao `0x12` devem ser lidas apenas como histórico superado.

## Papel do host e da board

### Host

Responsabilidades do host:

- validar comandos SDH;
- serializar comandos semânticos para SDGW compacto;
- mapear payload TLV binário da GSA;
- correlacionar resposta funcional;
- interpretar erro funcional;
- publicar evento assíncrono de fault para camadas acima.

### Board

Responsabilidades da GSA:

- aplicar setpoint no domínio físico da board;
- converter `0..255` para a faixa elétrica real do canal;
- manter fault latched;
- devolver status real mesmo com canal `OFF`;
- armazenar offsets em RAM/EEPROM conforme a operação.

## Referências

- `docs/04-firmware/boards/03-gsa.md`
- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/12-documentacao-tecnica/03-contratos-software.md`

[Retornar ao README principal](../README.md)
