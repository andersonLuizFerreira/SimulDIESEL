# Arquitetura SDH no Host

## Objetivo

Este documento registra formalmente a primeira implementação funcional da arquitetura de comandos SDH no host C# do SimulDIESEL.

O objetivo desta camada é introduzir um envelope semântico de comandos acima do transporte atual, preservando a arquitetura já existente baseada em `SGGW`, `SdGwLinkEngine` e `SdGgwClient`.

## Escopo implementado

Nesta primeira fase, a implementação no host cobre o caso funcional:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

O SDH foi introduzido como camada semântica acima do cliente atual, sem alterar o motor de transporte nem o framing binário.

## Decisão arquitetural

A arquitetura aprovada para o host é:

    Caso de uso / BLL
        -> Client por board
        -> SdhClient
        -> SdhValidator
        -> SdhToSggwMapper
        -> SdGgwClient
        -> SdGwLinkEngine
        -> SerialTransport

Essa decisão preserva a separação entre:

- semântica de comando;
- transporte confiável;
- framing binário;
- infraestrutura de serial.

## Classes introduzidas

### Modelos

- `DTL\SdhCommand.cs`
- `DTL\SdhResponse.cs`
- `DTL\SdhTarget.cs`

### Serviços SDH

- `BLL\SDH\SdhTextParser.cs`
- `BLL\SDH\SdhTextSerializer.cs`
- `BLL\SDH\SdhValidator.cs`
- `BLL\SDH\SdhToSggwMapper.cs`
- `BLL\SDH\SdhClient.cs`

### Client por board

- `BLL\Boards\GsaClient.cs`

## Responsabilidade de cada componente

### SdhCommand

Representa o envelope semântico do comando no host:

- `Version`
- `Target`
- `Op`
- `Args`
- `Meta`

### SdhTarget

Resolve o target lógico em partes:

- `Board`
- `Resource`
- `Subresource`

Nesta fase, o parse já exige no mínimo:

    Board.resource

e aceita, opcionalmente:

    Board.resource.subresource

### SdhTextParser

Converte o texto canônico SDH em `SdhCommand`.

Exemplo suportado:

    sdh/1 GSA.led set state=on

### SdhTextSerializer

Gera a forma textual canônica de um `SdhCommand`.

### SdhValidator

Valida o comando semântico no host.

Na fase atual, o catálogo suportado é deliberadamente pequeno:

- versão: `sdh/1`
- target: `GSA.led`
- operação: `set`
- argumento obrigatório: `state`
- valores aceitos: `on`, `off`

### SdhToSggwMapper

Faz a adaptação entre o SDH e o contrato legado atualmente funcional no host.

Nesta fase, o mapper converte:

    sdh/1 GSA.led set state=on

em um envio compatível com o contrato já existente de LED no projeto.

### SdhClient

É a camada central de uso do SDH no host.

Responsabilidades:

- receber comando SDH em objeto ou texto;
- validar;
- mapear para o contrato atual;
- delegar o envio ao `SdGgwClient`.

### GsaClient

É a ergonomia por board para o caso `GSA`.

Exemplo de uso:

    GsaClient.SetLedAsync(true)

Esse client não conhece framing, `SEQ`, `ACK`, serial ou payload binário.

## Fluxo implementado

O fluxo atual do primeiro caso funcional é:

    GsaClient.SetLedAsync(true)
        -> monta SdhCommand
        -> SdhClient.SendAsync(...)
        -> SdhValidator.Validate(...)
        -> SdhToSggwMapper.Map(...)
        -> SdGgwClient.SendAsync(...)
        -> SdGwLinkEngine
        -> SerialTransport

## Mapeamento atual para o contrato legado

Nesta fase, o SDH não substitui o contrato atual do host. Ele apenas o encapsula semanticamente.

O caso suportado hoje utiliza:

- `SggwCmd.LED`
- payload de 1 byte
- `0x01` para `on`
- `0x00` para `off`

O binding entre endereço lógico e físico ainda não acontece no host.

## O que permaneceu intocado

Para preservar a estabilidade da solução, os seguintes componentes não foram reescritos:

- `SdGwLinkEngine`
- `SdGgwClient`
- `SerialTransport`
- framing `SGGW`
- transporte serial

## Estado atual da arquitetura

A situação atual do host pode ser resumida assim:

### Transmissão

A transmissão já passou a contar com uma camada semântica SDH.

### Recepção

A recepção ainda permanece baseada no modelo legado de `SggwFrame`.

Isso significa que, nesta fase:

- TX = SDH sobre SGGW
- RX = SGGW puro

Essa escolha é intencional e reduz risco na adoção incremental do SDH.

## Integração com a BLL atual

A `LedGwTest_BLL` foi ajustada para receber `GsaClient` por injeção, deixando de montar internamente a camada SDH.

Essa mudança corrige a responsabilidade da BLL de caso de uso, que deixa de compor infraestrutura e passa apenas a consumir dependências já prontas.

## Limitações atuais

A implementação atual ainda possui limitações intencionais:

- somente `GSA.led set state=on|off` é suportado;
- `Meta` e `SdhResponse` ainda não participam do fluxo de envio;
- o host ainda não traduz respostas legadas para envelope SDH;
- não existe catálogo amplo de boards;
- o binding lógico-físico ainda não foi implementado.

## Próximos passos recomendados

Os próximos passos coerentes com a arquitetura atual são:

- documentar e projetar a implementação do SDH no gateway;
- definir onde ocorrerá o binding lógico para físico;
- ampliar o catálogo de targets e operações;
- introduzir adaptação de respostas e eventos para o modelo SDH;
- adicionar novos clients por board conforme os casos de uso reais surgirem.

## Conclusão

A primeira implementação do SDH no host foi introduzida no lugar correto da arquitetura: acima do transporte e abaixo dos casos de uso de board.

Com isso, o projeto passa a ter:

- um modelo semântico de comando;
- uma camada de validação formal;
- uma adaptação controlada para o legado;
- uma base coerente para evolução futura.

[Retornar ao README principal](../README.md)
