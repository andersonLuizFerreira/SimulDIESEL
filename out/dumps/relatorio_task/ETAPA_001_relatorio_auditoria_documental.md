# ETAPA_001 - Relatorio de auditoria documental

## Escopo

Auditoria documental de `docs/` conforme `.agents/task-execution-workflow/ETAPA_001_saneamento_docs.md`, `.agents/README.md` e `.agents/skills/docs-governance/SKILL.md`.

Nao foram alterados firmware, codigo-fonte funcional, banco, UI ou contratos SDH/SDGW/SDCTP.

## Sumario executivo

- `docs/` contem 138 arquivos Markdown.
- Distribuicao principal: 82 arquivos em `docs/official/`, 51 em `docs/legacy/`, 1 em `docs/architecture/`, alem de `README.md`, `00-INDICE.md`, `DOCUMENTATION_RULES.md` e `ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`.
- A arvore alvo autorizada em `.agents/skills/docs-governance/SKILL.md` e apenas:

```text
docs/
|-- README.md
|-- 00-INDICE.md
|-- DOCUMENTATION_RULES.md
```

- Portanto, `official/`, `legacy/`, `architecture/` e o arquivo de ETAPA dentro de `docs/` sao divergencias estruturais relevantes a sanear por decisao humana.
- O verificador basico de links Markdown internos nao encontrou links quebrados.
- Foram encontradas paginas sem navegacao padrao e sem glossario, principalmente em `docs/legacy/`, alem de alguns arquivos fora da arvore oficial viva.

## Estruturas paralelas ou redundantes

- `docs/official/`: contem a arvore documental viva citada por `docs/00-INDICE.md`, mas conflita com a arvore alvo atualmente autorizada pela skill `docs-governance`.
- `docs/legacy/`: acervo historico ainda referenciado por `docs/README.md` e `docs/00-INDICE.md`, mas a skill `docs-governance` declara que nao deve haver fontes documentais concorrentes como `legacy`.
- `docs/architecture/`: contem `sdh_sdctp_sdgw_contracts.md` fora da arvore alvo e fora da navegacao oficial indicada pela skill.
- `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`: arquivo operacional de ETAPA localizado em `docs/`, concorrendo com a governanca atual que centraliza ETAPAS em `.agents/task-execution-workflow/`.

## Contradicoes e conflitos documentais

- `docs/DOCUMENTATION_RULES.md` declara ser a verdade oficial da estrutura documental ativa; a governanca global define `.agents/README.md` e `.agents/skills/<skill>/SKILL.md` como caminho oficial de regras para agentes.
- `docs/DOCUMENTATION_RULES.md` prioriza, em divergencias, codigo-fonte implementado, contratos tecnicos efetivos e documentacao existente. A skill `docs-governance` proibe escolher automaticamente entre docs e codigo enquanto a documentacao estiver em consolidacao.
- `docs/README.md` preserva e aponta para `docs/legacy/`; a skill `docs-governance` declara que pastas legadas dentro de `docs/` podem induzir erro e devem ser saneadas por ETAPA propria.
- `docs/00-INDICE.md` aponta para `docs/official/` e `docs/legacy/`; a arvore alvo da skill nao autoriza essas pastas como estrutura final.
- `docs/official/09-desenvolvimento/01-organizacao-repositorio.md` documenta `docs/official/` como vigente e `docs/legacy/` como consulta historica, divergindo do alvo atual da skill.

## Duplicidades

Duplicidade exata por hash:

- `docs/legacy/04_desenvolvimento/README.md` e `docs/legacy/05_hardware/README.md`.
- Grupo de arquivos vazios/equivalentes em `docs/legacy/01_arquitetura/01_protocolos/README.md`, `docs/legacy/01_arquitetura/05_babyboards/README.md`, `docs/legacy/01_arquitetura/00_contratos/README.md`, `docs/legacy/01_arquitetura/03_gateway/README.md`, `docs/legacy/01_arquitetura/04_pc/health-service.md`, `docs/legacy/01_arquitetura/04_pc/link-engine.md`, `docs/legacy/01_arquitetura/04_pc/sggw-client.md`, `docs/legacy/01_arquitetura/04_pc/serial-transport.md`, `docs/legacy/01_arquitetura/04_pc/README.md` e `docs/legacy/01_arquitetura/02_integracoes/README.md`.
- `docs/legacy/01_arquitetura/README.md`, `docs/legacy/00_visao-geral/README.md` e `docs/legacy/00_visao-geral/MASTER_SPEC.md`.
- `docs/legacy/04_desenvolvimento/adr/README.md`, `docs/legacy/01_arquitetura/05_babyboards/gsa/README.md`, `docs/legacy/01_arquitetura/05_babyboards/gsa/PINOUT.md` e `docs/legacy/01_arquitetura/05_babyboards/gsa/FLUXO_EXECUCAO.md`.
- `docs/legacy/01_arquitetura/01_protocolos/sggw/README.md`, `docs/legacy/01_arquitetura/05_babyboards/gsa/CONFIGURACAO.md` e `docs/legacy/01_arquitetura/05_babyboards/gsa/ARQUITETURA.md`.

Titulo duplicado relevante:

- `# GSA - Estrutura interna` aparece em multiplos arquivos legados de desenvolvimento, hardware e GSA.

## Links internos

