# SD-GW-LINK — Exemplos em Hex (Vetores de Teste)

Este diretório contém **vetores de teste oficiais** do protocolo de transporte
**SD-GW-LINK**, utilizados para validação de implementações do motor do protocolo,
independentemente do meio físico (Serial, Wi-Fi, Bluetooth).

Todos os arquivos `.hex` representam **frames completos no stream**, ou seja:

- Dados **já codificados em COBS**
- Incluem o **delimitador final `0x00`**
- Utilizam **CRC-8/ATM** conforme a especificação oficial

---

## Convenções

- Bytes representados em hexadecimal, separados por espaço
- Ordem exata de transmissão no stream de bytes
- CRC calculado sobre: `CMD + FLAGS + SEQ + PAYLOAD`

Parâmetros do CRC:

- Algoritmo: CRC-8/ATM  
- Poly: `0x07`  
- Init: `0x00`  
- RefIn: `false`  
- RefOut: `false`  
- XorOut: `0x00`

---

## Arquivos

### `ping.hex`
Exemplo de comando simples solicitando ACK de transporte.

Uso típico:
- Teste básico de framing
- Validação de COBS e CRC

---

### `ack.hex`
Exemplo de **ACK de transporte (T_ACK)**.

Uso típico:
- Teste de confirmação Stop-and-Wait
- Validação de SEQ e FLAGS

---

### `event-level.hex`
Exemplo de **evento assíncrono** enviado pelo Gateway para a API.

Uso típico:
- Teste de recepção espontânea
- Validação de IS_EVT

---

### `payload-with-zero.hex`
Exemplo contendo byte `0x00` no payload original,
demonstrando o funcionamento correto do **COBS**.

Uso típico:
- Teste de robustez do framing
- Validação de decodificação COBS

---

## Observações Importantes

- Os valores de `CMD` utilizados são **exemplos ilustrativos**
- A semântica dos comandos pertence à **camada de aplicação**
- Estes arquivos validam apenas o **protocolo de transporte**

---

Projeto: **SimulDIESEL**  
Protocolo: **SD-GW-LINK**  
Versão: **1.0.0**
