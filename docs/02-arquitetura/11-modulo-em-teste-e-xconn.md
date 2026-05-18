⬅ [Retornar para Visão Física do Projeto](02-visao-fisica.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Módulo em Teste e X-CONN

Esta página fixa onde o módulo externo se acopla à bancada.

## Estado confirmado

- **IMPLEMENTADO**: o host possui `XConnService` como ponto estrutural reservado para a interface inferior.
- **PARCIALMENTE IMPLEMENTADO**: a árvore física mantém `X-CONN`, chicote e módulo em teste como parte oficial da arquitetura.
- **PARCIALMENTE IMPLEMENTADO**: o backplane e a GSA mostram a direção do fluxo físico, mas o catálogo de chicotes e pinagens do módulo ainda não está consolidado em arquivos de hardware equivalentes.
- **PLANEJADO**: variações por família de módulo, chicote e conector.

## Posição na pilha física

```text
Baby boards / backplane
  -> X-CONN
  -> chicote
  -> módulo em teste
```

## Leitura correta desta página

- Esta página diz onde o módulo se conecta.
- Ela não descreve como o firmware roteia um comando.
- Ela não prova, sozinha, uma pinagem completa do módulo externo.

## Evidência atual

- documentação física do ramo `03-hardware`
- slot estrutural `XConnService` no host
- organização oficial da árvore física

## Glossário

- **Módulo em teste**: equipamento externo conectado à bancada para simulação ou diagnóstico.
- **X-CONN**: interface física intermediária entre chicote e bancada.
- **Chicote**: adaptação elétrica e mecânica entre a bancada e o módulo.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
