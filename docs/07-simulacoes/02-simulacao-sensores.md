# Simulação de Sensores

## Estado atual

O nome do firmware `gerador-sinais-analogicos-GSA` sugere uma direção clara para simulação de sensores, mas o código atualmente versionado não expõe ainda uma superfície funcional correspondente a sinais analógicos variados. O serviço implementado e comprovado no firmware é o controle de LED, com leitura e escrita de estado por TLV.

Portanto, a capacidade presente no repositório é de infraestrutura para simulação de sensores, não de um conjunto amplo de sensores já implementados.

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

No GSA, a pilha é curta e reaproveitável:

- `Transport` recebe bytes do barramento;
- `Link` valida `T/L/CRC` e trata erros;
- `Service` decide o comando funcional;
- o serviço específico altera o estado local da placa.

Esse padrão é adequado para sensores simulados por DAC, PWM, chaveamento ou outros mecanismos, desde que o firmware efetivamente passe a expor esses serviços.

## Limitações

Não há, no código analisado, comandos de amplitude, frequência, offset, calibração ou seleção de canal analógico que justifiquem documentar uma simulação de sensor mais rica como funcionalidade pronta. Também não foram encontrados exemplos reais de payload nesse sentido. O documento, portanto, delimita a plataforma existente e registra a lacuna entre o nome do módulo e o estado atual do firmware.

## Evolução prevista

Quando a simulação de sensores for expandida no firmware, a documentação oficial deve incluir:

- comandos e payloads reais por tipo de sinal;
- estados internos do gerador;
- limites físicos por canal;
- estratégia de teste de resposta em bancada.

Até esse ponto, o GSA deve ser lido como base de integração de periférico, não como simulador analógico plenamente documentado.

[Retornar ao README principal](../README.md)
