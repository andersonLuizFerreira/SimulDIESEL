⬅ [Retornar para Escopo](../01-visao-geral/03-escopo.md)

# Visão Arquitetural

A arquitetura do **SimulDIESEL** foi concebida em múltiplas camadas, com separação entre operação local, comunicação com o gateway, roteamento interno e hardware modular.

Essa organização foi projetada para garantir:

* escalabilidade
* manutenção modular
* expansão de recursos
* isolamento de responsabilidades
* robustez da comunicação

O sistema permite que o software local opere uma bancada modular por meio de uma arquitetura intermediária de comunicação e roteamento.

---

## Estrutura macro do sistema

A visão macro do sistema pode ser entendida nos seguintes blocos.

```text id="0gpl4x"
Aplicação Windows
    ↓
Camada de Comunicação
    ↓
Gateway Central
    ↓
Hardware Modular
```

---

## Visão conceitual

O fluxo conceitual do sistema é:

```text id="t35f89"
Usuário
→ Aplicação
→ Comunicação
→ Gateway
→ Unidade modular
→ resposta
```

Cada uma dessas camadas possui responsabilidade própria e será detalhada nas próximas páginas.

---

## Próximas camadas de aprofundamento

Nesta camada, o objetivo é entender a forma geral da solução.

Os componentes concretos, classes e mecanismos de transporte ficam para as páginas imediatamente inferiores.

## Próximas camadas

- [Camadas do Sistema](02-camadas-do-sistema.md)


