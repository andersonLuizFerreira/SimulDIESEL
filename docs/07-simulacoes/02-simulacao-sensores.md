# Simulação de Sensores

## Estado atual

O nome do firmware `gerador-sinais-analogicos-GSA` sugere uma direção clara para simulação de sensores e, no estado atual do host, essa direção já deixou de estar limitada ao LED builtin.

Hoje, a documentação oficial e o host já descrevem/suportam para a GSA:

- `16` canais;
- setpoint lógico `0..255`;
- leitura de status por canal e global;
- enable por canal e global;
- offsets por canal;
- reset de fault por canal;
- evento assíncrono de fault.

Isso não significa que a plataforma já documente um catálogo rico de sinais físicos por tipo de sensor, frequência ou forma de onda. O que existe hoje é um gerador analógico por canal com contrato funcional e binário já formalizado.

## Funcionamento técnico

Mesmo sem um catálogo completo de sinais, a arquitetura já mostra como uma simulação de sensor deverá operar:

```text
UI / caso de uso
  -> comando lógico no host
  -> gateway roteia para dispositivo
  -> dispositivo interpreta TLV
  -> dispositivo atualiza saída física
  -> resposta confirma aplicação do comando
```

No GSA, a pilha continua curta e reaproveitável:

- `Transport` recebe bytes do barramento;
- `Link` valida `T/L/CRC` e trata erros;
- `Service` decide o comando funcional;
- o serviço específico altera o estado local da placa.

Esse padrão é adequado para sensores simulados por DAC, PWM, chaveamento ou outros mecanismos, desde que o firmware efetivamente detalhe esses serviços no nível físico.

## Limitações

Ainda não há, na documentação oficial atual, contrato completo para:

- amplitude em unidade física contínua;
- frequência;
- forma de onda;
- catálogo por tipo de sensor simulado.

Também não há, no recorte oficial atual, documentação suficiente para afirmar uma simulação de sensor “plena” por domínio físico. O contrato oficial vigente da GSA está centrado em:

- canais analógicos genéricos;
- setpoint lógico;
- status real lido;
- offsets de correção;
- fault.

## Evolução prevista

Quando a simulação de sensores for expandida no firmware, a documentação oficial deve incluir:

- comandos e payloads reais por tipo de sinal;
- estados internos do gerador;
- limites físicos por canal;
- estratégia de teste de resposta em bancada.

Até esse ponto, o GSA deve ser lido como uma plataforma analógica funcional e já integrada, mas ainda não como catálogo completo de sensores simulados por tipo.

## Referência oficial

Para o contrato vigente da GSA, consultar:

- `docs/06-protocolos/06-gsa-sdh-tlv.md`

[Retornar ao README principal](../README.md)
