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

A semântica detalhada dos comandos deve ser alinhada ao contrato atual da GSA e ao firmware efetivamente versionado.