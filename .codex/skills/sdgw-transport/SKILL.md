# Nome

SDGW Transport

## Objetivo

Orientar ETAPAS no transporte/gateway SDGW, incluindo framing, COBS, CRC, ACK/ERR, sequencia, retry, endpoints e roteamento BPM.

## Quando usar

Use para gateway/transporte, endpoints Serial/Bluetooth, sessao, scheduler, supervisor, BPM gateway e roteamento fisico.

## Quando nao usar

Nao use para regra CAN, J1939, UI, Banco de Modulos ou DTOs de apresentacao.

## Escopo permitido

- `DAL/Protocols/SDGW/`
- `DAL/Transport/`
- firmware BPM `lib/Sdgw*`, `lib/Gateway`, `lib/GwRouter`
- docs de SDGW.

## Escopo proibido

- Inserir regra de protocolo automotivo em SDGW.
- Fazer SDGW interpretar PGN, SPN, CAN semantics ou UI.

## Arquivos/pastas provaveis

- `SdgwLinkEngine.cs`
- `SdgwSession.cs`
- `SdGwTxScheduler.cs`
- `SwitchableTransport.cs`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/`

## Padroes do projeto

- Uma sessao fisica ativa por vez no host.
- BPM multiplexa Serial/Bluetooth com ownership.
- SDGW entrega eventos e frames; quem interpreta dominio fica acima.

## Checklist de validacao

- [ ] Build C# se DAL alterada.
- [ ] `platformio run` da BPM se firmware alterado.
- [ ] Validar ACK/retry/timeouts se tocados.
- [ ] Confirmar ausencia de regra CAN/J1939 no SDGW.

## Checklist de entrega

- [ ] Partes host/firmware afetadas.
- [ ] Evidencia de transporte.
- [ ] Contratos preservados.
- [ ] Limitacoes.

## Riscos comuns

- Transformar gateway em regra de negocio.
- Misturar roteamento fisico com semantica SDH.
- Quebrar compatibilidade de eventos.

## Regras de nao regressao

- SDGW continua transporte puro.
- Serial e Bluetooth continuam endpoints mutuamente exclusivos.
- BPM continua gateway/roteador.

## Documentacao humana equivalente

`docs/agents/skills/sdgw-transport-skill.md`
