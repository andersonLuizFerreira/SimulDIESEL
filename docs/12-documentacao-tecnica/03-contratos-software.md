# Contratos de Software

## Contratos de integração

- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_CENTRAL.md`
- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_GATEWAY.md`
- `docs/legacy-docs/01_arquitetura/00_contratos/CONTRATO_GSA.md`

Esses documentos são a base de rastreabilidade para:

- endereçamento `CMD` (ADDR/OP);
- roteamento de barramento;
- semântica TLV e tratamento de erros.

## Contratos de protocolo binário

- `docs/legacy-docs/01_arquitetura/01_protocolos/sggw/spec.pt-BR.md`
- `docs/legacy-docs/01_arquitetura/01_protocolos/sggw/interface.pt-BR.md`

## Contratos de implementação na API local

- `local-api` usa `SggwCmd` e `SdGwLinkEngine` como contrato lógico do lado cliente.
- Estados de transporte e eventos são modelados por `SerialLinkService.LinkState`.

## Regra de manutenção

- Qualquer mudança de comando/endereço deve atualizar simultaneamente:
  1) implementação;
  2) contratos;
  3) documentação oficial.

[Retornar ao README principal](../README.md)
