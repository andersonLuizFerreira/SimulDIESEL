⬅ [Retornar para API e Host Local](04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# BLL do Host

## Posição na pilha

A BLL real do host fica entre a UI WinForms e a DAL SDH/SDGW. Ela recebe eventos de tela, projeta estado para o operador e decide qual client funcional deve chamar a pilha de protocolo.

```text
DashBoard / forms
  -> FrmBpmLogic / FrmGsaLogic
  -> BpmSerialService
  -> BpmClient / GsaClient
  -> SdhClient / SdgwSession
```

## Classes reais da camada

| grupo | arquivos e classes | acima | abaixo | estado | papel |
| --- | --- | --- | --- | --- | --- |
| FormsLogic | `BLL/FormsLogic/BPM/FrmBpmLogic.cs`, `BLL/FormsLogic/GSA/FrmGsaLogic.cs` | `DashBoard`, `frmPortaSerial_UI`, `frmBluetoothConnect`, `frmGSA_UI` | `BpmSerialService`, `GsaClient` | `IMPLEMENTADO` | traduz eventos e estados da UI |
| Fachada BPM | `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs` | `FrmBpmLogic` | `SwitchableTransport`, `SdgwHostSession`, `BpmClient`, `GsaClient` | `IMPLEMENTADO` | compõe a árvore do host e projeta estado de conexão |
| Serviço Bluetooth | `BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs` | `FrmBpmLogic`, `DashBoard` | `BluetoothDeviceCatalog`, `BpmSerialService.ConnectBluetooth(...)` | `IMPLEMENTADO` | resolve o dispositivo preferencial e conecta via COM SPP |
| Client BPM | `BLL/Boards/BPM/BpmClient.cs` | `FrmBpmLogic` | `SdhClient` | `IMPLEMENTADO` | expõe `GetStatus()` e `PingGatewayAsync()` |
| Client GSA | `BLL/Boards/GSA/GsaClient.cs` | `FrmGsaLogic`, `frmGSA_UI` | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | envia operações GSA, correlaciona respostas e repassa eventos |
| Serviços auxiliares | `BLL/Boards/BPM/Backplane/BackplaneService.cs`, `BLL/Boards/BPM/XConn/XConnService.cs` | `BpmClient` | nenhum conector inferior ativo | `PARCIALMENTE IMPLEMENTADO` | preservam pontos de expansão já nomeados na BLL |
| Serviço de rede | `BLL/Boards/BPM/Comm/Network/BpmNetworkService.cs` | `BpmSerialService.Network` | nenhum | `PLANEJADO` | reserva de API para um transporte futuro |

## Conectores com a camada acima

- `FrmBpmLogic` sobe `StatusChanged` e `Error` para as telas BPM.
- `FrmGsaLogic` sobe `ChannelFaultEventReceived` e `PhysicalOperationEventReceived` para `frmGSA_UI`.
- `BpmSerialService` converte `SdgwHostSession.SessionState` em `BpmSerialService.LinkState`, que é o estado realmente consumido pela UI.

## Conectores com a camada abaixo

- `BpmClient.PingGatewayAsync()` desce para `SdhClient.SendAsync(...)`.
- `GsaClient.ExecuteOperationAsync(...)` desce para `SdhClient.SendAsync(...)` e volta a ouvir `SdgwSession.FrameReceived` e `SdgwSession.EventReceived`.
- `BpmSerialService.Connect(...)` e `ConnectBluetooth(...)` não abrem `SerialPort` diretamente; ambos passam por `SwitchableTransport`.

## Observações confirmadas

- O nome `BpmSerialService` permaneceu histórico, mas a classe já é a fachada comum de serial e Bluetooth.
- `FrmGsaLogic` protege a GSA com uma guarda simples: sem `IsLinked`, todas as operações retornam falha imediata.
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
Bpm = new BpmClient(Sdh, this, Backplane, XConn);
```

Esse bloco deixa claro que a BLL não implementa framing nem transporte; ela só compõe serviços, decide responsabilidades e entrega uma API mais estável para as telas.

## Glossário

- **FormsLogic**: subcamada da BLL que evita que o formulário conheça a árvore interna do host.
- **Client funcional**: classe que concentra operações de uma board específica.
- **Fachada**: ponto único da BLL que concentra estado e composição.

## Próximas camadas

- [FormsLogic e Fachadas do Host](05-bll-do-host/01-formslogic-e-fachadas.md)
- [Clients BPM e GSA](05-bll-do-host/02-clients-bpm-e-gsa.md)
