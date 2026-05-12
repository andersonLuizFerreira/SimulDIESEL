# AGENTS.md - hardware

Antes de atuar em hardware ou firmware, leia:

- `../AGENTS.md`
- `../docs/agents/skills/firmware-bpm-skill.md`
- `../docs/agents/skills/firmware-uce-skill.md`
- `../docs/agents/skills/sdgw-transport-skill.md`
- `../docs/agents/skills/sdctp-contract-skill.md`
- `../.codex/skills/firmware-bpm/SKILL.md`
- `../.codex/skills/firmware-uce/SKILL.md`

Regras locais:

- BPM deve permanecer gateway/roteador.
- UCE deve permanecer camada de execucao fisica CAN/LED.
- GSA deve permanecer geradora de sinais analogicos.
- Nao alterar API, UI, banco ou contratos sem pedido explicito.
- Em divergencias, aplicar a regra mais conservadora e registrar no dump da ETAPA.
