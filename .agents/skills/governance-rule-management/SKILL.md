# Nome

Governance Rule Management

## Objetivo

Orientar agentes de IA na criacao, alteracao, remocao e consolidacao de regras de governanca do projeto SimulDIESEL.

## Quando usar

Use obrigatoriamente antes de criar, alterar, remover ou consolidar qualquer regra de governanca do projeto.

## Quando nao usar

Nao use para alterar codigo-fonte da aplicacao, firmware, banco de dados ou arquitetura funcional do SimulDIESEL.

## Escopo permitido

- `.agents/README.md`
- `.agents/skills/<skill>/SKILL.md`

## Escopo proibido

- Implementar regra mecanicamente sem avaliacao operacional.
- Criar estruturas paralelas de governanca.
- Alterar comportamento funcional do projeto.
- Resolver conflitos de governanca por interpretacao propria.
- Aprovar sozinho mudancas permanentes de governanca.

## Autoridade humana

A autoridade final sobre criacao, alteracao, remocao, consolidacao, excecao ou resolucao de conflito de governanca pertence ao humano responsavel pelo projeto.

A IA pode identificar lacunas, sugerir ajustes, validar coerencia e reportar conflitos, mas nao pode aprovar sozinha mudancas permanentes de governanca.

Quando houver duvida, lacuna, conflito ou ambiguidade de governanca, a IA deve interromper a ETAPA e solicitar decisao humana explicita.

## Padroes do projeto

- A IA deve validar se a regra proposta faz sentido operacional para a maquina.
- A documentacao deve ser deterministica, objetiva e sem ambiguidades interpretativas para a maquina.
- A governanca segue obrigatoriamente a hierarquia Global -> Local: primeiro `.agents/README.md`, depois `.agents/skills/<skill>/SKILL.md`.
- Regras globais pertencem exclusivamente a `.agents/README.md`.
- Regras locais pertencem exclusivamente a skill correspondente em `.agents/skills/<skill>/SKILL.md`.
- Skills locais complementam a governanca global. Skills locais nao podem reescrever, duplicar, substituir ou reinterpretar regras globais.
- Se uma regra nao existir nesse caminho Global -> Local, ela nao deve existir em nenhum outro local.
- A busca por regras deve ocorrer somente em `.agents/README.md` e na skill da atividade em questao.
- Quando uma ETAPA exigir mais de uma skill local, todas as skills aplicaveis devem ser lidas.
- Se houver conflito, divergencia ou sobreposicao operacional entre skills locais, a IA deve interromper a ETAPA, reportar o conflito e solicitar decisao humana.
- Antes de alterar governanca, a IA deve verificar se ja existe regra equivalente, redundante, conflitante, sobreposta ou de mesmo impacto.
- Nao e permitida duplicidade de regras.
- Ao detectar duplicidade, conflito, divergencia ou ambiguidade, a IA deve interromper a ETAPA e comunicar imediatamente ao humano.
- Havendo duvida interpretativa, a IA deve interromper a ETAPA e questionar o humano antes de implementar.
- Skills locais nao podem violar regras globais implicitamente.
- Violacoes locais de regras globais exigem secao explicita `EXCEÇÕES`.

## EXCEÇÕES

Quando uma skill local precisar violar expressamente uma regra global, a secao `EXCEÇÕES` deve declarar:

- regra global afetada;
- motivo da excecao;
- escopo exato da excecao;
- limite operacional da excecao;
- temporalidade da excecao: temporaria, experimental ou permanente.

## Checklist de validacao

- [ ] A estrutura oficial de governanca foi identificada antes da alteracao.
- [ ] A hierarquia Global -> Local foi respeitada.
- [ ] A busca por regras ocorreu somente em `.agents/README.md` e na skill da atividade em questao.
- [ ] Foi verificada a existencia de regra equivalente, redundante, conflitante ou sobreposta.
- [ ] Nao foi criada duplicidade de regra.
- [ ] Conflitos, divergencias, ambiguidades ou duvidas interpretativas foram comunicados ao humano antes da alteracao.
- [ ] A regra criada ou alterada e operacionalmente compreensivel para a maquina.
- [ ] A autoridade humana final foi preservada.
- [ ] A regra nao viola regra global implicitamente.
- [ ] Excecoes explicitas, quando existirem, declaram todos os campos obrigatorios.

## Checklist de entrega

- [ ] Arquivos criados ou alterados.
- [ ] Estrutura final utilizada.
- [ ] Confirmacao de ausencia de conflito de governanca.
- [ ] Confirmacao de ausencia de duplicidade documental.
- [ ] Resumo objetivo da ETAPA executada.
