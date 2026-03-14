# Manutenção de Módulos

## Estado atual

O repositório já permite documentar um fluxo de manutenção de módulos em bancada, mesmo que o conjunto de módulos seja pequeno. A manutenção, nesse contexto, significa validar comunicação, identificar o destino correto, executar comandos controlados e limpar ou consultar erros do periférico quando o contrato do firmware oferece essa função.

O melhor caso concreto hoje envolve gateway + GSA.

## Funcionamento técnico

### Procedimento operacional

1. Energizar gateway e periférico.
2. Conectar o software local à serial correta.
3. Aguardar o estado `Linked`.
4. Executar `ping` para confirmar saúde do enlace.
5. Enviar comando ao módulo desejado.
6. Observar resposta funcional ou erro.

### Consulta e limpeza de erro no periférico

O `Link` do GSA já implementa comandos próprios para erro:

- `GET_ERR`
- `CLR_ERR`

Isso permite um fluxo de manutenção simples:

```text
Falha percebida
  -> consultar erro atual do módulo
  -> corrigir condição de bancada
  -> limpar erro registrado
  -> repetir comando funcional
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
- ampliar o repertório de erros retornáveis pelo firmware;
- registrar rastros de manutenção no software local;
- padronizar testes de pós-manutenção com payloads conhecidos.

[Retornar ao README principal](../README.md)
