⬅ [Retornar para Arquitetura SDH no Host](../04-sdh-host-architecture.md)
⬅ [Retornar para Índice Geral](../../../00-INDICE.md)

# Handshake e Estados da Sessão

## Função lógica

`SdgwHostSession` é a classe que leva o host de `Disconnected` para `Linked`. Ela segura o binário SDGW até terminar o bootstrap textual com a BPM.

## Estados confirmados

| estado na sessão | papel | estado de implementação |
| --- | --- | --- |
| `Disconnected` | sem transporte aberto | `IMPLEMENTADO` |
| `TransportConnected` | transporte abriu, mas ainda sem banner aceito | `IMPLEMENTADO` |
| `Draining` | janela curta para drenar ruído inicial | `IMPLEMENTADO` |
| `BannerSent` | banner textual foi enviado e a sessão aguarda a linha da BPM | `IMPLEMENTADO` |
| `Linked` | sessão SDGW estabelecida | `IMPLEMENTADO` |
| `LinkFailed` | tentativa expirou ou supervisor derrubou o link | `IMPLEMENTADO` |

Na UI BPM, esse estado ainda é projetado como `SerialConnected` ou `BluetoothConnected` por `BpmSerialService.MapState(...)`.

## Parâmetros reais

- `DrainWindow = 300 ms`
- `HandshakeTimeout = 2000 ms`
- `RetryInterval = 3 s`
- `ApiBanner = "\nSIMULDIESELAPI\n"`
- prefixo de identificação esperado pela BPM: `SimulDIESEL ver`

## Trecho comentado: abertura de tentativa

Em `SdgwHostSession.LinkTick(...)`, linhas em torno da abertura da tentativa:

```csharp
if (!_attemptActive)
{
    _rxBuffer.Clear();
    _draining = true;
    _drainUntil = now.Add(DrainWindow);
    _attemptDeadlineUtc = now.Add(HandshakeTimeout);
    _attemptActive = true;
    SetState(SessionState.Draining);
    return;
}
```

O que esse trecho faz:

- inicia uma tentativa formal de subida do link;
- limpa texto residual acumulado no buffer;
- abre uma janela curta de drenagem antes do banner;
- fixa um deadline absoluto para a tentativa atual.

## Trecho comentado: envio do banner

Ainda em `LinkTick(...)`, quando a janela de drain termina:

```csharp
if (_draining && now >= _drainUntil)
{
    _draining = false;
    _transport.Write(Encoding.ASCII.GetBytes(ApiBanner));
    SetState(SessionState.BannerSent);
}
```

Esse é o ponto exato em que o host troca o estado de apenas conectado para tentativa efetiva de sessão.

## Trecho comentado: aceitação da interface

Em `HandleHandshakeBytes(...)`, o host fecha a transição para `Linked`:

```csharp
if (BpmParsers.TryParseInterfaceInfo(line, out deviceInfo))
{
    _sdgwSessionEstablished = true;
    SetState(SessionState.Linked);
    _attemptActive = false;
    StopAndDisposeLinkTimer_NoLock();
    InterfaceName = deviceInfo.Version;
}
```

O que esse trecho faz:

- testa cada linha ASCII recebida contra `BpmParsers.TryParseInterfaceInfo(...)`;
- só considera o handshake concluído quando a linha contém `SimulDIESEL ver`;
- grava o texto inteiro em `InterfaceName`, que depois sobe para a UI.

## Regras de transição observadas

- se o transporte caiu, a sessão volta para `Disconnected`;
- se a tentativa local de handshake expirou, a sessão entra em `LinkFailed` e só tenta de novo após `RetryInterval`;
- se o supervisor derruba a saúde do link já em `Linked`, a sessão entra em `LinkFailed` e rearma a tentativa imediatamente.

## Glossário

- **Bootstrap textual**: etapa inicial em que o host ainda conversa por linhas ASCII.
- **Drain**: janela curta de descarte de ruído antes do banner.
- **Linha de interface**: texto da BPM que identifica a versão e autoriza a entrada em `Linked`.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
