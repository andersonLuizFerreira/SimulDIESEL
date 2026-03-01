# Gerador de Sinais Analógicos (GSA) — Firmware

Este diretório documenta o firmware do módulo **GSA (Gerador de Sinais Analógicos)** do projeto **SimulDIESEL**.

## Visão geral

O firmware implementa um dispositivo **I2C (modo slave)** que recebe e responde comandos em **formato TLV (Type-Length-Value)** com **validação por CRC-8**.

Funções identificadas no firmware:
- Comunicação I2C via `Wire.h`
- Processamento de frames TLV
- Controle de LED (via `LED_BUILTIN`)
- Gerenciamento de link/estado e erros (BAD_LEN, BAD_CRC, etc.)

## Local do firmware

- `hardware/firmware/gerador-sinais-analogicos-GSA/`

## Estrutura do projeto (firmware)

- `src/main.cpp` — entrypoint (contém `setup()` e `loop()`)
- `lib/` — bibliotecas do projeto (ex.: `Transport`, `LedService`, `Service`, `Link`, `Tlv`, `Crc8`)
- `include/` — headers compartilhados (ex.: `defs.h`, `config.h`)
- `platformio.ini` — configuração PlatformIO
- `test/` — reservado (sem código no momento)
- `.vscode/` — configurações do VS Code
- `build/` — gerado automaticamente (artefatos de build)

## Compilação e gravação

Ferramenta base:
- PlatformIO (VS Code) — **assumido pelo `platformio.ini`**

### Build
No diretório do firmware:
- `pio run`

### Upload (gravação)
No diretório do firmware:
- `pio run -t upload`

Configuração de porta:
- Ajustar `upload_port` no `platformio.ini` (quando necessário).  
  **Detalhes específicos da placa/porta não identificados no resumo.**

## Referências internas
- Definições e comandos TLV: `include/defs.h`
- Protocolo (TLV + CRC): ver `docs/05_hardware/gerador-sinais-analogicos-GSA/PROTOCOLO.md`
- Pinagem (LED): ver `docs/05_hardware/gerador-sinais-analogicos-GSA/PINOUT.md`