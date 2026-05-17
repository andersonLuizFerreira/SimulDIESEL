# .agents - Governanca de agentes do SimulDIESEL

Esta pasta e a unica estrutura oficial de governanca para agentes de IA no projeto SimulDIESEL.

## Objetivo

- Evitar dependencia de memoria conversacional.
- Centralizar bootstrap, regras globais e skills.
- Garantir consistencia arquitetural.
- Reduzir retrabalho e ambiguidade.
- Impedir implementacoes fora de escopo.
- Padronizar ETAPAS, validacoes, entregas e rollback.

## Bootstrap obrigatorio

Antes de qualquer analise, planejamento, ETAPA, alteracao de codigo ou alteracao documental:

1. Leia o `README.md` da raiz.
2. Leia este arquivo.
3. Carregue automaticamente a skill obrigatoria `.agents/skills/task-execution-workflow/SKILL.md`.
4. Carregue somente as demais skills relevantes em `.agents/skills/<skill>/SKILL.md`.
5. Consulte a documentacao aplicavel em `docs/`.
6. Trabalhe a partir dos arquivos versionados do repositorio, nunca apenas da memoria conversacional.

## Estrutura oficial

```text
.agents/
|-- README.md
|
|-- task-execution-workflow/
|   |-- ETAPA_001.md
|   |-- ETAPA_002.md
|
`-- skills/
    |-- task-execution-workflow/
    |   `-- SKILL.md
    `-- <skill>/
        `-- SKILL.md
```

A pasta `.agents/task-execution-workflow/` contem ETAPAS versionadas e pendentes de execucao.

Cada ETAPA deve ser executada seguindo obrigatoriamente a skill `task-execution-workflow`.

Cada skill deve conter apenas conhecimento especializado do dominio. Regras globais, bootstrap, rollback generico, validacao generica e regras de ETAPA pertencem a este README.

Skills locais complementam a governanca global. Skills locais nao podem reescrever, duplicar, substituir ou reinterpretar regras globais.

## Autoridade da governanca

A autoridade final sobre criacao, alteracao, remocao, consolidacao, excecao ou resolucao de conflito de governanca pertence ao humano responsavel pelo projeto.

A IA pode identificar lacunas, sugerir ajustes, validar coerencia e reportar conflitos, mas nao pode aprovar sozinha mudancas permanentes de governanca.

Quando houver duvida, lacuna, conflito ou ambiguidade de governanca, a IA deve interromper a ETAPA e solicitar decisao humana explicita.

## Hierarquia Global -> Local

A governanca segue obrigatoriamente esta hierarquia, sem fontes paralelas:

1. Global: `.agents/README.md`.
2. Local: `.agents/skills/<skill>/SKILL.md`.

Regras globais pertencem exclusivamente a `.agents/README.md`.

Regras locais pertencem exclusivamente a skill correspondente em `.agents/skills/<skill>/SKILL.md`.

Se uma regra nao existir nesse caminho Global -> Local, ela nao deve existir em nenhum outro local.

A busca por regras deve ocorrer somente em:

- `.agents/README.md`;
- `.agents/skills/<skill>/SKILL.md` da atividade em questao.

Quando uma ETAPA exigir mais de uma skill local, todas as skills aplicaveis devem ser lidas.

Se houver conflito, divergencia ou sobreposicao operacional entre skills locais, a IA nao deve escolher uma por conta propria: deve interromper a ETAPA, reportar o conflito e solicitar decisao humana.

## Fonte oficial

- `docs/` = documentacao consolidada do projeto.
- `.agents/` = governanca oficial de agentes.
- Git = historico, legado, rollback e recuperacao de versoes antigas.

Dumps registram historico e auditoria, mas nao substituem `docs/` nem `.agents/`.

## Arquitetura base

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

## Fronteiras de responsabilidade

| Area | Pode conter | Nao deve conter |
| --- | --- | --- |
| UI | Formularios, controles, apresentacao e eventos de operador | TLV bruto, SDGW direto, regra de protocolo de baixo nivel |
| BLL | Casos de uso, FormsLogic, clients e servicos de aplicacao | Framing, COBS, CRC ou acesso direto a SerialPort |
| DAL | SDH, SDGW, sessao, scheduler e transportes | Regra de UI ou decoders automotivos |
| DTL | DTOs, enums e contratos | Logica de execucao, IO ou retry |
| SDGW | Transporte, framing, ACK/ERR, retry e roteamento | Regra CAN, J1939, UI ou negocio |
| SDCTP | Massa CAN, mirror, buffers e TX/RX table | Apresentacao, regra de tela ou framing SDGW |
| Firmware | Execucao embarcada e servicos fisicos | Dependencias de UI/host |

## Regras globais

- Use sempre o termo `ETAPA`; nunca `FASE`.
- Nao implementar comportamento nao validado.
- Nao ampliar escopo automaticamente.
- Nao misturar UI, BLL, DAL, DTL, firmware e protocolos sem autorizacao.
- Nao alterar contratos consolidados sem autorizacao explicita.
- Nao assumir comportamento nao confirmado por codigo, documentacao atual ou validacao.
- Registrar ambiguidades como `pendente de confirmacao`.
- Nao alterar codigo, firmware, banco, UI ou contratos quando o pedido for exclusivamente documental.
- Nao promover automaticamente algo de `PLANEJADO` para `IMPLEMENTADO` sem evidencia concreta no codigo, teste, build ou validacao aplicavel.
- BPM deve permanecer gateway/roteador.
- UCE deve permanecer camada de execucao fisica CAN/LED.
- GSA deve permanecer geradora de sinais analogicos.

