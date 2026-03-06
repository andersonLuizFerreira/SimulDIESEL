# SGGW --- Interface de Integração

Define a separação entre:

-   Transporte
-   Engine de protocolo
-   Consumidor de alto nível

------------------------------------------------------------------------

## Transporte

Responsável por:

-   abrir conexão
-   enviar bytes
-   receber bytes
-   sinalizar falhas

Exemplo no SimulDIESEL:

SerialTransport

------------------------------------------------------------------------

## Engine

Responsável por:

-   montar frames
-   CRC
-   COBS
-   parsing de frames
-   ACK/ERR

Implementação atual:

SdGwLinkEngine

------------------------------------------------------------------------

## Cliente de alto nível

Responsável por:

-   enviar comandos
-   receber eventos
-   expor API de domínio

Implementação atual:

SdGgwClient
