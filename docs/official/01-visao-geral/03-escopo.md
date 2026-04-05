⬅ [Retornar para Objetivos](02-objetivos.md)

# Escopo

O escopo atual do **SimulDIESEL** abrange as áreas fundamentais para operação de bancada, simulação e diagnóstico de módulos eletrônicos em ambiente controlado.

Nesta etapa, o foco está concentrado na integração entre a aplicação local e o hardware modular da bancada.

---

## Dentro do escopo atual

Atualmente fazem parte do escopo operacional do projeto:

* aplicação Windows para operação da bancada
* comunicação segura entre software e hardware
* controle e monitoramento do equipamento em teste
* módulos físicos especializados para geração e leitura de sinais
* documentação técnica oficial do sistema

O foco atual está concentrado no ambiente de bancada local.

---

## Estrutura do escopo

O escopo pode ser entendido na seguinte cadeia funcional:

```text
Operação de bancada
    ↓
Aplicação local
    ↓
Comunicação com hardware
    ↓
Hardware modular
    ↓
Módulo em teste
```

---

## Fora do escopo atual

Neste estágio, ainda não fazem parte do escopo consolidado:

* infraestrutura em nuvem
* backend remoto operacional
* integração ampla com protocolos externos
* catálogo completo de módulos diesel
* automação avançada de testes

Esses pontos permanecem como evolução futura.

---

## Expansão prevista

O escopo tende a crescer em duas direções:

* **horizontal** → novos módulos físicos e serviços
* **vertical** → novos níveis de abstração, protocolos e testes

---

## Próximas camadas

* [Visão Arquitetural](../02-arquitetura/01-visao-arquitetural.md)
