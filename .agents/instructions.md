# SimulDIESEL Agents Bootstrap

Este arquivo e o bootstrap oficial e unico para agentes de IA no projeto SimulDIESEL.

## Ordem obrigatoria de leitura

1. `README.md`
2. `.agents/instructions.md`
3. `.agents/README.md`
4. `.agents/skills/` conforme o tema da ETAPA
5. `docs/`

## Governanca

A pasta `.agents/` e a unica estrutura oficial de governanca para agentes.

Toda skill, bootstrap, regra de validacao, arquitetura e comportamento de agentes deve existir exclusivamente em `.agents/`.

Historico, rollback e legado pertencem ao Git.

`out/dumps/` sao evidencias temporarias e nao documentacao oficial.

## Visao geral

O SimulDIESEL e uma plataforma de bancada para simulacao, diagnostico e validacao de modulos diesel.

Arquitetura base:

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

## Regras fundamentais

- `docs/` e a unica fonte documental oficial do projeto.
- Nao implementar comportamento nao validado.
- Nao ampliar escopo automaticamente.
- Nao misturar UI, BLL, DAL, DTL, firmware e protocolos sem autorizacao.
- Registrar ambiguidades como `pendente de confirmacao`.
- Usar sempre o termo `ETAPA`; nunca `FASE`.
