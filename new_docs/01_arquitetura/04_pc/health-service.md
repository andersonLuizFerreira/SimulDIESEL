# SdGwHealthService

Serviço responsável por monitorar a saúde do link após o estado
`Linked`.

## Funcionamento

-   envia ping periódico
-   aguarda ACK
-   detecta falhas de transporte
-   notifica mudança de estado
