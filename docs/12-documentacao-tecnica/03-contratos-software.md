⬅ [Retornar para Diagramas](02-diagramas.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Contratos de Software

## Contratos preservados

Os contratos históricos preservados pelo Git e por `out/dumps/` continuam úteis como referência de framing e semântica básica do SDGW, mas não devem prevalecer sobre a documentação oficial vigente quando houver divergência.

## Contrato vigente do host

O contrato ativo do host é:

```text
BpmSerialService
    -> GsaClient / BpmClient
    -> SdhClient
    -> SdgwSession
    -> SdGwTxScheduler
    -> SdGwLinkEngine
    -> SwitchableTransport
    -> SerialTransport / BluetoothTransport
```

Regras vigentes:

- a sessão host atual é transport-agnostic, embora a fachada preserve o nome `BpmSerialService`;
- o envio funcional passa por `SdgwSession.SendAsync(...)`;
- `SdGwTxScheduler` é o único caminho normal de TX;
- RX SDGW válido prova vida do link;
- ping só ocorre sob silêncio;
- BUSY/IDLE não é mais o mecanismo oficial de concorrência da GSA.

## Contrato vigente da BPM

No firmware da BPM:

- a sessão continua mantida por atividade SDGW válida;
- a BPM roteia comandos para a GSA no barramento físico I2C;
- a BPM trata a GSA como device remoto, não como regra de negócio local;
- a BPM usa `D21/D22` para o I2C físico com a GSA;
- a BPM detecta IRQ físico da GSA em `D19`;
- a BPM controla o reset da GSA em `D23`;
- ao detectar IRQ, busca um TLV assíncrono da GSA e o encaminha como evento SDGW.

## Contrato vigente da GSA

Na arquitetura oficial atual da GSA:

- barramento físico com a BPM:
  - GSA = `slave`
  - endereço = `0x23`
  - Nano `A4/A5`
- barramento lógico com `TCA9548A` + `MCP4725`:
  - GSA = `master`
  - Nano `D2/D3`
- IRQ físico:
  - Nano `D4`
  - ativo em `LOW`
  - open-drain por software
  - pull-up externo `3,3 V`
- reset do `TCA9548A`:
  - Nano `D8`

O contrato síncrono do comando passou a significar apenas:

- recepção válida;
- payload aceito;
- operação enfileirada para processamento.

O resultado físico final é assíncrono.

## Evento assíncrono oficial da execução física

O evento oficial é:

- `type = 0x31`
- `len = 0x03`
- `data = [origin_type][channel][status]`

Status oficiais:

- `0x01` = operação OK
- `0x02` = falha. `TCA9548A` não respondeu
- `0x03` = falha. `MCP4725` não respondeu

Regras:

- o `0x31` é emitido sempre, inclusive em sucesso;
- ele não substitui a resposta síncrona;
- ele representa exclusivamente o resultado da etapa física.

## Política de falha física da GSA

Quando houver falha de `ACK` no barramento lógico:

- o valor anterior do canal é mantido;
- o shadow não é alterado;
- o estado de enable não é alterado;
- não é criado `fault latched` por essa falha;
- não há retry automático;
- o host é informado pelo `0x31`.

## Legados removidos do fluxo ativo

O modelo anterior deixou de ser oficial:

- BUSY/IDLE com troca de papel slave/master no mesmo barramento I2C;
- polling semântico da BPM aguardando `IDLE`;
- retry do host aguardando evento `IDLE`.

## Documentos oficiais relacionados

- `docs/04-firmware/boards/BPM/01-bpm.md`
- `docs/04-firmware/boards/GSA/03-gsa.md`
- `docs/05-software-dashboard/04-sdh-host-architecture.md`
- `docs/06-protocolos/06-gsa-sdh-tlv.md`

## Glossário

- **Especificação**: descrição formal de comportamento, limites ou contratos técnicos.
- **Diagrama**: representação visual simplificada da arquitetura ou do fluxo.
- **Contrato**: acordo técnico entre camadas, serviços ou dispositivos.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.
- **TLV**: Type-Length-Value, formato interno de payload usado em transações específicas.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
