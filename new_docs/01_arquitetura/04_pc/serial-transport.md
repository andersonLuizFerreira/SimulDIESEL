# SerialTransport --- DAL

## Propósito

Implementar o transporte serial cru utilizando
`System.IO.Ports.SerialPort`.

Responsabilidades:

-   listar portas
-   abrir conexão
-   fechar conexão
-   enviar bytes
-   receber bytes
-   sinalizar falhas físicas

O componente não implementa:

-   framing
-   COBS
-   CRC
-   ACK/ERR
-   lógica de protocolo
