⬅ [Retornar para Boards de Firmware](../README.md)

# GSA

## Nome canônico

Gerador de Sinais Analógicos (GSA)

## Identificador SDH da board

GSA

## Responsabilidade principal no firmware

Receber comandos TLV da BPM no barramento físico, manter o estado lógico dos 16 canais e executar a atuação elétrica real em um barramento I2C independente para `TCA9548A` + `MCP4725`.

## Arquitetura oficial vigente

O firmware atual da GSA trabalha com dois barramentos I2C distintos:

1. barramento físico:
   - ligação com a BPM
   - GSA = `slave`
   - endereço fixo = `0x23`
2. barramento lógico/eletrônico:
   - ligação com `TCA9548A` + `MCP4725`
   - GSA = `master`
   - sem troca de papel com o barramento físico

Consequência importante:

- a GSA não usa mais o modelo antigo de virar `master` no mesmo barramento da BPM;
- o modelo BUSY/IDLE por polling deixou de ser a arquitetura oficial.

## Pinagem oficial da GSA

- I2C físico com a BPM: `A4` = SDA, `A5` = SCL
- I2C lógico com `TCA9548A` e `MCP4725`: `D2` = SDA, `D3` = SCL
- IRQ físico para a BPM: `D4`
- reset do `TCA9548A`: `D8`
- reset da placa GSA: pino `RESET` físico do Nano, comandado pela BPM via `D23`

## Pilha interna preservada

O caminho lógico da board continua curto:

```text
I2C callback -> Transport -> Link -> Service -> LedService / AnalogService / EepromService
```

A atuação elétrica real ficou acoplada ao fluxo lógico por serviços dedicados:

- `BusArbiterService`
- `Tca9548Service`
- `Mcp4725Service`

## Fluxo operacional oficial

1. a BPM envia um TLV para a GSA no barramento físico;
2. a GSA valida o payload e responde sincronicamente apenas com o ACK lógico da recepção;
3. a GSA enfileira a operação física pendente;
4. o `BusArbiterService` executa a operação no barramento lógico;
5. ao concluir, a GSA publica um evento assíncrono `0x31`;
6. a GSA aciona a linha de IRQ física para a BPM;
7. a BPM busca o TLV assíncrono e o encaminha ao host.

## IRQ físico

Definições vigentes:

- Nano `D4`
- ativo em `LOW`
- `INPUT_PULLUP` desativado
- pull-up externo em `3,3 V`
- comportamento de open-drain por software:
  - inativo = alta impedância
  - ativo = forçar `LOW`

## Modelo funcional documentado

- `16` canais no total
- canais `1..8` na faixa `0..5 V`
- canais `9..16` na faixa `0..12 V`
- setpoint lógico transportado como `0..255`
- shadow RAM mantido por canal
- offsets por canal:
  - `vout`
  - `vread`
  - `iread`
- EEPROM preservada para offsets

Enquanto a medição analógica real completa não existe, o status reportado continua derivado do shadow e da simulação já existente no firmware.

## Mapeamento elétrico obrigatório

- canal `1`  -> `SC0` + `MCP4725 0x61`
- canal `2`  -> `SC0` + `MCP4725 0x60`
- canal `3`  -> `SC1` + `MCP4725 0x61`
- canal `4`  -> `SC1` + `MCP4725 0x60`
- canal `5`  -> `SC2` + `MCP4725 0x61`
- canal `6`  -> `SC2` + `MCP4725 0x60`
- canal `7`  -> `SC3` + `MCP4725 0x61`
- canal `8`  -> `SC3` + `MCP4725 0x60`
- canal `9`  -> `SC4` + `MCP4725 0x61`
- canal `10` -> `SC4` + `MCP4725 0x60`
- canal `11` -> `SC5` + `MCP4725 0x61`
- canal `12` -> `SC5` + `MCP4725 0x60`
- canal `13` -> `SC6` + `MCP4725 0x61`
- canal `14` -> `SC6` + `MCP4725 0x60`
- canal `15` -> `SC7` + `MCP4725 0x61`
- canal `16` -> `SC7` + `MCP4725 0x60`

Endereços:

- `TCA9548A = 0x70`
- `MCP4725 A0 = GND -> 0x60`
- `MCP4725 A0 = VCC -> 0x61`

## Resultado físico assíncrono

O evento assíncrono oficial da execução elétrica é:

- `0x31`

Payload:

```text
[origin_type][channel][status]
```

Status:

- `0x01` = operação OK
- `0x02` = `TCA9548A` sem ACK
- `0x03` = `MCP4725` sem ACK

Regra:

- o `0x31` é emitido sempre, inclusive em sucesso;
- ele não substitui a resposta síncrona do comando original.

## Política de falha física

Quando a etapa física falha:

- o valor anterior do canal é preservado;
- o shadow lógico não é alterado;
- o enable não muda;
- não há retry automático;
- não é criado `fault latched` apenas por falta de ACK do barramento lógico;
- o resultado é comunicado exclusivamente pelo evento `0x31`.

## Targets SDH vigentes da GSA

- `GSA.led`
- `GSA.channel.setpoint`
- `GSA.channel.enable`
- `GSA.channels.enable`
- `GSA.channel.status`
- `GSA.channel.fault`
- `GSA.channel.offset`
- `GSA.offset`

## Observações

- o caso `GSA.led set state=on|off` continua válido e é o caminho mais simples de teste ponta a ponta;
- o modelo antigo BUSY/IDLE deixou de ser referência para a arquitetura oficial da board;
- o contrato TLV detalhado está em `docs/official/06-protocolos/06-gsa-sdh-tlv.md`.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.

