# Dump - Criacao de AGENTS.md, Skills e Prompts ETAPA

Data: 2026-05-12

Escopo: ETAPA exclusivamente documental e organizacional para criar orientacoes reutilizaveis de agentes no projeto SimulDIESEL.

## Arquivos criados

### Raiz

- `AGENTS.md`

### Guias em `docs/agents/`

- `docs/agents/agents_overview.md`
- `docs/agents/project_conventions.md`
- `docs/agents/etapa_prompt_template.md`
- `docs/agents/validation_checklist.md`
- `docs/agents/freeze_checkpoint_template.md`

### Skills em `docs/agents/skills/`

- `docs/agents/skills/simuldiesel-architecture-skill.md`
- `docs/agents/skills/winforms-ui-skill.md`
- `docs/agents/skills/bll-dal-dtl-skill.md`
- `docs/agents/skills/sdh-contract-skill.md`
- `docs/agents/skills/sdctp-contract-skill.md`
- `docs/agents/skills/sdgw-transport-skill.md`
- `docs/agents/skills/j1939-decode-skill.md`
- `docs/agents/skills/module-database-skill.md`
- `docs/agents/skills/firmware-uce-skill.md`
- `docs/agents/skills/firmware-bpm-skill.md`
- `docs/agents/skills/git-checkpoint-skill.md`
- `docs/agents/skills/build-validation-skill.md`
- `docs/agents/skills/dump-generation-skill.md`

### Dump

- `out/dumps/agents_skills_prompts_creation.md`

## Resumo do conteudo por arquivo

| Arquivo | Resumo |
| --- | --- |
| `AGENTS.md` | Orientacao principal para agentes: visao geral, arquitetura, fontes de verdade, regras de escopo, nomenclatura, ETAPA, validacao, entrega, commits/tags, rollback e documentacao. |
| `docs/agents/agents_overview.md` | Mapa de uso da estrutura de agentes e relacao das skills disponiveis. |
| `docs/agents/project_conventions.md` | Convencoes de camada, estados documentais, contratos e entrega. |
| `docs/agents/etapa_prompt_template.md` | Templates para 12 tipos de ETAPA: UI, BLL, DAL, DTO/DTL, Firmware, Protocolo, Banco, Validacao, Refatoracao controlada, Limpeza de legado, Congelamento e Consolidacao Git. |
| `docs/agents/validation_checklist.md` | Checklists gerais e por area para API, firmware, protocolos, banco e documentacao. |
| `docs/agents/freeze_checkpoint_template.md` | Modelo para congelamento de contrato/componente, com decisoes congeladas, pendencias e nao regressao. |
| `simuldiesel-architecture-skill.md` | Skill para arquitetura geral, fronteiras, estados e divergencias. |
| `winforms-ui-skill.md` | Skill para UI WinForms e limites contra acesso a TLV/SDGW bruto. |
| `bll-dal-dtl-skill.md` | Skill para host C# em BLL, DAL e DTL. |
| `sdh-contract-skill.md` | Skill para contrato semantico SDH. |
| `sdctp-contract-skill.md` | Skill para massa CAN RX/TX, mirror e output buffer. |
| `sdgw-transport-skill.md` | Skill para transporte/gateway SDGW no host e BPM. |
| `j1939-decode-skill.md` | Skill para decodificacao J1939 sobre `CanFrameDto` e catalogos. |
| `module-database-skill.md` | Skill para Banco de Modulos, schema e relacao com SDH. |
| `firmware-uce-skill.md` | Skill para firmware UCE, SPI, LED, CAN e SDCTP embarcado. |
| `firmware-bpm-skill.md` | Skill para firmware BPM como gateway/roteador. |
| `git-checkpoint-skill.md` | Skill para consolidacao Git com autorizacao explicita. |
| `build-validation-skill.md` | Skill para builds, scripts e validacoes. |
| `dump-generation-skill.md` | Skill para dumps de ETAPA e registro de decisoes. |

## Decisoes adotadas

- Criar a nova documentacao de agentes em `docs/agents/`, sem alterar a arvore oficial em `docs/official/`.
- Manter `AGENTS.md` na raiz como ponto de entrada obrigatorio para agentes.
- Reforcar a arquitetura solicitada: `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- Registrar explicitamente a separacao: SDH semantico, SDGW transporte, SDCTP massa CAN, J1939 decoder sobre `CanFrameDto`.
- Usar regra conservadora para divergencias: registrar como pendente ou divergente, sem decidir arbitrariamente.
- Incluir regras de nao commit, nao branch, nao tag e preservacao de rollback.
- Marcar build funcional como nao aplicavel para esta ETAPA, pois nao houve alteracao de codigo.

## Pontos de incerteza e divergencia

- A documentacao oficial `docs/official/06-protocolos/05-j1939.md` descreve J1939 como ainda nao implementado no momento em que foi escrita, mas a arvore atual contem classes BLL/DTL J1939, catalogos em `Data/Protocols/J1939/` e scripts de validacao em `tools/testes/`. A instrucao criada trata J1939 de forma conservadora: existe codigo de decodificacao, mas suporte operacional completo deve ser confirmado por build/testes/ETAPA especifica.
- A documentacao oficial antiga de firmware UCE pode nao refletir todos os arquivos atuais da UCE, pois o codigo listado na varredura contem estrutura `lib/services/can/sdctp/`, `CanDriver`, `CanService`, tabelas RX/TX e SDCTP. As skills foram escritas usando a leitura mais conservadora dos dumps recentes e da arvore atual.
- Nao ha politica Git formal completa no repositorio; por isso a skill Git registra apenas regras seguras: nao criar branch, commit ou tag sem autorizacao explicita.

## Recomendacoes para uso futuro

- Para cada nova ETAPA, iniciar pelo `AGENTS.md`, escolher a skill correspondente e copiar o template de `docs/agents/etapa_prompt_template.md`.
- Quando uma ETAPA alterar contrato, gerar dump em `out/dumps/` e considerar um documento de congelamento.
- Atualizar `docs/agents/skills/` quando a arquitetura real mudar, especialmente nas fronteiras SDH/SDCTP/SDGW/J1939.
- Em ETAPAS de J1939, revisar a divergencia entre documentacao oficial antiga e codigo atual antes de declarar suporte implementado.
- Formalizar politica Git em documento proprio antes de exigir branch/tag/merge como regra institucional do projeto.

## Validacao desta ETAPA

- Criacao documental realizada somente em Markdown.
- Nenhum codigo-fonte funcional foi alterado por esta ETAPA.
- Nenhum firmware foi alterado.
- Nenhum banco de dados foi alterado.
- Nenhuma UI foi alterada.
- Nenhum contrato SDH, SDGW ou SDCTP foi alterado.
- Nenhum commit, branch ou tag foi criado.

## Validacoes nao executadas

- Build C# nao executado: nao aplicavel, pois a ETAPA nao alterou codigo funcional.
- Build PlatformIO nao executado: nao aplicavel, pois a ETAPA nao alterou firmware.
- Validadores de banco nao executados: nao aplicavel, pois a ETAPA nao alterou schema ou `modules.db`.
