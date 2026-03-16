# Resolver Engine do Gateway

## Objetivo

Este documento define a arquitetura interna do Resolver Engine do gateway do SimulDIESEL.

O Resolver Engine é o núcleo lógico responsável por transformar um comando SDH recebido pelo gateway em uma ação concreta sobre o hardware da bancada.

Em termos práticos, ele responde à pergunta:

> Como o gateway pega um comando lógico como `sdh/1 GSA.led set state=on` e o converte em uma transação real para a board correta?

## Papel arquitetural

O Resolver Engine fica no gateway, acima do transporte e abaixo dos drivers de barramento.

Ele não deve ficar:

- no host;
- na UI;
- no `SdGwLinkEngine` do host;
- no framing `SGGW`;
- dentro do firmware de cada baby board.

Ele deve permanecer centralizado no gateway porque é o ponto único que conhece simultaneamente:

- o envelope lógico SDH;
- a tabela de binding lógico-físico;
- o catálogo de boards e resources;
- os barramentos físicos;
- os contratos internos das boards.

## Posição na arquitetura

O fluxo arquitetural alvo é:

    Host
        -> SGGW / transporte atual
        -> Gateway
            -> Resolver Engine
                -> Binding lógico-físico
                -> Mapper interno
                -> Bus adapter
                -> Device
        -> Resposta

## Responsabilidades do Resolver Engine

O Resolver Engine deve ser responsável por:

- receber o comando lógico extraído do transporte;
- validar a versão SDH;
- decompor e validar o target;
- localizar a entrada de binding correspondente;
- resolver o mapper de tradução para a board;
- preparar a operação para o barramento físico;
- invocar o adapter do barramento;
- montar a resposta lógica padronizada;
- propagar erros padronizados quando necessário.

## O que o Resolver Engine não deve fazer

O Resolver Engine não deve:

- implementar framing binário;
- gerenciar ACK/SEQ do transporte;
- conhecer a serial do host;
- implementar diretamente os drivers de I2C/SPI/CAN/GPIO;
- deixar regras de binding espalhadas em vários pontos do firmware;
- deixar a board remota interpretar o target SDH completo.

## Entradas do Resolver Engine

A entrada lógica mínima do Resolver Engine deve conter:

- `version`
- `target`
- `op`
- `args`
- `meta`

Exemplo conceitual:

    version = sdh/1
    target  = GSA.led
    op      = set
    args    = { state = on }

## Saídas do Resolver Engine

A saída deve ser uma resposta padronizada contendo:

- `version`
- `ok`
- `target`
- `op`
- `code`
- `message`
- `data`
- `meta`

Exemplo conceitual de sucesso:

    version = sdh/1
    ok      = true
    target  = GSA.led
    op      = set
    code    = OK
    message = "LED atualizado"
    data    = { state = on }

Exemplo conceitual de erro:

    version = sdh/1
    ok      = false
    target  = GSA.led
    op      = set
    code    = INVALID_ARG
    message = "State inválido"

## Pipeline interno recomendado

O pipeline interno do Resolver Engine deve seguir esta sequência:

### 1. Parse do envelope SDH

Responsável por transformar a entrada bruta do comando em uma estrutura interna.

Saída esperada:

- `SdhCommand`

### 2. Validação estrutural

Responsável por validar:

- versão suportada;
- presença de target;
- presença de operação;
- integridade mínima dos argumentos.

### 3. Parse do target

Responsável por decompor o target em:

- `board`
- `resource`
- `subresource`

Exemplo:

    GSA.led
        -> board = GSA
        -> resource = led
        -> subresource = N/A

### 4. Lookup na tabela de binding

Responsável por localizar a entrada correspondente na tabela mestra de binding lógico-físico.

A entrada de binding deve informar, no mínimo:

- board
- target base
- barramento
- código legado
- mapper
- handler

### 5. Resolução do mapper

Responsável por selecionar o mapper interno adequado para a board/resource.

Exemplo conceitual:

    GSA.led -> GsaLedMapper

### 6. Tradução para contrato interno

Responsável por converter a intenção SDH em operação compatível com a board.

Exemplo conceitual para `GSA.led`:

- operação lógica: `set`
- argumento: `state=on`
- contrato interno resultante:
  - operação interna SET
  - subcomando LED
  - payload 1

### 7. Seleção do barramento

Responsável por escolher o adapter correto:

