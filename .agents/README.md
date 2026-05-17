# .agents - Governanca de agentes do SimulDIESEL

Esta pasta contem a governanca operacional oficial para agentes de IA no projeto SimulDIESEL.

Objetivo:

- evitar dependencia de memoria conversacional;
- centralizar bootstrap, regras e skills;
- garantir consistencia arquitetural;
- reduzir retrabalho;
- impedir implementacoes fora de escopo;
- padronizar ETAPAS, validacoes e entregas.

## Fluxo obrigatorio de leitura

Antes de qualquer analise, planejamento, ETAPA, alteracao de codigo ou alteracao documental:

1. Leia `.agents/README.md`;
2. Leia `.agents/instructions.md`;
3. Carregue as skills relevantes em `.agents/skills/`;
4. Consulte a documentacao aplicavel em `docs/`.

## Estrutura da governanca

```text
.agents/
│
├── README.md
├── instructions.md
└── skills/
```

## Responsabilidades

### `.agents/README.md`

Ponto inicial de entrada para agentes.

Responsavel por:

- bootstrap;
- onboarding;
- roteamento;
- organizacao da governanca.

### `.agents/instructions.md`

Contem:

- regras operacionais;
- arquitetura;
- restricoes;
- validacoes;
- workflow;
- nomenclatura;
- regras de ETAPA;
- regras de entrega;
- sincronizacao de ambiente.

### `.agents/skills/`

Contem conhecimento especializado reutilizavel.

Exemplos:

- arquitetura;
- WinForms;
- firmware;
- SDH;
- SDGW;
- SDCTP;
- J1939;
- Banco de Modulos;
- validacao;
- dumps;
- Git.

## Regra principal

Agentes devem trabalhar a partir dos arquivos versionados do repositorio, nunca apenas da memoria conversacional.

## Fonte oficial

- `docs/` = documentacao oficial do projeto.
- `.agents/` = governanca oficial de agentes.
