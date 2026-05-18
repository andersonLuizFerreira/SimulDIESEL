⬅ [Retornar para API e Host Local](04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# BLL do Host

## Posição na pilha

A BLL real do host fica entre a UI WinForms e a DAL SDH/SDGW. Ela recebe eventos de tela, projeta estado para o operador e decide qual client funcional deve chamar a pilha de protocolo.

```text
DashBoard / forms
  -> FrmBpmLogic / FrmGsaLogic / FrmUceLogic
  -> BpmSerialService
  -> BpmClient / GsaClient / UceClient / dispatchers CAN-SDCTP
  -> SdhClient / SdgwSession
```

## Classes reais da camada

| grupo | arquivos e classes | acima | abaixo | estado | papel |
| --- | --- | --- | --- | --- | --- |
| FormsLogic | `BLL/FormsLogic/BPM/FrmBpmLogic.cs`, `BLL/FormsLogic/GSA/FrmGsaLogic.cs`, `BLL/FormsLogic/UCE/FrmUceLogic.cs` | `DashBoard`, `frmPortaSerial_UI`, `frmBluetoothConnect`, `frmGSA_UI`, `frmUCE_UI` | `BpmSerialService`, `GsaClient`, `UceDispatcher` | `IMPLEMENTADO` | traduz eventos e estados da UI |
| Fachada BPM | `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs` | `FrmBpmLogic` e forms de boards | `SwitchableTransport`, `SdgwHostSession`, `BpmClient`, `GsaClient`, `UceClient`, `UceDispatcher` | `IMPLEMENTADO` | compõe a árvore do host e projeta estado de conexão |
| Serviço Bluetooth | `BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs` | `FrmBpmLogic`, `DashBoard` | `BluetoothDeviceCatalog`, `BpmSerialService.ConnectBluetooth(...)` | `IMPLEMENTADO` | resolve o dispositivo preferencial e conecta via COM SPP |
| Client BPM | `BLL/Boards/BPM/BpmClient.cs` | `FrmBpmLogic` | `SdhClient` | `IMPLEMENTADO` | expõe `GetStatus()` e `PingGatewayAsync()` |
| Client GSA | `BLL/Boards/GSA/GsaClient.cs` | `FrmGsaLogic`, `frmGSA_UI` | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | envia operações GSA, correlaciona respostas e repassa eventos |
| Client UCE | `BLL/Boards/UCE/UceClient.cs`, `BLL/Boards/UCE/UceDispatcher.cs` | `FrmUceLogic`, serviços CAN/SDCTP | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | envia `UCE.*`, interpreta respostas e centraliza operações da board |
| Serviços CAN/SDCTP | `BLL/Services/CAN/CanControlApiService.cs`, `BLL/Services/CAN/SDCTP/SdctpApiService.cs` | UI e serviços de protocolo | `UceDispatcher` | `IMPLEMENTADO` | expõe controle CAN e tabelas SDCTP por cima da UCE |
| Despacho de boards | `BLL/Boards/BoardDispatcher.cs` | camada de aplicação | `UceDispatcher`, `GsaDispatcher` | `IMPLEMENTADO` | roteia operações para dispatchers específicos |
| Serviços auxiliares | `BLL/Boards/BPM/Backplane/BackplaneService.cs`, `BLL/Boards/BPM/XConn/XConnService.cs` | `BpmClient` | nenhum conector inferior ativo | `PARCIALMENTE IMPLEMENTADO` | preservam pontos de expansão já nomeados na BLL |
| Serviço de rede | `BLL/Boards/BPM/Comm/Network/BpmNetworkService.cs` | `BpmSerialService.Network` | nenhum | `PLANEJADO` | reserva de API para um transporte futuro |

## Conectores com a camada acima

- `FrmBpmLogic` sobe `StatusChanged` e `Error` para as telas BPM.
- `FrmGsaLogic` sobe `ChannelFaultEventReceived` e `PhysicalOperationEventReceived` para `frmGSA_UI`.
- `FrmUceLogic` liga a tela UCE ao dispatcher e aos serviços CAN/SDCTP.
- `BpmSerialService` converte `SdgwHostSession.SessionState` em `BpmSerialService.LinkState`, que é o estado realmente consumido pela UI.

## Conectores com a camada abaixo

- `BpmClient.PingGatewayAsync()` desce para `SdhClient.SendAsync(...)`.
- `GsaClient.ExecuteOperationAsync(...)` desce para `SdhClient.SendAsync(...)` e volta a ouvir `SdgwSession.FrameReceived` e `SdgwSession.EventReceived`.
- `UceClient.ExecuteOperationAsync(...)` desce para `SdhClient.SendAsync(...)` e alimenta `UceDispatcher`, `CanControlApiService` e `SdctpApiService`.
- `BpmSerialService.Connect(...)` e `ConnectBluetooth(...)` não abrem `SerialPort` diretamente; ambos passam por `SwitchableTransport`.

## Observações confirmadas

- O nome `BpmSerialService` permaneceu histórico, mas a classe já é a fachada comum de serial e Bluetooth.
- `FrmGsaLogic` protege a GSA com uma guarda simples: sem `IsLinked`, todas as operações retornam falha imediata.
- A UCE já participa da composição do host com client, dispatcher, controle CAN e SDCTP.
- `BackplaneService` e `XConnService` já participam da composição do host, mas ainda não descem para a DAL.
- `BpmNetworkService` existe somente como placeholder e devolve mensagem de não implementado.

## Trecho âncora

No próprio construtor de `BpmSerialService`, a BLL monta a árvore superior do host:

```csharp
Backplane = new BackplaneService();
XConn = new XConnService();
Bluetooth = new Comm.Bluetooth.BpmBluetoothService(this);
Network = new Comm.Network.BpmNetworkService();
Gsa = new GsaClient(Sdh, Sdgw);
Uce = new UceClient(Sdh, Sdgw);
UceDispatcher = new UceDispatcher(Uce);
CanControl = new CanControlApiService(UceDispatcher);
Sdctp = new SdctpApiService(UceDispatcher);
BoardDispatcher = new BoardDispatcher(UceDispatcher, GsaDispatcher);
Bpm = new BpmClient(Sdh, this, Backplane, XConn);
```

Esse bloco deixa claro que a BLL não implementa framing nem transporte; ela só compõe serviços, decide responsabilidades e entrega uma API mais estável para as telas.

## Glossário

- **FormsLogic**: subcamada da BLL que evita que o formulário conheça a árvore interna do host.
- **Client funcional**: classe que concentra operações de uma board específica.
- **Fachada**: ponto único da BLL que concentra estado e composição.

## Próximas camadas

- [FormsLogic e Fachadas do Host](05-bll-do-host/01-formslogic-e-fachadas.md)
- [Clients BPM, GSA e UCE](05-bll-do-host/02-clients-bpm-e-gsa.md)
