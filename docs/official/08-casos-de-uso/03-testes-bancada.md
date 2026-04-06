⬅ [Retornar para Diagnóstico de Falhas](02-diagnostico.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Testes de Bancada

## Estado atual

Os testes de bancada sustentados pelo repositório continuam majoritariamente manuais, mas agora devem seguir a arquitetura operacional atual do link host <-> BPM.

O foco é validar:

- conexão serial
- bootstrap textual inicial
- operação binária SDGW
- arbitragem correta de TX no host
- resposta funcional da BPM e da board remota

## Elementos concretos disponíveis

- abertura de porta serial
- handshake e transição para `Linked`
- supervisão de saúde do link por RX válido
- roteamento da BPM para a GSA
- leitura e escrita do LED embutido da GSA

## Roteiro mínimo de bancada

```text
1. Ligar a bancada
2. Abrir a serial no software local
3. Confirmar banner da BPM
4. Verificar se o link entra em Linked
5. Executar um comando funcional real
6. Validar resposta e efeito físico
7. Observar se o link permanece vivo por atividade SDGW válida
```

Ping manual pode ser usado como ferramenta auxiliar de observação, mas não deve mais ser tratado como rotina central da operação normal.

## Papel das próximas camadas

As páginas inferiores deste ramo separam o mesmo cenário de bancada em três lentes complementares:

* **hardware**: integridade física do caminho entre host, gateway e board
* **firmware**: comportamento do parser, do roteamento e do serviço embarcado
* **integração**: travessia ponta a ponta entre UI, host, gateway e device

Essa divisão existe para que o leitor consiga validar a bancada por camada, sem perder o roteiro operacional comum.

## Exemplo ponta a ponta atual

Caso de teste: acionamento do LED da GSA.

1. a UI aciona `SetBuiltinLedAsync`
2. o comando entra em `GsaClient -> SdhClient -> SdgwSession`
3. o `SdGwTxScheduler` envia com prioridade `High`
4. o `SdGwLinkEngine` aguarda `ACK`
5. a BPM roteia a transação para a GSA
6. a GSA grava o estado e responde
7. a BPM devolve a resposta ao host
8. o `GsaClient` valida o payload e confirma o estado aplicado

## Evidências técnicas esperadas

- o host entra em `Linked`
- o comando funcional passa pela fila central de TX
- há RX SDGW válido mantendo a saúde do link
- a BPM não derruba a sessão só por ausência de ping explícito
- o router responde dentro da janela operacional atual

## Parâmetros relevantes no cenário atual

- supervisor do host com ping apenas sob silêncio
- timeout lógico do host em `3000 ms`
- timeout de atividade da BPM em `4000 ms`
- timeout do router/gateway em `100 ms`
- comando de LED da GSA com timeout de `400 ms` e `2` retries

## Limitações

O procedimento ainda depende:

- de observação do operador
- da montagem física correta
- de instrumentação manual quando for necessário isolar falha entre host, BPM e board

## Evolução prevista

Os testes de bancada podem evoluir para:

- roteiros repetíveis por board
- captura estruturada de payloads e respostas
- checklists de diagnóstico por camada
- ampliação para novos comandos além da GSA

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.

## Próximas camadas

- [Testes de Hardware](../10-testes/01-testes-hardware.md)
- [Testes de Firmware](../10-testes/02-testes-firmware.md)
- [Testes de Integração](../10-testes/03-testes-integracao.md)
