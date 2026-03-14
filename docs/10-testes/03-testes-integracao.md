# Testes de Integração

## Estado atual

Os testes de integração mais importantes do SimulDIESEL são os que atravessam toda a cadeia host -> gateway -> periférico. O repositório já possui todos os blocos necessários para esse teste no caso do GSA, tornando possível validar o desenho arquitetural completo sem depender de uma camada de nuvem.

## Funcionamento técnico

### Caminho integrado

```text
WinForms
  -> SdGgwClient
  -> SdGwLinkEngine
  -> SerialTransport
  -> ESP32 SggwLink / GatewayApp
  -> GwRouter / GwDeviceTable
  -> I2C ou SPI
  -> dispositivo remoto
```

### Cenário de integração mais representativo

Caso de teste: alterar e ler o estado do LED no GSA.

1. A UI dispara o comando.
2. O cliente local cria o frame lógico.
3. O gateway valida `COBS` e `CRC8`.
4. O roteador seleciona o barramento correto.
5. O GSA aplica o comando e monta a resposta TLV.
6. O gateway devolve a resposta ao host.
7. O host correlaciona a resposta ao `SEQ` enviado.

### Evidência de robustez do fluxo

- estados de link explícitos no host e no gateway;
- tolerância a retransmissão por `SEQ`;
- erro local por camada;
- diferenciação entre erro de transporte e erro de serviço.

## Limitações

O conjunto de testes de integração ainda é pequeno em diversidade funcional, porque o repertório de serviços de dispositivo também é pequeno. O repositório também não mostra integração fim a fim com `cloud`, nem com protocolos automotivos ativos como `CAN` e `J1939`.

## Evolução prevista

Os testes de integração devem crescer junto com os dispositivos suportados e com os contratos funcionais. Os próximos ganhos naturais são:

- mais cenários além do LED;
- validação de eventos assíncronos;
- testes cruzando múltiplos dispositivos da tabela;
- integração com contratos remotos quando a camada `cloud` estiver operacional.

[Retornar ao README principal](../README.md)
