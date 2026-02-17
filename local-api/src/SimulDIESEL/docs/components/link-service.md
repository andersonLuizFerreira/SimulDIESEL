# SerialLinkService (Serviço de Link Serial)

## Propósito
Gerenciar a conexão serial de alto nível, handshake de estabelecimento de link com o firmware (banner), máquina de estados do link, integração entre `SerialTransport`, `SdGwLinkEngine` e `SdGwHealthService`, e expor `SdGgwClient` ao restante da aplicação.

## Escopo
Classe: `SimulDIESEL.BLL.SerialLinkService`

## Responsabilidades
- Inicializar `SerialTransport`, `SdGwLinkEngine` e `SdGwHealthService`.
- Gerenciar ciclo de conexão / reconexão e handshake baseado em envio de banner API.
- Roteamento de bytes recebidos para handshake ou engine (dependendo do estado).
- Expor eventos: `LinkStateChanged`, `ConnectionChanged`, `Error`, `BytesReceived`, `NomeDaInterfaceChanged`.
- Fornecer helper `ListarPortas()` que delega a `SerialTransport.ListPorts()`.

## Máquina de estados (LinkState)
Estados enumerados em `LinkState`:
- `Disconnected` — sem conexão serial ativa.
- `SerialConnected` — transporte aberto, ainda sem handshake finalizado.
- `Draining` — janela de "drain" aguardando limpeza do RX antes do banner.
- `BannerSent` — banner API enviado, aguardando resposta do firmware.
- `Linked` — link estabelecido (linha aceita e nome gravado).
- `LinkFailed` — tentativa de handshake falhou (aguarda retry).

Transições principais:
- `Disconnected` -> (transport connected) -> `SerialConnected`
- `SerialConnected` -> start handshake -> `Draining`
- `Draining` -> (após DrainWindow) -> `BannerSent` (envia banner "\nSIMULDIESELAPI\n")
- `BannerSent` -> (recebe linha contendo "SimulDIESEL ver") -> `Linked`
- Timeout durante tentativa -> `LinkFailed` -> aguardando `_nextAttemptAtUtc` -> recomeça handshake
- Transport down (em qualquer estado) -> `Disconnected`

Observação: `LinkStateChanged` é disparado sempre que muda o estado.

## Handshake operacional
- Ao detectar `ConnectionChanged(true)` do transporte:
  - Estado para `SerialConnected`.
  - Zera flags e buffers e aciona `StartLinkLoop_NoLock()`.
- O loop de handshake (`_linkTimer` + `LinkTick`) faz:
  - Se iniciar tentativa: limpa `_rxBuffer`, define `_draining = true` e `_drainUntil = now + DrainWindow`, deadline `_attemptDeadlineUtc = now + HandshakeTimeout`.
  - Ao sair do drain (`now >= _drainUntil`) envia `API_BANNER = "\nSIMULDIESELAPI\n"` via transporte e muda estado para `BannerSent`.
  - Durante `BannerSent` acumula linhas ASCII recebidas e procura por linha que contenha `ESP_OK_PREFIX = "SimulDIESEL ver"` (case-insensitive). Ao encontrá-la:
    - Estado -> `Linked`
    - Para timer
    - `NomeDaInterface` é preenchido com a linha recebida
    - Dispara `NomeDaInterfaceChanged`.
  - Se o deadline expirar sem resposta, `LinkFailed` e agenda próxima tentativa após `RetryInterval` (3s).

## Reconexão automática
- Se transporte cair, `OnTransportConnectionChanged(false)`:
  - Desabilita health (`_health.SetEnabled(false)`).
  - Informa engine via `_engine.OnTransportDown`.
  - Para handshake e reseta estado (`Disconnected`).
- Ao reconectar, inicia novamente handshake com temporizador.

## Integração com `SdGwLinkEngine`
- `SerialLinkService` cria `SdGwLinkEngine` com config e fornece delegate `WriteRaw` (escreve no `SerialTransport`).
- Enquanto `State == Linked` os bytes recebidos são encaminhados a `_engine.OnBytesReceived(data)`.
- `SerialLinkService` invoca `_engine.OnTransportDown(...)` ao desconectar para garantir completa finalização de envios pendentes.

## Integração com `SdGwHealthService`
- `SdGwHealthService` é criado e registrado para receber notificações.
- `SerialLinkService` ativa `_health.SetEnabled(true)` apenas quando estado for `Linked` (via handler `OnLinkStateChanged_ForHealth`).
- Ao receber `LinkHealthChanged(false)` enquanto em `Linked` e transporte ainda aberto, `SerialLinkService`:
  - Muda estado para `LinkFailed`
  - Reseta handshake e reinicia tentativa de link (imediatamente definindo `_nextAttemptAtUtc = now`).

## Eventos públicos
- `LinkStateChanged(LinkState)` — alterações do estado lógico do link.
- `ConnectionChanged(bool)` — repassa evento do transporte.
- `Error(string[])` — repassa erros do transporte.
- `BytesReceived(byte[])` — repassa bytes brutos recebidos do transporte (útil para logs).
- `NomeDaInterfaceChanged()` — notifica alteração do identificador textual do firmware quando link estabelecido.

## Tratamento de erros
- Erros de transporte repassados do `SerialTransport` via `Error` são simplesmente retransmitidos.
- Handshake timeouts são tratados internamente e resultam em `LinkFailed`.
- Health service detecta falhas de nível aplicativo e força renegociação do link.

## Thread safety
- Usa `lock(_linkSync)` para proteger estado relacionado ao handshake (`_rxBuffer`, `_draining`, `_attemptActive`, timers, `_nextAttemptAtUtc`, etc).
- Eventos externos são disparados fora do lock (ex.: `NomeDaInterfaceChanged`, `ConnectionChanged`) para evitar reentrância.

## Exemplo de sequência (ASCII)
PC Application (UI)
   ↓
`SerialLinkService.Sggw` (SdGgwClient)
   ↓
`SdGwLinkEngine` (via delegate WriteRaw)
   ↓
`SerialTransport.Write` -> SerialPort -> ESP32

Handshake:
SerialTransport (port up)
   ↓
SerialLinkService (SerialConnected -> Draining -> BannerSent)
   ↓
envia "\nSIMULDIESELAPI\n"
   ↓
ESP32 responde linha "SimulDIESEL ver X.Y"
   ↓
SerialLinkService => Linked, NomeDaInterface preenchido