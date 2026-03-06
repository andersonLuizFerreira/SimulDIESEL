# Serial Connection --- Aplicação PC

## Componentes

-   SerialLink
-   SerialLinkService
-   SerialTransport

## Fluxo

1.  A UI solicita conexão.
2.  SerialLinkService usa SerialTransport para abrir a porta.
3.  Eventos de conexão são propagados para a UI.
4.  Handshake textual é iniciado.
