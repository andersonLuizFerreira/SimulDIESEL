⬅ [Retornar para Transporte do Host](../08-transporte-do-host.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# SwitchableTransport e Contratos de Transporte

## Posição estrutural

`SwitchableTransport` é o hub físico do host. Acima dele a sessão vê apenas `IByteTransport`; abaixo dele existem implementações concretas.

## Classes e interfaces

| arquivo | elemento | papel | estado |
| --- | --- | --- | --- |
| `DAL/Transport/SwitchableTransport.cs` | `SwitchableTransport` | escolhe e mantém o transporte ativo | `IMPLEMENTADO` |
| `DAL/Transport/Serial/IByteTransport.cs` | `IByteTransport` | contrato comum de bytes, conexão e erro | `IMPLEMENTADO` |
| `DAL/Transport/TransportConnectionSettings.cs` | `TransportConnectionSettings` | base comum de configuração | `IMPLEMENTADO` |
| `DAL/Transport/TransportKind.cs` | `TransportKind` | enum de seleção | `IMPLEMENTADO` |
| `DAL/Transport/Serial/SerialConnectionSettings.cs` | `SerialConnectionSettings` | COM, baud, paridade, data bits, stop bits e timeouts | `IMPLEMENTADO` |
| `DAL/Transport/Bluetooth/BluetoothConnectionSettings.cs` | `BluetoothConnectionSettings` | COM SPP, nome do dispositivo e baud | `IMPLEMENTADO` |

## Métodos principais

| classe | método | função estrutural |
| --- | --- | --- |
| `SwitchableTransport` | `Connect(...)` | escolhe o transporte concreto e impede sessão dupla |
| `SwitchableTransport` | `Write(...)` | encaminha bytes ao transporte ativo |
| `SwitchableTransport` | `Disconnect()` | delega o fechamento ao transporte concreto |
| `SwitchableTransport` | `ResolveDisplayName(...)` | sobe o nome amigável do endpoint |
| `BluetoothConnectionSettings` | `GetDisplayName()` | monta texto como `DeviceName (COMx)` |

## Trecho comentado: escolha do transporte

Em `SwitchableTransport.CreateTransport(...)`, o hub resolve a implementação concreta:

```csharp
switch (kind)
{
    case TransportKind.Serial:
        return new SerialTransport();
    case TransportKind.Bluetooth:
        return new BluetoothTransport();
```

O que esse trecho faz:

- recebe apenas o enum `TransportKind`;
- instancia a implementação concreta apropriada;
- impede que as camadas superiores conheçam `SerialTransport` e `BluetoothTransport` ao mesmo tempo.

## Trecho comentado: nome exibido para cima

Em `ResolveDisplayName(...)`, o hub decide o texto que subirá para a BLL:

```csharp
if (!string.IsNullOrWhiteSpace(settings.EndpointDisplayName))
    return settings.EndpointDisplayName;

BluetoothConnectionSettings bluetoothSettings = settings as BluetoothConnectionSettings;
if (bluetoothSettings != null)
    return bluetoothSettings.GetDisplayName();
```

Por que isso existe:

- o host quer manter um nome estável de interface ativa para a UI;
- serial e Bluetooth podem gerar esse nome por caminhos diferentes, mas o contrato acima continua único.

## Observações de fidelidade

- `OnConnectionChanged(false)` redefine `EndpointDisplayName` para `Nenhum`.
- `Write(...)` falha com erro explícito quando não há transporte ativo.
- `TransportConnectionSettings` só carrega `TransportKind` e `EndpointDisplayName`; os detalhes físicos ficam nos tipos derivados.

## Glossário

- **Hub físico**: classe que encapsula a escolha da implementação concreta.
- **Display name**: nome amigável do endpoint ativo entregue para as camadas superiores.
- **Sessão dupla**: tentativa de conectar um segundo transporte sem desconectar o primeiro.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
