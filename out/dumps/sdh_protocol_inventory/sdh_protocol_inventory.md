# Inventario tecnico do protocolo SDH

Projeto: SimulDIESEL  
Escopo: dump tecnico para futura modelagem do Banco de Dados de Modulos  
Base: codigo real em `local-api/src/SimulDIESEL/SimulDIESEL`, firmware em `hardware/firmware`, e documentos oficiais quando citados como referencia.

## Resumo

O SDH aparece no host como envelope semantico de comando, representado por `SdhCommand` e trafegado pela camada DAL `SdhClient`. O SDH nao vai diretamente para as boards: ele e validado, mapeado para um comando compacto SDGW, encapsula um payload TLV quando a board e GSA ou UCE, e entao segue para `SdgwSession`/`SdGwLinkEngine` sobre Serial ou Bluetooth.

Fluxo confirmado:

```text
UI / FormsLogic
  -> BLL board client/dispatcher
  -> SdhCommand
  -> SdhClient
  -> SdhValidator
  -> SdhToSdgwMapper
  -> SdgwSession
  -> SdGwTxScheduler / SdGwLinkEngine
  -> IByteTransport Serial/Bluetooth
  -> BPM gateway
  -> TLV para GSA/UCE quando aplicavel
```

Fluxo reverso confirmado:

```text
BPM/board
  -> frame SDGW
  -> SdgwSession.FrameReceived/EventReceived
  -> BoardTlvDispatcher ou client de board
  -> parsers BLL
  -> DTOs DTL
  -> FormsLogic/UI ou servicos CAN/SDCTP
```

## Contratos centrais

| Area | Classe/arquivo | Papel |
|---|---|---|
| Comando SDH | `DTL/Protocols/SDGW/SdhCommand.cs` | `Version`, `Target`, `Op`, `Args`, `Meta`. |
| Resposta SDH | `DTL/Protocols/SDGW/SdhResponse.cs` | Modelo textual/JSON documentado; pouco usado no pipeline atual de board. |
| Target SDH | `DTL/Protocols/SDGW/SdhTarget.cs` | Parser `Board.resource` ou `Board.resource.subresource`. |
| Validacao | `DAL/Protocols/SDGW/SdhValidator.cs` | Catalogo efetivamente aceito antes do envio. |
| Mapeamento | `DAL/Protocols/SDGW/SdhToSdgwMapper.cs` | Traduz target/op/args para `MappedSdgwCommand`. |
| Sessao SDH | `DAL/Protocols/SDGW/SdhClient.cs` | Entrada semantica; envia via `SdgwSession`. |
| Contrato compacto | `DTL/Protocols/SDGW/GwProtocol.cs` | Enderecos, ops, TLV types, payload lengths e codigos. |
| Sessao SDGW | `DAL/Protocols/SDGW/SdgwSession.cs` | Publica `FrameReceived` e `EventReceived`. |

## Boards encontradas no codigo host

| Board | Endereco SDGW | Status no host | Observacoes |
|---|---:|---|---|
| BPM | `0x0` | Parcial | `BPM.gateway ping` confirmado; BPM tambem e gateway/transport owner. |
| GSA | `0x1` | Implementado | LED, canais analogicos, enable/status/fault/offsets e eventos. |
| UCE | `0x2` | Implementado | LED, CAN config/enable/status/reset/RX/TX, CAN CRUD/eventos e diagnosticos. |
| Broadcast | `0xF` | Constante | `GwProtocol.BroadcastAddress`; uso funcional SDH nao confirmado. |

Boards citadas em documentos, mas sem suporte confirmado pelo host atual: PSU, UCO, URL, UIOD, GSC. Estes nomes aparecem em docs oficiais de exemplos/planejamento, mas `SdhValidator` nao os aceita.

## Forma textual e JSON

Forma textual implementada por `SdhTextParser`:

```text
sdh/1 <target> <op> chave=valor ...
```

Forma JSON aparece nos documentos oficiais `docs/official/06-protocolos/01-sdh-command-model.md` e `02-sdh-response-model.md`, mas nao ha serializador/desserializador JSON SDH implementado no host atual. Portanto, contratos JSON SDH runtime: NAO CONFIRMADO NO CODIGO.

## Inconsistencias confirmadas

| Item | Evidencia | Impacto |
|---|---|---|
| `UCE.can.config` com `rxMode` | `UceClient.CreateCanConfigCommand(..., UceCanRxMode?)` e `SdhToSdgwMapper.MapUce` aceitam `rxMode`; `SdhValidator.ValidateUceCan` exige exatamente 3 argumentos. | O caminho via `SdhClient` tende a rejeitar comandos com `rxMode`. |
| `UCE.can.rx readAll` | `UceClient.CreateCanReadAllCommand` e `SdhToSdgwMapper.MapUce` aceitam `Op = readAll`; `SdhValidator.ValidateUceCan` para `rx` exige `poll`. | O caminho via `SdhClient` tende a rejeitar `CAN_READ_ALL`. |

## Arquivos gerados neste dump

- `sdh_command_catalog.md`
- `sdh_json_contracts.md`
- `sdh_board_services.md`
- `sdh_event_catalog.md`
- `sdh_code_references.md`
- `sdh_database_relevance.md`

