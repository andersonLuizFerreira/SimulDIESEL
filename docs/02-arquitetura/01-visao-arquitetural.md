⬅ [Retornar para Visão Geral do Projeto](../01-visao-geral/01-visao-geral-projeto.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Visão Arquitetural

A arquitetura do **SimulDIESEL** organiza a documentação em duas leituras complementares: uma para entender **onde** cada elemento está no sistema e outra para entender **como** o sistema funciona de ponta a ponta.

Essa separação foi adotada para reduzir ambiguidade na navegação e dar suporte simultâneo a:

* entendimento estrutural do projeto
* leitura progressiva por camadas
* governança documental com pais únicos
* revisão rápida via índice global

## Estrutura macro do sistema

A visão macro do sistema pode ser entendida nos seguintes blocos.

```text
Aplicação Windows
    ↓
Camada de Comunicação
    ↓
Gateway Central
    ↓
Hardware Modular
    ↓
Módulo em Teste
```

## Dois caminhos oficiais de leitura

### Visão Física do Projeto

Este ramo responde **onde** cada parte se encontra fisicamente na bancada e como as interligações materiais se organizam.

O foco está em:

* localização física dos elementos
* conectores e interfaces
* camadas superior e inferior de cada bloco
* relações entre API, hardware e módulo em teste

### Visão Lógica do Projeto

Este ramo responde **como** o sistema opera, transporta comandos, roteia decisões e produz efeitos funcionais sobre a bancada.

O foco está em:

* funções de cada camada
* protocolos e transporte
* fluxos entre UI, software, gateway e boards
* transformação de comando em ação observável

## Regra de navegação entre as duas leituras

As duas leituras podem se referenciar por texto, mas a troca de ramo deve acontecer sempre por esta página-pai.

Assim, a árvore mantém navegação controlada e sem múltiplos pais.

## Glossário

- **Camada**: nível de responsabilidade dentro da arquitetura do sistema.
- **Gateway**: ponto de passagem entre host, roteamento interno e hardware.
- **Arquitetura**: organização estrutural e funcional das partes do SimulDIESEL.

## Próximas camadas

* [Visão Física do Projeto](02-visao-fisica.md)
* [Visão Lógica do Projeto](03-visao-logica.md)
