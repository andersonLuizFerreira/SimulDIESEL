# Nome

Firmware BPM

## Objetivo

Orientar ETAPAS do firmware BPM como gateway, endpoints Serial/Bluetooth, SDGW embarcado e roteamento para I2C/SPI.

## Quando usar

Use para gateway BPM, ownership de endpoints, `SdgwLink`, `GatewayApp`, `GwRouter`, DeviceTable e eventos de boards.

## Quando nao usar

Nao use para regra CAN/J1939, UI ou schema do Banco de Modulos.

## Escopo permitido

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/`
- docs/dumps relacionados a BPM/SDGW.

## Escopo proibido

- Colocar regra de negocio de UCE/GSA no gateway.
- Mudar contratos host/firmware sem ETAPA de protocolo.

## Arquivos/pastas provaveis

- `src/main.cpp`
- `include/SdgwDefs.h`
- `lib/SdgwTransport/`
- `lib/SdgwLink/`
- `lib/Gateway/`
- `lib/GwRouter/`
- `lib/GwI2cBus/`
- `lib/GwSpiBus/`

## Padroes do projeto

- BPM e gateway/roteador.
- Serial e Bluetooth sao endpoints de entrada com ownership.
- I2C atende GSA; SPI atende UCE.
- Gateway roteia TLV, nao interpreta semantica CAN/J1939.
- GSA permanece geradora de sinais analogicos; BPM apenas roteia o acesso fisico correspondente.

## Checklist de validacao

- [ ] `platformio run` em BPM.
- [ ] Confirmar endpoints e ownership.
- [ ] Confirmar rotas I2C/SPI.
- [ ] Validar que nao entrou regra CAN/J1939.

## Checklist de entrega

- [ ] Roteamento/endpoint alterado.
- [ ] Pinos/defs impactados.
- [ ] Resultado PlatformIO.
- [ ] Impacto em host/UCE/GSA.

## Riscos comuns

- Gateway virar camada de negocio.
- Quebrar evento assincrono de board.
- Alterar DeviceTable sem atualizar docs.

## Regras de nao regressao

- BPM permanece SDGW + roteador fisico.
- Contratos de pinos e barramentos permanecem rastreaveis.
- GSA/UCE continuam donas da execucao fisica.


