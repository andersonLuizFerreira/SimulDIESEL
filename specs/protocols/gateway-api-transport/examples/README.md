# SD-GW-LINK – Exemplos em Hex

Este diretório contém **vetores de teste oficiais** do protocolo de transporte
**SD-GW-LINK**, utilizados para validação e referência de implementações.

Todos os arquivos `.hex` representam **frames completos no stream**, ou seja:

- Dados **já codificados em COBS**
- Incluem o **delimitador final `00`**
- Utilizam **CRC-8/ATM** conforme a especificação oficial

---

## Convenções

- Bytes representados em hexadecimal, separados por espaço
- Ordem exata de transmissão no link físico
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
Exemplo de comando simples (PING) solicitando ACK de transporte.

Uso típico:
- Teste básico de link
- Validação de framing e CRC

---

### `ack.hex`
Exemplo de **ACK de transporte (T_ACK)** confirmando um frame anterior.

Uso típico:
- Teste de mecanismo de confirmação
- Validação de SEQ e FLAGS

---

### `event-level.hex`
Exemplo de **evento assíncrono** enviado pelo Gateway para a API,
contendo payload de aplicação.

Uso típico:
- Teste de recepção espontânea
- Verificação de IS_EVT

---

### `payload-with-zero.hex`
Exemplo contendo byte `00` no payload original,
demonstrando o funcionamento correto do **COBS**.

Uso típico:
- Teste de robustez do framing
- Validação de decodificação COBS

---

## Observações Importantes

- Os valores de `CMD` utilizados são **exemplos ilustrativos**
- A semântica dos comandos pertence à **camada de aplicação**
- Estes arquivos fazem parte da **especificação oficial** do protocolo

---

Projeto: **SimulDIESEL**  
Protocolo: **SD-GW-LINK**  
Versão: **1.0.0**
