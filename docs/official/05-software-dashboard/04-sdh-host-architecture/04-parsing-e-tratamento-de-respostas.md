⬅ [Retornar para Arquitetura SDH no Host](../04-sdh-host-architecture.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# Parsing e Tratamento de Respostas

## Função lógica

Esta página cobre a trilha `COMO` do parsing real do host: como texto SDH vira comando validado, como o mapper vira TLV SDGW e como a resposta volta como DTO ou erro tipado.

## Blocos lógicos envolvidos

| classe | função lógica | estado |
| --- | --- | --- |
| `SdhTextParser` | transforma texto em `SdhCommand` | `IMPLEMENTADO` |
| `SdhTextSerializer` | serializa `SdhCommand` em texto ordenado | `IMPLEMENTADO` |
| `SdhTarget` | quebra `target` em `Board`, `Resource`, `Subresource` | `IMPLEMENTADO` |
| `SdhValidator` | valida versão, target, op e argumentos | `IMPLEMENTADO` |
| `SdhToSdgwMapper` | transforma SDH em `cmd + payload + timeout + retries` | `IMPLEMENTADO` |
| `GsaParsers` | interpreta TLVs síncronos, erros e eventos da GSA | `IMPLEMENTADO` |
| `SdhResponse` | contrato pronto para resposta semântica | `PARCIALMENTE IMPLEMENTADO` |

## Trecho comentado: parser textual

Em `SdhTextParser.Parse(...)`:

```csharp
string[] tokens = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
if (tokens.Length < 3)
    throw new InvalidOperationException("Comando SDH inválido. Esperado: version target op [chave=valor...]");
```

O que esse trecho faz:

- exige pelo menos `version target op`;
- permite argumentos extras apenas no formato `chave=valor`;
- cria um `SdhCommand` usado depois pela mesma trilha de validação e mapeamento da UI.

## Trecho comentado: validação do catálogo

Em `SdhValidator.Validate(...)`, o host fecha explicitamente o escopo aceito:

```csharp
if (string.Equals(target.Board, GsaBoard, StringComparison.OrdinalIgnoreCase))
{
    ValidateGsaCommand(target, command);
    return;
}

if (string.Equals(target.Board, BpmBoard, StringComparison.OrdinalIgnoreCase) &&
    string.Equals(target.Resource, BpmResource, StringComparison.OrdinalIgnoreCase) &&
    string.IsNullOrWhiteSpace(target.Subresource))
{
    ValidateBpmGateway(command);
    return;
}
```

Por que isso importa:

- impede que a documentação trate targets futuros como presentes;
- concentra em um ponto único as regras de `channel`, `state`, `kind`, `value` e faixa de canais `1..16`.

## Trecho comentado: montagem do TLV

Em `SdhToSdgwMapper.BuildTlvPayload(...)`, a DAL fecha o payload GSA:

```csharp
byte[] payload = new byte[payloadLength + 3];
payload[0] = type;
payload[1] = (byte)payloadLength;
...
payload[payload.Length - 1] = SdgwFrameCodec.Crc8Atm(payload, 0, payload.Length - 1);
```

O que esse trecho faz:

- monta `type + len + data + crc`;
- deixa o payload GSA pronto para ser encapsulado no frame SDGW;
- explica por que `GsaParsers.TryReadTlv(...)` aceita payload com ou sem o CRC final.

## Trecho comentado: tratamento de resposta GSA

Em `GsaClient.ExecuteOperationAsync(...)`, o client tenta três caminhos de interpretação antes do DTO final:

```csharp
if (GsaParsers.TryReadGatewayError(responseFrame, out gatewayError, out gatewayErrorParseMessage))
    return GsaOperationResult<T>.Fail(gatewayError.Message, outcome);

if (GsaParsers.TryReadFunctionalError(responseFrame, out functionalError, out functionalErrorParseMessage))
    return GsaOperationResult<T>.FunctionalFail(functionalError, outcome);

if (!parser(responseFrame, out response, out error))
    return GsaOperationResult<T>.Fail(error, outcome);
```

O que esse trecho faz:

- separa erro de gateway vindo da BPM;
- separa erro funcional devolvido pela própria GSA;
- só no final chama o parser específico do DTO esperado.

## Trecho comentado: leitura do TLV

Em `GsaParsers.TryReadTlv(...)`, o host aplica checagens mínimas antes de aceitar o payload:

```csharp
if (payloadLength != expectedLen + 2 && payloadLength != expectedLen + 3)
{
    error = "Resposta da GSA com tamanho inválido para " + operationName + ".";
    return false;
}
```

Esse detalhe é importante porque o host aceita:

- payload sem CRC final destacado;
- payload com CRC TLV embutido;
- mas continua exigindo `type` e `len` exatos para cada operação.

## O que ainda não virou fluxo quente

- `SdhResponse` existe como tipo de contrato, mas não é a forma usada hoje para devolver resposta ao restante do host.
- `OperationStatusDto` existe como DTO comum, mas a trilha ativa prefere resultados específicos de BPM e GSA.

## Glossário

- **Parsing**: interpretação estrutural de texto, target ou payload em tipos do host.
- **Erro de gateway**: falha retornada pela BPM ao intermediar uma operação da GSA.
- **Erro funcional**: falha devolvida pela própria GSA para o TLV solicitado.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
