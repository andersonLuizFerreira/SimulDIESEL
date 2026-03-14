# Especificações

## Estado atual

As especificações mais sólidas do SimulDIESEL, no estado atual do repositório, estão concentradas no protocolo do gateway e no contrato interno do GSA. Esses dois blocos já permitem registrar limites, estruturas de payload e decisões técnicas sem extrapolar o que não está implementado.

## Funcionamento técnico

### Especificação do frame host/gateway

Fonte principal: `Sggw.defs.h`, `SggwLink`, `SdGwLinkEngine`.

Estrutura lógica:

```text
CMD | FLAGS | SEQ | PAYLOAD... | CRC8
```

Regras observadas:

- delimitação externa por `0x00`;
- codificação `COBS`;
- verificação com `CRC8 ATM`;
- `CMD` dividido em endereço e operação;
- `FLAGS` contendo, entre outros, pedido de `ACK` e marcação de evento.

Limites:

- `SGGW_LOGICAL_MTU = 250`;
- `payload` máximo lógico de `247` bytes, considerando cabeçalho e `CRC`.

### Especificação do enlace

Estados do host:

```text
Disconnected -> SerialConnected -> Draining -> BannerSent -> Linked/LinkFailed
```

Estados do gateway:

```text
WaitingBanner -> Linked
```

Há timeout e repetição no host, além de deduplicação por `SEQ` em ambos os lados.

### Especificação do contrato interno do GSA

Estrutura:

```text
T | L | V... | CRC8
```

Comandos comprovados:

- leitura de erro;
- limpeza de erro;
- leitura do LED;
- escrita do LED.

Exemplo de escrita:

```text
T = CMD_SET_LED
L = 1
V = 0x00 | 0x01 | 0x02
CRC8
```

## Limitações

As especificações atualmente consolidadas são fortes na camada de transporte e ainda enxutas na camada funcional. Não há catálogo amplo de comandos de domínio, nem especificação formal ativa para `CAN`, `J1939` ou serviços `cloud`. Também faltam especificações textuais completas das placas físicas.

## Evolução prevista

As próximas especificações que merecem formalização são:

- contratos por dispositivo além do GSA;
- matriz oficial de erros e eventos do gateway;
- contratos `cloud` quando os endpoints deixarem de estar vazios;
- critérios de compatibilidade entre revisões de firmware e software local.

[Retornar ao README principal](../README.md)
