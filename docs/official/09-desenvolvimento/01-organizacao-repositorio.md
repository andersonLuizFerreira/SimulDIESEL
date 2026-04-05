⬅ [Retornar para 00-INDICE — Mapa da árvore documental](../../00-INDICE.md)

# Organização do Repositório

## Estrutura de referência

- `local-api/`: aplicação desktop (.NET WinForms), transporte e protocolo cliente.
- `hardware/`: firmwares e boards.
- `cloud/`: API/cloud scaffold atual com contratos de openapi.
- `docs/`: hub documental do projeto.
- `docs/official/`: documentação oficial vigente.
- `docs/legacy/`: acervo legado para consulta histórica.
- `docs/archive/` e `docs/generated/`: material arquivado e artefatos gerados.
- `infra/`, `tools/`, `tests/`: suporte operacional.

## Convenções de pasta

- Pastas de firmware organizadas por domínio (`esp32-api-bridge`, `gerador-sinais-analogicos-GSA`).
- `.gitkeep` marca áreas aguardando evolução sem remover intenção do diretório.

## Locais de documentação consolidada

- `docs/README.md` e `docs/00-INDICE.md` como hub.
- Subpastas temáticas de arquitetura, hardware, firmware, protocolos etc.

## Gestão de mudanças

A documentação foi escrita para refletir o estado real atual da árvore e registrar lacunas reais.

## Próximas camadas

- [Padrões de Código](02-padroes-codigo.md)


