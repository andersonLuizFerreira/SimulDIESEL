# SimulDIESEL Codex Bootstrap

Official CODEX bootstrap file: `.codex/instructions.md`.

Read the project guidance in this order before acting:

1. `AGENTS.md` - primary project instructions and scope rules.
2. `.codex/instructions.md` - this bootstrap and ordering rule.
3. `.codex/skills/` - structured `SKILL.md` files for Codex use.
4. `docs/agents/` - human-readable agent guidance, ETAPA templates, validation checklists, freeze templates, and official human skill documentation.

`AGENTS.md` is the main source of truth for project behavior, scope boundaries, validation, delivery, rollback, and Git restrictions.

`docs/agents/skills/` remains the official human-readable skill documentation. `.codex/skills/` mirrors those skills in Codex's structured format.

Before executing any ETAPA, never skip reading `AGENTS.md`.

Before acting, identify and load the most relevant skill in `.codex/skills/`.

Do not automatically expand the ETAPA scope.

Completed ETAPAS require updating impacted official documentation in `/docs/`.

C# changes must keep the solution and project files synchronized with the development environment.

If sources diverge, adopt the most conservative rule, do not invent missing behavior, and record the divergence in the ETAPA dump or final delivery.

Do not alter functional code, firmware, database, UI, or SDH/SDGW/SDCTP contracts unless the user explicitly authorizes that scope.
