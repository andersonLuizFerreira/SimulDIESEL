# Baby Boards

## Estado atual

A estrutura do repositório e a presença de um gateway com tabela de dispositivos indicam que o hardware foi concebido de forma modular, com placas filhas ou periféricos conectados ao backplane. O exemplo concreto mais claro no código é o `GSA`, acessado como dispositivo remoto via barramento e tratado como serviço independente.

As baby boards, no contexto atual do projeto, cumprem dois papéis:

- encapsular funções específicas de simulação ou interface física;
- permitir expansão do sistema sem alteração estrutural do software local.

## Funcionamento técnico

O comportamento esperado de uma baby board já aparece cristalizado no firmware:

1. o gateway identifica o destino por endereço lógico;
2. a tabela de dispositivos informa o barramento adequado;
3. o barramento entrega uma mensagem curta ao periférico;
4. o periférico responde dentro de um contrato simples e validado.

No caso do `GSA`, a placa atua como escravo `I2C` no endereço `0x23` e usa uma pilha curta:

```text
I2C callback -> Transport -> Link -> Service -> LedService
```

Essa decomposição deixa claro que uma baby board não precisa conhecer o protocolo do host. Ela precisa conhecer apenas o contrato interno do barramento que o gateway encaminha.

## Limitações

Nem todas as baby boards sugeridas pela árvore de hardware possuem descrição textual ou firmware analisável no repositório atual. A tabela de dispositivos do gateway contém entradas suficientes para provar a intenção modular, mas não descreve sozinha função elétrica completa, revisão de placa ou conjunto de sensores/atuadores de cada módulo.

## Evolução prevista

Para que a documentação de baby boards fique completa, a evolução recomendada é publicar para cada placa:

- finalidade funcional;
- barramento usado;
- endereço físico ou critérios de seleção;
- serviços expostos ao gateway;
- limites de operação observáveis em firmware e hardware.

Enquanto essa consolidação não acontece, o GSA permanece como referência mais concreta do padrão de integração de placas filhas.

[Retornar ao README principal](../README.md)
