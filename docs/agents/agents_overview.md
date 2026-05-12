# Agents Overview - SimulDIESEL

## Objetivo

Esta pasta cria uma base reutilizavel para agentes de IA trabalharem no SimulDIESEL com menor risco de misturar camadas, alterar contratos sem autorizacao ou inventar estado inexistente.

## Estrutura criada

- `AGENTS.md`: orientacao principal na raiz do repositorio.
- `.codex/instructions.md`: bootstrap oficial CODEX e ordem de leitura.
- `docs/agents/project_conventions.md`: convencoes de escopo, nomes, ETAPAS e entrega.
- `docs/agents/etapa_prompt_template.md`: templates padronizados para ETAPAS.
- `docs/agents/validation_checklist.md`: checklists de validacao por area.
- `docs/agents/freeze_checkpoint_template.md`: modelo para congelamento tecnico.
- `docs/agents/skills/`: skills reutilizaveis por dominio.

## Como usar

1. Leia `AGENTS.md` na raiz.
2. Leia `.codex/instructions.md`.
3. Identifique a area da ETAPA.
4. Leia a skill correspondente em `.codex/skills/`.
5. Consulte `docs/agents/` e `docs/agents/skills/` quando precisar da documentacao humana equivalente.
6. Use o template de ETAPA adequado.
7. Registre validacao, arquivos alterados e dump quando solicitado.

## Mapa de skills

| Skill | Uso principal |
| --- | --- |
| `simuldiesel-architecture-skill.md` | Arquitetura geral e fronteiras de responsabilidade |
| `winforms-ui-skill.md` | UI WinForms |
| `bll-dal-dtl-skill.md` | Camadas BLL, DAL e DTL |
| `sdh-contract-skill.md` | Contrato semantico SDH |
| `sdctp-contract-skill.md` | Massa CAN RX/TX e sincronizacao |
| `sdgw-transport-skill.md` | Transporte/gateway |
| `j1939-decode-skill.md` | Decodificacao J1939 |
| `module-database-skill.md` | Banco de Modulos |
| `firmware-uce-skill.md` | Firmware UCE |
| `firmware-bpm-skill.md` | Firmware BPM |
| `git-checkpoint-skill.md` | Checkpoints Git sem automacao indevida |
| `build-validation-skill.md` | Build e validacoes |
| `dump-generation-skill.md` | Dumps de ETAPA |

## Regra de conservadorismo

Se documentos e codigo divergirem, registre a divergencia no dump e mantenha a instrucao mais restritiva ate confirmacao.
