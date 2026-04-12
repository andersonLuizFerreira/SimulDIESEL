⬅ [Retornar para Boards de Firmware](../README.md)
⬅ [Retornar para Índice Geral](../../../../00-INDICE.md)

# UCE

Esta página responde à trilha **ONDE** para a UCE: onde a board se encaixa, quais classes compõem o firmware atual e como ela se conecta fisicamente à BPM.

## Papel estrutural

A UCE é uma board remota controlada pela BPM por `SPI` em modo slave.

Ela fica entre:

- acima: BPM por `SPI`, `CS`, `IRQ` e reset físico
- abaixo: `Transport`, `Link`, `Service` e `LedService`

## Arquivos e classes reais

| arquivo real | classe/bloco | acima de | abaixo de | status |
| --- | --- | --- | --- | --- |
| `src/main.cpp` | composição estática da UCE | BPM | `Transport`, `Link`, serviços | `IMPLEMENTADO` |
| `lib/Transport/Transport.h` | `Transport` | `Link` | `SPI0` nativo da Arduino Due | `IMPLEMENTADO` |
| `lib/Link/Link.h` | `Link` | `Transport` | `Service` | `IMPLEMENTADO` |
| `lib/Service/Service.h` | `Service` | `Link` | `LedService` | `IMPLEMENTADO` |
| `lib/LedService/LedService.h` | `LedService` | `Service` | `LED_BUILTIN` | `IMPLEMENTADO` |
| `lib/DiagTrace/DiagTrace.h` | `DiagTrace` | firmware inteiro | `SerialUSB` | `IMPLEMENTADO` |

## Empilhamento real

```text
SPI da BPM
  -> Transport
  -> Link
  -> Service
  -> LedService
  -> LED_BUILTIN da Arduino Due
```

## Conectores físicos confirmados

As definições vivas estão em `include/config.h` na UCE e `include/SdgwDefs.h` na BPM.

| função | pino UCE | pino BPM | observação | status |
| --- | --- | --- | --- | --- |
| `SPI SCK` | `SPI header SCK` | `18` | clock do master BPM | `IMPLEMENTADO` |
| `SPI MISO` | `SPI header MISO` | `26` | resposta da UCE para a BPM | `IMPLEMENTADO` |
| `SPI MOSI` | `SPI header MOSI` | `25` | comando da BPM para a UCE | `IMPLEMENTADO` |
| `CS / NPCS0` | `D10 / PA28` | `33` | slave select nativo do `SPI0` | `IMPLEMENTADO` |
| `IRQ` | `D2` | `27` | saída da UCE para sinalizar resposta pronta | `IMPLEMENTADO` |
| reset | `RESET` físico | `23` | compartilhado com o reset global da BPM | `IMPLEMENTADO` |

## Contrato lógico e resposta TLV

- endereço lógico da UCE no gateway: `0x2`
- operação compacta usada entre host e BPM: `GW_OP_UCE_TLV_TRANSACT = 0x0`
- comando funcional validado em bancada: `UCE.led set state=on|off`
- TLV funcional atual da UCE: `type 0x12`, `len 0x01`, valor `0x00` ou `0x01`
- erro funcional da UCE: `type 0x7F`

## Observações elétricas confirmadas

- o `CS` da UCE permanece em função periférica `PA28/NPCS0` e recebe pull-up por registrador `PIOA->PIO_PUER`
- a `IRQ` da UCE é saída e não recebe `INPUT_PULLUP` no lado da UCE
- a BPM aplica `INPUT_PULLUP` no `GPIO27` para ler a `IRQ` da UCE em nível estável
- `MISO`, `MOSI` e `SCK` não exigiram pull-up externo para o caso de uso validado

## Caso de uso validado

O caso funcional validado em bancada é o acionamento do `LED_BUILTIN` da Arduino Due.

Fluxo confirmado:

1. o host envia `UCE.led set state=on|off`
2. a BPM valida, roteia para `SPI` e seleciona a UCE por `CS`
3. a UCE monta a resposta TLV síncrona em `Link`
4. a UCE sinaliza resposta pronta por `IRQ`
5. a BPM lê `header` e `payload+CRC` em dois bursts SPI
6. o host confirma o estado aplicado do `LED_BUILTIN`

## Comentário orientado a código

Em `Transport::begin()`, este trecho fixa a característica elétrica mais delicada da integração:

```cpp
PIOA->PIO_PDR = kUceSpiSignalPins;
PIOA->PIO_ABSR &= ~kUceSpiSignalPins;
PIOA->PIO_PUER = kUceSpiNativeCsPin;
```

Ele mantém `PA28/NPCS0` sob controle do periférico `SPI0` e, ao mesmo tempo, polariza o `CS` em nível alto quando a BPM não está selecionando a UCE.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
