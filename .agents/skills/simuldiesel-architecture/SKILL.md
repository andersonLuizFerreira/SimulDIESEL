# Nome

SimulDIESEL Architecture

## Objetivo

Orientar ETAPAS de arquitetura geral, fronteiras de responsabilidade, congelamento e leitura do estado real do projeto.

## Quando usar

Use para arquitetura geral, fronteiras entre UI/BLL/DAL/DTL/protocolos/firmware, analise de divergencias, congelamento tecnico e planejamento documental.

## Quando nao usar

Nao use para implementar detalhe visual, alterar firmware isolado, mudar schema ou editar contratos sem ETAPA especifica.

## Escopo permitido

- Ler `.agents/README.md`, `.agents/skills/`, `docs/`, `local-api/src/`, `hardware/firmware/` e `Data/Modules/`.
- Criar ou atualizar documentos arquiteturais autorizados.

## Escopo proibido

- Alterar codigo funcional sem pedido.
- Alterar SDH, SDGW, SDCTP, firmware ou banco sem autorizacao.

## Arquivos/pastas provaveis

- `.agents/README.md`
- `.agents/skills/`
- `docs/`
- `local-api/src/`
- `hardware/firmware/`
- `Data/Modules/`

## Padroes do projeto

- Arquitetura base: `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- SDH e semantica; SDGW e transporte; SDCTP e massa CAN; J1939 e decoder sobre `CanFrameDto`.
- Estados documentais: `IMPLEMENTADO`, `PARCIALMENTE IMPLEMENTADO`, `PLANEJADO`, `LEGADO`, `pendente de confirmacao`.
- Nao inferir implementacao apenas pela existencia de documentacao, nomes de arquivos, classes, namespaces ou estruturas de pasta.
- Diferenciar claramente codigo existente, codigo compilavel, codigo validado e codigo operacional em bancada.
- Nao promover implementacoes experimentais, fake, mock ou bench-only para estado oficial sem validacao explicita.

## Checklist de validacao

- [ ] Priorizar fonte de verdade: codigo, contratos e `docs/`.
- [ ] Registrar divergencias.
- [ ] Nao inventar comportamento.
- [ ] Confirmar que camadas nao foram misturadas.
- [ ] Limitacoes e incertezas registradas como `pendente de confirmacao` quando nao houver evidencia suficiente.

## Checklist de entrega

- [ ] Arquivos criados/alterados.
- [ ] Decisoes e pendencias.
- [ ] Validacao documental.
- [ ] `docs/` revisado/atualizado quando a ETAPA arquitetural ou funcional for concluida.
- [ ] Dump quando exigido pela ETAPA.

## Riscos comuns

- Tratar documentacao antiga como verdade atual sem conferir codigo.
- Declarar planejado como implementado.
- Usar nomenclatura diferente de `SDCTP` para o SimulDIESEL CAN Transport Protocol.
- Declarar componente como operacional apenas porque compila ou porque existe documentado.

## Regras de nao regressao

- Nao enfraquecer fronteiras entre camadas.
- Nao remover compatibilidade legada por inferencia.
- Nao transformar decisao pendente em decisao oficial.
- Ao concluir ETAPAS arquiteturais ou funcionais, revisar `docs/` e documentos de arquitetura relacionados, atualizando fluxos, contratos, estados e responsabilidades impactadas.
- Ao analisar arquitetura, separar fato observado de inferencia e registrar evidencia ou `pendente de confirmacao`.
