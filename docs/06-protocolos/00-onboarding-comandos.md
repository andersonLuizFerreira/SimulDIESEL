# Onboarding — Arquitetura de Comandos (SDH)

## Objetivo

Este documento define a trilha recomendada para engenheiros que estão ingressando no projeto SimulDIESEL e precisam compreender a arquitetura de comandos da camada Hardware.

Ao final desta leitura, o engenheiro deverá ser capaz de:

- entender como um comando é estruturado;
- compreender como o comando trafega no sistema;
- entender como o comando é roteado até uma baby board;
- compreender como o firmware executa a operação;
- entender como o software host consome o protocolo.

## Modelo mental do fluxo de comandos

A arquitetura de comandos do SimulDIESEL pode ser compreendida na seguinte sequência lógica:

    Semântica do comando
        ↓
    Transporte confiável
        ↓
    Roteamento no gateway
        ↓
    Execução na baby board
        ↓
    Consumo no software host

## Ordem recomendada de leitura

### 1. Visão arquitetural geral

Arquivo:

    docs/02-arquitetura/01-visao-arquitetural.md

Objetivo:

- entender a modularidade do sistema;
- compreender o papel do gateway;
- compreender a existência das baby boards;
- entender as camadas do sistema.

Tempo estimado: 15 minutos.

---

### 2. Fluxo de comunicação

Arquivo:

    docs/02-arquitetura/03-fluxo-de-comunicacao.md

Objetivo:

- entender handshake textual;
- compreender framing binário (COBS);
- compreender CRC e sequenciamento;
- entender ACK e confiabilidade;
- compreender o roteamento TLV interno.

Tempo estimado: 25 minutos.

---

### 3. Modelo de comandos SDH

Arquivo:

    docs/06-protocolos/01-sdh-command-model.md

Objetivo:

- entender o envelope de comando;
- compreender `target`, `op`, `args`, `meta`;
- compreender a forma textual canônica;
- compreender a forma JSON equivalente;
- entender a motivação arquitetural do modelo.

Tempo estimado: 20 minutos.

---

### 4. Modelo de respostas SDH

Arquivo:

    docs/06-protocolos/02-sdh-response-model.md

Objetivo:

- entender o envelope de resposta;
- compreender códigos de erro;
- entender previsibilidade e rastreabilidade;
- compreender estrutura de dados retornados.

Tempo estimado: 15 minutos.

---

### 5. Exemplos práticos de comandos

Arquivo:

    docs/06-protocolos/03-sdh-examples.md

Objetivo:

- visualizar comandos reais;
- compreender cenários de configuração;
- entender leitura de recursos;
- entender atuação sobre canais.

Tempo estimado: 15 minutos.

---

### 6. Arquitetura de firmware

Arquivo:

    docs/04-firmware/01-arquitetura-firmware.md

Objetivo:

- entender parser de comandos;
- compreender router de recursos;
- compreender dispatcher de operações;
- entender execução física nos periféricos;
- compreender geração de respostas e eventos.

Tempo estimado: 25 minutos.

---

### 7. Camada hardware no software host

Arquivo:

    docs/05-software-dashboard/03-camada-hardware.md

Objetivo:

- entender montagem de comandos no Dashboard;
- compreender envio e correlação de respostas;
- entender consumo de eventos;
- compreender abstração da UI sobre o protocolo.

Tempo estimado: 20 minutos.

## Resultado esperado após a trilha

Após seguir essa sequência, o engenheiro deverá ser capaz de:

- implementar uma nova operação SDH;
- adicionar suporte a novo recurso em uma baby board;
- debugar problemas de comunicação;
- interpretar logs de comandos;
- contribuir com firmware e software host;
- escrever testes de integração baseados no contrato de comandos.

## Observações importantes

- SDH é o envelope semântico do comando.
- SGGW é o transporte confiável atual.
- TLV é o contrato interno entre gateway e dispositivos.
- O target é sempre lógico, nunca físico.
- Argumentos devem ser sempre explícitos.

## Próximos passos recomendados

Após compreender a arquitetura de comandos, recomenda-se estudar:

- DeviceTable e roteamento físico;
- drivers específicos de cada baby board;
- planejamento técnico do projeto;
- estratégia de testes automatizados.

[Retornar ao README principal](../README.md)
