⬅ [Retornar para Visão Lógica do Projeto](../02-arquitetura/03-visao-logica.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Arquitetura de Firmware

Esta página descreve a arquitetura de firmware realmente observável em `hardware/firmware`.

O foco aqui é estrutural: onde cada bloco embarcado fica na pilha, quais arquivos materializam essa pilha e quais boards têm implementação real.

## Estado confirmado

- **IMPLEMENTADO**: firmware da `BPM - BACKPLANE MANAGER MODULE` com sessão `SDGW`, endpoints Serial e Bluetooth, roteamento local e acesso a `I2C`/`SPI`.
- **IMPLEMENTADO**: firmware da `GSA - Gerador de sinais analógicos` com `Transport -> Link -> Service -> AnalogService`, fila de operações físicas, `IRQ` e eventos assíncronos.
- **IMPLEMENTADO**: firmware da `UCE - Unidade de comunicacao externa` com `SpiLink`, `UceTransport`, `UceServiceDispatcher`, `LedService`, `CanService` e fachadas `Sdctp*`.
- **IMPLEMENTADO**: a tabela viva de devices da BPM publica `GW_ADDR_GSA` em `I2C` e `GW_ADDR_UCE` em `SPI`.
- **PLANEJADO**: demais boards documentadas em `docs/04-firmware/boards/` ainda não possuem firmware equivalente nesta auditoria.
- **LEGADO**: a ideia de parser `SDH` residente no gateway continua apenas documental; o firmware ativo consome `SDGW` compacto e `TLV`.

## Pilha real do firmware

```text
Host local
  -> SDH semântico no host
  -> SDGW compacto
  -> BPM (ESP32)
       -> SdgwTransport / SdgwBluetoothEndpoint
       -> SdgwEndpointMux / SdgwSessionOwner
       -> SdgwLink
       -> GatewayApp
       -> GwRouter
       -> GwI2cBus / GwSpiBus
  -> GSA (Nano Every)
       -> Transport
       -> Link
       -> Service
       -> AnalogService / LedService
       -> BusArbiterService
       -> Tca9548Service / Mcp4725Service / EepromService
  -> UCE (Arduino Due)
       -> SpiLink
       -> UceTransport
       -> UceServiceDispatcher
       -> LedService / SdctpService
       -> CanService / CanDriver / tabelas RX-TX
```

## Boards com firmware vivo

| board | pasta real | ponto de entrada | papel na pilha | status |
| --- | --- | --- | --- | --- |
| BPM | `hardware/firmware/BPM - BACKPLANE MANAGER MODULE` | `src/main.cpp` | gateway físico entre host e boards | `IMPLEMENTADO` |
| GSA | `hardware/firmware/GSA - Gerador de sinais analógicos` | `src/main.cpp` | board remota de geração analógica | `IMPLEMENTADO` |
| UCE | `hardware/firmware/UCE - Unidade de comunicacao externa` | `src/main.cpp` | board remota SPI para LED, CAN e SDCTP | `IMPLEMENTADO` |
| PSU, GSC, URL, SLU, UCO, UCS, UIOD, UHM | não encontrado em `hardware/firmware` | inexistente | reservadas na árvore documental | `PLANEJADO` |

## BPM: composição real

Em `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/main.cpp`, a BPM é composta de forma estática:

```cpp
static SdgwTransport serialTransport(Serial);
static SdgwBluetoothEndpoint bluetoothTransport("SimulDIESEL - BPM");
static SdgwSessionOwner sessionOwner(SDGW_ENDPOINT_NONE);
static SdgwEndpointMux transportMux(sessionOwner, serialTransport, bluetoothTransport);
static SdgwLink sdgwLink(transportMux, sessionOwner);
static GwI2cBus i2cBus(Wire);
static GwSpiBus spiBus(SPI);
static GwRouter router(i2cBus, spiBus);
static GatewayApp app(sdgwLink, router);
```

Esse trecho mostra a ordem estrutural real:

1. endpoints físicos
2. arbitragem de ownership
3. link `SDGW`
4. aplicação do gateway
5. roteador para barramentos

## GSA: composição real

Em `hardware/firmware/GSA - Gerador de sinais analógicos/src/main.cpp`, a GSA é montada assim:

```cpp
static Transport g_transport;
static LedService g_led;
static EepromService g_eeprom;
static SoftwareWire g_logicalI2c(...);
static Tca9548Service g_tca9548(g_logicalI2c);
static Mcp4725Service g_mcp4725(g_logicalI2c);
static BusArbiterService g_busArbiter(g_logicalI2c, g_tca9548, g_mcp4725);
static AnalogService g_analog(g_eeprom, g_busArbiter);
static Service g_service(g_led, g_analog);
static Link g_link(g_transport, g_service);
```

Aqui a pilha real é:

1. `Transport` no `I2C` físico com a BPM
2. `Link` para validar `TLV + CRC`
3. `Service` como despachante
4. `AnalogService` e `LedService`
5. `BusArbiterService` e periféricos físicos

## UCE: composição real

Em `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`, a UCE é montada com SPI, transporte, dispatcher e serviços físicos:

```cpp
SpiLink g_link;
UceTransport g_transport(g_link);
LedService g_ledService;
SdctpService g_sdctpService;
UceServiceDispatcher g_dispatcher(g_ledService, g_sdctpService);
```

Aqui a pilha real é:

1. `SpiLink` como enlace físico com a BPM
2. `UceTransport` como transporte TLV da board
3. `UceServiceDispatcher` como despachante funcional
4. `LedService` para execução simples de LED
5. `SdctpService` como fachada atual para o caminho CAN RX/TX validado

## Decisões estruturais confirmadas

- O gateway ativo fala `SDGW`, não `SDH` textual.
- A BPM multiplexa Serial e Bluetooth, mas só um endpoint pode alimentar a sessão por vez.
- A GSA separa `I2C` físico com a BPM de `I2C` lógico com `TCA9548A` e `MCP4725`.
- O retorno físico da GSA usa `IRQ` e evento assíncrono, não BUSY/IDLE no mesmo barramento.
- A UCE fica atrás da BPM por `SPI`, usa `IRQ` para sinalizar resposta pronta e concentra a execução física de LED e CAN.

## Glossário

- **BPM**: board gateway do projeto, implementada em ESP32.
- **GSA**: board de geração de sinais analógicos, implementada em Nano Every.
- **UCE**: board de comunicação externa, implementada em Arduino Due.
- **SDGW**: protocolo binário efetivamente implementado entre host e BPM.
- **SDCTP**: protocolo de massa CAN RX/TX exposto pela UCE e consumido pelo host.
- **TLV**: contrato curto usado entre BPM e board remota.
- **Ownership**: política que garante um único endpoint ativo por sessão no gateway.

## Próximas camadas

- [Drivers de Firmware](02-drivers.md)
- [Gerenciamento de Recursos em Firmware](03-gerenciamento-recursos.md)
- [Arquitetura SDH no Gateway](04-sdh-gateway-architecture.md)
