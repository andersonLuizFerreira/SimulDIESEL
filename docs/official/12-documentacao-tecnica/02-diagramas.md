⬅ [Retornar para Especificações](01-especificacoes.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Diagramas

## Estado atual

Os diagramas abaixo sintetizam a arquitetura comprovada pelo código. Eles não substituem esquemas elétricos nem diagramas mecânicos do hardware, mas representam com fidelidade o fluxo lógico hoje implementado.

## Funcionamento técnico

### Diagrama de camadas

```text
+------------------------------+
| Software local (WinForms)    |
| UI / BLL / DAL / DTL         |
+--------------+---------------+
               |
               v
+------------------------------+
| Link host/gateway            |
| Serial + Banner + SDGW       |
| COBS + CRC8 + ACK + SEQ      |
+--------------+---------------+
               |
               v
+------------------------------+
| Gateway ESP32                |
| GatewayApp / GwRouter        |
| GwDeviceTable / Buses        |
+---------+--------------+-----+
          |              |
          v              v
     +---------+    +---------+
     | I2C Bus |    | SPI Bus |
     +----+----+    +----+----+
          |              |
          v              v
     +---------+    +---------+
     |  GSA    |    | outros  |
     | TLV     |    | módulos |
     +---------+    +---------+
```

### Diagrama de sequência do enlace

```text
Host                BPM ESP32                   GSA Nano
 |                     |                           |
 |-- banner ---------->|                           |
 |<-- "SimulDIESEL..."-|                           |
 |-- frame SDGW ------>|                           |
 |                     |-- TLV por D21/D22 ----->|
 |                     |<-- TLV síncrono ---------|
 |<-- resposta SDGW ---|                           |
 |                     |<-- IRQ em D19 -----------|
 |                     |-- fetch de evento ------>|
 |                     |<-- TLV 0x31 -------------|
 |<-- evento SDGW -----|                           |
```

### Diagrama de estados do host

```text
[Disconnected]
      |
      v
[SerialConnected]
      |
      v
[Draining]
      |
      v
[BannerSent] ---> [LinkFailed]
      |
      v
[Linked]
```

### Diagrama de serviço do GSA

```text
I2C IRQ/Callback
  -> Transport
  -> Link
  -> Service
  -> LedService
  -> resposta TLV
```

## Limitações

Os diagramas aqui descritos representam o que o código confirma hoje. A pinagem crítica BPM <-> GSA já está consolidada na documentação oficial, mas estes diagramas ainda não substituem esquemas elétricos completos, topologia mecânica final do backplane nem uma matriz completa de todos os dispositivos possíveis.

## Evolução prevista

Conforme o projeto crescer, esta seção deve incorporar:

- diagramas por dispositivo da tabela do gateway;
- sequência de erro e recuperação do enlace;
- diagramas de energia e interconexão física do backplane;
- fluxos de protocolos adicionais quando houver implementação real.

## Glossário

- **Especificação**: descrição formal de comportamento, limites ou contratos técnicos.
- **Diagrama**: representação visual simplificada da arquitetura ou do fluxo.
- **Contrato**: acordo técnico entre camadas, serviços ou dispositivos.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.
- **TLV**: Type-Length-Value, formato interno de payload usado em transações específicas.

## Próximas camadas

- [Contratos de Software](03-contratos-software.md)
