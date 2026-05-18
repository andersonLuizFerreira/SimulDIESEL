⬅ [Retornar para Arquitetura do Software Dashboard (Local API)](01-arquitetura-software.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Camada Hardware do Software

## O que esta camada faz

Na leitura lógica do host, a camada hardware do software é o conjunto de classes que:

- sobe a sessão entre host e BPM;
- mantém o link lógico vivo;
- transforma comandos SDH em frames SDGW;
- devolve respostas e eventos para a camada funcional.

## Componentes ativos

| função | classes | estado | observação |
| --- | --- | --- | --- |
| sessão e estados do link | `SdgwHostSession`, `BpmSerialService` | `IMPLEMENTADO` | fazem handshake textual, rearmam tentativa e projetam estado |
| semântica SDH | `SdhClient`, `SdhValidator`, `SdhToSdgwMapper` | `IMPLEMENTADO` | validam targets e montam payloads compactos |
| fila, framing e ACK | `SdGwTxScheduler`, `SdGwLinkEngine` | `IMPLEMENTADO` | stop-and-wait, COBS, CRC, timeout e retry |
| watchdog de saúde | `SdGwLinkSupervisor` | `IMPLEMENTADO` | mede silêncio de RX válido e agenda ping |
| adaptação física | `SwitchableTransport`, `SerialTransport`, `BluetoothTransport` | `IMPLEMENTADO` | entregam bytes ao endpoint físico ativo |
| casos funcionais de board | `GsaClient`, `UceClient`, `BpmClient` | `IMPLEMENTADO` | transformam resposta técnica em resultado funcional; `UceClient` agora cobre LED e configuração CAN |

## Fluxos lógicos confirmados

### Subida de sessão

`SdgwHostSession` abre o transporte, drena ruído inicial, envia o banner textual `SIMULDIESELAPI` e só então libera o estado `Linked`.

### Caminho de comando

`FrmGsaLogic`, `FrmUceLogic` ou `BpmClient` chamam `SdhClient`, que valida o comando, mapeia o target para TLV SDGW e entrega o envio para `SdgwSession`.

Na UCE, isso agora cobre dois grupos de uso:

- LED residual: `UCE.led set state=on|off`
- CAN: `UCE.can.config`, `UCE.can.enable`, `UCE.can.status` e `UCE.can reset`

### Caminho de resposta

`SdGwLinkEngine` decodifica o frame, valida CRC, trata `ACK` e `ERR`, e `SdgwSession` repassa o frame lógico para quem estiver inscrito.

Para a UCE, as respostas continuam síncronas e compactas. `UceParsers` agora interpreta:

- confirmação do LED
- resposta de `config`
- resposta de `enable`
- resposta de `status`
- resposta de `reset`
- erro funcional `0x7F`
- erro de gateway `0xFE`

### Caminho de evento

`GsaClient` consome `SdgwSession.EventReceived`, interpreta `fault` e resultado físico, e sobe esses eventos para `FrmGsaLogic` e `frmGSA_UI`. `UceClient` continua trabalhando apenas com resposta síncrona compacta e entrega o resultado para `FrmUceLogic` e `frmUCE_UI`; a feature CAN da UCE não abriu canal assíncrono novo no host.

## Ponto específico da UI da UCE

Na tela `frmUCE_UI`, a área CAN agora faz:

- leitura inicial de `UCE.can.status get` ao carregar a janela
- `UCE.can.config set` ao alterar velocidade ou modo
- `UCE.can.enable set` ao marcar ou desmarcar a porta

O controller segue fixo em `can0` na UI desta rodada.

## Trecho comentado: composição lógica do host

No construtor de `SdgwHostSession`, a pilha operacional aparece inteira:

```csharp
_engine = new SdGwLinkEngine(cfg, WriteRaw);
_txScheduler = new SdGwTxScheduler(_engine);
Sdgw = new SdgwSession(_engine, _txScheduler);
Sdh = new SdhClient(Sdgw);
_linkSupervisor = new SdGwLinkSupervisor(linkSupervisorCfg, SendSupervisorPingAsync);
```

O que esse trecho faz:

- define o encadeamento real entre envio semântico, fila, engine e supervisor;
- mostra que o host possui uma sessão própria, independente de `SerialPort`;
- prepara a base que todas as operações BPM/GSA usam depois.

## O que o código não sustenta

- Não há evidência em `local-api` de barramentos internos da BPM, pinos, ISR ou IRQ de firmware como entidades observáveis pelo host.
- Não há modelo remoto `BUSY`/`IDLE` além do `Busy` local do `SdGwLinkEngine`.
- Não há stack Bluetooth nativa além do reaproveitamento de COM SPP.

## Glossário

- **Subida de sessão**: processo que leva o host de desconectado para `Linked`.
- **Watchdog lógico**: regra que considera o link vivo ou morto a partir de atividade recebida.
- **Camada hardware do software**: zona do software PC que fala diretamente com sessão, enlace e transporte.

## Próximas camadas

- [Arquitetura SDH no Host](04-sdh-host-architecture.md)
