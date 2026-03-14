# Planejamento

## Estado atual

O principal artefato de planejamento identificado no repositório está em `docs/legacy-docs/04_desenvolvimento/technical-roadmap.md`. Ele descreve uma evolução por fases, começando pela base serial e avançando para protocolo, roteamento e expansão funcional. O estado atual do código confirma que a fundação de comunicação local e o primeiro periférico já saíram do nível conceitual e entraram em implementação.

Em termos práticos, o planejamento realizado até aqui materializou:

- enlace serial funcional com handshake;
- protocolo host/gateway com `COBS + CRC8`;
- gateway com roteamento por tabela;
- primeiro dispositivo remoto com contrato real.

## Funcionamento técnico

### Linha de evolução observável

```text
Fase 1: transporte serial confiável
Fase 2: protocolo de gateway e ACK
Fase 3: roteamento para periféricos
Fase 4: ampliação de serviços e dispositivos
```

O planejamento técnico do projeto faz sentido porque cada fase depende da anterior:

- sem serial estável, o dashboard não opera;
- sem protocolo, não há confiabilidade de bancada;
- sem roteamento, não há modularidade;
- sem serviços de dispositivo, não há simulação útil.

### Decisões já consolidadas

- host e gateway falam um protocolo próprio leve;
- o gateway é o ponto central de integração;
- dispositivos remotos recebem contratos internos mais simples que o protocolo do host;
- a documentação histórica é preservada, mas a documentação oficial deve refletir o código atual.

## Limitações

O roadmap técnico existente não é um plano executivo detalhado com datas, donos e marcos de entrega atuais. Ele funciona melhor como direção arquitetural do que como cronograma operacional. Também há frentes planejadas que ainda não aparecem no código, como expansão mais rica de protocolos e serviços.

## Evolução prevista

Com base no que já está implementado, as prioridades técnicas mais coerentes são:

- ampliar os serviços embarcados além do caso de LED;
- documentar de forma mais rígida cada dispositivo da tabela;
- consolidar testes repetíveis de integração;
- transformar os contratos externos e a camada `cloud` em componentes realmente executáveis.

[Retornar ao README principal](../README.md)
