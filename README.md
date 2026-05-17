# SimulDIESEL

SimulDIESEL e uma plataforma de bancada para simulacao, diagnostico e validacao de modulos diesel, com aplicacao local em C# WinForms, protocolos SDH/SDGW/SDCTP, comunicacao com hardware embarcado e suporte a analise CAN/J1939.

Este arquivo e a porta de entrada do repositorio.

## Para humanos

A documentacao oficial do projeto fica em:

- [`docs/`](docs/)
- [`docs/README.md`](docs/README.md)
- [`docs/00-INDICE.md`](docs/00-INDICE.md)

Use `docs/` para entender arquitetura, protocolos, hardware, firmware, banco, testes e regras documentais.

## Para agentes de IA

Antes de analisar, planejar, propor ETAPA, alterar codigo, alterar documentacao ou orientar implementacao, leia obrigatoriamente:

- [`.agents/README.md`](.agents/README.md)
- [`.agents/instructions.md`](.agents/instructions.md)
- [`.agents/skills/`](.agents/skills/)

Agentes nao devem depender apenas de memoria conversacional. A pasta `.agents/` e o ponto oficial de governanca operacional para agentes de IA.

## Fontes de verdade

- `docs/`: documentacao oficial consolidada do projeto.
- `.agents/`: governanca operacional para agentes de IA.
- `out/dumps/`: evidencias temporarias de ETAPA, auditoria e pos-codificacao; nao e documentacao oficial.
- Git: historico, legado, rollback e recuperacao de versoes antigas.

## Regra principal

Humanos entram por `docs/`.

Agentes entram por `.agents/README.md`.

Nada em `out/dumps/` deve ser tratado como fonte oficial do projeto.
