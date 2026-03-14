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
| Serial + Banner + SGGW       |
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
Host                Gateway                GSA
 |                     |                    |
 |-- banner ---------->|                    |
 |<-- "SimulDIESEL..."-|                    |
 |-- frame SGGW ------>|                    |
 |                     |-- TLV ----------->|
 |                     |<-- TLV -----------|
 |<-- resposta SGGW ---|                    |
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

Os diagramas aqui descritos representam o que o código confirma hoje. Eles não trazem pinagem detalhada, topologia física final do backplane nem uma matriz completa de todos os dispositivos possíveis, porque essa informação não está consolidada textualmente no repositório atual.

## Evolução prevista

Conforme o projeto crescer, esta seção deve incorporar:

- diagramas por dispositivo da tabela do gateway;
- sequência de erro e recuperação do enlace;
- diagramas de energia e interconexão física do backplane;
- fluxos de protocolos adicionais quando houver implementação real.

[Retornar ao README principal](../README.md)
