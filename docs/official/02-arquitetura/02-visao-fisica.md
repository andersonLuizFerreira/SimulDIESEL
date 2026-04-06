⬅ [Retornar para Visão Arquitetural](01-visao-arquitetural.md)
⬅ [Retornar para Índice Geral](../../00-INDICE.md)

# Visão Física do Projeto

Esta página descreve **onde** cada elemento do SimulDIESEL está organizado na solução real, sempre priorizando a estrutura confirmada em `local-api`, `hardware` e `hardware/firmware`.

Nesta leitura, "físico" cobre dois tipos de empilhamento:

- a organização material da bancada, do gateway, das boards e do módulo em teste;
- a organização estrutural real do host, onde a API também possui camadas concretas entre UI e transporte.

## Topologia oficial desta visão

```text
API e Host Local
  ↓
Serial / Bluetooth
  ↓
Gateway BPM
  ↓
Backplane / barramentos / boards
  ↓
X-CONN / chicote
  ↓
Módulo em teste
```

## Estado confirmado no projeto

- **IMPLEMENTADO**: host WinForms com UI, BLL, DAL, DTL e transporte serial/Bluetooth.
- **IMPLEMENTADO**: BPM com dois endpoints físicos de acesso ao host, serial e Bluetooth.
- **IMPLEMENTADO**: interligação BPM <-> GSA por `I2C`, IRQ e reset dedicados.
- **IMPLEMENTADO**: GSA com barramento físico com a BPM e barramento lógico interno próprio.
- **PARCIALMENTE IMPLEMENTADO**: X-CONN e backplane já ocupam lugar estrutural claro, mas ainda não possuem o mesmo nível de detalhamento ativo em código que o host e a GSA.
- **PLANEJADO**: ampliação de boards e conectores além do conjunto hoje confirmado em firmware.

## Papel desta página na árvore

Esta página deixou de ser terminal. A partir daqui, a documentação física se divide em três ramos reais:

- `API e Host Local`
- `Hardware da Bancada`
- `Módulo em Teste e X-CONN`

O detalhamento de handshake, framing, retry, sessão e fluxo funcional continua reservado ao ramo de Visão Lógica.

## Glossário

- **Camada**: nível de responsabilidade dentro da arquitetura do sistema.
- **Gateway**: ponto de passagem entre host, roteamento interno e hardware.
- **Visão física**: leitura focada em onde cada elemento está fisicamente no sistema.
- **Interface**: ponto de conexão material entre blocos do projeto.

## Próximas camadas

- [API e Host Local](04-api-e-host-local.md)
- [Hardware da Bancada](10-hardware-da-bancada.md)
- [Módulo em Teste e X-CONN](11-modulo-em-teste-e-xconn.md)
