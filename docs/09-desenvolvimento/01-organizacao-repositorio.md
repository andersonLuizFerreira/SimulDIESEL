⬅ [Retornar para Pai Imediato (Índice Geral)](../00-INDICE.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Organização do Repositório

## Estrutura de referência

- `local-api/`: aplicação desktop (.NET WinForms), transporte e protocolo cliente.
- `hardware/`: firmwares e boards.
- `cloud/`: API/cloud scaffold atual com contratos de openapi.
- `docs/`: documentação oficial vigente.
- `out/dumps/docs_legacy_2026-05-18/`: acervo legado removido da árvore viva e preservado para auditoria histórica.
- `out/dumps/`: relatórios de execução, dumps e acervos históricos fora da árvore viva de documentação.
- `infra/`, `tools/`, `tests/`: suporte operacional.

## Convenções de pasta

- Pastas de firmware organizadas por board ou domínio (`BPM - BACKPLANE MANAGER MODULE`, `GSA - Gerador de sinais analógicos`, `UCE - Unidade de comunicacao externa`).
- `.gitkeep` marca áreas aguardando evolução sem remover intenção do diretório.

## Locais de documentação consolidada

- `docs/README.md` e `docs/00-INDICE.md` como hub.
- Subpastas temáticas de arquitetura, hardware, firmware, protocolos etc.

## Gestão de mudanças

A documentação foi escrita para refletir o estado real atual da árvore e registrar lacunas reais.

## Glossário

- **Repositório**: estrutura de diretórios e arquivos versionados do projeto.
- **Branch**: linha de desenvolvimento isolada dentro do Git.
- **Commit**: registro versionado de uma mudança aplicada ao repositório.
- **Padrão**: convenção adotada para manter consistência técnica.

## Próximas camadas

- [Padrões de Código](02-padroes-codigo.md)
