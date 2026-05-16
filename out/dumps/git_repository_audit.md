# Auditoria de versionamento Git - SimulDIESEL

- Data da auditoria: 2026-05-16
- Escopo: auditoria e levantamento de arquivos versionados, rastreados, ignorados, untracked e artefatos no workspace.
- Restricao aplicada: nenhuma limpeza, nenhum commit, nenhum ajuste de `.gitignore`, nenhum build funcional, nenhum `git gc`.
- Arquivo gerado: `out/dumps/git_repository_audit.md`.

## Bootstrap obrigatório lido

- `AGENTS.md`: lido.
- `.codex/instructions.md`: lido.
- `.codex/skills/build-validation/SKILL.md`: lido.
- `.codex/skills/simuldiesel-architecture/SKILL.md`: lido.

## Estado Git atual

```text
## feature/j1939-reference-catalogs
 M Data/Modules/modules.db
?? local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx
?? "tests/Leitura de alarmes/3 - somente motor.md"
```

| Métrica | Quantidade | Observação |
|---|---:|---|
| Arquivos rastreados | 704 | git ls-files |
| Arquivos modificados unstaged | 1 | git diff --name-only |
| Arquivos staged | 0 | git diff --cached --name-only |
| Arquivos untracked | 2 | git ls-files --others --exclude-standard |
| Arquivos ignorados detectados | 410 | git ls-files --others --ignored --exclude-standard |
| Arquivos no workspace sem .git | 1116 | varredura filesystem |

### Banco de objetos Git
```text
count: 1794
size: 1.13 MiB
in-pack: 20614
packs: 1
size-pack: 274.42 MiB
prune-packable: 0
garbage: 0
size-garbage: 0 bytes
```

Observacao: o pack Git atual tem 274,42 MiB. Esta ETAPA nao reescreveu historico nem executou `git gc`; portanto a auditoria identifica riscos do estado atual e nao faz remocao de blobs historicos.

## Achados executivos

