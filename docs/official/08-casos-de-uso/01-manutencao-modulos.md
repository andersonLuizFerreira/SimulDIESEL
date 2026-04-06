⬅ [Retornar para Arquitetura do Software Dashboard (Local API)](../05-software-dashboard/01-arquitetura-software.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Manutenção de Módulos

## Estado atual

O repositório já permite documentar um fluxo de manutenção de módulos em bancada, mesmo que o conjunto de módulos seja pequeno. A manutenção, nesse contexto, significa validar comunicação, identificar o destino correto, executar comandos controlados e observar respostas ou estados do periférico quando o contrato disponível oferece essa informação.

O melhor caso concreto hoje envolve host + BPM + GSA.

## Funcionamento técnico

### Procedimento operacional

1. Energizar gateway e periférico.
2. Conectar o software local à serial correta.
3. Aguardar o estado `Linked`.
4. Confirmar que há atividade SDGW válida e que o link permanece saudável.
5. Executar `ping` apenas se for necessário isolar a conexão com a BPM.
6. Enviar comando ao módulo desejado.
7. Observar resposta funcional, estado retornado ou erro.

### Consulta de estado e resposta do periférico

Quando o contrato da board oferece leitura de status, offset, fault ou resultado de execução, a manutenção pode seguir o fluxo:

```text
Falha percebida
  -> consultar estado atual do módulo
  -> corrigir condição de bancada
  -> repetir comando funcional
  -> comparar o resultado obtido
```

### Responsabilidades por camada

- host: abrir sessão, correlacionar sequência e exibir resultado;
- gateway: chegar ao módulo correto e devolver erro de roteamento quando necessário;
- módulo: validar payload local e registrar erro específico.

## Limitações

O repositório não contém ainda procedimentos oficiais por família extensa de módulos, nem scripts automatizados de manutenção. A manutenção hoje depende de conhecimento técnico do operador e do uso cuidadoso das telas de bancada. Também não há catálogo textual completo de códigos de erro por dispositivo além da infraestrutura básica observada no GSA.

## Evolução prevista

Os próximos ganhos para manutenção de módulos são:

- documentar procedimentos por dispositivo real da tabela;
- ampliar o repertório de estados e erros retornáveis pelo firmware;
- registrar rastros de manutenção no software local;
- padronizar testes de pós-manutenção com payloads conhecidos.

## Glossário

- **Caso de uso**: fluxo funcional documentado para operação, simulação, diagnóstico ou teste.
- **GSA**: board de geração de sinais analógicos hoje mais madura na árvore oficial.
- **Evento**: mensagem assíncrona publicada durante ou após uma operação.
- **Validação**: verificação de comportamento esperado em bancada.
- **SDGW**: nomenclatura oficial vigente do enlace host/gateway: SimulDiesel GateWay.

## Próximas camadas

- [Diagnóstico de Falhas](02-diagnostico.md)
