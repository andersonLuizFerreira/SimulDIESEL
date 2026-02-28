# ADR-0007 — Framing COBS + CRC-8 no transporte serial (SGGW)

- Status: Aceita
- Data: 2026-02-28
- Decisores: SimulDIESEL Team
- Contexto: Gateway (ESP32) ↔ PC (API local) via Serial/USB

## Contexto

O SimulDIESEL usa um transporte serial entre o PC e o Gateway (ESP32) para enviar comandos e receber eventos/telemetria. Em transporte serial, é comum ocorrer:

- Perda de alinhamento de quadro (bytes podem chegar em qualquer ponto do stream)
- Ruído/bytes corrompidos
- Re-sincronização após desconexão/reconexão
- Necessidade de delimitar frames sem depender de temporização

O protocolo do SimulDIESEL (SGGW) utiliza payload estruturado em TLV (Type-Length-Value). Para que o TLV seja interpretado corretamente, precisamos de:

- Um mecanismo robusto de framing (delimitação de mensagens no stream)
- Um mecanismo de detecção de erro (integridade) no nível do frame

## Decisão

Adotar:

1) **COBS (Consistent Overhead Byte Stuffing)** para framing, usando `0x00` como delimitador de frame no stream.
2) **CRC-8/ATM (poly `0x07`)** para detecção de corrupção do frame.

Regra do frame no stream:

- O transmissor constrói: `frame_raw = payload_tlv + crc8(payload_tlv)`
- Aplica COBS: `frame_cobs = COBS(frame_raw)`
- Escreve no Serial: `frame_cobs + 0x00` (delimitador)

No receptor:

- Lê bytes até encontrar `0x00` (um frame completo)
- Decodifica COBS → `frame_raw`
- Se `len(frame_raw) < 2`, descarta
- Separa `payload_tlv` e `crc_rx`
- Calcula `crc_calc = CRC8(payload_tlv)`
- Se `crc_calc != crc_rx`, descarta o frame
- Se ok, parseia TLV e encaminha para a camada superior

## Motivação

### Por que COBS?
- Permite delimitação simples via byte `0x00` sem ambiguidade
- Possui overhead pequeno e previsível
- Facilita re-sincronização: basta descartar até o próximo `0x00`
- É amplamente usado em links seriais orientados a bytes

### Por que CRC-8/ATM?
- Implementação pequena e rápida (adequado para microcontroladores)
- Boa detecção de erros para frames curtos/médios
- Mantém simplicidade no PC e no ESP32

### Por que CRC no payload (antes do COBS)?
- O CRC deve verificar o conteúdo lógico (payload TLV)
- COBS é transformação de transporte; o CRC deve ser calculado no dado original para consistência
- Na recepção: COBS → raw → valida CRC → TLV

## Alternativas consideradas

1) **Delimitador por newline (`\n`)**
   - Rejeitado: payload pode conter qualquer byte; exigiria escaping complexo.

2) **SLIP**
   - Viável, mas exige escaping e tem overhead variável; COBS é mais simples para delimitação por `0x00`.

3) **Length-prefix (tamanho no cabeçalho)**
   - Viável, porém falha em re-sincronização quando bytes são perdidos/corrompidos (o tamanho vira incorreto).
   - Exigiria lógica extra para realinhamento.

4) **CRC-16/32**
   - Mais robusto, porém aumenta overhead e custo de processamento; CRC-8 atende a necessidade atual.

## Consequências

### Positivas
- Framing robusto e fácil de depurar
- Re-sincronização rápida após erros
- Implementação simples no firmware e no PC
- TLV pode transportar qualquer byte (incluindo `0x00`) sem quebrar delimitação

### Negativas / trade-offs
- Overhead do COBS (pequeno, porém existe)
- CRC-8 é menos forte que CRC-16/32 (aceito pelo escopo atual)
- Frames corrompidos serão descartados (o protocolo deve tolerar perda e permitir retry no nível de comando)

## Notas de implementação

- O receptor deve tolerar frames vazios (ex.: múltiplos `0x00`), descartando-os.
- Deve existir limite de tamanho de frame para evitar uso excessivo de memória (ex.: `MAX_FRAME_LEN`).
- Logs/debug devem permitir imprimir:
  - tamanho do frame recebido (antes e depois do COBS)
  - motivo do descarte (COBS inválido, CRC inválido, TLV inválido)

## Referências

- Especificação do protocolo SGGW em `docs/01_arquitetura/01_protocolos/sggw/`
