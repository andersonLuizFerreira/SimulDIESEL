# Link Handshake

## Estados

-   Disconnected
-   SerialConnected
-   Draining
-   BannerSent
-   Linked
-   LinkFailed

## Processo

1.  Porta serial abre.
2.  Bytes iniciais são descartados (draining).
3.  Banner `SIMULDIESELAPI` é enviado.
4.  Gateway responde com `SimulDIESEL ver`.
5.  Link passa para estado `Linked`.
