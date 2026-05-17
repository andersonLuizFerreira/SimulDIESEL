# .agents - Guia comum para agentes

Esta pasta contem as instrucoes comuns para agentes de IA que trabalham no SimulDIESEL.

Objetivo: evitar dependencia de memoria conversacional e centralizar regras de comportamento, escopo, documentacao, arquitetura, validacao e evolucao do projeto.

## Leitura obrigatoria

Antes de qualquer analise, planejamento, ETAPA, alteracao de codigo ou alteracao documental, leia:

1. `README.md` na raiz do repositorio;
2. `.agents/instructions.md`;
3. skills relevantes em `.agents/skills/`;
4. documentacao aplicavel em `docs/`.

## Separacao de responsabilidades

- `docs/`: documentacao oficial consolidada para humanos e projeto.
- `.agents/`: regras operacionais para agentes de IA.
- `.codex/`: adaptador especifico para CODEX.
- `out/dumps/`: evidencias temporarias pos-codificacao ou auditoria; nao e documentacao oficial.
- Git: historico, legado, rollback e recuperacao de versoes antigas.

## Regra principal

Se houver divergencia entre memoria, conversa, dumps e arquivos do repositorio, o agente deve priorizar os arquivos versionados e registrar qualquer incerteza como `pendente de confirmacao`.
