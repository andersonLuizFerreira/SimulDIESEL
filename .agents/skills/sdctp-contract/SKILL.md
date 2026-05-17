# Nome

SDCTP Contract

## Objetivo

Orientar ETAPAS de massa CAN RX/TX, mirror, buffers, compactacao e sincronizacao pelo protocolo SDCTP.

## Quando usar

Use para `CanRxOutputBuffer`, mirror table, `DATA_MASK`, eventos `CAN_CREATE/EDIT/DELETE/TIC/ROW` e wrappers `Sdctp*`.

## Quando nao usar

Nao use para framing SDGW, regra de UI ou decodificacao de PGN J1939.

## Escopo permitido

- `BLL/Services/CAN/SDCTP/`
- `BLL/Services/CAN/ApiCanService.cs`
- `BLL/Services/CAN/CanEventProcessor.cs`
- `BLL/Services/CAN/CanRxMirrorManager.cs`
- `BLL/Services/CAN/CanRxOutputBuffer.cs`
- firmware UCE em `lib/services/can/sdctp/`
- dumps SDCTP.

## Escopo proibido

- Alterar UI visual sem pedido.
- Alterar SDGW para conhecer CAN.
- Remover `CAN_READ_ALL` legado sem autorizacao.

## Arquivos/pastas provaveis

- `docs/ETAPA_10_SDCTP_CAN_TRANSPORT_PROTOCOL.md`
- `docs/architecture/sdh_sdctp_sdgw_contracts.md`
- `DTL/Protocols/SDCTP/`
- `DTL/Boards/UCE/Can/`

## Padroes do projeto

- SDCTP e protocolo de massa/sincronizacao CAN.
- `CanRxOutputBuffer` e saida oficial para consumidores superiores.
- Mirror table e estado interno/diagnostico.
- J1939 consome `CanFrameDto`, nao TLV.

## Checklist de validacao

- [ ] Build C# e firmware UCE quando alterados.
- [ ] Testes em `tools/testes/` quando aplicaveis.
- [ ] RX/TX preservam semantica validada.
- [ ] Diagnosticos nao foram silenciados.

## Checklist de entrega

- [ ] TLVs impactados.
- [ ] Fluxo RX/TX afetado.
- [ ] Buffer/mirror impactados.
- [ ] Resultados de validacao.

## Riscos comuns

- Fazer UI consumir mirror como fila oficial.
- Colocar CAN dentro de SDGW.
- Tratar compatibilidade legada como removida sem prova.

## Regras de nao regressao

- Consumidores superiores leem `CanRxOutputBuffer`.
- SDGW permanece transporte.
- TX CAN preserva contratos congelados ate nova decisao.


