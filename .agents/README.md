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
2. Carregue as skills relevantes em `.agents/skills/`;
3. Consulte a documentação aplicável em `docs/`.

## Estrutura da governança

```text
.agents/
│
├── README.md
└── skills/
```

## Governança

A pasta `.agents/` é a única estrutura oficial de governança para agentes.

Toda skill, bootstrap, regra de validação, arquitetura e comportamento de agentes deve existir exclusivamente em `.agents/`.

Histórico, rollback e legado pertencem ao Git.

## Visão geral

O SimulDIESEL é uma plataforma de bancada para manutenção, diagnóstico, análise, simulação e validação de centrais e módulos automotivos da linha DIESEL.

Arquitetura base:

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

## Responsabilidades

### `.agents/README.md`

Ponto inicial de entrada para agentes.

Responsável por:

- bootstrap;
- onboarding;
- roteamento;
- organização da governança;
- regras fundamentais;
- arquitetura base.

### `.agents/skills/`

Contém conhecimento especializado reutilizável.

Exemplos:

- arquitetura;
- WinForms;
- firmware;
- SDH;
- SDGW;
- SDCTP;
- J1939;
- Banco de Módulos;
- validação;
- Git.

## Regras fundamentais

- `docs/` é a única fonte documental oficial do projeto.
- Não implementar comportamento não validado.
- Não ampliar escopo automaticamente.
- Não misturar UI, BLL, DAL, DTL, firmware e protocolos sem autorização.
- Registrar ambiguidades como `pendente de confirmação`.
- Usar sempre o termo `ETAPA`; nunca `FASE`.

## Regra principal

Agentes devem trabalhar a partir dos arquivos versionados do repositório, nunca apenas da memória conversacional.

## Fonte oficial

- `docs/` = documentação oficial do projeto.
- `.agents/` = governança oficial de agentes.
