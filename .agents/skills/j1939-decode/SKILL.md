# Nome

J1939 Decode

## Objetivo

Orientar ETAPAS de decodificacao J1939, PGN, SPN, diagnosticos DM1/DM2, gerenciamento de rede e catalogos.

## Quando usar

Use para decoders J1939, catalogos em JSON, DTOs J1939 e scripts de validacao J1939.

## Quando nao usar

Nao use para alterar SDGW, TLV, firmware CAN fisico ou UI visual sem pedido.

## Escopo permitido

- `BLL/Protocols/J1939/`
- `DTL/Protocols/J1939/`
- `Data/Protocols/J1939/`
- testes J1939 em `tools/testes/`.

## Escopo proibido

- Colocar J1939 dentro de SDGW.
- Consumir TLV bruto.
- Declarar suporte operacional em bancada sem validacao.

## Arquivos/pastas provaveis

- `J1939ProtocolService.cs`
- `J1939PgnCatalog.cs`
- `J1939PgnStandardCatalog.cs`
- `j1939-pgn-standard-catalog.json`
- `j1939-71-mini-catalog.json`

## Padroes do projeto

- J1939 deve consumir `CanFrameDto`.
- Decoders ficam acima de SDCTP.
- Catalogos ficam em `Data/Protocols/J1939`.

## Checklist de validacao

- [ ] Build C#.
- [ ] Scripts `tools/testes/j1939_*_validation.py`, quando aplicaveis.
- [ ] Validar dados de catalogo.
- [ ] Registrar divergencia entre docs antigas e codigo atual quando houver.

## Checklist de entrega

- [ ] PGNs/SPNs impactados.
- [ ] Decoders afetados.
- [ ] Validacao executada.
- [ ] Limites operacionais.

## Riscos comuns

- Tratar catalogo como suporte fisico completo.
- Inserir J1939 no SDGW.
- Acoplar mais decodificacao a FormsLogic sem decisao arquitetural.

## Regras de nao regressao

- J1939 continua sobre `CanFrameDto`.
- SDCTP continua responsavel pela massa CAN.
- SDGW nao conhece PGN/SPN.


