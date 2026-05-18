⬅ [Retornar para BLL do Host](../05-bll-do-host.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# FormsLogic e Fachadas do Host

## Posição estrutural

Este é o primeiro degrau da BLL. Aqui a UI para de falar com widgets e começa a falar com métodos de aplicação.

```text
DashBoard / frmPortaSerial_UI / frmBluetoothConnect / frmGSA_UI
  -> FrmBpmLogic / FrmGsaLogic
  -> BpmSerialService
  -> SdgwHostSession / clients funcionais
```

## Classes e interfaces reais

| arquivo | classe | quem chama | o que ela chama abaixo | estado |
| --- | --- | --- | --- | --- |
| `BLL/FormsLogic/BPM/FrmBpmLogic.cs` | `FrmBpmLogic` | `DashBoard`, `frmPortaSerial_UI`, `frmBluetoothConnect` | `BpmSerialService`, `BpmBluetoothService`, `BpmClient` | `IMPLEMENTADO` |
| `BLL/FormsLogic/GSA/FrmGsaLogic.cs` | `FrmGsaLogic` | `frmGSA_UI` | `GsaClient` | `IMPLEMENTADO` |
| `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs` | `BpmSerialService` | `FrmBpmLogic` | `SwitchableTransport`, `SdgwHostSession`, `BpmClient`, `GsaClient` | `IMPLEMENTADO` |
| `BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs` | `BpmBluetoothService` | `FrmBpmLogic`, `DashBoard` | `BluetoothDeviceCatalog`, `BpmSerialService.ConnectBluetooth(...)` | `IMPLEMENTADO` |

## Fluxo estrutural confirmado

- `DashBoard.toolStripConectar_Click(...)` abre a tela serial, não conecta direto.
- `DashBoard.toolStripBluetooth_Click(...)` chama `FrmBpmLogic.ConnectBluetoothPadrao()` e tenta ligar ao dispositivo preferencial imediatamente.
- `frmBluetoothConnect` continua disponível para seleção manual, listagem de dispositivos e conexão explícita.
- `frmGSA_UI` cria `FrmGsaLogic` e passa por ele em todas as operações de setpoint, enable, status e offsets.

## Métodos principais desta faixa

| classe | método | função estrutural |
| --- | --- | --- |
| `FrmBpmLogic` | `Connect(...)` | valida porta/baud e chama a fachada BPM |
| `FrmBpmLogic` | `ConnectBluetoothPadrao(...)` | delega o fluxo automático ao serviço Bluetooth |
| `FrmBpmLogic` | `GetInterfaceDisplayName()` | adapta `BpmStatusDto` para um rótulo pronto de UI |
| `FrmGsaLogic` | `SetChannelSetpointAsync(...)` | cria `GsaChannelSetpointRequest` e chama o client |
| `FrmGsaLogic` | `FailWhenNotLinked<T>()` | bloqueia a GSA enquanto o link não está em `Linked` |
| `BpmSerialService` | `Connect(...)` / `ConnectBluetooth(...)` | traduz chamadas da UI em `TransportConnectionSettings` |
| `BpmSerialService` | `MapState(...)` | projeta `TransportConnected` em `SerialConnected` ou `BluetoothConnected` |

## Trecho comentado: adaptação para a UI

Em `FrmBpmLogic.GetInterfaceDisplayName()` o host decide qual texto a UI verá:

```csharp
if (status.TransportKind == TransportKind.Bluetooth)
{
    string bluetoothName = !string.IsNullOrWhiteSpace(status.TransportDisplayName)
        ? status.TransportDisplayName
        : status.InterfaceName;
    return "Bluetooth - " + bluetoothName;
}
```

O que esse trecho faz:

- recebe um `BpmStatusDto` já pronto da BLL de board;
- escolhe o nome amigável do endpoint ativo;
- encapsula o caso Bluetooth sem expor a árvore interna da sessão para a UI.

## Trecho comentado: projeção de estado

Em `BpmSerialService.MapState(...)`, a fachada converte o estado da sessão em um estado mais útil para a UI:

```csharp
case Comm.SdgwHostSession.SessionState.TransportConnected:
    return SelectedTransportKind == TransportKind.Bluetooth
        ? LinkState.BluetoothConnected
        : LinkState.SerialConnected;
```

O que esse trecho faz:

- preserva a distinção física entre serial e Bluetooth sem duplicar a sessão;
- deixa a UI reagir com indicadores diferentes mesmo quando o núcleo abaixo usa a mesma `SdgwHostSession`.

## Classificação dos blocos

- `IMPLEMENTADO`: `FrmBpmLogic`, `FrmGsaLogic`, `BpmSerialService`, `BpmBluetoothService`.
- `PARCIALMENTE IMPLEMENTADO`: o fluxo manual de `frmBluetoothConnect` existe, mas não é o caminho padrão do atalho do `DashBoard`.
- `PLANEJADO`: a FormsLogic não possui ainda um equivalente para `BpmNetworkService`.

## Glossário

- **Adaptador de UI**: método que transforma DTOs e enums em texto e estado visual.
- **Fachada BPM**: classe que concentra a composição da sessão do host.
- **Fluxo manual Bluetooth**: caminho que passa por `frmBluetoothConnect` em vez do atalho automático do `DashBoard`.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
