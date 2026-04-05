⬅ [Retornar para Testes de Bancada](../08-casos-de-uso/03-testes-bancada.md)

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
- operação do LED embutido;
- setpoint e enable por canal;
- leitura de status por canal;
- eventos assíncronos de fault e de resultado físico quando aplicáveis.

### Exemplo de estratégia de teste

```text
1. Enviar quadro válido com ACK requerido
2. Confirmar resposta com mesmo SEQ
3. Repetir quadro para validar deduplicação
4. Enviar comando funcional válido para a GSA
5. Enviar payload inválido para observar erro
6. Repetir cenário válido para confirmar recuperação do fluxo
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

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.


