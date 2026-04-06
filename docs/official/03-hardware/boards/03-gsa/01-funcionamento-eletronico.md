⬅ [Retornar para GSA — Visão Geral](README.md)
⬅ [Retornar para Índice Geral](../../../../00-INDICE.md)

# Funcionamento Eletrônico

Esta página responde à trilha **COMO** da GSA física: como o sinal atravessa a placa depois que a BPM envia um comando.

## Fluxo físico-funcional confirmado

```text
BPM
  -> I2C físico (A4/A5)
  -> Transport / Link / Service
  -> AnalogService
  -> BusArbiterService
  -> TCA9548A
  -> MCP4725
  -> estágio analógico do canal
  -> resultado elétrico
  -> IRQ + evento assíncrono
```

## Como o canal é escolhido

O firmware faz a seleção em duas etapas:

1. `Tca9548Service::switchIndexForChannel(channel)` escolhe um dos 8 ramos do hub.
2. `Mcp4725Service::addressForChannel(channel)` escolhe `0x61` para canais ímpares e `0x60` para pares.

## Como o valor vira tensão

`BusArbiterService::rawToOutputMillivolts(...)` converte o `setpointRaw` de `0..255` para milivolts conforme a faixa do canal.

Depois, `Mcp4725Service::toDacVoltage(...)` aplica a lógica física:

- canais `1..8`: faixa até `5 V`
- canais `9..16`: faixa até `12 V`
- canais altos usam ganho de amplificação `2.4`

## Comentário orientado a código

Trecho real de `BusArbiterService::executeOperation(...)`:

```cpp
if (!_tca9548.selectChannel(operation.channel, &ackCode)) {
  return GSA_PHYSICAL_STATUS_TCA_NO_ACK;
}

if (!_mcp4725.probeChannel(operation.channel, &ackCode)) {
  return GSA_PHYSICAL_STATUS_MCP_NO_ACK;
}
```

Esse bloco existe para validar a rota elétrica antes de aplicar a escrita no DAC.

Em seguida:

```cpp
bool ok = operation.disableOutput
  ? _mcp4725.disableChannel(operation.channel, &ackCode)
  : _mcp4725.writeChannel(operation.channel, rawToOutputMillivolts(...), &ackCode);
```

Aqui a GSA decide entre zerar a saída ou escrever um novo valor físico.

## Como o resultado volta

Depois da etapa elétrica:

- a GSA empilha `CMD_PHYSICAL_EVENT (0x31)` com `origin_type`, `channel` e `status`
- `Link::tick()` arma `IRQ`
- a BPM detecta `LOW` e busca o evento

## O que ainda permanece parcial

- descrição completa do estágio analógico por componente fora do subprojeto da GSA
- documentação elétrica consolidada do caminho até X-CONN e módulo em teste

## Glossário

- **Resultado elétrico**: efeito físico aplicado ao canal depois da escrita no DAC.
- **Probe**: teste de presença do periférico antes da operação.
- **Evento físico**: mensagem assíncrona usada para reportar sucesso ou falha da etapa elétrica.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
