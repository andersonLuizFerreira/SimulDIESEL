# Simulação de Atuadores

## Estado atual

A simulação de atuadores tem, no código atual, um exemplo concreto e verificável: o acionamento de LED por comando remoto. Embora seja um atuador simples, ele demonstra toda a cadeia técnica necessária para controlar um elemento físico a partir do host, passando pelo gateway.

No software local, `LedGwTest_BLL` ainda adiciona um comportamento de alternância por timer, útil como teste contínuo de acionamento.

## Funcionamento técnico

### Fluxo real

```text
Operador aciona teste
  -> aplicação monta comando
  -> gateway roteia para o GSA
  -> GSA executa Service::handle
  -> LedService grava LED_PIN
  -> resposta confirma o novo estado
```

### Comandos observados no GSA

O firmware do GSA já trata, de forma inequívoca:

- `CMD_SET_LED`
- `CMD_GET_LED`

O valor de escrita aceita ao menos três interpretações observáveis no código:

- `0`: desligar;
- `1`: ligar;
- `2`: alternar.

Exemplo conceitual de payload TLV interno:

```text
T = CMD_SET_LED
L = 1
V = 0x01
CRC8
```

### Estado de máquina do serviço

```text
LED_OFF <-> LED_ON
   ^         |
   |---------|
     toggle
```

Esse caso é pequeno, mas valida elementos importantes para atuadores futuros: comando remoto, aplicação local da mudança física, leitura de estado e resposta ao host.

## Limitações

O repositório ainda não contém outros atuadores com maior complexidade, como saídas analógicas elaboradas, relés múltiplos, drivers de carga ou perfis temporizados. Também não há uma camada de segurança operacional documentada para impedir comandos incompatíveis com o hardware final.

## Evolução prevista

O padrão já implementado é suficiente para expandir o conjunto de atuadores simulados. Cada novo caso deve explicitar:

- contrato de payload;
- estado interno do dispositivo;
- confirmação de aplicação;
- limites elétricos e temporais do hardware correspondente.

[Retornar ao README principal](../README.md)
