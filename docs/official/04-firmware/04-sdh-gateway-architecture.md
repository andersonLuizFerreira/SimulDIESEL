⬅ [Retornar para Arquitetura de Firmware](01-arquitetura-firmware.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Arquitetura SDH no Gateway

O nome histórico desta página foi preservado, mas a verdade do código hoje é outra:

- o host interpreta `SDH`;
- a BPM não faz parse de `SDH`;
- o gateway embarcado recebe `SDGW` compacto e payload `TLV` já resolvido.

Esta página documenta **como o gateway realmente opera hoje**.

## Estado real do gateway

- **IMPLEMENTADO**: sessão `SDGW` com handshake textual, `COBS`, `CRC8`, `ACK`, `ERR`, sequência e watchdog.
- **IMPLEMENTADO**: roteamento local `BPM.gateway ping`.
- **IMPLEMENTADO**: roteamento remoto para a GSA via `GwRouter`.
- **PARCIALMENTE IMPLEMENTADO**: infraestrutura `SPI` pronta no gateway, mas sem device vivo na tabela.
- **PLANEJADO**: parser `SDH` dentro da BPM.

## Fluxo operacional real

```text
Host
  -> resolve SDH para SDGW compacto
  -> envia frame para BPM
  -> SdgwLink valida e trata handshake/ACK
  -> GatewayApp decide local x remoto
  -> GwRouter escolhe I2C ou SPI
  -> board remota responde TLV
  -> BPM devolve resposta SDGW
```

## Ponto de entrada do gateway

Em `src/main.cpp`, o loop principal da BPM é mínimo:

```cpp
void loop()
{
    sdgwLink.poll();
    app.tick();
}
```

Esse desenho é importante porque separa duas responsabilidades:

- `sdgwLink.poll()` cuida de sessão, framing e dispatch de comando;
- `app.tick()` drena eventos físicos pendentes, especialmente os da GSA.

## Onde o handshake acontece

O handshake não está em `GatewayApp`, mas em `SdgwLink`.

Trecho real de `SdgwLink::processHandshakeByte(...)`:

```cpp
if (memcmp(tail, SDGW_PC_BANNER, need) == 0)
{
    sendBanner();
    _hs = Linked;
    _tr.setTextEnabled(false);
}
```

Esse trecho faz três coisas críticas:

1. detecta o banner textual do host
2. responde com o banner do device
3. desliga o modo texto e entra em `Linked`

## Onde o comando vira ação local ou remota

O corte entre BPM local e board remota está em `GatewayApp::onCommand(...)`:

```cpp
const uint8_t addr = GW_CMD_ADDR(cmd);

if (addr == GW_ADDR_BPM) {
    handleGatewayLocal(cmd, data, dataLen);
    return;
}
```

Hoje isso significa:

- `GW_ADDR_BPM` fica na própria BPM;
- `GW_ADDR_GSA` segue para o roteador;
- `GW_ADDR_BROADCAST` é ignorado no código atual.

## Onde a GSA entra no caminho

Quando a GSA baixa `IRQ`, a BPM não executa lógica analógica; ela apenas drena eventos:

```cpp
if (!_router.pollGsaEvent(eventPacket, sizeof(eventPacket), eventLen)) {
    break;
}

_link.sendEvent(SDGW_CMD_GSA_TLV, eventPacket, (uint8_t)eventLen);
```

Esse bloco existe para transformar o evento curto da GSA em evento `SDGW` para o host.

## Consequência arquitetural

O gateway ativo do projeto não é um interpretador semântico amplo. Ele é:

- um terminador de sessão `SDGW`;
- um roteador compacto `ADDR/OP`;
- um adaptador entre host e barramentos da bancada.

## Glossário

- **GatewayApp**: camada da BPM que separa comandos locais de comandos roteados.
- **Handshake**: transição do banner textual para o modo binário `SDGW`.
- **ADDR/OP**: compactação do comando usada pelo gateway atual.
- **Tick**: trabalho cooperativo executado no loop fora do parser principal.

## Próximas camadas

- [Catálogo de Baby Boards e Targets SDH](05-catalogo-baby-boards.md)
- [Tabela Mestra de Binding Lógico-Físico do Gateway](06-gateway-binding-logico-fisico.md)
- [Resolver Engine do Gateway](07-resolver-engine-gateway.md)
