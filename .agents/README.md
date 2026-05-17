# .agents - Governança de agentes do SimulDIESEL

Esta pasta contém a governança operacional oficial para agentes de IA no projeto SimulDIESEL.

## Objetivo

- evitar dependência de memória conversacional;
- centralizar bootstrap, regras e skills;
- garantir consistência arquitetural;
- reduzir retrabalho;
- impedir implementações fora de escopo;
- padronizar ETAPAS, validações e entregas.

## Fluxo obrigatório de leitura

Antes de qualquer análise, planejamento, ETAPA, alteração de código ou alteração documental:

1. Leia `.agents/README.md`;
2. Carregue somente as skills relevantes em `.agents/skills/`;
3. Consulte a documentação aplicável em `docs/`.

## Estrutura da governança

```text
.agents/
│
├── README.md
└── skills/
    └── <dominio>/SKILL.md
```

## Fonte oficial

- `docs/` = documentação oficial do projeto.
- `.agents/` = governança oficial de agentes.
- Git = histórico, legado, rollback e recuperação de versões antigas.

## Regra principal

Agentes devem trabalhar a partir dos arquivos versionados do repositório, nunca apenas da memória conversacional.

A pasta `.agents/` é a única estrutura oficial de governança para agentes.

Toda skill, bootstrap, regra de validação, arquitetura e comportamento de agentes deve existir exclusivamente em `.agents/`.

## Arquitetura base

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

## Regras globais

- Não implementar comportamento não validado.
- Não ampliar escopo automaticamente.
- Não misturar UI, BLL, DAL, DTL, firmware e protocolos sem autorização.
- Não alterar contratos consolidados sem autorização explícita.
- Não assumir comportamento não confirmado por código, documentação atual ou validação.
- Registrar ambiguidades como `pendente de confirmação`.
- Usar sempre o termo `ETAPA`; nunca `FASE`.

## Regras de ETAPA

Toda ETAPA deve declarar:

- tema e objetivo;
- escopo permitido;
- fora de escopo;
- arquivos prováveis;
- regras de implementação;
- validação obrigatória;
- entrega esperada;
- restrições e rollback.

## Regras de validação

Valide apenas o que se aplica ao escopo.

- API C# WinForms: build da solução `local-api/src/SimulDIESEL/SimulDIESEL.sln`.
- Firmware BPM/UCE/GSA: `platformio run` na pasta correspondente, quando a ETAPA permitir.
- Protocolos CAN/SDCTP/J1939: scripts em `tools/testes/`, quando pertinentes.
- Documentação: conferir arquivos criados, links básicos e coerência com `docs/`.

Para ETAPA exclusivamente documental, não execute build funcional se não houver alteração de código; registre que não se aplica.

Nunca promova automaticamente algo de `PLANEJADO` para `IMPLEMENTADO` sem evidência concreta no código, teste, build ou validação aplicável.

## Regras de entrega

Toda entrega deve conter:

- lista de arquivos alterados/criados/removidos;
- resumo objetivo do que mudou;
- validações executadas e resultados;
- warnings, erros ou validações não executadas;
- pontos pendentes;
- confirmação de que rollback foi preservado.

## Regras das skills

Cada skill deve conter apenas conhecimento específico do domínio.

Regras globais não devem ser repetidas dentro das skills, salvo quando forem necessárias para delimitar o escopo do domínio.

Se duas skills contiverem regras conflitantes, preserve a regra mais atual, mais completa e mais coerente com o estado real do projeto.
