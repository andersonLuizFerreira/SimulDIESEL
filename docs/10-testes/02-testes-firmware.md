# Testes de Firmware

## Estado atual

O firmware do SimulDIESEL já permite testes objetivos de três blocos distintos:

- sessão e protocolo do gateway;
- roteamento por tabela de dispositivos;
- tratamento de serviço no periférico GSA.

Esses testes podem ser executados a partir do software local ou por meio de ferramentas seriais equivalentes, desde que respeitem o protocolo implementado.

## Funcionamento técnico

### Testes do gateway ESP32

Itens validáveis no código:

- reconhecimento do banner e transição para `Linked`;
- parser de `COBS` e `CRC8`;
- `ACK` e cache por `SEQ`;
- resposta local a comandos do endereço do gateway;
- erro de roteamento quando o destino não é resolvido.

### Testes do GSA

Itens validáveis no código:

- recepção `I2C` do request;
- validação `T/L/CRC`;
- leitura de erro (`GET_ERR`);
- limpeza de erro (`CLR_ERR`);
- `CMD_SET_LED` e `CMD_GET_LED`.

### Exemplo de estratégia de teste

```text
1. Enviar quadro válido com ACK requerido
2. Confirmar resposta com mesmo SEQ
3. Repetir quadro para validar deduplicação
4. Enviar payload inválido para observar erro
5. Limpar erro e repetir cenário válido
```

Essa sequência cobre o núcleo do firmware: robustez de enlace, roteamento e tratamento funcional.

## Limitações

Não há no repositório atual uma suíte automatizada formal de testes unitários e de integração de firmware com relatórios publicados. O diagnóstico depende da observação do comportamento em execução e de payloads cuidadosamente construídos. Também não foram identificados mocks formais de barramento para execução contínua fora da bancada.

## Evolução prevista

Os testes de firmware podem amadurecer por meio de:

- vetores de teste documentados para cada comando;
- harness de regressão para parser e `CRC`;
- testes por dispositivo da tabela do gateway;
- registros automáticos de falha por timeout, `ACK` e erro de payload.

[Retornar ao README principal](../README.md)
