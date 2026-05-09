# SDCTP OutputBuffer Consumer Validation

## Metodo usado

- Compilado harness C# temporario contra `SimulDIESEL.exe` x86 Debug.
- Instanciado `SdctpApiService` com `IUceDispatcher` fake.
- Emitidos eventos RX ja parseados pelo dispatcher fake.
- Consumidor leu exclusivamente por `SdctpApiService.TryReadRxFrame(out frame)`.
- O harness nao acessa `CanRxMirrorManager` e nao interpreta TLV.

## Resultado

- Total frames enfileirados: `16`
- Eventos CanRxFrameAvailable: `16`
- Total frames lidos por TryReadRxFrame: `16`
- Matches: `16`
- Mismatches: `0`
- OutputBuffer restante: `0`
- Acesso direto ao mirror: `false`
- Interpretacao de TLV pelo consumidor: `false`

CONSUMIDOR SDCTP VALIDADO: TryReadRxFrame e a saida oficial de RX

## Saida bruta

```text
frames_enfileirados=16
eventos_disponiveis=16
frames_lidos=16
matches=16
mismatches=0
output_buffer_restante=0
mirror_acessado=false
tlv_interpretado=false
result=OK
```
