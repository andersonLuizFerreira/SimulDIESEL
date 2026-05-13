# ETAPA - Exportacao TXT/MD da captura temporal J1939

Data: 2026-05-13

## Objetivo

Criar exportador de capturas temporais J1939 para `.txt` e `.md`, com formato legivel para humanos e para analise posterior pelo CODEX.

## Arquivo criado

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939CaptureExportService.cs`

## Responsabilidade

`J1939CaptureExportService` exporta uma `J1939CaptureSessionDto` para arquivo UTF-8 sem BOM.

O exportador nao acessa UI, banco, internet, firmware, SDGW ou SDCTP.

## Conteudo exportado

O arquivo contem:

- identificacao da sessao;
- inicio;
- fim;
- duracao;
- total de frames recebidos pela captura;
- frames unicos;
- eventos exportados;
- resumo por PGN;
- resumo por Source Address;
- resumo de Address Claims detectados;
- periodicidade detectada;
- top talkers;
- lista temporal dos eventos.

## Eventos

Cada evento exportado inclui:

- timestamp;
- delta em ms;
- origem;
- destino;
- Raw CAN ID, quando disponivel;
- PGN;
- dados;
- tipo de evento;
- `NameHex` e `ClaimedSA` para `ADDRESS_CLAIM`;
- repeticoes;
- intervalo medio quando aplicavel;
- notas.

## Validacao

Smoke test em memoria executou:

```text
J1939TemporalCaptureService.Start()
20 registros identicos de PGN 0x00DA00
J1939TemporalCaptureService.Stop()
J1939CaptureExportService.ExportToString(session, true)
```

Resultado:

- `Events=2`
- `TotalFrames=20`
- `UniqueFrames=1`
- `HasPeriodic=True`

Smoke test adicional de Address Claim:

```text
SA=0xC0
PGN=0x00EE00
Dados=00 00 C0 0B 00 13 0E 20
RawCanId=0x18EEFFC0
```

Resultado:

- resumo `Address Claims detectados` gerado;
- `Evento=ADDRESS_CLAIM`;
- `NameHex=200E13000BC00000`;
- `ClaimedSA=0xC0`;
- `RawCanId=0x18EEFFC0`.

## Limitacoes

- Validacao de arquivo via `SaveFileDialog` depende de interacao manual em desktop e nao foi executada automaticamente neste ambiente.
- Exportacao fisica foi validada indiretamente pelo build e pela existencia do metodo `ExportToFile`; o smoke test automatizado validou o texto gerado em memoria.

## Rollback

Rollback preservado: exportador e independente do servico de transporte e do firmware.
