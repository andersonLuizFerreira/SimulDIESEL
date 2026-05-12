# AGENTS.md - local-api

Antes de atuar no host C# WinForms, leia:

- `../AGENTS.md`
- `../docs/agents/skills/winforms-ui-skill.md`
- `../docs/agents/skills/bll-dal-dtl-skill.md`
- `../docs/agents/skills/sdh-contract-skill.md`
- `../docs/agents/skills/sdgw-transport-skill.md`
- `../docs/agents/skills/sdctp-contract-skill.md`
- `../docs/agents/skills/j1939-decode-skill.md`
- `../.codex/skills/winforms-ui/SKILL.md`
- `../.codex/skills/bll-dal-dtl/SKILL.md`

Regras locais:

- Nao alterar firmware, banco ou contratos sem pedido explicito.
- UI deve chamar FormsLogic/BLL; nao consumir TLV ou SDGW bruto.
- BLL, DAL e DTL devem manter suas fronteiras.
- Em divergencias, aplicar a regra mais conservadora e registrar no dump da ETAPA.