- `Data/Modules/modules.db` esta rastreado e modificado. Classificacao: RISCO ALTO, BINARIO MUTAVEL, AVALIAR. Motivo: banco operacional mutavel tende a gerar conflitos e crescimento historico.
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite` esta rastreado. Classificacao: RISCO ALTO, BINARIO MUTAVEL, GERADO AUTOMATICAMENTE. Motivo: cache de IDE SQLite dentro de `artifacts/ide/visual-studio-cache`.
- Existem 410 arquivos ignorados ocupando aproximadamente 189,48 MiB no workspace auditado por somatorio dos grupos retornados; predominam builds PlatformIO, builds C# e saidas em `out/build-*`.
- `out/dumps/**` esta explicitamente permitido pelo `.gitignore`; os dumps técnicos versionados sao coerentes com a governanca atual, mas têm risco de crescimento futuro.
- Dois arquivos untracked foram encontrados: um `.resx` de UI e uma captura/teste em Markdown grande. Ambos exigem avaliacao antes de qualquer commit futuro.
- O `.gitignore` cobre bem builds, caches e temporarios, mas ha lacunas de governanca para bancos mutaveis versionados intencionalmente, `artifacts/` de firmware e capturas grandes em `tests/`.

## Arquivos modificados, staged e untracked

### Modificados unstaged
```text
M	Data/Modules/modules.db
```

### Staged
```text
(nenhum)
```

### Untracked detectados
| Tamanho | Arquivo |
|---:|---|
| 601,37 KB | tests/Leitura de alarmes/3 - somente motor.md |
| 5,68 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx |

Classificacao dos untracked:

- `local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx`: AVALIAR. Arquivo de recurso WinForms; pode ser fonte de UI se integrado ao `.csproj`, mas esta ETAPA nao valida inclusao funcional.
- `tests/Leitura de alarmes/3 - somente motor.md`: AVALIAR / RISCO DE CRESCIMENTO. Captura ou registro tecnico em Markdown com 601,37 KiB; pode ser evidência operacional, mas precisa regra de retencao.

## Tamanhos e consumidores de espaco

### Diretorios raiz no workspace
| Grupo | Arquivos | Tamanho aproximado |
|---|---:|---:|
| out | 156 | 121,95 MB |
| hardware | 402 | 57,45 MB |
| local-api | 294 | 16,43 MB |
| tests | 14 | 925,53 KB |
| Data | 17 | 422,03 KB |
| docs | 160 | 389,54 KB |
| tools | 17 | 85,41 KB |
| .codex | 15 | 33,96 KB |
| AGENTS.md | 1 | 7,69 KB |
| path.bat | 1 | 5,79 KB |
| .gitignore | 1 | 3,54 KB |
| .github | 1 | 654 B |
| check_docs.bat | 1 | 623 B |
| .gitattributes | 1 | 579 B |
| specs | 12 | 156 B |
| .gitlab-ci.yml | 1 | 137 B |
| cloud | 13 | 118 B |
| VERSIONING.md | 1 | 18 B |
| .editorconfig | 1 | 17 B |
| infra | 7 | 0 B |

### Diretorios raiz rastreados
| Grupo | Arquivos | Tamanho aproximado |
|---|---:|---:|
| local-api | 257 | 3,17 MB |
| hardware | 107 | 647,17 KB |
| Data | 17 | 422,03 KB |
| docs | 160 | 389,54 KB |
| tests | 13 | 324,16 KB |
| out | 44 | 323,66 KB |
| tools | 17 | 85,41 KB |
| .codex | 15 | 33,96 KB |
| AGENTS.md | 1 | 7,69 KB |
| path.bat | 1 | 5,79 KB |
| .gitignore | 1 | 3,54 KB |
| .github | 1 | 654 B |
| check_docs.bat | 1 | 623 B |
| .gitattributes | 1 | 579 B |
| .gitlab-ci.yml | 1 | 137 B |
| cloud | 13 | 118 B |
| specs | 8 | 78 B |
| VERSIONING.md | 1 | 18 B |
| .editorconfig | 1 | 17 B |
| "local-api | 1 | 0 B |
| "hardware | 36 | 0 B |
| infra | 7 | 0 B |

### Diretorios raiz ignorados
| Grupo | Arquivos | Tamanho aproximado |
|---|---:|---:|
| out | 112 | 121,63 MB |
| hardware | 259 | 54,81 MB |
| local-api | 35 | 13,04 MB |
| specs | 4 | 78 B |

### Tipos de arquivo rastreados por extensao
| Grupo | Arquivos | Tamanho aproximado |
|---|---:|---:|
| .cs | 208 | 1,04 MB |
| .md | 235 | 1,03 MB |
| .png | 19 | 976,07 KB |
| .jpg | 6 | 623,63 KB |
| .resx | 5 | 353,13 KB |
| .db | 1 | 340,00 KB |
| .sqlite | 1 | 272,00 KB |
| .ico | 1 | 202,06 KB |
| .cpp | 25 | 154,11 KB |
| .kicad_sch | 4 | 126,09 KB |
| .py | 10 | 80,40 KB |
| .json | 9 | 66,49 KB |
| .h | 40 | 55,18 KB |
| .sql | 4 | 31,04 KB |
| .csproj | 1 | 21,67 KB |
| .kicad_pro | 3 | 18,91 KB |
| .fcstd | 1 | 8,10 KB |
| .dxf | 1 | 7,38 KB |
| .mjs | 2 | 6,59 KB |
| .bat | 2 | 6,39 KB |
| .vcxproj | 1 | 4,28 KB |
| .sln | 2 | 4,17 KB |
| .txt | 4 | 3,99 KB |
| .gitignore | 4 | 3,83 KB |
| .ino | 5 | 3,71 KB |
| [sem extensao] | 3 | 2,86 KB |
| .filters | 1 | 1,02 KB |
| .ini | 3 | 855 B |
| .yml | 2 | 791 B |
| .gitattributes | 1 | 579 B |
| .settings | 1 | 249 B |
| .kicad_pcb | 3 | 237 B |
| .config | 1 | 189 B |
| .code-workspace | 1 | 105 B |
| .yaml | 1 | 83 B |
| .editorconfig | 1 | 17 B |
| [ausente no workspace] | 37 | 0 B |
| .gitkeep | 55 | 0 B |

### 50 maiores arquivos rastreados
| Tamanho | Arquivo |
|---:|---|
| 868,19 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Rede_can_ico.png |
| 602,66 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Simbolo ANDERTECH.jpg |
| 340,00 KB | Data/Modules/modules.db |
| 323,86 KB | local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.resx |
| 272,00 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite |
| 202,06 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/ANDERTECH.ico |
| 192,91 KB | tests/Leitura de alarmes/2 - CXCM LIGADA.md |
| 79,37 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs |
| 75,88 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/turn on.png |
| 65,90 KB | hardware/boards/GSA -gerador-sinais-analogicos/driver_5v.kicad_sch |
| 58,93 KB | tests/captura-j1939-20260513-160725.md |
| 56,52 KB | tests/captura-j1939-20260513-161658.md |
| 45,14 KB | hardware/boards/SimulDIESEL/legacy-comunicacao/kicad/comunicacao.kicad_sch |
| 44,13 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaControls.cs |
| 40,98 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdVerticalGauge.cs |
| 39,05 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaChannelControl.cs |
| 37,25 KB | out/dumps/sdh_sdctp_architecture_current_state.md |
| 32,65 KB | hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp |
| 32,28 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.Designer.cs |
| 29,77 KB | out/dumps/sdh_contract_export/sdh_contract_export.json |
| 29,50 KB | local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhValidator.cs |
| 28,61 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaChannelControl.Designer.cs |
| 25,26 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs |
| 24,85 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs |
| 23,00 KB | hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver_fake.cpp |
| 22,51 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs |
| 22,48 KB | local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs |
| 21,67 KB | local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj |
| 21,33 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdVerticalSlider.cs |
| 19,54 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs |
| 19,20 KB | out/dumps/etapa_topologia_api_j1939_catalog_consumption.md |
| 17,55 KB | hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver.cpp |
| 17,36 KB | out/dumps/bd_persistence_candidates.md |
| 17,00 KB | local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwLinkEngine.cs |
| 16,14 KB | local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Bluetooth/BluetoothDeviceCatalog.cs |
| 16,03 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaParsers.cs |
| 15,54 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaClient.cs |
| 15,16 KB | out/dumps/module_database_model/module_schema_dump_v1.md |
| 14,81 KB | hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_sch |
| 14,57 KB | local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.Designer.cs |
| 14,55 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/SdgwHostSession.cs |
| 14,26 KB | tools/testes/can_tx_loopback_validation.py |
| 13,98 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs |
| 13,25 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/frmGSA_UI.cs |
| 13,10 KB | out/dumps/sdh_protocol_inventory/sdh_command_catalog.md |
| 12,93 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939TemporalCaptureService.cs |
| 12,76 KB | local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanEventProcessor.cs |
| 12,32 KB | Data/Modules/schema/postgres_schema_v1.sql |
| 11,86 KB | local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.cs |
| 11,75 KB | local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/UceCanProtocol.cs |

### 50 maiores arquivos ignorados
| Tamanho | Arquivo |
|---:|---|
| 17,46 MB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/firmware.elf |
| 14,40 MB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/firmware.map |
| 3,18 MB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libFrameworkArduino.a |
| 1,91 MB | out/build-etapa-rede-can-03/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-main-pre-j1939/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-bd-provider/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-temporal-capture-enrichment/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-rede-can-04/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-j1939-reference-catalogs/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-rede-can-02/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-temporal-capture/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-j1939-diagnostics-dedupe/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-j1939-seed-pre/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-topologia-j1939-catalog/x64/SQLite.Interop.dll |
| 1,91 MB | local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-j1939-seed/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-bd-local/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-rede-can-refresh-fix/x64/SQLite.Interop.dll |
| 1,91 MB | out/build-etapa-rede-can-01/x64/SQLite.Interop.dll |
| 1,81 MB | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.exe |
| 1,81 MB | out/build-etapa-temporal-capture-enrichment/SimulDIESEL.exe |
| 1,81 MB | local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/SimulDIESEL.exe |
| 1,80 MB | out/build-etapa-temporal-capture/SimulDIESEL.exe |
| 1,78 MB | out/build-j1939-diagnostics-dedupe/SimulDIESEL.exe |
| 1,78 MB | out/build-rede-can-refresh-fix/SimulDIESEL.exe |
| 1,78 MB | out/build-etapa-rede-can-04/SimulDIESEL.exe |
| 1,77 MB | out/build-etapa-rede-can-03/SimulDIESEL.exe |
| 1,77 MB | out/build-etapa-rede-can-02/SimulDIESEL.exe |
| 1,77 MB | out/build-etapa-rede-can-01/SimulDIESEL.exe |
| 1,75 MB | out/build-etapa-j1939-seed/SimulDIESEL.exe |
| 1,75 MB | out/build-etapa-topologia-j1939-catalog/SimulDIESEL.exe |
| 1,74 MB | out/build-main-pre-j1939/SimulDIESEL.exe |
| 1,74 MB | out/build-etapa-j1939-seed-pre/SimulDIESEL.exe |
| 1,74 MB | out/build-etapa-j1939-reference-catalogs/SimulDIESEL.exe |
| 1,73 MB | out/build-bd-provider/SimulDIESEL.exe |
| 1,73 MB | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.pdb |
| 1,73 MB | out/build-etapa-temporal-capture-enrichment/SimulDIESEL.pdb |
| 1,73 MB | local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/SimulDIESEL.pdb |
| 1,73 MB | out/build-bd-local/SimulDIESEL.exe |
| 1,72 MB | out/build-etapa-temporal-capture/SimulDIESEL.pdb |
| 1,69 MB | hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/.sconsign311.dblite |
| 1,69 MB | hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/.sconsign313.dblite |
| 1,69 MB | hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/.sconsign311.dblite |
| 1,69 MB | hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/.sconsign313.dblite |
| 1,67 MB | out/build-j1939-diagnostics-dedupe/SimulDIESEL.pdb |
| 1,66 MB | out/build-rede-can-refresh-fix/SimulDIESEL.pdb |
| 1,65 MB | out/build-etapa-rede-can-04/SimulDIESEL.pdb |
| 1,64 MB | out/build-etapa-rede-can-03/SimulDIESEL.pdb |
| 1,62 MB | out/build-etapa-rede-can-02/SimulDIESEL.pdb |
| 1,61 MB | out/build-etapa-rede-can-01/SimulDIESEL.pdb |

## Classificacao por diretorio importante

| Diretorio/padrao | Estado observado | Classificacao | Diagnostico |
|---|---|---|---|
| Data | all=17, tracked=17, ignored=0, untracked=0, size=422,03 KB | AVALIAR / RISCO ALTO | Contem schema/catalogos versionaveis e `Data/Modules/modules.db` mutavel rastreado e modificado. |
| out | all=156, tracked=44, ignored=112, untracked=0, size=121,95 MB | AVALIAR / RISCO DE CRESCIMENTO | `out/dumps/**` versionado por excecao; `out/build-*` ignorado e volumoso. |
| tests | all=14, tracked=13, ignored=0, untracked=1, size=925,53 KB | AVALIAR / RISCO DE CRESCIMENTO | Testes e capturas Markdown rastreados; novo arquivo grande untracked exige regra de retencao. |
| docs | all=160, tracked=160, ignored=0, untracked=0, size=389,54 KB | DEVE VERSIONAR | Documentacao oficial, agentes, arquitetura e legado; baixo tamanho atual. |
| local-api | all=294, tracked=257, ignored=35, untracked=1, size=16,43 MB | DEVE VERSIONAR + GERADO IGNORADO | Fonte C# rastreado; `.vs`, `bin` e `obj` ignorados corretamente. |
| .codex | all=15, tracked=15, ignored=0, untracked=0, size=33,96 KB | DEVE VERSIONAR | Bootstrap e skills do projeto rastreados; baixo risco. |
| .vs | all=0, tracked=0, ignored=0, untracked=0, size=0 B | NAO DEVE VERSIONAR | Artefato IDE; nao apareceu como raiz, mas existe ignorado em `local-api/src/SimulDIESEL/.vs`. |
| bin | all=0, tracked=0, ignored=0, untracked=0, size=0 B | NAO DEVE VERSIONAR | Padrao ignorado; ocorrencias estao dentro de `local-api` e `out`. |
| obj | all=0, tracked=0, ignored=0, untracked=0, size=0 B | NAO DEVE VERSIONAR | Padrao ignorado; ocorrencias estao dentro de `local-api`. |
| node_modules | all=0, tracked=0, ignored=0, untracked=0, size=0 B | NAO DEVE VERSIONAR | Padrao ignorado; nenhuma ocorrencia relevante detectada. |
| __pycache__ | all=0, tracked=0, ignored=0, untracked=0, size=0 B | NAO DEVE VERSIONAR | Padrao ignorado; nenhuma ocorrencia relevante detectada. |
| hardware | all=402, tracked=107, ignored=259, untracked=0, size=57,45 MB | DEVE VERSIONAR + AVALIAR artifacts | Firmware, KiCad e mecanica rastreados; `.pio` ignorado; `artifacts/ide` rastreado exige revisao futura. |
| tools | all=17, tracked=17, ignored=0, untracked=0, size=85,41 KB | DEVE VERSIONAR | Scripts de validacao e utilitarios rastreados; outputs de tools ignorados. |
| specs | all=12, tracked=8, ignored=4, untracked=0, size=156 B | DEVE VERSIONAR / AVALIAR exemplos binarios | Specs quase vazias rastreadas; `.hex` ignorados em examples aparecem como ignorados. |
| cloud | all=13, tracked=13, ignored=0, untracked=0, size=118 B | DEVE VERSIONAR | Estrutura futura com `.gitkeep`, contratos e migrations placeholders. |
| infra | all=7, tracked=7, ignored=0, untracked=0, size=0 B | AVALIAR | Diretorio presente no workspace com arquivos vazios nao rastreados/ignorados detectados pela varredura; pendente de confirmacao. |

## Bancos de dados rastreados
### Bancos rastreados
| Tamanho | Arquivo |
|---:|---|
| 340,00 KB | Data/Modules/modules.db |
| 272,00 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite |

- `Data/Modules/modules.db`: banco operacional/local do Banco de Modulos. Por estar modificado no working tree, representa risco concreto de conflito Git e acoplamento entre runtime e source control.
- `hardware/.../slnx.sqlite`: cache SQLite de Visual Studio dentro de artifacts de firmware. Deve ser tratado futuramente como artefato de IDE, nao como contrato tecnico, salvo justificativa documental especifica.

## Binarios e arquivos perigosos rastreados
### Binarios/imagens/bancos rastreados
| Tamanho | Arquivo |
|---:|---|
| 868,19 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Rede_can_ico.png |
| 602,66 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Simbolo ANDERTECH.jpg |
| 340,00 KB | Data/Modules/modules.db |
| 272,00 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite |
| 202,06 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/ANDERTECH.ico |
| 75,88 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/turn on.png |
| 10,60 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Conectado.png |
| 9,41 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Desconectado.png |
| 8,10 KB | hardware/boards/mechanical/gsa-board-outline.FCStd |
| 7,38 KB | hardware/boards/mechanical/gsa-board-outline.dxf |
| 5,37 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7_P1.jpg |
| 5,12 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/MR_P.jpg |
| 4,59 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/MR.jpg |
| 4,38 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7.jpg |
| 1,52 KB | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7_P.jpg |
| 881 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowLight_18x18.png |
| 874 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenBright_18x18.png |
| 873 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowBright_18x18.png |
| 862 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedBright_18x18.png |
| 862 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowDark_18x18.png |
| 851 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenLight_18x18.png |
| 843 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedLight_18x18.png |
| 822 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenDark_18x18.png |
| 805 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/VERDE OFF.png |
| 790 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedDark_18x18.png |
| 780 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/vermelho ON.png |
| 778 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/VERDE ON.png |
| 776 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/vermelho OFF.png |
| 767 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Rede_can_toolbar_ico.png |
| 709 B | local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGrayOff_18x18.png |

Observacao: imagens de UI (`.png`, `.jpg`, `.ico`) podem ser DEVE VERSIONAR quando forem recursos da aplicacao. O risco maior nao e a existencia de binarios em si, mas binarios mutaveis/runtime como bancos, caches e artefatos de IDE.

## Artefatos runtime atualmente versionados
### Candidatos a runtime/cache/build ja rastreados
| Tamanho | Arquivo |
|---:|---|
| 340,00 KB | Data/Modules/modules.db |
| 272,00 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite |
| 4,28 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.vcxproj |
| 2,71 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.sln |
| 1,59 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/Teste_Hand_Shake/Teste_Hand_Shake.ino |
| 1,02 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.vcxproj.filters |
| 844 B | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/src/arduino folders read me.txt |
| 258 B | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.ino |
| 233 B | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/TesteEnvio/TesteEnvio.ino |
| 105 B | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/workspaces/esp32-api-bridge.code-workspace |
| 55 B | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/ProjectSettings.json |
| 0 B | "hardware/firmware/GSA - Gerador de sinais anal/303/263gicos/artifacts/workspaces/gerador-sinais-analogicos-gsa.code-workspace" |

Classificacao geral: AVALIAR, com RISCO ALTO para bancos/caches mutaveis. As regras atuais do `.gitignore` nao removem arquivos ja rastreados; qualquer correcao futura deve ser ETAPA separada e autorizada.

## Logs, temporarios, dumps e capturas no workspace
### Logs/temporarios/caches/dumps/capturas detectados por extensao/pasta
| Tamanho | Arquivo |
|---:|---|
| 21,05 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/tmpleuuoa2z.tmp |
| 21,05 KB | hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/tmpxt2suwc1.tmp |
| 8,52 KB | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.AssemblyReference.cache |
| 6,88 KB | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/DesignTimeResolveAssemblyReferencesInput.cache |
| 999 B | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.GenerateResource.cache |
| 542 B | local-api/src/SimulDIESEL/SimulDIESEL/obj/project.nuget.cache |
| 139 B | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/DesignTimeResolveAssemblyReferences.cache |
| 66 B | local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.CoreCompileInputs.cache |

- Dumps tecnicos em `out/dumps/**`: DEVE VERSIONAR segundo regra atual, mas com governanca de tamanho e retencao.
- Builds em `out/build-*`: NAO DEVE VERSIONAR, GERADO AUTOMATICAMENTE, ignorado corretamente.
- Capturas em `tests/*.md` e `tests/Leitura de alarmes/*.md`: AVALIAR; podem ser evidencias tecnicas, mas tendem a crescimento.

## Estado do `.gitignore`

Arquivos `.gitignore` encontrados:
```text
.gitignore
tools/GSA_Teste/.gitignore
hardware/firmware/GSA - Gerador de sinais analógicos/.gitignore
hardware/firmware/UCE - Unidade de comunicacao externa/.gitignore
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.gitignore
```

Coberturas observadas:

- Windows/sistema: `Thumbs.db`, `Desktop.ini`, `$RECYCLE.BIN/`.
- Logs/temporarios: `*.log`, `*.tmp`, `*.temp`, swaps.
- Visual Studio/.NET: `.vs/`, `bin/`, `obj/`, `*.user`, `*.suo`, `*.cache`, `*.pdb`, `*.dll`, `*.exe`.
- VS Code: `.vscode/`, `.history/`.
- PlatformIO/Arduino: `.pio/`, `.pioenvs/`, `.piolibdeps/`, `*.elf`, `*.hex`, `*.bin`, `*.map`.
- Python: `__pycache__/`, bytecode, `.venv/`, `env/`.
- Node: `node_modules/`, `dist/`, logs npm/yarn.
- KiCad/FreeCAD/fabricacao: caches, backups, locks, gerbers, drill, bom, pickplace, zip.
- `out/*` ignorado com excecao de `out/dumps/**`.

Lacunas e pontos para ETAPA futura:

- Definir politica explicita para `Data/Modules/modules.db`: versionar seed congelado, mover runtime mutavel para local ignored, ou gerar a partir de migrations/catalogos.
- Definir politica para `hardware/**/artifacts/`: separar evidencias manuais versionaveis de caches de IDE/runtime.
- Avaliar ignorar/limitar capturas grandes em `tests/` ou mover capturas operacionais para `out/dumps/` com indice e retencao.
- Avaliar padroes para `*.sqlite`, `*.sqlite3`, `*.db-journal`, `*.db-wal`, `*.db-shm`, se bancos runtime forem usados localmente.
- Avaliar `*.ps1` em minusculo alem de `*.PS1`, dependendo da politica desejada e da sensibilidade a case do Git no ambiente.
- Avaliar padroes de logs/capturas CAN futuros como `*.asc`, `*.blf`, `*.trc`, `*.candump`, `*.pcapng`, alem de `*.pcap`.

## Relatorio de riscos

| Risco | Severidade | Evidencia | Impacto provável |
|---|---|---|---|
| Banco mutavel rastreado | RISCO ALTO | `Data/Modules/modules.db` modificado | Conflitos frequentes, historico binario crescente, ambiente local acoplado ao Git. |
| Cache SQLite de IDE rastreado | RISCO ALTO | `hardware/.../slnx.sqlite` | Mudancas locais nao semanticas e blobs binarios no historico. |
| Capturas/testes grandes em Markdown | RISCO DE CRESCIMENTO | `tests/Leitura de alarmes/3 - somente motor.md` untracked com 601,37 KiB; outros rastreados acima de 50 KiB | Crescimento gradual e revisoes dificeis. |
| Dumps versionados sem limite formal de retencao | RISCO DE CRESCIMENTO | 44 arquivos rastreados em `out`, maioria dumps | Repositorio pode crescer se cada ETAPA acumular dumps longos/binarios. |
| Builds volumosos no workspace | GERADO AUTOMATICAMENTE | `out/build-*`, `.pio`, `bin`, `obj` ignorados | Baixo risco Git imediato por estarem ignorados, mas alto consumo local. |
| Recursos binarios de UI | AVALIAR | imagens `.png/.jpg/.ico` rastreadas | Aceitavel quando fonte da aplicacao; risco se forem capturas temporarias. |

## Relatorio de crescimento futuro

- Maior crescimento local atual vem de `out/` (121,95 MiB), `hardware/` (57,45 MiB) e `local-api/` (16,43 MiB), quase todo ignorado como build/cache.
- O pack Git tem 274,42 MiB, maior que o tamanho rastreado atual no working tree; isto sugere historico com blobs maiores ou acumulados. Confirmacao detalhada exigiria auditoria de historico em ETAPA propria.
- Crescimento perigoso futuro: bancos SQLite, dumps acumulados, capturas CAN/J1939, imagens grandes e artefatos de IDE/firmware.
- `out/dumps/**` e uma excecao consciente: deve permanecer textual, auditavel e com nomes de ETAPA; evitar binarios nesta arvore.

## Sugestoes futuras de governanca Git

Estas sugestoes nao foram implementadas nesta ETAPA.

- Criar ETAPA especifica para decidir a politica do Banco de Modulos: migrations e seeds versionados; banco runtime local ignorado; snapshot versionado apenas quando congelado.
- Criar regra documental para `tests/`: diferenciar teste automatizado versionavel, evidencia manual versionavel e captura operacional mutavel.
- Criar limite recomendado de tamanho por dump Markdown e criterio de arquivamento para capturas longas.
- Revisar `hardware/**/artifacts/` e mover caches de IDE para ignorado sem apagar conteudo nesta auditoria.
- Adicionar validacao de pre-commit ou checklist para detectar `.db`, `.sqlite`, `.pdb`, `.exe`, `.dll`, `.elf`, `.bin`, `.map`, `.vs`, `.pio`, `bin`, `obj` antes de commits.
- Manter `out/dumps/**` somente para relatorios textuais; builds devem continuar em `out/build-*` ignorado.
- Se for necessario auditar historico, executar ETAPA separada com `git rev-list --objects --all` para maiores blobs historicos, sem reescrever historico sem autorizacao.

## Validacao desta ETAPA

- Validacao documental: relatorio gerado em `out/dumps/git_repository_audit.md`.
- Build funcional: nao aplicavel; ETAPA exclusivamente de auditoria Git/filesystem sem alteracao de codigo.
- Comandos destrutivos: nao executados.
- `.gitignore`: lido, nao modificado.
- Commit/branch/tag: nao realizados.
- Rollback: preservado; unica escrita autorizada foi o dump de auditoria.

## Apêndice A - Inventario completo dos arquivos rastreados
```text
.codex/instructions.md
.codex/skills/README.md
.codex/skills/bll-dal-dtl/SKILL.md
.codex/skills/build-validation/SKILL.md
.codex/skills/dump-generation/SKILL.md
.codex/skills/firmware-bpm/SKILL.md
.codex/skills/firmware-uce/SKILL.md
.codex/skills/git-checkpoint/SKILL.md
.codex/skills/j1939-decode/SKILL.md
.codex/skills/module-database/SKILL.md
.codex/skills/sdctp-contract/SKILL.md
.codex/skills/sdgw-transport/SKILL.md
.codex/skills/sdh-contract/SKILL.md
.codex/skills/simuldiesel-architecture/SKILL.md
.codex/skills/winforms-ui/SKILL.md
.editorconfig
.gitattributes
.github/workflows/ci.yml
.gitignore
.gitlab-ci.yml
AGENTS.md
Data/AGENTS.md
Data/Modules/docs/bancada.md
Data/Modules/docs/local_api_database_runtime.md
Data/Modules/docs/module_database_model_v1.md
Data/Modules/docs/module_database_sdh_relation.md
Data/Modules/modules.db
Data/Modules/schema/migrations/0002_sync_metadata.sql
Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql
Data/Modules/schema/postgres_schema_v1.sql
Data/Modules/schema/sqlite_schema_v1.sql
Data/Protocols/J1939/catalogs/j1939_functions.json
Data/Protocols/J1939/catalogs/j1939_industry_groups.json
Data/Protocols/J1939/catalogs/j1939_manufacturers.json
Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json
Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json
Data/Protocols/J1939/j1939-71-mini-catalog.json
Data/Protocols/J1939/j1939-pgn-standard-catalog.json
VERSIONING.md
check_docs.bat
cloud/.gitkeep
cloud/README.md
cloud/api-contracts/openapi.yaml
cloud/api-contracts/schemas/.gitkeep
cloud/database/migrations/.gitkeep
cloud/database/schemas/.gitkeep
cloud/database/seed/.gitkeep
cloud/deploy/docker/.gitkeep
cloud/src/SimulDiesel.Cloud/Api/.gitkeep
cloud/src/SimulDiesel.Cloud/Application/.gitkeep
cloud/src/SimulDiesel.Cloud/Contracts/.gitkeep
cloud/src/SimulDiesel.Cloud/Domain/.gitkeep
cloud/src/SimulDiesel.Cloud/Infrastructure/.gitkeep
docs/.gitkeep
docs/00-INDICE.md
docs/DOCUMENTATION_RULES.md
docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md
docs/README.md
docs/agents/agents_overview.md
docs/agents/etapa_prompt_template.md
docs/agents/freeze_checkpoint_template.md
docs/agents/project_conventions.md
docs/agents/skills/bll-dal-dtl-skill.md
docs/agents/skills/build-validation-skill.md
docs/agents/skills/dump-generation-skill.md
docs/agents/skills/firmware-bpm-skill.md
docs/agents/skills/firmware-uce-skill.md
docs/agents/skills/git-checkpoint-skill.md
docs/agents/skills/j1939-decode-skill.md
docs/agents/skills/module-database-skill.md
docs/agents/skills/sdctp-contract-skill.md
docs/agents/skills/sdgw-transport-skill.md
docs/agents/skills/sdh-contract-skill.md
docs/agents/skills/simuldiesel-architecture-skill.md
docs/agents/skills/winforms-ui-skill.md
docs/agents/validation_checklist.md
docs/architecture/sdh_sdctp_sdgw_contracts.md
docs/legacy/00-INDICE-LEGACY.md
docs/legacy/00_visao-geral/MASTER_SPEC.md
docs/legacy/00_visao-geral/README.md
docs/legacy/01_arquitetura/00_contratos/CONTRATO_CENTRAL.md
docs/legacy/01_arquitetura/00_contratos/CONTRATO_GATEWAY.md
docs/legacy/01_arquitetura/00_contratos/CONTRATO_GSA.md
docs/legacy/01_arquitetura/00_contratos/README.md
docs/legacy/01_arquitetura/01_protocolos/README.md
docs/legacy/01_arquitetura/01_protocolos/sggw/README.md
docs/legacy/01_arquitetura/01_protocolos/sggw/examples/README.md
docs/legacy/01_arquitetura/01_protocolos/sggw/interface.pt-BR.md
docs/legacy/01_arquitetura/01_protocolos/sggw/spec.pt-BR.md
docs/legacy/01_arquitetura/02_integracoes/README.md
docs/legacy/01_arquitetura/02_integracoes/esp32_to_due.md
docs/legacy/01_arquitetura/02_integracoes/esp32_to_mega.md
docs/legacy/01_arquitetura/02_integracoes/local-api_to_esp32.md
docs/legacy/01_arquitetura/03_gateway/DEVICE_TABLE.md
docs/legacy/01_arquitetura/03_gateway/I2C_TLV_CRC.md
docs/legacy/01_arquitetura/03_gateway/README.md
docs/legacy/01_arquitetura/03_gateway/ROUTER.md
docs/legacy/01_arquitetura/04_pc/README.md
docs/legacy/01_arquitetura/04_pc/architecture-overview.md
docs/legacy/01_arquitetura/04_pc/health-service.md
docs/legacy/01_arquitetura/04_pc/link-engine.md
docs/legacy/01_arquitetura/04_pc/link-handshake.md
docs/legacy/01_arquitetura/04_pc/serial-connection.md
docs/legacy/01_arquitetura/04_pc/serial-transport.md
docs/legacy/01_arquitetura/04_pc/sggw-client.md
docs/legacy/01_arquitetura/05_babyboards/README.md
docs/legacy/01_arquitetura/05_babyboards/gsa/ARQUITETURA.md
docs/legacy/01_arquitetura/05_babyboards/gsa/CONFIGURACAO.md
docs/legacy/01_arquitetura/05_babyboards/gsa/ESTRUTURA_INTERNA.md
docs/legacy/01_arquitetura/05_babyboards/gsa/FLUXO_EXECUCAO.md
docs/legacy/01_arquitetura/05_babyboards/gsa/PINOUT.md
docs/legacy/01_arquitetura/05_babyboards/gsa/PROTOCOLO.md
docs/legacy/01_arquitetura/05_babyboards/gsa/README.md
docs/legacy/01_arquitetura/README.md
docs/legacy/04_desenvolvimento/README.md
docs/legacy/04_desenvolvimento/adr/ADR-0007-cobs-crc8.pt-BR.md
docs/legacy/04_desenvolvimento/adr/README.md
docs/legacy/04_desenvolvimento/technical-roadmap.md
docs/legacy/05_hardware/README.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/ARQUITETURA.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/CONFIGURACAO.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/FLUXO_EXECUCAO.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/PINOUT.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/PROTOCOLO.md
docs/legacy/05_hardware/gerador-sinais-analogicos-GSA/README.md
docs/legacy/06_reestruturacao_documental/01-visao-geral/01-introducao.md
docs/legacy/06_reestruturacao_documental/01-visao-geral/02-objetivos.md
docs/legacy/06_reestruturacao_documental/01-visao-geral/03-escopo.md
docs/legacy/_auxiliar/Script Gerador auto indice.txt
docs/legacy/_auxiliar/Script listar todas as pastas.txt
docs/legacy/_auxiliar/_MIGRACAO_BASE.txt
docs/official/01-visao-geral/01-visao-geral-projeto.md
docs/official/02-arquitetura/01-visao-arquitetural.md
docs/official/02-arquitetura/02-camadas-do-sistema.md
docs/official/02-arquitetura/02-visao-fisica.md
docs/official/02-arquitetura/03-fluxo-de-comunicacao.md
docs/official/02-arquitetura/03-visao-logica.md
docs/official/02-arquitetura/04-api-e-host-local.md
docs/official/02-arquitetura/05-bll-do-host.md
docs/official/02-arquitetura/05-bll-do-host/01-formslogic-e-fachadas.md
docs/official/02-arquitetura/05-bll-do-host/02-clients-bpm-e-gsa.md
docs/official/02-arquitetura/06-dal-do-host.md
docs/official/02-arquitetura/06-dal-do-host/01-sessao-sdh-e-sdgw.md
docs/official/02-arquitetura/06-dal-do-host/02-framing-scheduler-e-supervisor.md
docs/official/02-arquitetura/07-dtl-do-host.md
docs/official/02-arquitetura/07-dtl-do-host/01-contratos-sdh-e-dtos.md
docs/official/02-arquitetura/08-transporte-do-host.md
docs/official/02-arquitetura/08-transporte-do-host/01-switchable-transport.md
docs/official/02-arquitetura/09-serial-e-bluetooth.md
docs/official/02-arquitetura/09-serial-e-bluetooth/01-catalogo-e-portas-bluetooth.md
docs/official/02-arquitetura/10-hardware-da-bancada.md
docs/official/02-arquitetura/11-modulo-em-teste-e-xconn.md
docs/official/02-arquitetura/12-banco-local-api.md
docs/official/03-hardware/01-backplane.md
docs/official/03-hardware/02-baby-boards.md
docs/official/03-hardware/03-barramentos.md
docs/official/03-hardware/04-alimentacao.md
docs/official/03-hardware/05-boards-fisicas.md
docs/official/03-hardware/boards/03-gsa/01-funcionamento-eletronico.md
docs/official/03-hardware/boards/03-gsa/README.md
docs/official/04-firmware/01-arquitetura-firmware.md
docs/official/04-firmware/02-drivers.md
docs/official/04-firmware/03-gerenciamento-recursos.md
docs/official/04-firmware/04-sdh-gateway-architecture.md
docs/official/04-firmware/05-catalogo-baby-boards.md
docs/official/04-firmware/06-gateway-binding-logico-fisico.md
docs/official/04-firmware/07-resolver-engine-gateway.md
docs/official/04-firmware/boards/04-gsc.md
docs/official/04-firmware/boards/05-url.md
docs/official/04-firmware/boards/06-slu.md
docs/official/04-firmware/boards/07-uco.md
docs/official/04-firmware/boards/08-ucs.md
docs/official/04-firmware/boards/09-uiod.md
docs/official/04-firmware/boards/10-uhm.md
docs/official/04-firmware/boards/BPM/01-bpm.md
docs/official/04-firmware/boards/GSA/03-gsa.md
docs/official/04-firmware/boards/PSU/02-psu.md
docs/official/04-firmware/boards/README.md
docs/official/04-firmware/boards/UCE/11-uce.md
docs/official/05-software-dashboard/01-arquitetura-software.md
docs/official/05-software-dashboard/02-interface-usuario.md
docs/official/05-software-dashboard/03-camada-hardware.md
docs/official/05-software-dashboard/04-sdh-host-architecture.md
docs/official/05-software-dashboard/04-sdh-host-architecture/01-handshake-e-estados-da-sessao.md
docs/official/05-software-dashboard/04-sdh-host-architecture/02-scheduler-retry-e-supervisao.md
docs/official/05-software-dashboard/04-sdh-host-architecture/03-fluxo-gsa-do-comando-ao-evento.md
docs/official/05-software-dashboard/04-sdh-host-architecture/04-parsing-e-tratamento-de-respostas.md
docs/official/06-protocolos/00-onboarding-comandos.md
docs/official/06-protocolos/01-sdh-command-model.md
docs/official/06-protocolos/02-sdh-response-model.md
docs/official/06-protocolos/03-sdh-examples.md
docs/official/06-protocolos/04-can.md
docs/official/06-protocolos/05-j1939.md
docs/official/06-protocolos/06-gsa-sdh-tlv.md
docs/official/06-protocolos/07-uce-sdh-tlv.md
docs/official/06-protocolos/README.md
docs/official/07-simulacoes/01-simulacao-modulos.md
docs/official/07-simulacoes/02-simulacao-sensores.md
docs/official/07-simulacoes/03-simulacao-atuadores.md
docs/official/08-casos-de-uso/01-manutencao-modulos.md
docs/official/08-casos-de-uso/02-diagnostico.md
docs/official/08-casos-de-uso/03-testes-bancada.md
docs/official/09-desenvolvimento/01-organizacao-repositorio.md
docs/official/09-desenvolvimento/02-padroes-codigo.md
docs/official/09-desenvolvimento/03-fluxo-git.md
docs/official/10-testes/01-testes-hardware.md
docs/official/10-testes/02-testes-firmware.md
docs/official/10-testes/03-testes-integracao.md
docs/official/11-planejamento/01-planejamento.md
docs/official/11-planejamento/02-proximas-funcionalidades.md
docs/official/12-documentacao-tecnica/01-especificacoes.md
docs/official/12-documentacao-tecnica/02-diagramas.md
docs/official/12-documentacao-tecnica/03-contratos-software.md
hardware/.gitkeep
hardware/AGENTS.md
hardware/README.md
"hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE N\303\215VEIS/Canal_Analogico.kicad_sch"
"hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE N\303\215VEIS/Canal_Analogico12V.kicad_sch"
"hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE N\303\215VEIS/GERADOR_NIVEIS.kicad_pcb"
"hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE N\303\215VEIS/GERADOR_NIVEIS.kicad_pro"
"hardware/boards/GSA -gerador-sinais-analogicos/GERADOR DE N\303\215VEIS/GERADOR_NIVEIS.kicad_sch"
hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_pcb
hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_pro
hardware/boards/GSA -gerador-sinais-analogicos/GSA - gerador de sinais analogicos.kicad_sch
hardware/boards/GSA -gerador-sinais-analogicos/driver_5v.kicad_sch
hardware/boards/SimulDIESEL/SimulDIESEL.kicad_pcb
hardware/boards/SimulDIESEL/SimulDIESEL.kicad_pro
hardware/boards/SimulDIESEL/SimulDIESEL.kicad_sch
hardware/boards/SimulDIESEL/legacy-comunicacao/kicad/comunicacao.kicad_pcb
hardware/boards/SimulDIESEL/legacy-comunicacao/kicad/comunicacao.kicad_pro
hardware/boards/SimulDIESEL/legacy-comunicacao/kicad/comunicacao.kicad_sch
hardware/boards/babyboards/comunicacao/.gitkeep
hardware/boards/babyboards/fonte-alimentacao/.gitkeep
hardware/boards/babyboards/gerador-niveis/.gitkeep
hardware/boards/babyboards/reles/.gitkeep
hardware/boards/backplane/.gitkeep
hardware/boards/mechanical/gsa-board-outline.FCStd
hardware/boards/mechanical/gsa-board-outline.dxf
hardware/boards/x-conn/assembly/.gitkeep
hardware/boards/x-conn/bom/.gitkeep
hardware/boards/x-conn/gerbers/.gitkeep
hardware/boards/x-conn/kicad/.gitkeep
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.gitignore
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/ProjectSettings.json
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/ide/visual-studio-cache/slnx.sqlite
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.ino
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.sln
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.vcxproj
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/BlinkLed.vcxproj.filters
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/BlinkLed/src/arduino folders read me.txt
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/TesteEnvio/TesteEnvio.ino
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/tests-manual/legacy-arduino-sketches/Teste_Hand_Shake/Teste_Hand_Shake.ino
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/artifacts/workspaces/esp32-api-bridge.code-workspace
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/README
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwBus/GwBus.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwBus/NullGwBus.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwErr/GwErr.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwI2cBus/GwI2cBus.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwI2cBus/GwI2cBus.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/IGatewayApp/IGatewayApp.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/README
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwCobs/SdgwCobs.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwCobs/SdgwCobs.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwCrc8/SdgwCrc8.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwCrc8/SdgwCrc8.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwLink/SdgwLink.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwLink/SdgwLink.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwParser/SdgwParser.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwParser/SdgwParser.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/ISdgwEndpoint.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/SdgwBluetoothEndpoint.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/SdgwEndpointMux.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/SdgwSessionOwner.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/SdgwTransport.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/SdgwTransport/SdgwTransport.h
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/platformio.ini
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/.gitkeep
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/main.cpp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/test/README
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/.gitignore"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/artifacts/workspaces/gerador-sinais-analogicos-gsa.code-workspace"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/include/README"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/include/SoftwareWire.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/include/config.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/include/defs.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/AnalogService/AnalogService.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/AnalogService/AnalogService.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/BusArbiterService/BusArbiterService.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/BusArbiterService/BusArbiterService.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/EepromService/EepromService.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/EepromService/EepromService.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/LedService/LedService.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/LedService/LedService.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Link/Link.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Link/Link.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Link/crc8.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Mcp4725Service/Mcp4725Service.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Mcp4725Service/Mcp4725Service.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/README"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Service/Service.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Service/Service.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Tca9548Service/Tca9548Service.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Tca9548Service/Tca9548Service.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Tlv/Tlv.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Tlv/TlvBuilder.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Transport/Transport.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/lib/Transport/Transport.h"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/platformio.ini"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/src/main.cpp"
"hardware/firmware/GSA - Gerador de sinais anal\303\263gicos/test/README"
hardware/firmware/UCE - Unidade de comunicacao externa/.gitignore
hardware/firmware/UCE - Unidade de comunicacao externa/include/config.h
hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/SpiLink.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/SpiLink.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/services/UceServiceDispatcher.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/UceTransport.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriverSelector.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver_fake.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/driver/CanDriver_fake.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/protocol/CanCrudProtocol.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/protocol/CanCrudProtocol.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/rxhub/CanRxHub.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/rxhub/CanRxHub.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpCodec.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpRxTableManager.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpService.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpTxTableManager.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/sdctp/SdctpTypes.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/service/CanService.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanRxTableManager.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanRxTableManager.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanTxTableManager.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/table/CanTxTableManager.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/can/types/CanTypes.h
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/led/LedService.cpp
hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/led/LedService.h
hardware/firmware/UCE - Unidade de comunicacao externa/platformio.ini
hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp
hardware/test-jigs/.gitkeep
infra/.gitkeep
infra/cloud/.gitkeep
infra/cloud/docker/.gitkeep
infra/cloud/pipelines/.gitkeep
infra/local/.gitkeep
infra/local/config-templates/.gitkeep
infra/local/installers/.gitkeep
local-api/.gitkeep
local-api/AGENTS.md
local-api/README.md
local-api/src/SimulDIESEL/SimulDIESEL.sln
local-api/src/SimulDIESEL/SimulDIESEL/App.config
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Backplane/BackplaneService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/BpmClient.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/BpmCommandResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/BpmParsers.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Bluetooth/BpmBluetoothService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Network/BpmNetworkService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/SdgwHostSession.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BPM/XConn/XConnService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BoardDispatcher.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/BoardTlvDispatcher.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaChannelScaling.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaClient.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaCommandResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaDispatcher.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaOperationResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/GSA/GsaParsers.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceCommandResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceOperationResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/BPM/FrmBpmLogic.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/GSA/FrmGsaLogic.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939ApplicationLayerService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939PgnCatalog.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939PgnDecoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939RawValueReader.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939SignalRangeEvaluator.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939SpnDecoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939CaptureExportService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Capture/J1939TemporalCaptureService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Common/J1939ByteOrder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Common/J1939Constants.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Common/J1939PgnStandardCatalog.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Common/J1939ToolAddressConfig.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939DataLinkService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939IdParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939MessageTypeClassifier.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939PduClassifier.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939TransportProtocolService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939TransportSessionManager.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939DiagnosticRequestService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939DiagnosticsService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939Dm1Decoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939Dm2Decoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939DtcParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939FmiCatalog.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939LampStatusDecoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/J1939ProtocolService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939AddressClaimDecoder.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939AddressClaimRequestService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939AddressRegistry.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939CommandedAddressService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NameParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NetworkManagementService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NodeIdentityService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939WorkingSetService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/ProtocolDecoderGateway.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanControlApiService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanEventProcessor.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxMirrorManager.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanRxOutputBuffer.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/CanTxManager.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpApiService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpDiagnostics.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpProtocol.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpRxMirrorManager.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpRxOutputBuffer.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpTxManager.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpTypes.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/J1939ReferenceCatalogService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseStatus.cs
local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/ModuleProfileService.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/BdCommandParameter.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/DatabaseInitializationResult.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IBdServiceProvider.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseConnectionFactory.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseInitializer.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IMigrationRunner.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/J1939CatalogSeedImporter.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabasePaths.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabaseRuntimeFactory.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteBdServiceProvider.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteConnectionFactory.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteDatabaseInitializer.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteMigrationRunner.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdGwTxScheduler.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwFrameCodec.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwFrameReader.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwFrameWriter.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwLinkEngine.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwLinkSupervisor.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdgwSession.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhClient.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhJsonParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhJsonSerializer.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhTextParser.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhTextSerializer.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhValidator.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IJ1939ReferenceCatalogRepository.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IModuleProfileRepository.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteJ1939ReferenceCatalogRepository.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteModuleProfileRepository.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Bluetooth/BluetoothConnectionSettings.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Bluetooth/BluetoothDeviceCatalog.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Bluetooth/BluetoothTransport.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Serial/IByteTransport.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Serial/SerialConnectionSettings.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Serial/SerialPortAdapter.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/Serial/SerialTransport.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/SwitchableTransport.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/TransportConnectionSettings.cs
local-api/src/SimulDIESEL/SimulDIESEL/DAL/Transport/TransportKind.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/BPM/BluetoothDeviceDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/BPM/BpmStatusDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/GSA/GsaCommon.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/GSA/GsaLedResponse.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/GSA/GsaRequests.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/GSA/GsaResponses.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanCreateDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanDeleteDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanEditDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanFrameDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanRowDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanTicDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/Can/CanTxRowDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/UceCanProtocol.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/UceCanResponses.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Boards/UCE/UceLedResponse.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Common/DeviceInfo.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Common/OperationStatusDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileCreateRequest.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileUpdateRequest.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Application/J1939ApplicationMessageDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Application/J1939DecodedSignalDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Application/J1939PgnDefinitionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Application/J1939SignalStatusDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Application/J1939SpnDefinitionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CaptureSessionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939CapturedEventDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Capture/J1939TemporalTickDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939CatalogEntryDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939ManufacturerCatalogDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939NameFieldDefinitionCatalogDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Catalogs/J1939PreferredAddressCatalogDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Common/J1939PgnDefinitionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Common/J1939ProcessingStatusDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939DataLinkMessageDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939DataLinkProcessingResultDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939IdFieldsDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939MessageTypeDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939ReassembledMessageDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939TransportControlMessageDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939TransportDataPacketDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/DataLink/J1939TransportSessionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Diagnostics/J1939DiagnosticMessageDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Diagnostics/J1939DiagnosticReadResultDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Diagnostics/J1939DtcDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Diagnostics/J1939FmiDefinitionDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/Diagnostics/J1939LampStatusDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939AddressClaimDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939AddressRegistryEntryDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NameDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NetworkEventDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NodeIdentityDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939WorkingSetDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939WorkingSetMemberDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDCTP/SdctpRawEventDto.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/GwProtocol.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdgwCommand.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdgwFrame.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdhCommand.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdhResponse.cs
local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/SdhTarget.cs
local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.cs
local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.resx
local-api/src/SimulDIESEL/SimulDIESEL/Program.cs
local-api/src/SimulDIESEL/SimulDIESEL/Properties/AssemblyInfo.cs
local-api/src/SimulDIESEL/SimulDIESEL/Properties/Resources.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/Properties/Resources.resx
local-api/src/SimulDIESEL/SimulDIESEL/Properties/Settings.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/Properties/Settings.settings
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/ANDERTECH.ico
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Conectado.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Desconectado.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7.jpg
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7_P.jpg
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/EDC7_P1.jpg
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGrayOff_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenBright_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenDark_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedGreenLight_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedBright_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedDark_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedRedLight_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowBright_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowDark_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/LedYellowLight_18x18.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/MR.jpg
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/MR_P.jpg
"local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/M\303\223DULO.png"
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Rede_can_ico.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Rede_can_toolbar_ico.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/Simbolo ANDERTECH.jpg
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/VERDE OFF.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/VERDE ON.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/turn on.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/vermelho OFF.png
local-api/src/SimulDIESEL/SimulDIESEL/RESOURCES/vermelho ON.png
local-api/src/SimulDIESEL/SimulDIESEL/SDH_HOST_OVERVIEW.md
local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaChannelControl.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaChannelControl.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/GsaControls.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdLedIndicator.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdVerticalGauge.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/Controls/SdVerticalSlider.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/FormGaugeDemo.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/FormGaugeDemo.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/FormGsaChannelsDemo.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/FormGsaChannelsDemo.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmBluetoothConnect.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmBluetoothConnect.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmGSA_UI.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmGSA_UI.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmGSA_UI.resx
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmPortaSerial_UI.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmPortaSerial_UI.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmPortaSerial_UI.resx
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.Designer.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs
local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.resx
local-api/src/SimulDiesel.Application/.gitkeep
local-api/src/SimulDiesel.Domain/.gitkeep
local-api/src/SimulDiesel.Drivers.Esp32/.gitkeep
local-api/src/SimulDiesel.Infrastructure/.gitkeep
local-api/src/SimulDiesel.LocalApi/.gitkeep
local-api/src/SimulDiesel.LocalApp/.gitkeep
local-api/src/SimulDiesel.Protocols/.gitkeep
local-api/src/SimulDiesel.Shared/.gitkeep
local-api/tests/integration/.gitkeep
local-api/tests/unit/.gitkeep
out/dumps/agents_bootstrap_governance_adjustments.md
out/dumps/agents_skills_prompts_creation.md
out/dumps/bd_crud_consumption_rule.md
out/dumps/bd_local_consolidacao_final.md
out/dumps/bd_local_estrutura.md
out/dumps/bd_persistence_candidates.md
out/dumps/bd_service_provider.md
out/dumps/bd_service_provider_module_profile_crud.md
out/dumps/codex_agents_skills_bootstrap.md
out/dumps/critical_skills_governance_hardening.md
out/dumps/csharp_environment_sync_governance.md
out/dumps/documentation_governance_auto_update.md
out/dumps/etapa02_can_control_sdctp_split.md
out/dumps/etapa03_uceclient_sdctp_routing.md
out/dumps/etapa04_sdctp_tx_and_readall_cleanup.md
out/dumps/etapa05_remove_legacy_dispatcher_events.md
out/dumps/etapa06_remove_legacy_uceparsers_can.md
out/dumps/etapa_j1939_81_reference_catalog_tables.md
out/dumps/etapa_j1939_catalog_seed_import.md
out/dumps/etapa_j1939_catalog_web_captured_data.md
out/dumps/etapa_rede_can_01_repository_catalogo_j1939.md
out/dumps/etapa_rede_can_02_bll_catalogo_j1939.md
out/dumps/etapa_rede_can_03_node_identity_service.md
out/dumps/etapa_rede_can_04_frm_rede_can.md
out/dumps/etapa_rede_can_consumo_j1939_catalogos_consolidado.md
out/dumps/etapa_temporal_capture_consolidado.md
out/dumps/etapa_temporal_capture_export.md
out/dumps/etapa_temporal_capture_service.md
out/dumps/etapa_temporal_capture_ui.md
out/dumps/etapa_temporal_capture_validation.md
out/dumps/etapa_topologia_api_j1939_catalog_consumption.md
out/dumps/instructions_md_consolidation.md
out/dumps/module_database_model/module_schema_dump_v1.md
out/dumps/sdctp_naming_cleanup.md
out/dumps/sdh_contract_export/sdh_contract_export.json
out/dumps/sdh_contract_export/sdh_contract_export.md
out/dumps/sdh_protocol_inventory/sdh_board_services.md
out/dumps/sdh_protocol_inventory/sdh_code_references.md
out/dumps/sdh_protocol_inventory/sdh_command_catalog.md
out/dumps/sdh_protocol_inventory/sdh_database_relevance.md
out/dumps/sdh_protocol_inventory/sdh_event_catalog.md
out/dumps/sdh_protocol_inventory/sdh_json_contracts.md
out/dumps/sdh_protocol_inventory/sdh_protocol_inventory.md
out/dumps/sdh_sdctp_architecture_current_state.md
path.bat
specs/.gitkeep
specs/adr/.gitkeep
specs/data-models/.gitkeep
specs/protocols/.gitkeep
specs/protocols/esp32_to_due.md
specs/protocols/esp32_to_mega.md
specs/protocols/local-api_to_esp32.md
specs/requirements/.gitkeep
tests/.gitkeep
tests/Leitura de alarmes/1 - Somente a Service tool.md
tests/Leitura de alarmes/2 - CXCM LIGADA.md
tests/captura-j1939-20260513-160725.md
tests/captura-j1939-20260513-161658.md
tests/commissioning_spi_v1/README.md
tests/commissioning_spi_v1/simulate_spi_handshake_log.mjs
tests/commissioning_spi_v1/simulate_spi_handshake_log.py
tests/commissioning_spi_v1/test_spi_v1_protocol.mjs
tests/commissioning_spi_v1/test_spi_v1_protocol.py
tests/e2e/.gitkeep
tests/hardware-in-the-loop/.gitkeep
tests/performance/.gitkeep
tools/.gitkeep
tools/GSA_Teste/.gitignore
tools/GSA_Teste/platformio.ini
tools/GSA_Teste/src/main.cpp
tools/dev/.gitkeep
tools/esp32_i2c_master_blink/esp32_i2c_master_blink.ino
tools/flash/.gitkeep
tools/nano_i2c_slave_led/nano_i2c_slave_led.ino
tools/release/.gitkeep
tools/testes/can_rx_direct_vs_auto_validation.py
tools/testes/can_tx_loopback_validation.py
tools/testes/j1939_application_validation.py
tools/testes/j1939_datalink_validation.py
tools/testes/j1939_diagnostics_validation.py
tools/testes/j1939_network_management_validation.py
tools/testes/j1939_pgn_catalog_validation.py
tools/testes/sdctp_output_buffer_consumer_validation.py
```

## Apêndice B - Inventario completo dos arquivos ignorados detectados
```text
hardware/boards/babyboards/gerador-niveis/KiCad/GERADOR_NIVEIS-backups/GERADOR_NIVEIS-2026-02-02_123352.zip
hardware/boards/babyboards/gerador-niveis/KiCad/GERADOR_NIVEIS.kicad_prl
hardware/boards/babyboards/gerador-niveis/KiCad/fp-info-cache
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/.sconsign311.dblite
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/.sconsign313.dblite
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Esp.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Esp.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/FirmwareMSC.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/FirmwareMSC.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/FunctionalInterrupt.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/FunctionalInterrupt.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/HWCDC.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/HWCDC.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/HardwareSerial.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/HardwareSerial.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/IPAddress.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/IPAddress.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/IPv6Address.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/IPv6Address.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/MD5Builder.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/MD5Builder.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Print.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Print.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Stream.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Stream.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/StreamString.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/StreamString.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Tone.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/Tone.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USB.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USB.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USBCDC.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USBCDC.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USBMSC.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/USBMSC.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/WMath.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/WMath.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/WString.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/WString.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/base64.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/base64.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/cbuf.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/cbuf.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-adc.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-adc.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-bt.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-bt.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-cpu.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-cpu.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-dac.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-dac.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-gpio.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-gpio.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-i2c-slave.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-i2c-slave.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-i2c.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-i2c.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-ledc.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-ledc.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-matrix.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-matrix.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-misc.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-misc.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-psram.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-psram.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-rgb-led.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-rgb-led.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-rmt.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-rmt.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-sigmadelta.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-sigmadelta.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-spi.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-spi.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-time.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-time.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-timer.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-timer.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-tinyusb.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-tinyusb.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-touch.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-touch.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-uart.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/esp32-hal-uart.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/firmware_msc_fat.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/firmware_msc_fat.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/libb64/cdecode.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/libb64/cdecode.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/libb64/cencode.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/libb64/cencode.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/main.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/main.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/stdlib_noniso.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/stdlib_noniso.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/wiring_pulse.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/wiring_pulse.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/wiring_shift.c.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/FrameworkArduino/wiring_shift.c.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/bootloader.bin
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/firmware.bin
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/firmware.elf
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/firmware.map
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/idedata.json
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib02e/GwRouter/GwRouter.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib02e/GwRouter/GwRouter.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib02e/libGwRouter.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib076/SdgwTransport/SdgwTransport.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib076/SdgwTransport/SdgwTransport.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib076/libSdgwTransport.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib0ef/GwI2cBus/GwI2cBus.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib0ef/GwI2cBus/GwI2cBus.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib0ef/libGwI2cBus.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTAddress.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTAddress.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTAdvertisedDeviceSet.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTAdvertisedDeviceSet.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTScanResultsSet.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BTScanResultsSet.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BluetoothSerial.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/BluetoothSerial/BluetoothSerial.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib257/libBluetoothSerial.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib2dd/GwTlv/GwTlv.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib2dd/GwTlv/GwTlv.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib2dd/libGwTlv.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/Gateway/GatewayApp.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/Gateway/GatewayApp.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/SdgwCrc8/SdgwCrc8.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/SdgwCrc8/SdgwCrc8.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/libGateway.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib44f/libSdgwCrc8.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib4f4/SdgwLink/SdgwLink.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib4f4/SdgwLink/SdgwLink.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib4f4/libSdgwLink.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib811/Wire/Wire.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib811/Wire/Wire.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib811/libWire.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib92e/GwDeviceTable/GwDeviceTable.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib92e/GwDeviceTable/GwDeviceTable.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/lib92e/libGwDeviceTable.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libFrameworkArduino.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libd71/SdgwCobs/SdgwCobs.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libd71/SdgwCobs/SdgwCobs.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libd71/libSdgwCobs.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libdfc/SPI/SPI.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libdfc/SPI/SPI.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libdfc/libSPI.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libf9d/GwSpiBus/GwSpiBus.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libf9d/GwSpiBus/GwSpiBus.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libf9d/libGwSpiBus.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libfcd/SdgwParser/SdgwParser.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libfcd/SdgwParser/SdgwParser.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/libfcd/libSdgwParser.a
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/partitions.bin
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/src/main.cpp.d
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/src/main.cpp.o
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/tmpleuuoa2z.tmp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/esp32dev/tmpxt2suwc1.tmp
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.pio/build/project.checksum
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.vscode/c_cpp_properties.json
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.vscode/extensions.json
hardware/firmware/BPM - BACKPLANE MANAGER MODULE/.vscode/launch.json
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/.sconsign311.dblite
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/.sconsign313.dblite
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/IPAddress.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/Print.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/Reset.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/RingBuffer.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/Stream.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/UARTClass.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/USARTClass.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/USB/CDC.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/USB/PluggableUSB.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/USB/USBCore.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/WInterrupts.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/WMath.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/WString.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/abi.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/avr/dtostrf.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/cortex_handlers.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/hooks.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/iar_calls_sam3.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/itoa.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/main.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/new.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/syscalls_sam3.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/watchdog.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring_analog.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring_digital.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring_pulse.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring_pulse_asm.S.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduino/wiring_shift.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/FrameworkArduinoVariant/variant.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/firmware.bin
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/firmware.elf
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/idedata.json
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/libFrameworkArduino.a
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/libFrameworkArduinoVariant.a
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/core/link/SpiLink.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/core/services/UceServiceDispatcher.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/core/transport/UceTransport.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/driver/CanDriver.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/driver/CanDriver_fake.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/protocol/CanCrudProtocol.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/service/CanService.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/table/CanRxTableManager.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/can/table/CanTxTableManager.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/lib/services/led/LedService.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB/src/src/main.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/.sconsign311.dblite
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/.sconsign313.dblite
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/IPAddress.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/Print.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/Reset.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/RingBuffer.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/Stream.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/UARTClass.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/USARTClass.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/USB/CDC.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/USB/PluggableUSB.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/USB/USBCore.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/WInterrupts.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/WMath.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/WString.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/abi.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/avr/dtostrf.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/cortex_handlers.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/hooks.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/iar_calls_sam3.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/itoa.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/main.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/new.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/syscalls_sam3.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/watchdog.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring_analog.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring_digital.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring_pulse.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring_pulse_asm.S.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduino/wiring_shift.c.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/FrameworkArduinoVariant/variant.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/firmware.bin
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/firmware.elf
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/libFrameworkArduino.a
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/libFrameworkArduinoVariant.a
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/core/link/SpiLink.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/core/services/UceServiceDispatcher.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/core/transport/UceTransport.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/driver/CanDriver.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/driver/CanDriver_fake.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/protocol/CanCrudProtocol.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/service/CanService.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/table/CanRxTableManager.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/can/table/CanTxTableManager.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/lib/services/led/LedService.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/dueUSB_canFake/src/src/main.cpp.o
hardware/firmware/UCE - Unidade de comunicacao externa/.pio/build/project.checksum
hardware/firmware/UCE - Unidade de comunicacao externa/.vscode/c_cpp_properties.json
hardware/firmware/UCE - Unidade de comunicacao externa/.vscode/extensions.json
hardware/firmware/UCE - Unidade de comunicacao externa/.vscode/launch.json
local-api/src/SimulDIESEL/.vs/SimulDIESEL/FileContentIndex/4a438f6c-0c2b-4c7a-9148-a3a005ca5971.vsidx
local-api/src/SimulDIESEL/.vs/SimulDIESEL/FileContentIndex/756c8139-6eec-4bf4-bf63-4d10295182b3.vsidx
local-api/src/SimulDIESEL/.vs/SimulDIESEL/FileContentIndex/a93da1e3-2abc-4a08-9bd3-85ecc15e17ed.vsidx
local-api/src/SimulDIESEL/.vs/SimulDIESEL/FileContentIndex/ac6bc625-d873-4c15-867f-9a1100294ba5.vsidx
local-api/src/SimulDIESEL/.vs/SimulDIESEL/FileContentIndex/b3d40600-a6e9-4a19-94c5-33ed6caeee13.vsidx
local-api/src/SimulDIESEL/.vs/SimulDIESEL/v17/.suo
local-api/src/SimulDIESEL/.vs/SimulDIESEL/v17/DocumentLayout.backup.json
local-api/src/SimulDIESEL/.vs/SimulDIESEL/v17/DocumentLayout.json
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/SimulDIESEL.exe
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/SimulDIESEL.exe.config
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/SimulDIESEL.pdb
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/System.Data.SQLite.dll
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/x64/SQLite.Interop.dll
local-api/src/SimulDIESEL/SimulDIESEL/bin/Debug/x86/SQLite.Interop.dll
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/.NETFramework,Version=v4.7.2.AssemblyAttributes.cs
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/DesignTimeResolveAssemblyReferences.cache
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/DesignTimeResolveAssemblyReferencesInput.cache
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIE.A32351C5.Up2Date
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.DashBoard.resources
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.Properties.Resources.resources
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.UI.frmGSA_UI.resources
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.UI.frmPortaSerial_UI.resources
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.UI.frmUCE_UI.resources
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.AssemblyReference.cache
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.CoreCompileInputs.cache
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.FileListAbsolute.txt
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.csproj.GenerateResource.cache
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.exe
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/SimulDIESEL.pdb
local-api/src/SimulDIESEL/SimulDIESEL/obj/Debug/TempPE/Properties.Resources.Designer.cs.dll
local-api/src/SimulDIESEL/SimulDIESEL/obj/SimulDIESEL.csproj.nuget.dgspec.json
local-api/src/SimulDIESEL/SimulDIESEL/obj/SimulDIESEL.csproj.nuget.g.props
local-api/src/SimulDIESEL/SimulDIESEL/obj/SimulDIESEL.csproj.nuget.g.targets
local-api/src/SimulDIESEL/SimulDIESEL/obj/project.assets.json
local-api/src/SimulDIESEL/SimulDIESEL/obj/project.nuget.cache
out/build-bd-local/SimulDIESEL.exe
out/build-bd-local/SimulDIESEL.exe.config
out/build-bd-local/SimulDIESEL.pdb
out/build-bd-local/System.Data.SQLite.dll
out/build-bd-local/x64/SQLite.Interop.dll
out/build-bd-local/x86/SQLite.Interop.dll
out/build-bd-provider/SimulDIESEL.exe
out/build-bd-provider/SimulDIESEL.exe.config
out/build-bd-provider/SimulDIESEL.pdb
out/build-bd-provider/System.Data.SQLite.dll
out/build-bd-provider/x64/SQLite.Interop.dll
out/build-bd-provider/x86/SQLite.Interop.dll
out/build-etapa-j1939-reference-catalogs/SimulDIESEL.exe
out/build-etapa-j1939-reference-catalogs/SimulDIESEL.exe.config
out/build-etapa-j1939-reference-catalogs/SimulDIESEL.pdb
out/build-etapa-j1939-reference-catalogs/System.Data.SQLite.dll
out/build-etapa-j1939-reference-catalogs/x64/SQLite.Interop.dll
out/build-etapa-j1939-reference-catalogs/x86/SQLite.Interop.dll
out/build-etapa-j1939-seed-pre/SimulDIESEL.exe
out/build-etapa-j1939-seed-pre/SimulDIESEL.exe.config
out/build-etapa-j1939-seed-pre/SimulDIESEL.pdb
out/build-etapa-j1939-seed-pre/System.Data.SQLite.dll
out/build-etapa-j1939-seed-pre/x64/SQLite.Interop.dll
out/build-etapa-j1939-seed-pre/x86/SQLite.Interop.dll
out/build-etapa-j1939-seed/SimulDIESEL.exe
out/build-etapa-j1939-seed/SimulDIESEL.exe.config
out/build-etapa-j1939-seed/SimulDIESEL.pdb
out/build-etapa-j1939-seed/System.Data.SQLite.dll
out/build-etapa-j1939-seed/x64/SQLite.Interop.dll
out/build-etapa-j1939-seed/x86/SQLite.Interop.dll
out/build-etapa-rede-can-01/SimulDIESEL.exe
out/build-etapa-rede-can-01/SimulDIESEL.exe.config
out/build-etapa-rede-can-01/SimulDIESEL.pdb
out/build-etapa-rede-can-01/System.Data.SQLite.dll
out/build-etapa-rede-can-01/x64/SQLite.Interop.dll
out/build-etapa-rede-can-01/x86/SQLite.Interop.dll
out/build-etapa-rede-can-02/SimulDIESEL.exe
out/build-etapa-rede-can-02/SimulDIESEL.exe.config
out/build-etapa-rede-can-02/SimulDIESEL.pdb
out/build-etapa-rede-can-02/System.Data.SQLite.dll
out/build-etapa-rede-can-02/x64/SQLite.Interop.dll
out/build-etapa-rede-can-02/x86/SQLite.Interop.dll
out/build-etapa-rede-can-03/SimulDIESEL.exe
out/build-etapa-rede-can-03/SimulDIESEL.exe.config
out/build-etapa-rede-can-03/SimulDIESEL.pdb
out/build-etapa-rede-can-03/System.Data.SQLite.dll
out/build-etapa-rede-can-03/x64/SQLite.Interop.dll
out/build-etapa-rede-can-03/x86/SQLite.Interop.dll
out/build-etapa-rede-can-04/SimulDIESEL.exe
out/build-etapa-rede-can-04/SimulDIESEL.exe.config
out/build-etapa-rede-can-04/SimulDIESEL.pdb
out/build-etapa-rede-can-04/System.Data.SQLite.dll
out/build-etapa-rede-can-04/x64/SQLite.Interop.dll
out/build-etapa-rede-can-04/x86/SQLite.Interop.dll
out/build-etapa-temporal-capture-enrichment/SimulDIESEL.exe
out/build-etapa-temporal-capture-enrichment/SimulDIESEL.exe.config
out/build-etapa-temporal-capture-enrichment/SimulDIESEL.pdb
out/build-etapa-temporal-capture-enrichment/System.Data.SQLite.dll
out/build-etapa-temporal-capture-enrichment/x64/SQLite.Interop.dll
out/build-etapa-temporal-capture-enrichment/x86/SQLite.Interop.dll
out/build-etapa-temporal-capture/SimulDIESEL.exe
out/build-etapa-temporal-capture/SimulDIESEL.exe.config
out/build-etapa-temporal-capture/SimulDIESEL.pdb
out/build-etapa-temporal-capture/System.Data.SQLite.dll
out/build-etapa-temporal-capture/x64/SQLite.Interop.dll
out/build-etapa-temporal-capture/x86/SQLite.Interop.dll
out/build-etapa-topologia-j1939-catalog/SimulDIESEL.exe
out/build-etapa-topologia-j1939-catalog/SimulDIESEL.exe.config
out/build-etapa-topologia-j1939-catalog/SimulDIESEL.pdb
out/build-etapa-topologia-j1939-catalog/System.Data.SQLite.dll
out/build-etapa-topologia-j1939-catalog/x64/SQLite.Interop.dll
out/build-etapa-topologia-j1939-catalog/x86/SQLite.Interop.dll
out/build-etapa02/SimulDIESEL.exe
out/build-etapa02/SimulDIESEL.exe.config
out/build-etapa02/SimulDIESEL.pdb
out/build-etapa03/SimulDIESEL.exe
out/build-etapa03/SimulDIESEL.exe.config
out/build-etapa03/SimulDIESEL.pdb
out/build-etapa04/SimulDIESEL.exe
out/build-etapa04/SimulDIESEL.exe.config
out/build-etapa04/SimulDIESEL.pdb
out/build-etapa05/SimulDIESEL.exe
out/build-etapa05/SimulDIESEL.exe.config
out/build-etapa05/SimulDIESEL.pdb
out/build-etapa06/SimulDIESEL.exe
out/build-etapa06/SimulDIESEL.exe.config
out/build-etapa06/SimulDIESEL.pdb
out/build-j1939-diagnostics-dedupe/SimulDIESEL.exe
out/build-j1939-diagnostics-dedupe/SimulDIESEL.exe.config
out/build-j1939-diagnostics-dedupe/SimulDIESEL.pdb
out/build-j1939-diagnostics-dedupe/System.Data.SQLite.dll
out/build-j1939-diagnostics-dedupe/x64/SQLite.Interop.dll
out/build-j1939-diagnostics-dedupe/x86/SQLite.Interop.dll
out/build-main-pre-j1939/SimulDIESEL.exe
out/build-main-pre-j1939/SimulDIESEL.exe.config
out/build-main-pre-j1939/SimulDIESEL.pdb
out/build-main-pre-j1939/System.Data.SQLite.dll
out/build-main-pre-j1939/x64/SQLite.Interop.dll
out/build-main-pre-j1939/x86/SQLite.Interop.dll
out/build-rede-can-refresh-fix/SimulDIESEL.exe
out/build-rede-can-refresh-fix/SimulDIESEL.exe.config
out/build-rede-can-refresh-fix/SimulDIESEL.pdb
out/build-rede-can-refresh-fix/System.Data.SQLite.dll
out/build-rede-can-refresh-fix/x64/SQLite.Interop.dll
out/build-rede-can-refresh-fix/x86/SQLite.Interop.dll
out/validation-bd-local/j1939_catalog_seed_20260513_094549/modules.db
out/validation-bd-local/j1939_reference_catalogs_20260513_092641/modules.db
out/validation-bd-local/j1939_reference_catalogs_20260513_092727/modules.db
out/validation-rede-can-01/catalog-smoke.db
out/validation-rede-can-02/catalog-bll-smoke.db
out/validation-rede-can-03/node-identity-smoke.db
out/validation-rede-can-04/frm-rede-can-smoke.db
specs/protocols/gateway-api-transport/examples/ack.hex
specs/protocols/gateway-api-transport/examples/event-level.hex
specs/protocols/gateway-api-transport/examples/payload-with-zero.hex
specs/protocols/gateway-api-transport/examples/ping.hex
```

## Apêndice C - Inventario completo dos arquivos untracked
```text
local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.resx
tests/Leitura de alarmes/3 - somente motor.md
```
