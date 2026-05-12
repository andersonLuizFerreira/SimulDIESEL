# Codex Skills - SimulDIESEL

Estas skills estruturadas espelham a documentacao humana em `docs/agents/skills/` para uso pelo CODEX. A fonte principal do projeto continua sendo `AGENTS.md`.

Em caso de divergencia entre `.codex/skills/`, `docs/agents/skills/` e `AGENTS.md`, adote a regra mais conservadora e registre a divergencia no dump da ETAPA.

| Skill Codex | Documentacao humana | Quando usar |
| --- | --- | --- |
| `simuldiesel-architecture` | `docs/agents/skills/simuldiesel-architecture-skill.md` | Arquitetura geral, fronteiras e congelamentos. |
| `winforms-ui` | `docs/agents/skills/winforms-ui-skill.md` | Telas WinForms, controles e FormsLogic. |
| `bll-dal-dtl` | `docs/agents/skills/bll-dal-dtl-skill.md` | Camadas BLL, DAL e DTL do host C#. |
| `sdh-contract` | `docs/agents/skills/sdh-contract-skill.md` | Comandos, validacao e contrato semantico SDH. |
| `sdctp-contract` | `docs/agents/skills/sdctp-contract-skill.md` | Massa CAN RX/TX, mirror e output buffer. |
| `sdgw-transport` | `docs/agents/skills/sdgw-transport-skill.md` | Transporte/gateway SDGW no host e BPM. |
| `j1939-decode` | `docs/agents/skills/j1939-decode-skill.md` | Decodificacao J1939, PGN, SPN e catalogos. |
| `module-database` | `docs/agents/skills/module-database-skill.md` | Banco de Modulos, schema, perfis e comandos SDH armazenados. |
| `firmware-uce` | `docs/agents/skills/firmware-uce-skill.md` | Firmware UCE, SPI, LED, CAN e SDCTP embarcado. |
| `firmware-bpm` | `docs/agents/skills/firmware-bpm-skill.md` | Firmware BPM como SDGW e roteador fisico. |
| `git-checkpoint` | `docs/agents/skills/git-checkpoint-skill.md` | Status, diff, rollback e consolidacao Git autorizada. |
| `build-validation` | `docs/agents/skills/build-validation-skill.md` | Builds, scripts e validacoes reproduziveis. |
| `dump-generation` | `docs/agents/skills/dump-generation-skill.md` | Dumps de ETAPA, inventarios e registros de decisoes. |
