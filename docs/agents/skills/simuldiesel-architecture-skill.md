# Skill: SimulDIESEL Architecture

## Quando usar

Use para ETAPAS que envolvam arquitetura geral, fronteiras entre camadas, leitura de estado atual, congelamento ou planejamento tecnico.

## Quando nao usar

Nao use para implementar detalhe visual, alterar firmware isolado ou editar schema sem uma ETAPA especifica.

## Escopo permitido

- Ler `docs/official/`, `docs/architecture/`, `out/dumps/`, `local-api/src/`, `hardware/firmware/`, `Data/Modules/`.
- Criar ou atualizar documentos arquiteturais autorizados.

## Escopo proibido

- Alterar codigo funcional sem pedido.
- Mudar contratos SDH, SDGW, SDCTP, firmware ou banco sem autorizacao.

## Arquivos/pastas provaveis

- `AGENTS.md`
- `docs/agents/`
- `docs/official/02-arquitetura/`
- `docs/architecture/`
- `out/dumps/`

## Padroes do projeto

- Arquitetura: `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- SDH e semantica; SDGW e transporte; SDCTP e massa CAN; J1939 e decoder sobre `CanFrameDto`.
- Estados: `IMPLEMENTADO`, `PARCIALMENTE IMPLEMENTADO`, `PLANEJADO`, `LEGADO`.

## Checklist de validacao

- [ ] A fonte de verdade foi priorizada: codigo, contrato, docs, dumps, legado.
- [ ] Divergencias foram registradas.
- [ ] Nenhum comportamento foi inventado.
- [ ] Camadas nao foram misturadas.

## Checklist de entrega

- [ ] Arquivos criados/alterados.
- [ ] Decisoes e pendencias.
- [ ] Validacao documental.
- [ ] Dump, se exigido.

## Riscos comuns

- Tratar documentacao antiga como verdade atual sem conferir codigo.
- Declarar planejado como implementado.
- Usar nomenclatura diferente de `SDCTP` para o SimulDIESEL CAN Transport Protocol.

## Regras de nao regressao

- Nao enfraquecer fronteiras entre UI, BLL, DAL, DTL, protocolos e firmware.
- Nao remover compatibilidade legada por inferencia.
- Nao consolidar decisao pendente como decisao oficial.
