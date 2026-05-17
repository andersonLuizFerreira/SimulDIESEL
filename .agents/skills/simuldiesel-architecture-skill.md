# Skill: SimulDIESEL Architecture

## Quando usar

Use para ETAPAS que envolvam arquitetura geral, fronteiras entre camadas, leitura de estado atual, congelamento ou planejamento tecnico.

## Quando nao usar

Nao use para implementar detalhe visual, alterar firmware isolado ou editar schema sem uma ETAPA especifica.

## Escopo permitido

- Ler `docs/`, `local-api/src/`, `hardware/firmware/`, `Data/Modules/`.
- Criar ou atualizar documentos arquiteturais autorizados.

## Escopo proibido

- Alterar codigo funcional sem pedido.
- Mudar contratos SDH, SDGW, SDCTP, firmware ou banco sem autorizacao.

## Arquivos/pastas provaveis

- `.agents/`
- `docs/`
- `local-api/src/`
- `hardware/firmware/`
- `Data/Modules/`

## Padroes do projeto

- Arquitetura: `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- SDH e semantica; SDGW e transporte; SDCTP e massa CAN; J1939 e decoder sobre `CanFrameDto`.
- Estados: `IMPLEMENTADO`, `PARCIALMENTE IMPLEMENTADO`, `PLANEJADO`, `LEGADO`.
- Nao inferir implementacao apenas pela existencia de documentacao, nomes de arquivos, classes, namespaces ou estruturas de pasta.
- Diferenciar claramente codigo existente, codigo compilavel, codigo validado e codigo operacional em bancada.
- Nao promover implementacoes experimentais, fake, mock ou bench-only para estado oficial sem validacao explicita.

## Checklist de validacao

- [ ] A fonte de verdade foi priorizada: codigo, contrato e docs.
- [ ] Divergencias foram registradas.
- [ ] Nenhum comportamento foi inventado.
- [ ] Camadas nao foram misturadas.
- [ ] Limitacoes e incertezas foram registradas como `pendente de confirmacao` quando nao houve evidencia suficiente.

## Checklist de entrega

- [ ] Arquivos criados/alterados.
- [ ] Decisoes e pendencias.
- [ ] Validacao documental.
- [ ] Dump, se exigido.

## Riscos comuns

- Tratar documentacao antiga como verdade atual sem conferir codigo.
- Declarar planejado como implementado.
- Usar nomenclatura diferente de `SDCTP` para o SimulDIESEL CAN Transport Protocol.
- Declarar componente como operacional apenas porque compila ou porque existe documentado.

## Regras de nao regressao

- Nao enfraquecer fronteiras entre UI, BLL, DAL, DTL, protocolos e firmware.
- Nao remover compatibilidade legada por inferencia.
- Nao consolidar decisao pendente como decisao oficial.
- Ao analisar arquitetura, separar fato observado de inferencia e registrar evidencia ou `pendente de confirmacao`.
