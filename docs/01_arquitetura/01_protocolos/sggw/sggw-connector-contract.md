# SGGW Connector Contract

**SimulDIESEL — Serial Gateway Connector Specification**
Version: 1.0
Status: Stable
Layer: BLL (Business Logic Layer)

---

# 1. Purpose

Este documento define o **contrato oficial do conector SGGW (SimulDIESEL Serial Gateway)**.

Este conector é o único ponto autorizado de acesso ao protocolo SGGW para qualquer camada consumidora dentro da aplicação.

Este documento deve ser considerado a fonte única de verdade para:

* envio de comandos
* recebimento de frames
* integração com dispositivos externos
* implementação de serviços consumidores do link

---

# 2. Architectural Overview

Arquitetura completa da comunicação:

```
UI Layer
   ↓
BLL Consumer Services
   ↓
SdGgwClient   ← OFFICIAL CONNECTOR
   ↓
SdGwLinkEngine
   ↓
SerialTransport
   ↓
Physical Device (ESP-32, Gateway, etc)
```

O conector encapsula completamente o protocolo.

Consumidores nunca devem acessar camadas inferiores diretamente.

---

# 3. Connector Access

O conector é acessado exclusivamente através de:

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

Definições:

| Propriedade | Significado                           |
| ----------- | ------------------------------------- |
| IsConnected | Transporte serial ativo               |
| IsLinked    | Handshake concluído                   |
| State       | Estado completo da máquina de estados |

Estados possíveis:

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

Método oficial de envio:

```csharp
Task<SendOutcome> SendAsync(
    SggwCmd cmd,
    byte[] payload,
    bool requireAck = true,
    int timeoutMs = 150,
    int retries = 2)
```

Parâmetros:

| Parâmetro  | Descrição                |
| ---------- | ------------------------ |
| cmd        | comando do protocolo     |
| payload    | dados do comando         |
| requireAck | requer confirmação       |
| timeoutMs  | timeout de ACK           |
| retries    | número de retransmissões |

---

# 6. SendOutcome Definition

Resultados possíveis:

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

Eventos disponíveis:

```csharp
event Action<SggwFrame> FrameReceived;
event Action<SggwFrame> EventReceived;
```

Definições:

FrameReceived
→ disparado quando qualquer frame válido é recebido

EventReceived
→ disparado quando frame contém flag Event

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

* CRC já validado
* COBS já decodificado
* integridade garantida
* duplicatas filtradas

Consumidor não deve validar CRC.

---

# 9. Thread Safety Model

O conector é thread-safe.

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

Consumidores não devem implementar estas funções.

---

# 11. Consumer Responsibilities

Consumidores devem:

* verificar SerialLink.IsLinked antes de enviar
* enviar comandos usando SdGgwClient
* receber eventos via SdGgwClient
* interpretar payload conforme DTL

Consumidores não devem:

* acessar SerialTransport
* acessar SdGwLinkEngine
* implementar framing ou CRC

---

# 12. Connector Lifecycle

O conector é:

* singleton
* criado automaticamente
* persistente durante execução
* destruído apenas com SerialLinkService

Consumidores não devem destruir o conector.

---

# 13. Communication Flow

Fluxo completo:

```
Consumer Service
   ↓
SdGgwClient
   ↓
SdGwLinkEngine
   ↓
SerialTransport
   ↓
Physical Device
   ↓
SerialTransport
   ↓
SdGwLinkEngine
   ↓
SdGgwClient
   ↓
Consumer Service
```

---

# 14. Stability Contract

Este conector é considerado estável.

Qualquer nova funcionalidade deve ser implementada acima desta camada.

Não modificar este comportamento sem atualização deste documento.

---

# 15. Ownership

Subsystem: SimulDIESEL Communication Layer
Layer: BLL
Component: SdGgwClient

---

# END OF DOCUMENT
