⬅ [Retornar para Transporte do Host](08-transporte-do-host.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Serial e Bluetooth no Host

## Meios físicos confirmados

O código do host confirma dois caminhos físicos para chegar à BPM:

- `IMPLEMENTADO`: serial direta via `SerialTransport`.
- `IMPLEMENTADO`: Bluetooth Classic SPP via `BluetoothTransport`, exposto como COM no Windows.

## Serial

| item | evidência em código |
| --- | --- |
| classe principal | `DAL/Transport/Serial/SerialTransport.cs` |
| adapter físico | `DAL/Transport/Serial/SerialPortAdapter.cs` |
| configuração padrão mais usada | `115200`, `8N1`, sem handshake, `DtrEnable=false`, `RtsEnable=false` |
| UI que abre a conexão | `UI/frmPortaSerial_UI.cs` |
| entrada de BLL | `FrmBpmLogic.Connect(...)` |

`SerialTransport` é apenas transporte cru. Framing, handshake SDGW, retry e estado funcional ficam fora dele.

## Bluetooth

| item | evidência em código |
| --- | --- |
| classe principal | `DAL/Transport/Bluetooth/BluetoothTransport.cs` |
| catálogo de dispositivos | `DAL/Transport/Bluetooth/BluetoothDeviceCatalog.cs` |
| serviço de BLL | `BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs` |
| seleção rápida | `FrmBpmLogic.ConnectBluetoothPadrao()` |
| fluxo do `DashBoard` | `DashBoard.toolStripBluetooth_Click(...)` |
| UI dedicada | `UI/frmBluetoothConnect.cs` |

O `DashBoard` não abre `frmBluetoothConnect` no atalho principal; ele tenta conexão automática com o dispositivo preferencial. A tela dedicada existe, lista dispositivos, mostra `StatusText` e permite conexão manual.

## Classificação de estado

- `IMPLEMENTADO`: serial host <-> BPM.
- `IMPLEMENTADO`: Bluetooth host <-> BPM usando COM SPP já pareada.
- `PARCIALMENTE IMPLEMENTADO`: UI dedicada para seleção Bluetooth existe, mas não é o fluxo padrão do `DashBoard`.
- `PLANEJADO`: outros meios físicos, como rede, ainda não descem até `DAL/Transport`.

## Trecho comentado: Bluetooth sobre serial

`BluetoothTransport` confirma que, no host atual, Bluetooth é um envelope sobre a infraestrutura serial:

```csharp
private readonly SerialTransport _serialTransport = new SerialTransport();

public bool Connect(BluetoothConnectionSettings settings)
{
    return _serialTransport.Connect(
        portName: settings.PortName,
        baudRate: settings.BaudRate);
}
```

O que esse trecho faz:

- preserva a distinção de `TransportKind` para a BLL e para a UI;
- reaproveita o mesmo mecanismo de COM do Windows;
- documenta corretamente que o host não tem uma stack de rádio própria.

## Glossário

- **SPP**: Serial Port Profile do Bluetooth Classic.
- **COM pareada**: porta criada pelo Windows para um dispositivo Bluetooth já pareado.
- **Fluxo principal**: caminho acionado diretamente pelo `DashBoard`.

## Próximas camadas

- [Catálogo Bluetooth e Portas no Windows](09-serial-e-bluetooth/01-catalogo-e-portas-bluetooth.md)
