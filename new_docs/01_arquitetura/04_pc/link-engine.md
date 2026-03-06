# SdGwLinkEngine

Responsável por implementar o protocolo SGGW no lado da aplicação PC.

## Funções

-   montar frames
-   calcular CRC
-   aplicar COBS
-   reconstruir frames recebidos
-   gerenciar ACK/ERR
-   controlar retransmissão

## Estrutura do frame

CMD \| FLAGS \| SEQ \| PAYLOAD \| CRC8
