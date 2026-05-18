⬅ [Retornar para Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Tabela Mestra de Binding Lógico-Físico do Gateway

Esta página descreve o binding realmente implementado entre endereço lógico do gateway e rota física.

## Binding vivo da BPM

| endereço lógico | símbolo real | resolvido onde | barramento | destino físico | status |
| --- | --- | --- | --- | --- | --- |
| `0x0` | `GW_ADDR_BPM` | `GatewayApp::handleGatewayLocal(...)` | interno | própria BPM | `IMPLEMENTADO` |
| `0x1` | `GW_ADDR_GSA` | `GwDeviceTable::get(...)` | `I2C` | `0x23` | `IMPLEMENTADO` |
| `0x2` | `GW_ADDR_UCE` | `GwDeviceTable::get(...)` | `SPI` | `CS=33`, `IRQ=27`, `RESET=23`, `SPI 18/26/25` | `IMPLEMENTADO` |
| `0xF` | `GW_ADDR_BROADCAST` | `GatewayApp::onCommand(...)` | nenhum | ignorado no código atual | `PARCIALMENTE IMPLEMENTADO` |

## Quem chama quem

```text
SdgwLink
  -> GatewayApp::onCommand(...)
  -> GwRouter::route(...)
  -> GwDeviceTable::get(...)
  -> GwI2cBus::transact(...) ou GwSpiBus::transact(...)
```

## Comentário orientado a código

O binding físico real vive em duas camadas.

Primeiro, `GatewayApp` separa BPM local de remoto:

```cpp
const uint8_t addr = GW_CMD_ADDR(cmd);
if (addr == GW_ADDR_BPM) {
    handleGatewayLocal(cmd, data, dataLen);
    return;
}
```

Depois, `GwRouter` exige que o endereço remoto exista na tabela:

```cpp
GwDeviceEntry e{};
if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;
```

Esse segundo bloco é o que efetivamente materializa o binding lógico-físico do gateway atual.

## O que a rota da UCE transporta hoje

O binding `0x2 -> SPI -> UCE` permanece único. Nesta rodada ele continua carregando:

- `UCE.led`
- `UCE.can.config`
- `UCE.can.enable`
- `UCE.can.status`
- `UCE.can reset`

Todos esses comandos reutilizam o mesmo:

- endereço lógico `GW_ADDR_UCE = 0x2`
- `GW_OP_UCE_TLV_TRANSACT = 0x0`
- comando compacto `SDGW_CMD_UCE_TLV`

Ou seja: a feature CAN da UCE não abriu rota paralela nova na BPM.

## O que ainda não existe

- **PLANEJADO**: tabela persistida ou configurável de devices.
- **PLANEJADO**: binding amplo por `SDH target`.
- **PARCIALMENTE IMPLEMENTADO**: catálogo remoto ainda é pequeno e bootstrap fixo.
- **PLANEJADO**: tabela persistida ou configurável de devices.
- **PLANEJADO**: múltiplas boards remotas além de GSA e UCE.

## Glossário

- **Binding**: associação entre nome/endereço lógico e rota física de hardware.
- **Unmapped**: endereço lógico sem device conhecido pelo gateway.
- **Rota interna**: tratamento local na própria BPM, sem usar `GwDeviceTable`.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
