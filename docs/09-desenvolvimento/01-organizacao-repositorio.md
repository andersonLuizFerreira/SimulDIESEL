# Organização do Repositório

## Estrutura de referência

- `local-api/`: aplicação desktop (.NET WinForms), transporte e protocolo cliente.
- `hardware/`: firmwares e boards.
- `cloud/`: API/cloud scaffold atual com contratos de openapi.
- `docs/`: documentação oficial moderna atual.
- `docs/legacy-docs/`: acervo legado para consulta histórica.
- `infra/`, `tools/`, `tests/`: suporte operacional.

## Convenções de pasta

- Pastas de firmware organizadas por domínio (`esp32-api-bridge`, `gerador-sinais-analogicos-GSA`).
- `.gitkeep` marca áreas aguardando evolução sem remover intenção do diretório.

## Locais de documentação consolidada

- `docs/README.md` e `docs/00-INDICE.md` como hub.
- Subpastas temáticas de arquitetura, hardware, firmware, protocolos etc.

## Gestão de mudanças

A documentação foi escrita para refletir o estado real atual da árvore e registrar lacunas reais.

[Retornar ao README principal](../README.md)
