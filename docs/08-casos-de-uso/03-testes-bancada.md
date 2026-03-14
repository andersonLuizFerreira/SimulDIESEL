# Testes de Bancada

## Estado atual

Os testes de bancada sustentados pelo repositório são majoritariamente manuais e guiados por software local. O foco atual é validar a cadeia completa de comunicação, do host ao periférico, e não executar uma suíte automatizada grande.

Os elementos concretos disponíveis para bancada são:

- abertura de porta serial;
- handshake e monitoramento de link;
- `ping` de saúde;
- roteamento para dispositivo;
- leitura e escrita de estado do GSA.

## Funcionamento técnico

### Roteiro mínimo de bancada

```text
1. Ligar a bancada
2. Abrir a serial no software local
3. Confirmar banner do gateway
4. Verificar se o link entra em Linked
5. Executar ping
6. Acionar comando funcional no periférico
7. Validar resposta e efeito físico
```

### Exemplo de teste ponta a ponta

Caso de teste: acionamento de LED no GSA.

1. Host envia comando com `ADDR_GSA`.
2. Gateway traduz para TLV interno.
3. GSA grava o estado do LED.
4. GSA retorna confirmação.
5. Host marca a requisição como concluída ao receber a resposta com o mesmo `SEQ`.

### Evidências técnicas

- o host possui timeout e retransmissão;
- o gateway possui `ACK`, cache de resposta e watchdog;
- o periférico possui validação de `CRC` e registro de erros de link.

Esses pontos tornam o teste de bancada útil também para depurar se a falha está no host, no enlace, no gateway ou no módulo.

## Limitações

O repositório não traz, no estado atual, uma suíte formal de teste automatizado de bancada, fixtures documentados ou instrumentação de medição elétrica integrada. O procedimento ainda depende de observação do operador e da montagem física correta do conjunto.

## Evolução prevista

Os testes de bancada podem evoluir para:

- roteiros repetíveis por dispositivo;
- captura estruturada de payloads e respostas;
- relatórios automáticos de falha por camada;
- integração com futuras funcionalidades de sensores e protocolos automotivos.

[Retornar ao README principal](../README.md)
