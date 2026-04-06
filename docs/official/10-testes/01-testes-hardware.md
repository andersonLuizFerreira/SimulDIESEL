⬅ [Retornar para Testes de Bancada](../08-casos-de-uso/03-testes-bancada.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

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
  -> atividade válida confirma sessão
  -> requisição roteada confirma barramento
  -> ACK síncrono confirma recepção na GSA
  -> IRQ em D4 -> D19
  -> evento 0x31 confirma a etapa física
```

### O que cada teste comprova

- handshake serial: gateway energizado e firmware em execução;
- atividade SDGW válida ou `ping` auxiliar: enlace host/gateway operante;
- comando roteado: `GwRouter` e `GwDeviceTable` coerentes com o hardware presente;
- resposta síncrona da GSA: integridade do barramento físico `D21/D22` <-> `A4/A5` e do firmware remoto;
- `IRQ + 0x31`: integridade do caminho assíncrono `D4` -> `D19` e da etapa física interna da GSA;
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
- validação de alimentação e reset, incluindo `D23` para reset da GSA e `D8` para reset do `TCA9548A`;
- testes dedicados por barramento;
- registros de bancada com resultados reproduzíveis.

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
