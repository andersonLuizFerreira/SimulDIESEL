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
- Documentos de governanca diretamente autorizados pela ETAPA.

## Escopo proibido

- Implementar regra mecanicamente sem avaliacao operacional.
- Criar estruturas paralelas de governanca.
- Alterar comportamento funcional do projeto.
- Resolver conflitos de governanca por interpretacao propria.

## Padroes do projeto

- A IA deve validar se a regra proposta faz sentido operacional para a maquina.
- A documentacao deve ser deterministica, objetiva e sem ambiguidades interpretativas para a maquina.
- Antes de alterar governanca, a IA deve verificar se ja existe regra equivalente, redundante, conflitante, sobreposta ou de mesmo impacto.
- Nao e permitida duplicidade de regras.
- Ao detectar duplicidade, conflito, divergencia ou ambiguidade, a IA deve interromper a ETAPA e comunicar imediatamente ao humano.
- Havendo duvida interpretativa, a IA deve interromper a ETAPA e questionar o humano antes de implementar.
- Skills locais nao podem violar regras globais implicitamente.
- Violacoes locais de regras globais exigem secao explicita `EXCECOES`.

## EXCECOES

Quando uma skill local precisar violar expressamente uma regra global, a secao `EXCECOES` deve declarar:

- regra global afetada;
- motivo da excecao;
- escopo exato da excecao;
- limite operacional da excecao;
- temporalidade da excecao: temporaria, experimental ou permanente.

## Checklist de validacao

- [ ] A estrutura oficial de governanca foi identificada antes da alteracao.
- [ ] Foi verificada a existencia de regra equivalente, redundante, conflitante ou sobreposta.
- [ ] Nao foi criada duplicidade de regra.
- [ ] Conflitos, divergencias, ambiguidades ou duvidas interpretativas foram comunicados ao humano antes da alteracao.
- [ ] A regra criada ou alterada e operacionalmente compreensivel para a maquina.
- [ ] A regra nao viola regra global implicitamente.
- [ ] Excecoes explicitas, quando existirem, declaram todos os campos obrigatorios.

## Checklist de entrega

- [ ] Arquivos criados ou alterados.
- [ ] Estrutura final utilizada.
- [ ] Confirmacao de ausencia de conflito de governanca.
- [ ] Confirmacao de ausencia de duplicidade documental.
- [ ] Resumo objetivo da ETAPA executada.
