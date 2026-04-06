⬅ [Retornar para Visão Física do Projeto](02-visao-fisica.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# API e Host Local

## Recorte auditado

Esta página documenta o host realmente implementado em `local-api/src/SimulDIESEL/SimulDIESEL`.

O código observado confirma uma aplicação WinForms única, sem separação em múltiplos executáveis no host atual. As pastas `local-api/src/SimulDiesel.Application`, `SimulDiesel.Domain`, `SimulDiesel.Infrastructure`, `SimulDiesel.LocalApi`, `SimulDiesel.LocalApp`, `SimulDiesel.Protocols` e `SimulDiesel.Shared` continuam apenas como estrutura futura com `.gitkeep`; portanto, neste recorte elas permanecem `PLANEJADO`.

## Empilhamento físico real

```text
Program.Main
  -> DashBoard
  -> UI (frmPortaSerial_UI, frmBluetoothConnect, frmGSA_UI)
  -> FormsLogic (FrmBpmLogic, FrmGsaLogic)
  -> Fachada do host (BpmSerialService)
  -> Clients funcionais (BpmClient, GsaClient)
  -> Sessão host (SdgwHostSession)
  -> DAL semântica (SdhClient, SdhValidator, SdhToSdgwMapper, SdgwSession)
  -> DAL de enlace (SdGwTxScheduler, SdGwLinkEngine, SdGwLinkSupervisor)
  -> Transporte (SwitchableTransport, IByteTransport)
  -> SerialTransport ou BluetoothTransport
  -> Porta COM do Windows
```

## Inventário real por camada

| camada | arquivos e classes reais | pai estrutural | filho estrutural | estado | papel no host |
| --- | --- | --- | --- | --- | --- |
| UI | `Program.cs`, `DashBoard.cs`, `UI/frmPortaSerial_UI.cs`, `UI/frmBluetoothConnect.cs`, `UI/frmGSA_UI.cs` | operador | `FrmBpmLogic`, `FrmGsaLogic` | `IMPLEMENTADO` | abre telas, exibe estado e dispara ações do operador |
| BLL de forms | `BLL/FormsLogic/BPM/FrmBpmLogic.cs`, `BLL/FormsLogic/GSA/FrmGsaLogic.cs` | UI | `BpmSerialService`, `GsaClient` | `IMPLEMENTADO` | adapta a árvore interna do host para métodos consumíveis pela UI |
| BLL de boards | `BLL/Boards/BPM/BpmClient.cs`, `BLL/Boards/GSA/GsaClient.cs` | FormsLogic | `SdhClient`, `SdgwSession` | `IMPLEMENTADO` | concentra casos de uso de BPM e GSA |
| Fachada da sessão | `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs`, `BLL/Boards/BPM/Comm/SdgwHostSession.cs` | FormsLogic e clients | DAL de protocolo e transporte | `IMPLEMENTADO` | compõe o host, sobe o link e projeta estado para cima |
| DAL de protocolo | `DAL/Protocols/SDGW/SdhClient.cs`, `SdhValidator.cs`, `SdhToSdgwMapper.cs`, `SdgwSession.cs` | BLL | scheduler e link engine | `IMPLEMENTADO` | valida, mapeia e entrega comandos SDH para o enlace SDGW |
| DAL de enlace | `DAL/Protocols/SDGW/SdGwTxScheduler.cs`, `SdgwLinkEngine.cs`, `SdgwLinkSupervisor.cs`, `SdgwFrameReader.cs`, `SdgwFrameWriter.cs`, `SdgwFrameCodec.cs` | DAL de protocolo | transporte | `IMPLEMENTADO` | fila, stop-and-wait, ACK, retry, framing, COBS e watchdog |
| Transporte | `DAL/Transport/SwitchableTransport.cs`, `DAL/Transport/Serial/*.cs`, `DAL/Transport/Bluetooth/*.cs` | DAL de enlace | Windows/COM | `IMPLEMENTADO` | mantém uma sessão física ativa e entrega bytes ao SO |
| DTL | `DTL/Common/*.cs`, `DTL/Boards/BPM/*.cs`, `DTL/Boards/GSA/*.cs`, `DTL/Protocols/SDGW/*.cs` | compartilhada por UI, BLL e DAL | compartilhada por UI, BLL e DAL | `IMPLEMENTADO` | fixa DTOs, enums, requests, responses e contratos SDH/SDGW |
| Rede | `BLL/Boards/BPM/Comm/Network/BpmNetworkService.cs` | `BpmSerialService.Network` | nenhum | `PLANEJADO` | slot reservado para um futuro transporte não serial |

## Pontos de entrada reais

- `Program.Main()` abre apenas `DashBoard`.
- `DashBoard.toolStripConectar_Click(...)` abre `frmPortaSerial_UI` para serial.
- `DashBoard.toolStripBluetooth_Click(...)` não abre a tela dedicada; ele chama `FrmBpmLogic.ConnectBluetoothPadrao()` e tenta conectar imediatamente ao dispositivo preferencial.
- `frmGSA_UI` cria `FrmGsaLogic` e opera a GSA inteira por essa trilha, sem falar direto com `SerialPort`.

## Conectores estruturais mais importantes

- `FrmBpmLogic` sobe `BpmStatusDto` para a UI e desce chamadas para `BpmSerialService`.
- `BpmSerialService` expõe `Sdh`, `Sdgw`, `Bpm`, `Gsa`, `Bluetooth`, `Backplane`, `XConn` e `Network`.
- `GsaClient` e `BpmClient` nunca falam com `IByteTransport`; ambos entram em `SdhClient`.
- `SdgwHostSession` usa `IByteTransport`, mas cria internamente `SdGwLinkEngine`, `SdGwTxScheduler`, `SdgwSession` e `SdhClient`.
- `SwitchableTransport` escolhe `SerialTransport` ou `BluetoothTransport` conforme `TransportConnectionSettings.TransportKind`.

## Trecho âncora da composição

Em `BLL/Boards/BPM/Comm/Serial/BpmSerialService.cs`, o construtor mostra exatamente onde a pilha do host se fecha:

```csharp
_transport = new SwitchableTransport();
_session = new Comm.SdgwHostSession(_transport);
Bluetooth = new Comm.Bluetooth.BpmBluetoothService(this);
Network = new Comm.Network.BpmNetworkService();
Gsa = new GsaClient(Sdh, Sdgw);
Bpm = new BpmClient(Sdh, this, Backplane, XConn);
```

Esse trecho é importante porque concentra a árvore ativa do host em um ponto físico único: acima dele ficam UI e FormsLogic; abaixo dele começam sessão, DAL e transporte.

## Limites confirmados pelo código

- `IMPLEMENTADO`: serial direta e Bluetooth Classic SPP sobre COM.
- `IMPLEMENTADO`: catálogo funcional ativo para `BPM.gateway ping` e operações `GSA.*`.
- `PARCIALMENTE IMPLEMENTADO`: `frmBluetoothConnect` existe e funciona, mas o atalho principal do `DashBoard` usa conexão automática por dispositivo preferencial.
- `PARCIALMENTE IMPLEMENTADO`: `BackplaneService` e `XConnService` já existem na composição, porém retornam apenas mensagens de expansão preparada.
- `PLANEJADO`: transporte de rede equivalente ao serial/Bluetooth.
- `LEGADO`: a documentação antiga usava `Sggw`; no host atual os tipos ativos são `SdgwFrame` e `SdgwCommand`.

## Glossário

- **Host local**: aplicação WinForms que roda no PC e controla a bancada.
- **Fachada do host**: ponto da BLL que compõe sessão, clients e transportes.
- **Borda BLL/DAL**: região onde intenção funcional vira chamada de protocolo.
- **Sessão única**: regra segundo a qual só um transporte físico fica ativo por vez.

## Próximas camadas

- [BLL do Host](05-bll-do-host.md)
- [DAL do Host](06-dal-do-host.md)
- [DTL do Host](07-dtl-do-host.md)
- [Transporte do Host](08-transporte-do-host.md)
