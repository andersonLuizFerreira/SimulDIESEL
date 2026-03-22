⚠️ Documento histórico e superado. Não usar como protocolo oficial vigente da GSA.

Referência oficial atual:

- `docs/06-protocolos/06-gsa-sdh-tlv.md`
- `docs/04-firmware/boards/03-gsa.md`

# GSA — Estrutura interna

**Status:** Rascunho técnico obsoleto

## Estrutura esperada

- Transport
- Link
- Service

## Papéis

- **Transport:** interface com o barramento
- **Link:** valida TLV + CRC
- **Service:** interpreta operações e subcomandos

## Observação

Este arquivo foi preservado apenas como rascunho histórico.

Ele não cobre:

- o catálogo atual de 16 canais;
- offsets por canal;
- evento assíncrono de fault;
- a inconsistência histórica do type `0x12`.

[Retornar ao README principal](..\..\..\..\README.md)

