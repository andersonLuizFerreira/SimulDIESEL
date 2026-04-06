⬅ [Retornar para Simulação de Módulos](01-simulacao-modulos.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Simulação de Atuadores

## Estado atual

A simulação de atuadores tem, no código atual, um exemplo concreto e verificável: o acionamento do LED embutido da GSA por comando remoto.

Embora seja um atuador simples, ele demonstra toda a cadeia técnica necessária para controlar um elemento físico a partir do host, passando pelo gateway e chegando à board remota.

## Funcionamento técnico

### Fluxo real

```text
Operador aciona teste
  -> aplicação monta comando semântico
  -> gateway roteia para o GSA
  -> GSA interpreta o contrato interno
  -> GSA altera o estado físico do LED
  -> resposta confirma o novo estado
```

### Comando oficial já sustentado pela árvore viva

O caso de atuador hoje legitimado pela documentação oficial é:

```text
sdh/1 GSA.led set state=on
sdh/1 GSA.led set state=off
```

Esse fluxo é suficiente para provar:

* comando remoto vindo da UI
* roteamento pela BPM
* atuação física em uma board remota
* confirmação funcional no retorno ao host

## Limitações

O repositório ainda não contém outros atuadores com maior complexidade, como relés múltiplos, drivers de carga ou perfis temporizados completos. Também não há, na árvore oficial atual, um catálogo amplo de atuadores por board.

## Evolução prevista

O padrão já implementado é suficiente para expandir o conjunto de atuadores simulados. Cada novo caso deve explicitar:

- contrato de payload;
- estado interno do dispositivo;
- confirmação de aplicação;
- limites elétricos e temporais do hardware correspondente.

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
