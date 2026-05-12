# AGENTS.md - Data

Antes de atuar em dados, protocolos ou Banco de Modulos, leia:

- `../AGENTS.md`
- `../docs/agents/skills/module-database-skill.md`
- `../docs/agents/skills/sdh-contract-skill.md`
- `../docs/agents/skills/j1939-decode-skill.md`
- `../.codex/skills/module-database/SKILL.md`
- `../.codex/skills/sdh-contract/SKILL.md`
- `../.codex/skills/j1939-decode/SKILL.md`

Regras locais:

- Banco de Modulos nao executa comandos por si so.
- Comandos SDH persistidos devem ser validaveis contra o contrato SDH.
- Nao alterar schema, dados reais, UI, API ou firmware sem pedido explicito.
- Em divergencias, aplicar a regra mais conservadora e registrar no dump da ETAPA.
