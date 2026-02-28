# SGGW Connector Contract

**SimulDIESEL â€” Serial Gateway Connector Specification**
Version: 1.0
Status: Stable
Layer: BLL (Business Logic Layer)

---

# 1. Purpose

Este documento define o **contrato oficial do conector SGGW (SimulDIESEL Serial Gateway)**.

Este conector Ã© o Ãºnico ponto autorizado de acesso ao protocolo SGGW para qualquer camada consumidora dentro da aplicaÃ§Ã£o.

Este documento deve ser considerado a fonte Ãºnica de verdade para:

* envio de comandos
* recebimento de frames
* integraÃ§Ã£o com dispositivos externos
* implementaÃ§Ã£o de serviÃ§os consumidores do link

---

# 2. Architectural Overview

Arquitetura completa da comunicaÃ§Ã£o:

```
UI Layer
   â†“
BLL Consumer Services
   â†“
SdGgwClient   â† OFFICIAL CONNECTOR
   â†“
SdGwLinkEngine
   â†“
SerialTransport
   â†“
Physical Device (ESP-32, Gateway, etc)
```

O conector encapsula completamente o protocolo.

Consumidores nunca devem acessar camadas inferiores diretamente.

---

# 3. Connector Access

O conector Ã© acessado exclusivamente atravÃ©s de:

```csharp
SerialLink.Service.Sggw
```

Tipo:

```csharp
SdGgwClient
```

Exemplo:

```csharp
var sggw = SerialLink.Service.Sggw;
```

---

# 4. Link State Model

Estado global do link:

```csharp
SerialLink.IsConnected
SerialLink.IsLinked
SerialLink.Service.State
```

DefiniÃ§Ãµes:

| Propriedade | Significado                           |
| ----------- | ------------------------------------- |
| IsConnected | Transporte serial ativo               |
| IsLinked    | Handshake concluÃ­do                   |
| State       | Estado completo da mÃ¡quina de estados |

Estados possÃ­veis:

```
Disconnected
SerialConnected
Draining
BannerSent
Linked
LinkFailed
```

---

# 5. Transmission Contract

MÃ©todo oficial de envio:

```csharp
Task<SendOutcome> SendAsync(
    SggwCmd cmd,
    byte[] payload,
    bool requireAck = true,
    int timeoutMs = 150,
    int retries = 2)
```

ParÃ¢metros:

| ParÃ¢metro  | DescriÃ§Ã£o                |
| ---------- | ------------------------ |
| cmd        | comando do protocolo     |
| payload    | dados do comando         |
| requireAck | requer confirmaÃ§Ã£o       |
| timeoutMs  | timeout de ACK           |
| retries    | nÃºmero de retransmissÃµes |

---

# 6. SendOutcome Definition

Resultados possÃ­veis:

```
Enqueued
Acked
Nacked
Timeout
TransportDown
Busy
```

---

# 7. Frame Reception Contract

Eventos disponÃ­veis:

```csharp
event Action<SggwFrame> FrameReceived;
event Action<SggwFrame> EventReceived;
```

DefiniÃ§Ãµes:

FrameReceived
â†’ disparado quando qualquer frame vÃ¡lido Ã© recebido

EventReceived
â†’ disparado quando frame contÃ©m flag Event

---

# 8. Frame Structure Delivered to Consumer

Tipo:

```csharp
SggwFrame
```

Propriedades:

```csharp
byte Cmd
byte Seq
byte Flags
byte[] Payload
```

Auxiliar:

```csharp
SggwCmd CommandEnum
```

Garantias:

* CRC jÃ¡ validado
* COBS jÃ¡ decodificado
* integridade garantida
* duplicatas filtradas

Consumidor nÃ£o deve validar CRC.

---

# 9. Thread Safety Model

O conector Ã© thread-safe.

Eventos podem ocorrer em threads internas.

Consumidores que utilizam UI devem usar:

```csharp
Invoke()
BeginInvoke()
```

O conector nunca acessa UI diretamente.

---

# 10. Connector Responsibilities

O conector encapsula completamente:

* COBS framing
* CRC-8/ATM validation
* ACK management
* ERR management
* retransmission
* timeout control
* sequence control
* stop-and-wait protocol
* frame parsing
* link reliability

Consumidores nÃ£o devem implementar estas funÃ§Ãµes.

---

# 11. Consumer Responsibilities

Consumidores devem:

* verificar SerialLink.IsLinked antes de enviar
* enviar comandos usando SdGgwClient
* receber eventos via SdGgwClient
* interpretar payload conforme DTL

Consumidores nÃ£o devem:

* acessar SerialTransport
* acessar SdGwLinkEngine
* implementar framing ou CRC

---

# 12. Connector Lifecycle

O conector Ã©:

* singleton
* criado automaticamente
* persistente durante execuÃ§Ã£o
* destruÃ­do apenas com SerialLinkService

Consumidores nÃ£o devem destruir o conector.

---

# 13. Communication Flow

Fluxo completo:

```
Consumer Service
   â†“
SdGgwClient
   â†“
SdGwLinkEngine
   â†“
SerialTransport
   â†“
Physical Device
   â†“
SerialTransport
   â†“
SdGwLinkEngine
   â†“
SdGgwClient
   â†“
Consumer Service
```

---

# 14. Stability Contract

Este conector Ã© considerado estÃ¡vel.

Qualquer nova funcionalidade deve ser implementada acima desta camada.

NÃ£o modificar este comportamento sem atualizaÃ§Ã£o deste documento.

---

# 15. Ownership

Subsystem: SimulDIESEL Communication Layer
Layer: BLL
Component: SdGgwClient

---

# END OF DOCUMENT

