# Dump - Criacao de .agents/README.md, Skills e Prompts ETAPA

Data: 2026-05-12

Escopo: ETAPA exclusivamente documental e organizacional para criar orientacoes reutilizaveis de agentes no projeto SimulDIESEL.

## Arquivos criados

### Raiz

- `.agents/README.md`

### Guias em `.agents/`

- `.agents/README.md`
- `.agents/README.md`
- `.agents/README.md`
- `.agents/README.md`
- `.agents/README.md`

### Skills em `.agents/skills/`

- `.agents/skills/simuldiesel-architecture/SKILL.md`
- `.agents/skills/winforms-ui/SKILL.md`
- `.agents/skills/bll-dal-dtl/SKILL.md`
- `.agents/skills/sdh-contract/SKILL.md`
- `.agents/skills/sdctp-contract/SKILL.md`
- `.agents/skills/sdgw-transport/SKILL.md`
- `.agents/skills/j1939-decode/SKILL.md`
- `.agents/skills/module-database/SKILL.md`
- `.agents/skills/firmware-uce/SKILL.md`
- `.agents/skills/firmware-bpm/SKILL.md`
- `.agents/skills/git-checkpoint/SKILL.md`
- `.agents/skills/build-validation/SKILL.md`
- `.agents/skills/dump-generation/SKILL.md`

### Dump

- `out/dumps/agents_skills_prompts_creation.md`

## Resumo do conteudo por arquivo

| Arquivo | Resumo |
| --- | --- |
| `.agents/README.md` | Orientacao principal para agentes: visao geral, arquitetura, fontes de verdade, regras de escopo, nomenclatura, ETAPA, validacao, entrega, commits/tags, rollback e documentacao. |
| `.agents/README.md` | Mapa de uso da estrutura de agentes e relacao das skills disponiveis. |
| `.agents/README.md` | Convencoes de camada, estados documentais, contratos e entrega. |
| `.agents/README.md` | Templates para 12 tipos de ETAPA: UI, BLL, DAL, DTO/DTL, Firmware, Protocolo, Banco, Validacao, Refatoracao controlada, Limpeza de legado, Congelamento e Consolidacao Git. |
| `.agents/README.md` | Checklists gerais e por area para API, firmware, protocolos, banco e documentacao. |
| `.agents/README.md` | Modelo para congelamento de contrato/componente, com decisoes congeladas, pendencias e nao regressao. |
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

- Criar a nova documentacao de agentes em `.agents/`, sem alterar a arvore oficial em `docs/official/`.
- Manter `.agents/README.md` na raiz como ponto de entrada obrigatorio para agentes.
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

- Para cada nova ETAPA, iniciar pelo `.agents/README.md`, escolher a skill correspondente e copiar o template de `.agents/README.md`.
- Quando uma ETAPA alterar contrato, gerar dump em `out/dumps/` e considerar um documento de congelamento.
- Atualizar `.agents/skills/` quando a arquitetura real mudar, especialmente nas fronteiras SDH/SDCTP/SDGW/J1939.
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
