# Arquitetura SDH no Gateway

## Objetivo

Este documento define formalmente a arquitetura pretendida para a implementação do SDH (SimulDiesel Hardware Command) no gateway do SimulDIESEL.

O foco deste documento não é o transporte host/gateway já existente, mas sim a camada lógica que deverá interpretar comandos SDH, resolver targets lógicos e adaptá-los para o contrato interno atualmente utilizado entre gateway e dispositivos.

## Contexto arquitetural

No estado atual do projeto:

- o host já possui uma primeira camada semântica SDH;
- o transporte host/gateway continua baseado em `SGGW`;
- o gateway atual já conhece `ADDR/OP`, roteamento por barramento e payloads internos;
- o contrato interno com devices, como o GSA, continua baseado em `TLV`.

A evolução aprovada é:

- manter `SGGW` como transporte confiável atual;
- introduzir SDH como envelope lógico de comando;
- preservar TLV como contrato interno para os devices;
- realizar o binding entre endereço lógico e físico dentro do gateway.

## Papel do gateway na arquitetura SDH

O gateway passa a ser o ponto responsável por:

- receber o comando vindo do host;
- interpretar o envelope SDH;
- validar versão, target, operação e argumentos;
- resolver a board e o recurso interno;
- mapear o target lógico para a rota física;
- traduzir a intenção do comando para `ADDR/OP/PAYLOAD`;
- encaminhar a transação ao barramento apropriado;
- devolver uma resposta padronizada ao host.

## Fluxo lógico esperado

A implementação alvo do gateway deve seguir o fluxo:

    Frame recebido do host
        -> extração do payload lógico
        -> parser SDH
        -> validator SDH
        -> target router
        -> device binding
        -> mapper para contrato interno
        -> barramento físico
        -> resposta do device
        -> montagem de resposta ao host

## Camadas propostas no gateway

A arquitetura recomendada para o gateway é composta por cinco camadas.

### 1. Transporte host/gateway

Responsável por:

- framing `SGGW`;
- recepção e transmissão de bytes;
- `ACK`;
- `SEQ`;
- `CRC`;
- watchdog e sessão.

Essa camada já existe e deve continuar separada do SDH.

### 2. Parser SDH

Responsável por:

- interpretar o envelope lógico do comando;
- validar presença de:
  - `version`
  - `target`
  - `op`
  - `args`
  - `meta`
- produzir uma estrutura interna de comando.

Essa camada não deve conhecer barramentos nem detalhes físicos.

### 3. Router de target

Responsável por:

- decompor o target lógico;
- identificar:
  - board
  - resource
  - subresource
- selecionar o handler apropriado.

Exemplos de target:

    GSA.led
    BPM.gateway
    BPM.gateway.serial
    PSU.power.main
    UCO.can1

### 4. Binding lógico-físico

Responsável por mapear o domínio lógico da board para a infraestrutura física do gateway.

Exemplo conceitual:

- board lógica: `GSA`
- endereço lógico do gateway: `0x1`
- barramento: `I2C`
- endereço físico: `0x23`

Esse binding não pertence ao host. Ele deve acontecer exclusivamente no gateway.

### 5. Mapper para contrato interno

Responsável por traduzir o comando SDH para o modelo atualmente entendido pelo firmware e pelos devices.

Exemplo para o primeiro caso de uso:

    sdh/1 GSA.led set state=on

convergindo para algo como:

- board `GSA`
- operação interna equivalente a `SET`
- subcomando `LED`
- payload com valor `1`

## Primeiro caso de uso oficial do gateway

O primeiro caso que deverá ser suportado pelo gateway é:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

Esse caso foi escolhido porque:

- já existe contrato funcional de LED no projeto;
- já existe caminho host -> gateway -> GSA;
- permite validar a arquitetura SDH sem romper o legado.

## Mapeamento esperado do primeiro caso

### Comando semântico

    sdh/1 GSA.led set state=on

### Interpretação no gateway

- `version = sdh/1`
- `target = GSA.led`
- `op = set`
- `args.state = on`

### Resolução lógica

- board = `GSA`
- resource = `led`

### Binding físico

- board lógica `GSA`
- rota conhecida do gateway para `GSA`

### Tradução interna

A intenção semântica é convertida para o contrato interno já existente do GSA.

## Componentes recomendados no firmware do gateway

Os nomes exatos podem variar conforme a organização atual do firmware, mas os blocos recomendados são:

- `sdh_command`
- `sdh_response`
- `sdh_parser`
- `sdh_validator`
- `sdh_target`
- `sdh_router`
- `device_binding_table`
- `gsa_command_mapper`

Esses componentes devem ficar acima do barramento e abaixo da sessão host/gateway.

## Regras de projeto

A implementação do SDH no gateway deve obedecer às seguintes regras:

- `SGGW` não deve virar parser SDH;
- o parser SDH não deve conhecer barramento físico;
- o binding lógico-físico deve ficar centralizado no gateway;
- a lógica por board deve ser expansível;
- a tradução para contratos legados deve ficar isolada em mappers;
- o target externo deve permanecer lógico, nunca físico.

## Estado desejado do sistema após essa etapa

Depois da implementação do SDH no gateway, o sistema deverá passar a ter:

### No host

- comando semântico SDH
- envio sobre `SGGW`

### No gateway

- parser e resolução SDH
- binding lógico-físico
- tradução para o contrato interno

### No device

- contrato interno atual preservado

## Benefícios esperados

A adoção dessa arquitetura traz os seguintes ganhos:

- separação clara entre semântica e transporte;
- host desacoplado do hardware interno da bancada;
- possibilidade de crescimento por board e por resource;
- manutenção do legado enquanto o SDH amadurece;
- redução de ambiguidade na expansão de comandos.

## Limitações e decisões desta fase

Este documento ainda não define:

- layout binário final do SDH no enlace host/gateway;
- formato definitivo de resposta SDH no gateway;
- política completa de eventos SDH;
- catálogo completo de boards e resources.

Esses pontos devem ser detalhados em etapas posteriores, após validação do primeiro caso funcional no gateway.

## Próximos passos recomendados

Os próximos passos coerentes com este documento são:

- definir a estrutura interna de `SdhCommand` no firmware;
- definir onde o parser SDH será acoplado ao fluxo atual do gateway;
- definir a tabela de binding lógico-físico;
- implementar o primeiro mapper:
  - `GSA.led set state=on|off`
- validar o fluxo ponta a ponta com o GSA.

## Conclusão

O gateway é o ponto correto para resolver a passagem entre o domínio lógico do SDH e a infraestrutura física da bancada.

Com isso, o projeto mantém:

- semântica de alto nível no host;
- binding físico no gateway;
- contrato interno preservado nos devices.

Essa é a base correta para a evolução modular do SimulDIESEL.

[Retornar ao README principal](../README.md)
