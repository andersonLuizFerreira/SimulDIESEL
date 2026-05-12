# Nome

SimulDIESEL Architecture

## Objetivo

Orientar ETAPAS de arquitetura geral, fronteiras de responsabilidade, congelamento e leitura do estado real do projeto.

## Quando usar

Use para arquitetura geral, fronteiras entre UI/BLL/DAL/DTL/protocolos/firmware, analise de divergencias, congelamento tecnico e planejamento documental.

## Quando nao usar

Nao use para implementar detalhe visual, alterar firmware isolado, mudar schema ou editar contratos sem ETAPA especifica.

## Escopo permitido

- Ler `AGENTS.md`, `docs/official/`, `docs/architecture/`, `out/dumps/`, `local-api/src/`, `hardware/firmware/` e `Data/Modules/`.
- Criar ou atualizar documentos arquiteturais autorizados.

## Escopo proibido

- Alterar codigo funcional sem pedido.
- Alterar SDH, SDGW, SDCTP, firmware ou banco sem autorizacao.

## Arquivos/pastas provaveis

- `AGENTS.md`
- `docs/agents/`
- `docs/official/02-arquitetura/`
- `docs/architecture/`
- `out/dumps/`

## Padroes do projeto

- Arquitetura base: `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- SDH e semantica; SDGW e transporte; SDCTP e massa CAN; J1939 e decoder sobre `CanFrameDto`.
- Estados documentais: `IMPLEMENTADO`, `PARCIALMENTE IMPLEMENTADO`, `PLANEJADO`, `LEGADO`, `pendente de confirmacao`.

## Checklist de validacao

- [ ] Priorizar fonte de verdade: codigo, contratos, docs, dumps e legado.
- [ ] Registrar divergencias.
- [ ] Nao inventar comportamento.
- [ ] Confirmar que camadas nao foram misturadas.

## Checklist de entrega

- [ ] Arquivos criados/alterados.
- [ ] Decisoes e pendencias.
- [ ] Validacao documental.
- [ ] Dump quando exigido.

## Riscos comuns

- Tratar documentacao antiga como verdade atual sem conferir codigo.
- Declarar planejado como implementado.
- Usar nomenclatura diferente de `SDCTP` para o SimulDIESEL CAN Transport Protocol.

## Regras de nao regressao

- Nao enfraquecer fronteiras entre camadas.
- Nao remover compatibilidade legada por inferencia.
- Nao transformar decisao pendente em decisao oficial.

## Documentacao humana equivalente

`docs/agents/skills/simuldiesel-architecture-skill.md`
