# Testes de Hardware

## Estado atual

Os testes de hardware sustentados pelo repositório são voltados à verificação funcional da bancada e da comunicação entre placas. Não há, no estado atual, uma suíte automatizada abrangente de validação elétrica, mas o conjunto de firmware e software local já permite executar testes estruturados de presença, resposta e atuação.

Os principais alvos de teste de hardware observáveis são:

- gateway ESP32;
- backplane e interconexões;
- barramentos `I2C` e `SPI`;
- periférico `GSA`.

## Funcionamento técnico

### Sequência mínima de validação

```text
Energia aplicada
  -> gateway inicializa
  -> host reconhece banner
  -> ping confirma sessão
  -> requisição roteada confirma barramento
  -> resposta do periférico confirma hardware remoto
```

### O que cada teste comprova

- handshake serial: gateway energizado e firmware em execução;
- `ping`: enlace host/gateway operante;
- comando roteado: `GwRouter` e `GwDeviceTable` coerentes com o hardware presente;
- resposta de periférico: integridade do barramento físico e do firmware remoto;
- atuação local, como LED: efeito elétrico visível ou mensurável na placa.

### Falhas típicas separáveis pelo fluxo

```text
Sem banner          -> problema no gateway ou na alimentação
Banner sem link     -> problema de sessão serial
Link sem resposta   -> problema de protocolo ou roteamento
Roteamento sem efeito físico -> problema no periférico ou na placa
```

## Limitações

O repositório não contém critérios formais de aceitação elétrica, medições documentadas de forma de onda, checklist por revisão de placa ou automação de instrumentos. A documentação oficial consegue descrever o teste funcional fim a fim, mas não substitui um plano metrológico de hardware.

## Evolução prevista

Os testes de hardware devem evoluir para incluir:

- checklists por placa e por revisão;
- validação de alimentação e reset;
- testes dedicados por barramento;
- registros de bancada com resultados reproduzíveis.

[Retornar ao README principal](../README.md)