Resultado do verificador basico de links Markdown relativos:

- Links quebrados encontrados: 0.

Observacao: a validacao foi sintatica e baseada em existencia de caminho. Ela nao valida ancora Markdown nem coerencia semantica do destino.

## Navegacao

Foram identificados documentos sem o padrao completo de topo com retorno ao pai imediato e retorno ao indice geral.

Principais grupos:

- `docs/README.md`.
- `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`.
- `docs/architecture/sdh_sdctp_sdgw_contracts.md`.
- `docs/official/02-arquitetura/12-banco-local-api.md`.
- A maior parte de `docs/legacy/`.

## Glossario

Foram identificados documentos sem secao `## Glossario`.

Principais grupos:

- `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`.
- `docs/architecture/sdh_sdctp_sdgw_contracts.md`.
- `docs/official/02-arquitetura/12-banco-local-api.md`.
- `docs/official/06-protocolos/07-uce-sdh-tlv.md`.
- A maior parte de `docs/legacy/`.
- `docs/official/04-firmware/boards/UCE/11-uce.md`.

## Divergencias docs x codigo

Divergencias registradas sem decidir fonte de verdade:

- `docs/official/04-firmware/01-arquitetura-firmware.md` registra boards planejadas e firmware BPM/GSA; o codigo atual tambem contem firmware UCE em `hardware/firmware/UCE - Unidade de comunicacao externa/`, que precisa de revisao documental especifica.
- `docs/official/04-firmware/boards/UCE/11-uce.md` e `docs/official/06-protocolos/07-uce-sdh-tlv.md` existem, mas nao aparecem em `docs/00-INDICE.md`, deixando a navegacao oficial incompleta para a UCE.
- O caminho de firmware GSA documentado em alguns pontos usa grafia diferente da pasta real `hardware/firmware/GSA - Gerador de sinais analogicos` com acento em `analógicos`; a consistencia de caminho deve ser validada antes de consolidacao.
- A documentacao historica ainda usa `SGGW`, enquanto a regra vigente consolidada usa `SDGW (SimulDiesel GateWay)`.

## Conteudo consolidado em skill

Conteudo util de `docs/DOCUMENTATION_RULES.md` absorvido em `.agents/skills/docs-governance/SKILL.md`:

- exigencia de retorno ao pai imediato e ao indice geral no topo das paginas oficiais;
- indice geral como unica pagina autorizada a apontar para qualquer pagina da arvore viva;
- nomenclatura oficial `SDGW (SimulDiesel GateWay)`.

Conteudo nao absorvido por conflito:

- prioridade automatica `codigo-fonte implementado > contratos tecnicos efetivos > documentacao existente`, por conflitar com a regra local atual que proibe escolher automaticamente entre docs e codigo durante consolidacao.

## Estrutura alvo proposta

Proposta conservadora, pendente de decisao humana:

```text
docs/
|-- README.md
|-- 00-INDICE.md
|-- DOCUMENTATION_RULES.md
```

Opcoes para saneamento posterior:

1. Consolidar conteudo util de `docs/official/` nos tres documentos alvo ou em nova arvore autorizada explicitamente pelo humano.
2. Migrar historico util de `docs/legacy/` para Git/dumps de auditoria, mantendo em `docs/` apenas referencias vivas.
3. Mover ETAPAS documentais para `.agents/task-execution-workflow/` ou registrar que `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md` e legado operacional.
4. Integrar ou remover `docs/architecture/sdh_sdctp_sdgw_contracts.md` apos absorcao do conteudo valido.

## Plano de saneamento proposto

1. Obter decisao humana sobre a arvore final: manter alvo minimalista atual ou autorizar uma nova arvore oficial com subpastas.
2. Classificar cada arquivo de `docs/official/` como consolidar, manter por autorizacao, migrar para outro no, ou remover apos absorcao.
3. Classificar cada arquivo de `docs/legacy/` como conteudo ja absorvido, conteudo util a absorver, ou historico sem necessidade documental viva.
4. Remover ou migrar documentos operacionais de ETAPA que estejam em `docs/`.
5. Atualizar `docs/README.md`, `docs/00-INDICE.md`, `docs/DOCUMENTATION_RULES.md` e `.agents/skills/docs-governance/SKILL.md` de forma sincronizada.
6. Reexecutar validacao de links, navegacao e glossarios.

## Validacoes executadas

- Coerencia com `.agents/README.md`: validada; conflitos documentais foram registrados, nao resolvidos autonomamente.
- Coerencia com `.agents/skills/docs-governance/SKILL.md`: validada; estrutura atual diverge da arvore alvo.
- Ausencia de governanca documental paralela: verificacao executada; governanca paralela identificada e registrada em `docs/DOCUMENTATION_RULES.md`.
- Navegacao documental basica: validada parcialmente; links internos relativos nao quebrados, mas varios documentos sem navegacao de topo.
- Build funcional: nao aplicavel; ETAPA exclusivamente documental.

## Pontos pendentes

- Decisao humana sobre manutencao ou remocao de `docs/official/` e `docs/legacy/`.
- Decisao humana sobre a prioridade entre codigo, contratos e docs durante divergencias.
- ETAPA propria para saneamento estrutural efetivo, com absorcao de conteudo antes de qualquer remocao.

## Rollback

Rollback preservado via Git. Nenhum arquivo funcional foi alterado.
