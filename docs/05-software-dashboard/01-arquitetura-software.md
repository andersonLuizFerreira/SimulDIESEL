# Arquitetura do Software Dashboard (Local API)

## Estrutura de camadas da aplicação WinForms

A aplicação em `local-api/` segue o fluxo:

`UI (Forms) → BLL → DAL → Transporte serial`

Onde:

- **UI:** `DashBoard`, `frmPortaSerial_UI`, `frmLedGw`.
- **BLL:** `SerialLinkService`, `SerialLink`, `SdGwLinkEngine`, `SdGgwClient`, `SdGwHealthService`, `LedGwTest_BLL`.
- **DAL:** `SerialTransport`, `IByteTransport`.

## Núcleo de orquestração

- `SerialLink` é um ponto de entrada único com propriedade estática do serviço de link.
- `SerialLinkService` mantém estado (`Disconnected`, `SerialConnected`, `Draining`, `BannerSent`, `Linked`, `LinkFailed`) e dispara eventos para a UI.

## Fluxo de execução

1. UI solicita conexão.
2. `SerialTransport` abre COM (ou mantém fechado).
3. Em estado serial ok, inicia handshake textual.
4. Após banner válido, ativa transporte de frames SGGW.
5. Comandos de negócio (`LED`) passam por `SdGgwClient`.

## Responsabilidades por camada

- UI trata interação e atualização visual.
- BLL cuida de protocolo e estado.
- DAL trata apenas bytes e conexão física.

## Pontos de estabilidade já observados

- Eventos são inscritos/unsubscritos com cuidado para atualização thread-safe.
- Fechamento da conexão não depende do fechamento de janelas auxiliares (`frmPortaSerial_UI`).

[Retornar ao README principal](../README.md)