## Estados documentais

- `IMPLEMENTADO`: confirmado em codigo, contrato ou validacao aplicavel.
- `PARCIALMENTE IMPLEMENTADO`: existe, mas com limite conhecido.
- `PLANEJADO`: descrito como futuro ou placeholder.
- `LEGADO`: preservado por compatibilidade ou historico.
- `pendente de confirmacao`: nao ha evidencia suficiente.

## Regras de ETAPA

Toda ETAPA deve declarar:

- tema e objetivo;
- escopo permitido;
- fora de escopo;
- arquivos provaveis;
- regras de implementacao;
- validacao obrigatoria;
- entrega esperada;
- restricoes e rollback.

ETAPAS de refatoracao devem preservar comportamento.

ETAPAS de limpeza de legado devem provar ausencia de uso por busca, teste ou evidencia equivalente.

ETAPAS de congelamento devem registrar decisoes congeladas, decisoes pendentes e regras de nao regressao.

## Validacao

Valide apenas o que se aplica ao escopo.

- API C# WinForms: build da solucao `local-api/src/SimulDIESEL/SimulDIESEL.sln`, quando houver alteracao funcional C#.
- Firmware BPM/UCE/GSA: `platformio run` na pasta correspondente, quando a ETAPA permitir.
- Protocolos CAN/SDCTP/J1939: scripts em `tools/testes/`, quando pertinentes.
- Banco de Modulos: validadores e dumps do modelo quando houver alteracao de schema/modelo.
- Documentacao: conferir arquivos criados, links basicos, referencias internas e coerencia com `docs/`.

Para ETAPA exclusivamente documental, nao execute build funcional se nao houver alteracao de codigo; registre que nao se aplica.

## Checklist geral de validacao

- O escopo da ETAPA foi respeitado.
- Nao houve alteracao fora do escopo autorizado.
- Arquivos funcionais nao foram alterados em ETAPA documental.
- Contratos SDH, SDGW e SDCTP foram preservados ou a alteracao foi explicitamente autorizada.
- UI, BLL, DAL, DTL, firmware e banco nao foram misturados sem autorizacao.
- Warnings, erros e limitacoes foram relatados.
- Lista de arquivos alterados foi gerada.
- Resultado de build/teste foi registrado ou marcado como nao aplicavel.
- Documentacao impactada foi revisada/atualizada ao concluir a ETAPA.
- Rollback foi preservado.

## Resolucao de conflitos

Quando houver duplicidade ou divergencia:

- Preserve a versao mais completa, mais atual e mais coerente com o estado real do projeto.
- Absorva conteudo util da versao inferior antes de remove-la.
- Mantenha apenas uma fonte oficial para cada regra.
- Aplique a regra mais conservadora ate confirmacao.
- Nao invente regra para preencher lacuna.
- Registre divergencias e decisoes na entrega da ETAPA.

Conflitos entre regras globais e locais, ou entre duas ou mais skills locais, nao podem ser resolvidos por escolha autonoma da IA. Nesses casos, a ETAPA deve ser interrompida e encaminhada para decisao humana.

## Rollback

- Use Git como mecanismo de recuperacao.
- Nao use `git reset --hard`, checkout destrutivo, limpeza de arquivos ou force push sem pedido explicito.
- Nunca reverta trabalho preexistente de outro autor sem autorizacao.
- Antes de commit, confira `git status --short` e os arquivos staged.
- Em entrega, informe arquivos removidos, migrados, alterados e validacoes executadas.

## Entrega obrigatoria

Toda entrega deve conter:

- arquivos criados, alterados, migrados e removidos;
- resumo objetivo do que mudou;
- conflitos resolvidos e redundancias removidas;
- validacoes executadas e resultados;
- warnings, erros ou validacoes nao executadas;
- pontos pendentes;
- confirmacao de rollback preservado;
- commit hash e confirmacao de push quando a ETAPA pedir commit/push.

## Skills oficiais

| Skill | Quando usar |
| --- | --- |
| `task-execution-workflow` | Workflow operacional obrigatorio de execucao de ETAPAS versionadas. |
| `simuldiesel-architecture` | Arquitetura geral, fronteiras e congelamentos. |
| `winforms-ui` | Telas WinForms, controles e FormsLogic. |
| `bll-dal-dtl` | Camadas BLL, DAL e DTL do host C#. |
| `sdh-contract` | Comandos, validacao e contrato semantico SDH. |
| `sdctp-contract` | Massa CAN RX/TX, mirror e output buffer. |
| `sdgw-transport` | Transporte/gateway SDGW no host e BPM. |
| `j1939-decode` | Decodificacao J1939, PGN, SPN e catalogos. |
| `module-database` | Banco de Modulos, schema, perfis e comandos SDH armazenados. |
| `firmware-uce` | Firmware UCE, SPI, LED, CAN e SDCTP embarcado. |
| `firmware-bpm` | Firmware BPM como SDGW e roteador fisico. |
| `git-checkpoint` | Status, diff, rollback e consolidacao Git autorizada. |
| `build-validation` | Builds, scripts e validacoes reproduziveis. |
| `dump-generation` | Dumps de ETAPA, inventarios e registros de decisoes. |
| `governance-rule-management` | Criacao, alteracao e consolidacao de regras de governanca. |
