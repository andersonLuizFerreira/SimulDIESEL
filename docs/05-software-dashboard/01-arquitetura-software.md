⬅ [Retornar para Visão Lógica do Projeto](../02-arquitetura/03-visao-logica.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Arquitetura do Software Dashboard (Local API)

## Papel lógico do software local

O software local implementado em `local-api/src/SimulDIESEL/SimulDIESEL` transforma ações do operador em comandos SDH, sessões SDGW e tráfego de bytes sobre serial ou Bluetooth.

Nesta trilha `COMO`, o foco não é a posição física da classe, mas a função que cada bloco exerce no comportamento do host.

## Fluxo lógico efetivamente implementado

```text
Operador
  -> DashBoard / frmPortaSerial_UI / frmBluetoothConnect / frmGSA_UI
  -> FrmBpmLogic / FrmGsaLogic
  -> BpmSerialService / BpmClient / GsaClient
  -> SdhClient
  -> SdhValidator / SdhToSdgwMapper
  -> SdgwSession
  -> SdGwTxScheduler
  -> SdGwLinkEngine
  -> SwitchableTransport
  -> SerialTransport ou BluetoothTransport
```

## Responsabilidades lógicas confirmadas

| responsabilidade | classes principais | estado |
| --- | --- | --- |
| abrir e fechar sessão | `FrmBpmLogic`, `BpmSerialService`, `SdgwHostSession` | `IMPLEMENTADO` |
| subir handshake textual e entrar em `Linked` | `SdgwHostSession`, `BpmParsers` | `IMPLEMENTADO` |
| traduzir intenção em comando | `FrmGsaLogic`, `GsaClient`, `BpmClient`, `SdhClient`, `SdhToSdgwMapper` | `IMPLEMENTADO` |
| arbitrar tráfego | `SdGwTxScheduler`, `SdGwLinkEngine` | `IMPLEMENTADO` |
| manter saúde do link | `SdgwHostSession`, `SdGwLinkSupervisor` | `IMPLEMENTADO` |
| tratar resposta funcional e eventos | `GsaClient`, `GsaParsers`, `frmGSA_UI` | `IMPLEMENTADO` |
| descobrir Bluetooth utilizável | `BluetoothDeviceCatalog`, `BpmBluetoothService` | `IMPLEMENTADO` |
| comunicação BPM em rede | `BpmNetworkService` | `PLANEJADO` |

## Catálogo funcional confirmado

- `IMPLEMENTADO`: `BPM.gateway ping`
- `IMPLEMENTADO`: LED builtin da GSA
- `IMPLEMENTADO`: setpoint, enable, status, fault e offsets da GSA
- `IMPLEMENTADO`: fault assíncrono `0x30`
- `IMPLEMENTADO`: resultado físico assíncrono `0x31`
- `PLANEJADO`: demais boards e demais recursos SDH ainda não mapeados no host

## Limites do comportamento atual

- O host mantém um único transporte físico ativo por vez.
- O bootstrap do link começa em ASCII e só entrega bytes binários ao engine depois de reconhecer a linha `SimulDIESEL ver...`.
- O supervisor mede silêncio por RX válido; ele não usa ping contínuo como prova exclusiva de vida.
- Bluetooth no host ainda é SPP sobre COM; não existe stack de rádio própria nesta aplicação.

## Leitura COMO deste ramo

Os próximos documentos descem até comportamento real de método:

- como a sessão sobe de `Disconnected` para `Linked`;
- como fila, ACK, timeout e retry operam;
- como um comando GSA sai da UI, volta como resposta síncrona e ainda pode gerar evento assíncrono;
- como parsing, validação e tratamento de respostas estão implementados hoje.

## Glossário

- **Fluxo lógico**: sequência funcional que transforma intenção em tráfego e resposta.
- **Resposta síncrona**: frame correlacionado diretamente ao comando recém-enviado.
- **Evento assíncrono**: frame recebido fora da correlação imediata do request atual.

## Próximas camadas

- [Camada Hardware do Software](03-camada-hardware.md)
- [Simulação de Módulos](../07-simulacoes/01-simulacao-modulos.md)
- [Manutenção de Módulos](../08-casos-de-uso/01-manutencao-modulos.md)
