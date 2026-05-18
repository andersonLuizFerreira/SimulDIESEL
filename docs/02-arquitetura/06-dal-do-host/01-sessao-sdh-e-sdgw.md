⬅ [Retornar para DAL do Host](../06-dal-do-host.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Sessão, SDH e SDGW na DAL

## Posição estrutural

Esta página cobre a parte da DAL que recebe o comando já decidido pela BLL e o transforma em envio SDGW.

```text
GsaClient / BpmClient
  -> SdhClient
  -> SdhValidator
  -> SdhToSdgwMapper
  -> SdgwSession
```

## Classes reais

| arquivo | classe | entrada | saída | estado | observação |
| --- | --- | --- | --- | --- | --- |
| `DAL/Protocols/SDGW/SdhClient.cs` | `SdhClient` | `SdhCommand` ou texto SDH | `SdgwSession.SendAsync(...)` | `IMPLEMENTADO` | porta de entrada semântica do host |
| `DAL/Protocols/SDGW/SdhValidator.cs` | `SdhValidator` | `SdhCommand` | exceção ou comando validado | `IMPLEMENTADO` | restringe o catálogo aceito nesta fase |
| `DAL/Protocols/SDGW/SdhToSdgwMapper.cs` | `SdhToSdgwMapper` | `SdhCommand` | `MappedSdgwCommand` | `IMPLEMENTADO` | traduz target/op para TLV e `cmd` |
| `DAL/Protocols/SDGW/SdgwSession.cs` | `SdgwSession` | `cmd`, `payload`, prioridade | `FrameReceived`, `EventReceived` | `IMPLEMENTADO` | encapsula o engine com uma API mais estável |
| `DAL/Protocols/SDGW/SdhTextParser.cs` | `SdhTextParser` | string | `SdhCommand` | `IMPLEMENTADO` | parser textual utilitário |
| `DAL/Protocols/SDGW/SdhTextSerializer.cs` | `SdhTextSerializer` | `SdhCommand` | string | `IMPLEMENTADO` | serializador textual ordenado por chave |
| `DTL/Protocols/SDGW/SdhTarget.cs` | `SdhTarget` | string target | `Board`, `Resource`, `Subresource` | `IMPLEMENTADO` | apoio ao validador |
| `DTL/Protocols/SDGW/SdhResponse.cs` | `SdhResponse` | contrato de resposta | nenhum caminho quente ativo | `PARCIALMENTE IMPLEMENTADO` | tipo presente, mas não integrado ao fluxo principal do host |

## Catálogo realmente aceito

`SdhValidator` e `SdhToSdgwMapper` convergem no mesmo catálogo:

- `IMPLEMENTADO`: `BPM.gateway ping`
- `IMPLEMENTADO`: `GSA.led set`
- `IMPLEMENTADO`: `GSA.channel.setpoint set`
- `IMPLEMENTADO`: `GSA.channel.enable set`
- `IMPLEMENTADO`: `GSA.channel.status get`
- `IMPLEMENTADO`: `GSA.channels.enable set`
- `IMPLEMENTADO`: `GSA.channels.status get`
- `IMPLEMENTADO`: `GSA.channel.fault reset`
- `IMPLEMENTADO`: `GSA.channel.offset set|get|save|reset`
- `IMPLEMENTADO`: `GSA.offset reset`
- `PLANEJADO`: qualquer outro target SDH fora dessa lista

## Trecho comentado: filtro do catálogo

Em `SdhToSdgwMapper.Map(...)`, o dispatch principal já mostra o recorte suportado:

```csharp
if (string.Equals(command.Target, BpmGatewayTarget, StringComparison.OrdinalIgnoreCase))
    return MapBpmGateway(command);

if (command.Target.StartsWith("GSA.", StringComparison.OrdinalIgnoreCase))
    return MapGsa(command);

throw new NotSupportedException(...);
```

O que esse trecho faz:

- aceita apenas `BPM.gateway` e o espaço `GSA.*`;
- força toda ampliação futura do catálogo a passar por um ponto único de mapeamento;
- impede que a documentação trate outros targets como já implementados.

## Trecho comentado: sessão lógica

Em `SdgwSession.OnAppFrameReceived(...)`, a DAL sobe o frame já decodificado para um tipo estável:

```csharp
var logicalFrame = new SdgwFrame(
    cmd: frame.Cmd,
    seq: frame.Seq,
    flags: frame.Flags,
    payload: frame.Payload);

FrameReceived?.Invoke(logicalFrame);
```

O que esse trecho faz:

- recebe do `SdGwLinkEngine` um `AppFrame` técnico;
- converte para `SdgwFrame`, que é o contrato compartilhado com a BLL;
- sobe o mesmo frame para `EventReceived` quando `flags & 0x02` indica evento.

## Trecho comentado: parser textual

Em `SdhTextParser.Parse(...)`, a DAL suporta uma entrada textual auxiliar:

```csharp
string[] tokens = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
if (tokens.Length < 3)
    throw new InvalidOperationException(...);
```

Esse caminho não é o hot path da UI atual, mas ele já existe como utilitário real para texto SDH e precisa ser documentado como implementado.

## Glossário

- **SdhCommand**: contrato semântico que carrega `Version`, `Target`, `Op` e `Args`.
- **MappedSdgwCommand**: estrutura interna com `cmd`, `payload`, `RequireAck`, `TimeoutMs` e `Retries`.
- **Frame lógico**: `SdgwFrame` já sem COBS e CRC, pronto para consumo da BLL.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
