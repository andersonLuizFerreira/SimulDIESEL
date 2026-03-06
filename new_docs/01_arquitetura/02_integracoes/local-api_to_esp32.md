# Integração --- Local API (.NET) ↔ ESP32 Gateway

**Status:** Documento consolidado com base no código atual.

------------------------------------------------------------------------

## 1. Escopo

Descrever a integração entre a aplicação PC (.NET) e o Gateway ESP32.

------------------------------------------------------------------------

## 2. Fluxo de comunicação

### 1. Conexão física

A aplicação PC abre a porta serial através de:

SerialTransport

Responsabilidades:

-   abrir e fechar a porta
-   enviar bytes
-   receber bytes
-   sinalizar falhas físicas

------------------------------------------------------------------------

### 2. Handshake textual

Após a abertura da porta:

1.  o estado muda para **SerialConnected**
2.  ocorre a fase **Draining** (descartar lixo de boot)
3.  a aplicação envia o banner:

SIMULDIESELAPI

4.  o gateway responde com:

SimulDIESEL ver ...

Quando reconhecido, o estado passa para:

Linked

------------------------------------------------------------------------

### 3. Operação binária

Após `Linked`, a comunicação passa a utilizar:

SGGW

Fluxo:

SerialTransport → SerialLinkService → SdGwLinkEngine → SdGgwClient

------------------------------------------------------------------------

### 4. Monitoramento do link

Após o link estabelecido:

SdGwHealthService executa **ping periódico** para validar a conexão.

------------------------------------------------------------------------

## 3. Componentes envolvidos

No lado PC:

-   SerialTransport
-   SerialLinkService
-   SdGwLinkEngine
-   SdGwHealthService
-   SdGgwClient

No lado Gateway:

-   parser SGGW
-   roteador de comandos
-   interface de barramento para baby boards
