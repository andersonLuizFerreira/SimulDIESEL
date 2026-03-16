# Arquitetura de Firmware

## Visão Geral

O firmware do SimulDIESEL é responsável por:

- gerenciar o hardware das baby boards;
- implementar o transporte físico e lógico de comunicação;
- interpretar comandos da camada Hardware;
- executar operações sobre recursos internos;
- gerar respostas e eventos padronizados.

A arquitetura foi projetada para ser modular, previsível e escalável, permitindo a inclusão de novas boards, novos recursos e novos tipos de operação sem impacto estrutural no restante do sistema.

No estado atual do projeto, a sessão host/gateway continua baseada em `SGGW`, mas a evolução aprovada prevê a introdução do SDH como camada lógica de comando acima do transporte atual.

## Camadas internas do firmware

A arquitetura lógica pode ser dividida em cinco camadas principais:

    Transporte físico
    Motor de frames
    Parser de protocolo
    Router de recursos
    Lógica funcional

Cada camada possui responsabilidades bem definidas.

### Transporte físico

Responsável por:

- UART
- I2C
- SPI
- CAN
- GPIO
- timers e periféricos auxiliares

Essa camada não conhece comandos de alto nível. Ela apenas envia e recebe bytes.

### Motor de frames

Responsável por:

- delimitação de frames;
- validação de CRC;
- controle de sequência;
- geração de ACK / RESP;
- detecção de timeout;
- retry quando aplicável.

No estado atual, essa camada é representada pelo transporte host/gateway baseado em `SGGW`.

O motor de frames transforma fluxo de bytes em unidades lógicas confiáveis.

### Parser de protocolo

Responsável por:

- interpretar o comando lógico recebido;
- validar a estrutura do protocolo ativo;
- preparar a estrutura interna de execução.

No estado atual, o firmware já possui parsing do fluxo host/gateway e contratos internos de barramento.
A evolução aprovada é introduzir o parser SDH acima do transporte atual, sem misturar semântica de comando com framing.

### Router de recursos

Responsável por:

- resolver a baby board;
- resolver o recurso interno;
- encaminhar a operação para o módulo correto;
- tratar indisponibilidade, busy e unsupported.

Essa camada implementa o endereçamento lógico:

    <BOARD>.<resource>.<subresource>

Exemplos:

    BPM.gateway
    BPM.gateway.serial
    PSU.power.main
    GSA.led
    UCO.can1

### Lógica funcional

Responsável por:

- executar operações físicas reais;
- configurar periféricos;
- alterar estados;
- ler sensores;
- atualizar DAC / PWM / GPIO;
- interagir com drivers específicos.

Cada baby board possui sua própria implementação funcional.

## Situação atual e evolução aprovada

Hoje o gateway já resolve:

- sessão host/gateway;
- roteamento por endereço;
- despacho por barramento;
- contratos internos com devices.

A evolução aprovada adiciona um novo passo no gateway:

    transporte confiável
        -> parser SDH
        -> router lógico
        -> binding lógico-físico
        -> mapper para contrato interno
        -> execução no barramento/device

Isso permite que o gateway passe a ser o responsável por converter comandos semânticos em transações concretas sobre a infraestrutura física da bancada.

## Modelo de comando SDH no firmware

O modelo lógico aprovado para a camada Hardware é:

    version
    target
    op
    args
    meta

Fluxo interno pretendido:

1. Receber frame válido do host
2. Extrair o comando lógico
3. Parser valida versão
4. Router resolve target
5. Binding lógico-físico define a rota real
6. Mapper interno converte para o contrato legado atual
7. Handler executa
8. Response Builder monta a resposta

## Primeiro caso de uso aprovado para o gateway

O primeiro comando oficial a ser suportado nessa arquitetura é:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

Esse caso foi escolhido porque já existe caminho funcional entre host, gateway e GSA, permitindo validar a nova arquitetura sem romper o legado já estável.

## Dispatcher de operações

O dispatcher utiliza um conjunto pequeno e estável de verbos:

    read
    set
    cfg
    run
    status
    reset

Formas qualificadas podem existir:

    read.id
    read.cfg
    set.state
    run.scan
    run.apply

Isso permite padronização entre boards diferentes.

## Modelo de resposta

Toda resposta gerada pelo firmware deve convergir para envelope comum:

    version
    ok
    target
    op
    code
    message
    data
    meta

Tipos de resposta:

- confirmação de escrita/configuração;
- retorno de leitura;
- resposta de status;
- erro padronizado.

Códigos de erro recomendados:

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

## Eventos assíncronos

O firmware pode gerar eventos independentes de requisição:

    sdh/1 evt <source> <name> chave=valor ...

Exemplos:

- inserção/remoção de X-CONN;
- fault de PSU;
- mudança de estado CAN;
- watchdog;
- alteração de entrada digital.

## Persistência e perfis

Configurações realizadas com `cfg` podem:

- ser temporárias;
- ser persistidas em NVS / EEPROM / Flash;
- ser aplicadas via `run.apply`;
- ser restauradas via `reset` ou `run.profile`.

## Escalabilidade

A arquitetura permite:

- inclusão de novas baby boards sem alterar o transporte host/gateway;
- inclusão de novos recursos apenas registrando handlers;
- expansão do protocolo mantendo compatibilidade por versão (`sdh/1`, `sdh/2` etc.);
- coexistência de múltiplos transportes.

## Integração com o software host

O firmware foi projetado para operar com:

- Dashboard local em C#;
- API web futura;
- terminal humano (UHM);
- scripts automatizados.

A evolução SDH mantém essa flexibilidade, mas desloca para o gateway a responsabilidade de resolver o target lógico e convertê-lo para a rota física real.

## Referências

- `docs/04-firmware/04-sdh-gateway-architecture.md`
- `docs/06-protocolos/01-sdh-command-model.md`
- `docs/06-protocolos/02-sdh-response-model.md`
- `docs/06-protocolos/03-sdh-examples.md`

[Retornar ao README principal](../README.md)
