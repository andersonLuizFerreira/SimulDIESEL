⬅ [Retornar para API e Host Local](04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# DAL do Host

## Posição na pilha

A DAL real do host concentra protocolo, sessão, framing e adaptação para o transporte. Na leitura `ONDE`, esta é a faixa que recebe intenção já escolhida pela BLL e a transforma em tráfego SDGW.

```text
BLL
  -> SdgwHostSession (borda superior)
  -> SdhClient
  -> SdhValidator / SdhToSdgwMapper
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SdGwLinkSupervisor
  -> SwitchableTransport
```

## Componentes reais

| grupo | arquivos e classes | função estrutural | estado |
| --- | --- | --- | --- |
| Semântica SDH | `SdhClient.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`, `SdhTextParser.cs`, `SdhTextSerializer.cs` | valida target/op, interpreta texto e mapeia intenção para `cmd + payload` SDGW | `IMPLEMENTADO` |
| Sessão SDGW | `SdgwSession.cs` | converte `AppFrame` do engine em `SdgwFrame` e sobe `FrameReceived` / `EventReceived` | `IMPLEMENTADO` |
| Scheduler | `SdGwTxScheduler.cs` | fila única com prioridades `High`, `Normal` e `Low` | `IMPLEMENTADO` |
| Link engine | `SdgwLinkEngine.cs` | delimitação, COBS, CRC8, ACK, timeout, retry e deduplicação RX | `IMPLEMENTADO` |
| Supervisor | `SdgwLinkSupervisor.cs` | watchdog por silêncio de RX válido | `IMPLEMENTADO` |
| Helpers de framing | `SdgwFrameReader.cs`, `SdgwFrameWriter.cs`, `SdgwFrameCodec.cs` | utilitários fora do hot path principal | `IMPLEMENTADO` |
| Transporte | `DAL/Transport/**` | degrau final da DAL antes do sistema operacional | `IMPLEMENTADO` |

## Conectores acima e abaixo

### Acima

- `SdgwHostSession` cria e possui `SdgwSession`, `SdhClient`, `SdGwTxScheduler`, `SdGwLinkEngine` e `SdGwLinkSupervisor`.
- `BpmClient` e `GsaClient` entram nesta camada via `SdhClient`.

### Abaixo

- `SdGwLinkEngine` só conhece um callback `WriteRaw(...)`.
- `WriteRaw(...)` desce até `IByteTransport.Write(...)`.
- `SwitchableTransport` escolhe `SerialTransport` ou `BluetoothTransport`.

## Observações de fidelidade

- `SdgwHostSession` está fisicamente em `BLL/Boards/BPM/Comm`, mas estruturalmente é a borda superior da DAL do host.
- `SdgwFrameReader` e `SdgwFrameWriter` existem e funcionam, porém o fluxo ativo usa principalmente o caminho interno de `SdGwLinkEngine`.
- O catálogo SDH aceito por esta DAL é bem menor que o catálogo documental global do projeto: o código atual confirma `BPM.gateway ping` e `GSA.*`.
- O caminho `SendOutcome.Enqueued` existe no engine, mas nenhum caso funcional ativo do host atual usa envio sem ACK.

## Trecho âncora: fronteira semântica

Em `DAL/Protocols/SDGW/SdhClient.cs`, a fronteira entre intenção e frame compacto é explícita:

```csharp
_validator.Validate(command);
SdhToSdgwMapper.MappedSdgwCommand mapped = _mapper.Map(command);

return _sdgw.SendAsync(
    mapped.Cmd,
    mapped.Payload,
    mapped.RequireAck,
    mapped.TimeoutMs,
    mapped.Retries,
    priority,
    origin ?? (command.Target + ":" + command.Op));
```

Esse trecho é o ponto onde a DAL deixa de falar em `Target`, `Op` e `Args` e passa a falar em `cmd`, `payload`, `timeout` e `retries`.

## Glossário

- **DAL**: camada que concentra protocolo, sessão e transporte.
- **Hot path**: caminho realmente usado durante a execução normal do link.
- **Frame compacto**: comando SDGW já pronto para descer ao enlace.

## Próximas camadas

- [Sessão, SDH e SDGW na DAL](06-dal-do-host/01-sessao-sdh-e-sdgw.md)
- [Framing, Scheduler e Supervisor](06-dal-do-host/02-framing-scheduler-e-supervisor.md)
