# ETAPA - Servico de captura temporal J1939

Data: 2026-05-13

## Tema e objetivo

Captura temporal de massa J1939 para engenharia reversa assistida por IA.

Objetivo desta ETAPA interna: criar servico BLL para capturar eventos J1939 relevantes, reduzir spam de frames repetitivos e manter uma sessao temporal em memoria.

## Escopo permitido

- `BLL/Protocols/J1939/Capture/`
- `DTL/Protocols/J1939/Capture/`
- Integracao posterior via `FrmUceLogic`
- Dumps e documentacao oficial impactada

## Fora de escopo

- SDGW
- SDCTP
- Firmware
- Parser J1939 existente
- Address Registry
- Banco/SQLite
- Persistencia automatica

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939TemporalCaptureService.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CapturedEventDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939TemporalTickDto.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CaptureSessionDto.cs`

## Implementacao

`J1939TemporalCaptureService` mantem uma sessao em memoria com:

- inicio/fim da captura;
- contagem total de frames recebidos pela captura;
- contagem de frames unicos;
- eventos exportaveis;
- estado ativo/parado.

O servico recebe campos J1939 ja decodificados:

- timestamp;
- Raw CAN ID, quando disponivel na pipeline J1939;
- Source Address;
- Destination Address;
- PGN;
- payload;
- texto auxiliar.

Nao consome CAN bruto, TLV bruto, SDGW, SDCTP, SQLite ou UI.

## Reducao de spam

Frames identicos consecutivos sao agrupados:

- primeira ocorrencia vira `NEW_FRAME`;
- repeticoes consecutivas nao geram linhas individuais;
- ao trocar de mensagem ou finalizar captura, o grupo repetitivo vira `PERIODIC_TICK`;
- `PERIODIC_TICK` registra `RepeatCount` e `IntervalMs` medio.

Transicoes por mudanca de origem, destino, PGN ou payload sao registradas como `TRANSITION`.

## Enriquecimento Address Claim

Atualizacao incremental no contexto da mesma ETAPA:

- `PGN 0x00EE00` passa a ser exportado com `EventType=ADDRESS_CLAIM`;
- payload de 8 bytes e convertido de ordem CAN little-endian para `NameHex` visual de 16 caracteres;
- `ClaimedSourceAddress` registra o SA reivindicado;
- `RawCanId` e preservado quando o DTO da pipeline atual fornece o identificador CAN.

Exemplo validado com a amostra CASE:

```text
PGN=0x00EE00
Dados=00 00 C0 0B 00 13 0E 20
EventType=ADDRESS_CLAIM
NameHex=200E13000BC00000
ClaimedSourceAddress=0xC0
RawCanId=0x18EEFFC0
```

## Validacao

Build C# completo executado em:

```text
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-temporal-capture/
```

Resultado final:

- 0 erros
- 0 avisos

Smoke test em memoria:

- 20 frames identicos J1939 simulados;
- resultado: `Events=2`;
- resultado: `TotalFrames=20`;
- resultado: `UniqueFrames=1`;
- eventos: `NEW_FRAME:1` e `PERIODIC_TICK:20:100`;
- exportacao contem `PERIODIC_TICK`.

## Limitacoes

- A captura temporal nao e perda zero de frames.
- A validacao de bancada com EST nao foi executada neste ambiente.
- A semantica de `RepeatCount` nesta ETAPA representa o total de frames do grupo agrupado.

## Rollback

Rollback preservado: arquivos novos isolados em namespaces `Capture`, sem alteracao de contratos SDGW/SDCTP/firmware/parser.
