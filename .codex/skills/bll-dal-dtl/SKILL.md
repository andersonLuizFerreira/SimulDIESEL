# Nome

BLL/DAL/DTL

## Objetivo

Orientar ETAPAS no host C# que envolvam casos de uso, clients, servicos, validadores, mapeadores, DTOs, transporte e sessao.

## Quando usar

Use para `BLL/`, `DAL/` e `DTL/`, especialmente clients de boards, FormsLogic, servicos CAN/J1939, SDH/SDGW e DTOs.

## Quando nao usar

Nao use para redesenhar UI, alterar firmware ou modificar Banco de Modulos sem pedido especifico.

## Escopo permitido

- `BLL/`
- `DAL/`
- `DTL/`
- Documentos e dumps relacionados.

## Escopo proibido

- Mudar firmware para acomodar mudanca host sem autorizacao.
- Alterar contratos publicos sem congelamento.
- Colocar regra de apresentacao na DAL/DTL.

## Arquivos/pastas provaveis

- `BLL/Boards/`
- `BLL/Services/CAN/`
- `BLL/Protocols/J1939/`
- `DAL/Protocols/SDGW/`
- `DAL/Transport/`
- `DTL/`

## Padroes do projeto

- BLL decide casos de uso e usa DTOs.
- DAL faz SDH, SDGW, sessao e transporte.
- DTL contem contratos sem IO.
- `SdgwHostSession` esta fisicamente na BLL, mas compoe a borda superior da DAL.

## Checklist de validacao

- [ ] Build C#.
- [ ] BLL nao recebeu framing/COBS/CRC.
- [ ] DAL nao conhece UI.
- [ ] DTL permanece sem IO.
- [ ] Contratos alterados foram documentados.

## Checklist de entrega

- [ ] Classes alteradas por camada.
- [ ] Contratos impactados.
- [ ] Testes/build.
- [ ] Pendencias e rollback.

## Riscos comuns

- Misturar controle SDH com massa SDCTP sem documentar.
- Fazer DTL executar logica.
- Colocar decoder automotivo dentro de SDGW.

## Regras de nao regressao

- Manter separacao BLL/DAL/DTL.
- Preservar contratos legados ainda usados.
- Nao ampliar acoplamento entre UI e DAL.

## Documentacao humana equivalente

`docs/agents/skills/bll-dal-dtl-skill.md`
