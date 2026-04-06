⬅ [Retornar para API e Host Local](04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Transporte do Host

## Posição na pilha

No host atual, transporte é o degrau que escolhe qual `IByteTransport` ficará ativo e encaminha bytes entre a sessão SDGW e a COM do Windows.

```text
SdgwHostSession / SdgwSession
  -> SwitchableTransport
  -> IByteTransport
  -> SerialTransport ou BluetoothTransport
```

## Peças reais

| arquivo | classe | papel | estado |
| --- | --- | --- | --- |
| `DAL/Transport/SwitchableTransport.cs` | `SwitchableTransport` | hub com sessão única e seleção de `TransportKind` | `IMPLEMENTADO` |
| `DAL/Transport/Serial/IByteTransport.cs` | `IByteTransport` | contrato comum de conexão, bytes, erro e desconexão | `IMPLEMENTADO` |
| `DAL/Transport/TransportKind.cs` | `TransportKind` | enum `Serial` e `Bluetooth` | `IMPLEMENTADO` |
| `DAL/Transport/TransportConnectionSettings.cs` | `TransportConnectionSettings` | base comum de configuração e nome amigável | `IMPLEMENTADO` |
| `DAL/Transport/Serial/SerialConnectionSettings.cs` | `SerialConnectionSettings` | parâmetros físicos da porta COM | `IMPLEMENTADO` |
| `DAL/Transport/Bluetooth/BluetoothConnectionSettings.cs` | `BluetoothConnectionSettings` | parâmetros da COM SPP e nome do dispositivo | `IMPLEMENTADO` |

## Interfaces entre camadas

### Acima

- `SdgwHostSession` abre e fecha o transporte.
- `SdGwLinkEngine` grava bytes por meio do callback `WriteRaw(...)`.
- `FrmBpmLogic` e `BpmSerialService` escolhem `SerialConnectionSettings` ou `BluetoothConnectionSettings`.

### Abaixo

- `SerialTransport` encapsula `SerialPortAdapter`.
- `BluetoothTransport` reaproveita `SerialTransport` sobre uma COM Bluetooth SPP já criada pelo Windows.

## O que o código impõe

- `IMPLEMENTADO`: uma única sessão ativa por vez; `SwitchableTransport.Connect(...)` rejeita abrir outra conexão enquanto uma já estiver aberta.
- `IMPLEMENTADO`: `EndpointDisplayName` sobe para a UI como nome amigável do endpoint ativo.
- `IMPLEMENTADO`: eventos `BytesReceived`, `ConnectionChanged` e `Error` passam por um contrato único.
- `PLANEJADO`: não há `TransportKind.Network`.

## Trecho âncora

O bloqueio estrutural de concorrência entre transportes está no início de `SwitchableTransport.Connect(...)`:

```csharp
if (_activeTransport != null && _activeTransport.IsOpen)
{
    RaiseError("Ja existe uma sessao ativa. Desconecte o transporte atual antes de iniciar outro.");
    return false;
}
```

Esse bloco explica por que serial e Bluetooth não coexistem no host atual: o hub foi projetado para uma sessão física única.

## Glossário

- **IByteTransport**: contrato mínimo de abertura, escrita, eventos e desconexão.
- **Hub de transporte**: classe que escolhe qual implementação física ficará ativa.
- **Endpoint físico**: porta COM efetivamente usada pelo host.

## Próximas camadas

- [SwitchableTransport e Contratos de Transporte](08-transporte-do-host/01-switchable-transport.md)
- [Serial e Bluetooth no Host](09-serial-e-bluetooth.md)
