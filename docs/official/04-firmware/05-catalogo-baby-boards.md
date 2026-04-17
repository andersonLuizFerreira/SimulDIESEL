⬅ [Retornar para Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Catálogo de Baby Boards e Targets SDH

Esta página registra o catálogo realmente observável nesta auditoria, sem promover boards planejadas a estado implementado.

## Inventário real

| board | evidência em código | endereço/slot real | papel atual | status |
| --- | --- | --- | --- | --- |
| BPM | `BPM - BACKPLANE MANAGER MODULE/src/main.cpp` | `GW_ADDR_BPM = 0x0` | gateway local | `IMPLEMENTADO` |
| GSA | `GwDeviceTable.cpp`, `GSA - Gerador de sinais analógicos/src/main.cpp` | `GW_ADDR_GSA = 0x1`, `I2C_GSA_ADDR = 0x23` | board remota analógica | `IMPLEMENTADO` |
| UCE | `GwDeviceTable.cpp`, `UCE - Unidade de comunicacao externa/src/main.cpp` | `GW_ADDR_UCE = 0x2`, `CS=33`, `IRQ=27` | board remota SPI para comandos funcionais compactos | `IMPLEMENTADO` |
| broadcast | `SdgwDefs.h` | `GW_ADDR_BROADCAST = 0xF` | reservado; ignorado por `GatewayApp` | `PARCIALMENTE IMPLEMENTADO` |
| PSU, GSC, URL, SLU, UCO, UCS, UIOD, UHM | somente documentação | nenhum | catálogo reservado | `PLANEJADO` |

## Targets realmente sustentados

### BPM

- `BPM.gateway ping`

### GSA

Os contratos semânticos aparecem no host e convergem para `SDGW_CMD_GSA_TLV`:

- `GSA.led`
- `GSA.channel.setpoint`
- `GSA.channel.enable`
- `GSA.channels.enable`
- `GSA.channel.fault`
- `GSA.channel.offset`
- `GSA.offset`
- `GSA.channel.status`

### UCE

O contrato semântico vivo da UCE converge para `SDGW_CMD_UCE_TLV` e continua usando a mesma rota compacta para LED e CAN:

- `UCE.led`
- `UCE.can.config`
- `UCE.can.enable`
- `UCE.can.status`
- `UCE.can`

## Comentário orientado a código

O catálogo vivo do gateway é hoje mínimo. Em `GwDeviceTable.cpp`:

```cpp
static const GwDeviceEntry kBootstrapDefaults[] = {
    {GW_ADDR_GSA,  GW_BUS_I2C, I2C_GSA_ADDR, -1,                 -1,              BPM_GLOBAL_RESET_PIN, false},
    {GW_ADDR_UCE,  GW_BUS_SPI, 0x00,         BPM_UCE_SPI_CS_PIN, BPM_UCE_IRQ_PIN, BPM_UCE_RESET_PIN,    true},
};
```

Esse trecho é a prova mais forte de que:

- a GSA está publicada como device remoto `I2C`;
- a UCE está publicada como device remoto `SPI`;
- o catálogo remoto vivo já não é mais exclusivo da GSA.

## Leitura correta deste catálogo

- O host já conhece mais semântica que o gateway.
- O gateway conhece poucos endereços compactos e poucos destinos reais, mas já roteia `I2C` e `SPI`.
- A documentação das demais boards continua útil como nomenclatura de projeto, mas não como firmware implementado.

## Glossário

- **Catálogo vivo**: conjunto de boards que possuem evidência simultânea em código e documentação.
- **Slot lógico**: endereço compacto usado pelo gateway para decidir o destino.
- **Bootstrap defaults**: tabela fixa atual enquanto a BPM não persiste configuração dinâmica.

## Próximas camadas

- [Boards de Firmware](boards/README.md)