- I2C
- SPI
- CAN
- GPIO
- INTERNAL

### 8. Execução da transação

Responsável por entregar a operação ao barramento/device correto.

### 9. Normalização da resposta

Responsável por converter a resposta do contrato interno em envelope SDH padronizado.

## Componentes recomendados

Os nomes exatos podem variar, mas a separação conceitual recomendada é esta:

- `SdhCommand`
- `SdhTarget`
- `SdhValidator`
- `SdhBindingTable`
- `SdhResolverEngine`
- `IBoardMapper`
- `GsaLedMapper`
- `IBusAdapter`
- `I2cBusAdapter`
- `SpiBusAdapter`
- `InternalBusAdapter`
- `SdhResponseBuilder`

## Interfaces conceituais recomendadas

### Resolver Engine

Responsabilidade conceitual:

    Resolve(command) -> response

### Binding Table

Responsabilidade conceitual:

    FindByTarget(target) -> bindingEntry

### Mapper por board

Responsabilidade conceitual:

    Map(command, bindingEntry) -> busRequest

### Bus Adapter

Responsabilidade conceitual:

    Execute(busRequest) -> busResponse

### Response Builder

Responsabilidade conceitual:

    Build(command, busResponse) -> sdhResponse

## Primeiro caso de uso obrigatório

O primeiro caso oficialmente suportado pelo Resolver Engine deve ser:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

Motivos:

- já existe fluxo host -> gateway -> GSA;
- já existe caso de uso oficial no host;
- já existe contrato interno reaproveitável;
- permite validar a arquitetura sem romper o legado.

## Fluxo do primeiro caso

### Entrada

    sdh/1 GSA.led set state=on

### Etapas esperadas

1. parser gera `SdhCommand`
2. validator aprova `sdh/1`
3. target parser resolve `GSA.led`
4. binding table retorna a entrada da GSA
5. resolver seleciona `GsaLedMapper`
6. mapper traduz para o contrato interno da GSA
7. adapter do barramento executa a transação
8. response builder monta resposta padronizada

### Resultado esperado

Resposta SDH de sucesso indicando atualização do estado do LED.

## Códigos de erro esperados no Resolver Engine

O Resolver Engine deve usar códigos padronizados, como:

    OK
    INVALID_TARGET
    INVALID_OP
    INVALID_ARG
    MISSING_ARG
    OUT_OF_RANGE
    UNSUPPORTED
    BUSY
    FAULT
    TIMEOUT

## Casos de erro que devem ser tratados

### Target inexistente

Quando o target não existir na tabela de binding:

    code = INVALID_TARGET

### Operação não suportada

Quando a board/resource existir, mas a operação não for suportada:

    code = INVALID_OP
ou
    code = UNSUPPORTED

### Argumento ausente

Quando faltar argumento obrigatório:

    code = MISSING_ARG

### Argumento inválido

Quando o argumento existir, mas o valor for inválido:

    code = INVALID_ARG

### Binding incompleto

Quando existir target lógico, mas a entrada de binding ainda não estiver pronta:

    code = UNSUPPORTED

### Falha de barramento ou device

Quando a transação física falhar:

    code = FAULT
ou
    code = TIMEOUT

## Regras de projeto

O Resolver Engine deve obedecer às seguintes regras:

- target externo sempre lógico;
- binding sempre centralizado;
- mapper sempre isolado por board/resource;
- barramento resolvido somente após binding;
- resposta sempre normalizada para SDH;
- nenhuma regra importante espalhada fora do resolver.

## Estratégia de crescimento

O crescimento recomendado do Resolver Engine deve seguir esta ordem:

1. suportar `GSA.led`
2. validar o padrão ponta a ponta
3. adicionar novos mappers por board/resource
4. ampliar a tabela de binding
5. ampliar a cobertura de respostas e eventos

## Critério de sucesso

O Resolver Engine estará bem implementado quando:

- o host puder enviar um target lógico sem conhecer barramento;
- o gateway conseguir resolver esse target sozinho;
- a board remota continuar recebendo apenas o contrato interno necessário;
- a resposta final voltar em formato SDH padronizado.

## Referências

- `docs/04-firmware/04-sdh-gateway-architecture.md`
- `docs/04-firmware/05-catalogo-baby-boards.md`
- `docs/04-firmware/06-gateway-binding-logico-fisico.md`
- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/02-sdh-response-model.md`

[Retornar ao README principal](../README.md)
