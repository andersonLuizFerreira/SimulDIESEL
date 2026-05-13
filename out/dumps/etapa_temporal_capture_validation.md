# ETAPA - Validacao da captura temporal J1939

Data: 2026-05-13

## Validacoes executadas

### Build C# completo

Primeira tentativa em sandbox:

```text
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-temporal-capture/
```

Resultado:

- falhou por bloqueio de ambiente;
- erro: acesso negado a `C:\Users\Escritório\AppData\Local\Microsoft SDKs`.

Segunda tentativa com permissao elevada:

```text
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-temporal-capture/
```

Resultado:

- compilacao com exito;
- 0 avisos;
- 0 erros.

### Smoke test de reducao/exportacao

Executado com os arquivos-fonte dos novos servicos compilados em memoria via PowerShell `Add-Type -Path`.

Entrada simulada:

- 20 frames identicos;
- SA `0xF9`;
- destino `GLOBAL`;
- PGN `0x00DA00`;
- dados `01 3E FF FF FF FF FF FF`;
- intervalo de 100 ms entre registros.

Resultado:

```text
Events=2
TotalFrames=20
UniqueFrames=1
NEW_FRAME:1:; PERIODIC_TICK:20:100
HasPeriodic=True
```

### Smoke test de Address Claim

Executado com os arquivos-fonte dos servicos de captura/exportacao compilados em memoria via PowerShell `Add-Type -Path`.

Entrada simulada:

- SA `0xC0`;
- destino `GLOBAL`;
- PGN `0x00EE00`;
- Raw CAN ID `0x18EEFFC0`;
- dados `00 00 C0 0B 00 13 0E 20`.

Resultado observado no texto exportado:

```text
# Address Claims detectados
- 2026-05-13 16:16:50.778 SA=0xC0 NAME=200E13000BC00000 RawCanId=0x18EEFFC0
RawCanId: 0x18EEFFC0
PGN: 0x00EE00
Evento: ADDRESS_CLAIM
NameHex: 200E13000BC00000
ClaimedSA: 0xC0
```

### Build C# apos enriquecimento

Primeira tentativa em sandbox:

```text
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-temporal-capture-enrichment/
```

Resultado:

- falhou por bloqueio de ambiente;
- erro: acesso negado a `C:\Users\Escritório\AppData\Local\Microsoft SDKs`.

Segunda tentativa com permissao elevada:

```text
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-temporal-capture-enrichment/
```

Resultado:

- compilacao com exito;
- 0 avisos;
- 0 erros.

## Validacoes nao executadas

- Teste real com a EST.
- Abrir tela `Status do Controlador` da EST.
- Mudar para tela `Monitor`.
- Confirmar em bancada que PGN `0x00DA00` aparece durante a operacao real.
- Confirmar visualmente abertura do `SaveFileDialog`.
- Medir responsividade com trafego CAN/J1939 continuo real.

Motivo: este ambiente nao possui a sessao fisica/interativa com a service tool e barramento real.

## Resultado de aceite tecnico

Atendido por build/smoke:

- servico temporal criado;
- reducao de spam implementada;
- exportacao TXT/MD implementada;
- integracao compilada na aba `Dados J1939`;
- `.csproj` sincronizado;
- SDGW, SDCTP, firmware e parser J1939 preservados.

Pendente de bancada:

- confirmacao operacional com EST;
- SaveFileDialog observado manualmente;
- responsividade sob trafego continuo real.

## Rollback

Rollback preservado: nenhum comando destrutivo foi executado, nenhum commit/tag/branch foi criado.
