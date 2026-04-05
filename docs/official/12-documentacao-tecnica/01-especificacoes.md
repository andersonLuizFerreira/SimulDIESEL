⬅ [Retornar para 00-INDICE — Mapa da árvore documental](../../00-INDICE.md)

# Especificações

## Estado atual

As especificações mais sólidas do SimulDIESEL, no estado atual do repositório, estão concentradas em três blocos:

* enlace host/gateway
* roteamento do gateway
* contrato funcional e binário da GSA

Esta página funciona como entrada para esse conjunto. Ela resume **o que já está especificado com segurança** e deixa o detalhe formal para as camadas imediatamente inferiores.

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

### Especificação do domínio funcional atual

O domínio funcional hoje especificado com mais profundidade é o da **GSA**.

Nele já existe documentação formal para:

* comandos SDH vigentes
* envelope TLV interno
* tipos binários
* políticas de resposta síncrona e evento assíncrono
* limites por canal e mapeamento elétrico já formalizado

## Limitações

As especificações atualmente consolidadas são fortes na camada de transporte e ainda seletivas na camada funcional. Não há catálogo amplo de comandos de domínio para todas as boards, nem especificação formal ativa para `CAN`, `J1939` ou serviços `cloud`. Também faltam especificações textuais completas das placas físicas.

## Evolução prevista

As próximas especificações que merecem formalização são:

- contratos por dispositivo além do GSA;
- matriz oficial de erros e eventos do gateway;
- contratos `cloud` quando os endpoints deixarem de estar vazios;
- critérios de compatibilidade entre revisões de firmware e software local.

## Próximas camadas

- [Diagramas](02-diagramas.md)


