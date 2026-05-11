# Relevancia do SDH para o futuro Banco de Dados de Modulos

## Premissa

Este dump nao implementa banco de dados. A analise abaixo identifica quais partes do SDH atual podem virar entidades, tabelas ou contratos de execucao para um Banco de Dados de Modulos.

## Entidades candidatas

| Entidade futura | Origem SDH/codigo | Campos candidatos |
|---|---|---|
| BoardType | `SdhTarget.Board`, `GwProtocol.*Address` | nome, endereco SDGW, firmware esperado, capacidades |
| BoardService | targets como `GSA.channel.setpoint`, `UCE.can.config` | board, resource, subresource, op, tlvType |
| CommandDefinition | `SdhValidator`, `SdhToSdgwMapper` | target, op, args obrigatorios, ranges, enum values, databaseRelevance |
| CommandArgument | `Require*Arg`, DTOs request | nome, tipo, obrigatorio, range, enum, default |
| TlvContract | `GwProtocol`, parsers | type, length, payload schema, direction |
| ResponseContract | DTOs response/event | dto, propriedades, tipos, mapping TLV |
| ModuleProfile | comandos SDH parametrizados | sequencia, args, versionamento, board dependencies |
| TestStep | comandos + assertions | comando, timeout, retries, expected response/event |
| CaptureChannel | CAN/GSA status events | fonte, periodicidade, tipo de dado |
| DiagnosticEvent | eventos/erros | board, type, payload, severidade |

## Relevancia por dominio

### Configurar hardware

Comandos relevantes:

- `GSA.channel.enable set`
- `GSA.channels.enable set`
- `GSA.channel.setpoint set`
- `GSA.channel.offset set/save/reset`
- `UCE.can.config set`
- `UCE.can.enable set`
- `UCE.can reset`

Observacao: outros boards de hardware aparecem em docs, mas nao estao confirmados em `SdhValidator`.

### Configurar sinais eletricos

Comandos mais relevantes:

- `GSA.channel.setpoint set`: valor 0..255 por canal.
- `GSA.channel.enable set`: liga/desliga canal.
- `GSA.channel.offset set/get/save/reset`: calibracao `vout`, `vread`, `iread`.
- `GSA.channels.status get`: snapshot de todos os canais.

Campos que merecem modelagem:

- canal 1..16;
- setpoint;
- offset por kind;
- enabled/fault;
- leituras `VoltageRead` e `CurrentRead`.

### Configurar alimentacao

No host atual nao ha comando SDH implementado para PSU/alimentacao. Exemplos `PSU.power.main` aparecem em docs, mas sao **NAO CONFIRMADO NO CODIGO**. Para banco futuro, tratar como capacidade planejada, nao como contrato executavel.

### Configurar CAN/J1939

Comandos relevantes:

- `UCE.can.config set`
- `UCE.can.enable set`
- `UCE.can.status get`
- `UCE.can.rx poll`
- `UCE.can.rx readAll` com ressalva de validator
- `UCE.can.tx direct/create/edit/delete/stop`

Eventos relevantes:

- `UceCanRxEvent 0x28`
- `CAN_CREATE 0x40`
- `CAN_EDIT 0x41`
- `CAN_DELETE 0x42`
- `CAN_ROW 0x44`
- `CAN_READ_ALL_DONE 0x45`
- `CAN_TIC 0x46`

J1939 atual e derivado de frames CAN no host, nao de comando SDH proprio. Para banco, armazenar perfis J1939 como especializacao de mensagens CAN/PGN, nao como protocolo SDH independente ate surgir contrato dedicado.

### Configurar sensores simulados

Confirmado no codigo via GSA:

- setpoints e enables de canais analogicos;
- offsets de calibracao;
- status/leitura por canal.

Nao confirmado no codigo:

- outros tipos de sensores via GSC/UIOD/UCO/PSU.

### Ler atuadores

Confirmado parcialmente:

- GSA status por canal/global.
- CAN RX/TX status via UCE.

Atuadores de outras boards: NAO CONFIRMADO NO CODIGO.

### Medir corrente

Confirmado em GSA:

- `GsaChannelStatusResponse.CurrentRead`
- `GsaChannelsStatusResponse.Channels[].CurrentRead`
- offset kind `iread`.

Unidades/escala fisica final: NAO CONFIRMADO NO CODIGO deste inventario; verificar `GsaChannelScaling.cs` antes de modelar unidade final.

### Executar testes automatizados

O SDH e adequado como contrato de test step:

- `SdhCommand` pode ser o comando executavel.
- `MappedSdgwCommand.TimeoutMs` e `Retries` dao parametros tecnicos iniciais.
- Responses/events sao DTOs para assertions.

Recomendacao de modelagem:

- `test_steps.command_target`
- `test_steps.command_op`
- `test_steps.args_json`
- `test_steps.expected_response_type`
- `test_steps.expected_event_type`
- `test_steps.timeout_ms`
- `test_steps.retry_count`

### Capturar dados temporais

Fontes confirmadas:

- `GSA.channel.status get` / `GSA.channels.status get`
- `GsaChannelFaultEvent`
- `GsaPhysicalOperationEvent`
- `UceCanRxEvent`
- CAN mirror events `CREATE/EDIT/DELETE/ROW/TIC`
- driver logs `UCE.can.driverLog poll`

Modelar timestamps no host, pois varios DTOs atuais nao carregam timestamp absoluto no payload.

### Salvar perfil de modulo

Campos SDH uteis:

- lista de comandos de preparacao GSA;
- configuracao CAN UCE;
- linhas CAN TX (`CanTxRowDto`);
- offsets GSA;
- criterios de leitura/status.

Guardar tambem:

- versao do contrato SDH (`sdh/1`);
- commit/versao do software;
- firmware esperado;
- board address / target.

### Reproduzir perfil de modulo

Sequencia sugerida baseada no contrato atual:

1. `BPM.gateway ping`
2. Configurar GSA offsets/setpoints/enables.
3. Configurar UCE CAN (`config`, `enable`).
4. Criar linhas CAN TX com `UCE.can.tx create` ou enviar frames com `direct`.
5. Capturar `UCE.can.rx`/eventos e status GSA.
6. Aplicar assertions.
7. Limpar/parar TX com `UCE.can.tx stop/delete` e desabilitar recursos conforme perfil.

## Riscos e lacunas para banco

| Lacuna | Evidencia | Impacto |
|---|---|---|
| JSON SDH nao implementado | Apenas docs; sem parser JSON SDH no host | Banco pode armazenar JSON, mas precisa adaptador. |
| `SdhValidator` diverge do mapper/client em `rxMode` e `readAll` | Codigo real | Algumas capacidades existentes em BLL podem falhar via SDH ate corrigir contrato. |
| Unidades fisicas GSA nao consolidadas neste dump | DTOs usam bytes; escala em outro servico | Banco deve guardar unidade/escala explicitamente. |
| Boards futuras sem validator | PSU/UCO/URL/UIOD/GSC em docs | Nao executar via SDH ate implementar host/firmware. |
| Timestamps ausentes em varios DTOs | DTOs atuais | Banco de captura deve adicionar timestamp no host. |

