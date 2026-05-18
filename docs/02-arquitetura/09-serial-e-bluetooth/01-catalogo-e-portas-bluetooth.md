⬅ [Retornar para Serial e Bluetooth no Host](../09-serial-e-bluetooth.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Catálogo Bluetooth e Portas no Windows

## Posição estrutural

Esta página aprofunda o ramo físico do Bluetooth no host Windows.

```text
FrmBpmLogic / BpmBluetoothService
  -> BluetoothDeviceCatalog
  -> BluetoothConnectionSettings
  -> BluetoothTransport
  -> SerialTransport
  -> COM SPP do Windows
```

## Classes reais

| arquivo | classe | papel | estado |
| --- | --- | --- | --- |
| `DAL/Transport/Bluetooth/BluetoothDeviceCatalog.cs` | `BluetoothDeviceCatalog` | descobre dispositivos pareados e COMs SPP | `IMPLEMENTADO` |
| `BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs` | `BpmBluetoothService` | escolhe dispositivo padrão e dispara a conexão | `IMPLEMENTADO` |
| `DAL/Transport/Bluetooth/BluetoothConnectionSettings.cs` | `BluetoothConnectionSettings` | carrega `PortName`, `DeviceName` e `BaudRate` | `IMPLEMENTADO` |
| `DAL/Transport/Bluetooth/BluetoothTransport.cs` | `BluetoothTransport` | adapta o Bluetooth ao contrato `IByteTransport` | `IMPLEMENTADO` |

## Fontes de descoberta usadas pelo host

`BluetoothDeviceCatalog` cruza duas categorias de dados:

- dispositivos pareados lidos do registro em `SYSTEM\CurrentControlSet\Services\BTHPORT\Parameters\Devices`;
- portas SPP usáveis encontradas ao caminhar `SYSTEM\CurrentControlSet\Enum\BTHENUM` e `SYSTEM\CurrentControlSet\Enum\BTHMODEM`.

Um dispositivo só sobe como realmente utilizável quando o host encontra:

- nome ou identificação Bluetooth;
- endereço ou chave estável;
- `PortName` que também exista em `BluetoothTransport.ListPorts()`.

## Trecho comentado: merge entre pareados e utilizáveis

Em `BluetoothDeviceCatalog.ListDevices()`, o catálogo junta as duas visões:

```csharp
Dictionary<string, BluetoothDeviceDto> pairedDevices = LoadPairedDevices();
Dictionary<string, BluetoothDeviceDto> usableDevices = LoadUsableDevices();
```

O que esse trecho faz:

- primeiro levanta tudo o que o Windows considera pareado;
- depois marca quais desses dispositivos têm uma COM SPP efetivamente utilizável;
- só então ordena o resultado colocando os disponíveis primeiro.

## Preferência da BPM

O código atual prioriza exatamente estes nomes:

- `SimulDIESEL - BPM`
- `SimulDIESEL-BPM`

Se nenhum deles aparecer com COM válida, `TryResolvePreferredBpmDevice(...)` devolve falha com mensagem descritiva em vez de abrir uma conexão parcial.

## Trecho comentado: critério de melhor correspondência

Em `SelectBestMatch(...)`, o catálogo busca primeiro o nome preferencial e só depois faz busca aproximada:

```csharp
BluetoothDeviceDto[] candidates = devices
    .Where(IsPreferredBpmDevice)
    .OrderByDescending(device => device.IsAvailable)
    .ThenBy(device => device.PortName)
```

Por que isso importa:

- o atalho Bluetooth do `DashBoard` depende deste critério;
- a documentação precisa refletir que a escolha automática não é arbitrária.

## Glossário

- **SPP**: perfil serial clássico do Bluetooth.
- **Dispositivo utilizável**: dispositivo que, além de pareado, possui COM visível no Windows.
- **Nome preferencial**: nome que o host procura primeiro ao tentar conectar automaticamente.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
