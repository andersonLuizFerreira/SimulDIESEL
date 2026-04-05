⚠️ Documento histórico e superado. Não usar como protocolo oficial vigente da GSA.

Referência oficial atual:

- `docs/official/06-protocolos/06-gsa-sdh-tlv.md`
- `docs/official/04-firmware/boards/03-gsa.md`

# GSA — Protocolo (TLV + CRC-8)

## Visão geral

O firmware do GSA é documentado como utilizando protocolo de barramento baseado em:

`[T][L][V...][CRC]`

Onde:

- `T` = tipo/operação
- `L` = comprimento do payload
- `V` = payload
- `CRC` = CRC-8

## Objetivo

Permitir comunicação simples e validada entre Gateway e GSA.

## Observação

A semântica detalhada dos comandos deste arquivo não representa mais o contrato oficial vigente da GSA.

Este material foi preservado apenas como registro histórico de uma fase anterior do protocolo.

Ele também antecede a resolução do conflito histórico do type `0x12`, superado no contrato oficial atual pela migração de `channel.status` para `0x1B`.

[Retornar ao README principal](../../../README.md)



