# Objetivos

## Estado atual

Os objetivos concretamente sustentados pelo repositório podem ser agrupados em quatro frentes:

- estabelecer um enlace local confiável entre estação de bancada e gateway;
- permitir roteamento uniforme para dispositivos em barramentos diferentes;
- desacoplar protocolo de host, transporte físico e serviços de dispositivo;
- criar base para expansão gradual de módulos de simulação e diagnóstico.

Esses objetivos são observáveis no código e não apenas em documentos. O uso de interfaces de transporte, máquinas de estado de link, tabela de dispositivos e serviços embarcados dedicados mostra que o projeto foi estruturado para crescer sem acoplamento direto entre UI, barramento e lógica de dispositivo.

## Funcionamento técnico

### Objetivo 1: enlace confiável com o host

No `local-api`, `IByteTransport` abstrai o meio físico. `SerialTransport` implementa a serial bruta, enquanto `SerialLinkService` cuida da transição do modo textual de bootstrap para o modo binário de operação. `SdGwLinkEngine` acrescenta confirmação por sequência, repetição por timeout e deduplicação de respostas.

### Objetivo 2: roteamento por endereço lógico

O protocolo usa `CMD = [ADDR:4][OP:4]`, permitindo que o host trate gateway e periféricos com a mesma estrutura básica. No ESP32, `GatewayApp` decide se o comando é local ou roteado. `GwRouter` consulta `GwDeviceTable` para descobrir barramento, endereço físico e metadados da transação.

### Objetivo 3: padronização de serviços embarcados

No GSA, a pilha `Transport -> Link -> Service -> LedService` mostra a intenção de separar recepção física, validação do frame e regra funcional. Isso reduz impacto quando um novo serviço for adicionado ao mesmo dispositivo ou quando outro periférico reaproveitar a mesma pilha.

### Objetivo 4: suporte a bancada e diagnóstico

O software local já possui telas voltadas a serial e teste de LED, além de um serviço de saúde de link (`SdGwHealthService`) baseado em `ping`. A combinação desses componentes atende a um objetivo claro de operação assistida em bancada, com observabilidade mínima do enlace.

## Limitações

Nem todos os objetivos amplos do nome SimulDIESEL aparecem implementados no código atual. Não há, por exemplo, uma camada de nuvem operacional, um conjunto extenso de sensores simulados nem contratos maduros para CAN/J1939 em produção. O objetivo presente no repositório é mais fundacional: consolidar a espinha dorsal de comunicação e o primeiro conjunto de serviços embarcados.

## Evolução prevista

O roadmap legado sugere expansão em ciclos curtos. A tendência técnica natural é:

- ampliar a tabela de dispositivos e os serviços roteáveis;
- transformar o GSA em periférico mais completo que o serviço atual de LED;
- consolidar contratos de software entre host, gateway e módulos;
- evoluir a observabilidade do software local para cenários de diagnóstico mais ricos.

[Retornar ao README principal](../README.md)
