⬅ [Retornar para Camadas do Sistema](02-camadas-do-sistema.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Fluxo de Comunicação

O fluxo de comunicação do SimulDIESEL descreve o caminho percorrido pela informação desde a interface do operador até o módulo físico em teste.

Essa camada é responsável por garantir:

* entrega confiável de comandos
* integridade de dados
* roteamento correto
* retorno de resposta ao operador

---

## Fluxo conceitual

O fluxo principal do sistema é:

```text
UI / Operador
    ↓
FormsLogic / clients
    ↓
SDH / SDGW
    ↓
Gateway BPM
    ↓
I2C / SPI interno
    ↓
Board remota
    ↓
evento, resposta ou efeito físico
```

---

## Etapa 1 — estabelecimento do link

Antes da operação funcional, o sistema realiza o estabelecimento inicial do enlace entre software e hardware.

Nesta etapa ocorre:

* abertura do transporte serial ou Bluetooth
* sincronização inicial por banner
* estabelecimento da sessão SDGW
* validação do link

Somente após a validação o sistema entra em operação.

---

## Etapa 2 — operação funcional

Após o enlace estar estabelecido, o fluxo passa a operar de forma contínua.

O caminho funcional segue:

```text
UI
→ FormsLogic / client funcional
→ SdhClient
→ SdgwSession
→ SdGwTxScheduler
→ SdGwLinkEngine
→ SwitchableTransport
→ BPM / GatewayApp
→ GwRouter
→ board remota
→ resposta ou evento
```

Cada comando enviado pelo operador percorre essa cadeia até atingir o equipamento em teste.

---

## Papel do gateway

O gateway atua como ponto central de roteamento.

Sua função é:

* receber frames SDGW
* validar integridade
* identificar endereço local ou remoto
* encaminhar ao barramento correto
* devolver resposta ou evento ao host

---

## Retorno da resposta

Após a execução no módulo, a resposta percorre o caminho inverso até a interface.

```text
Board remota
→ Gateway BPM
→ SDGW
→ BLL / FormsLogic
→ UI
```

Isso permite monitoramento e diagnóstico em tempo real.

---

## Robustez do enlace

O sistema foi projetado para manter robustez na comunicação por meio de:

* `CRC8`
* `COBS`
* `ACK` / `ERR`
* timeout e retry
* supervisão de atividade válida
* tolerância a respostas tardias com a porta ainda aberta

---

## Exemplo prático

Um comando de teste percorre o seguinte caminho:

```text
Operador
→ comando na UI
→ BLL / client funcional
→ SDH / SDGW
→ gateway BPM
→ board remota
→ leitura da resposta ou do evento
→ exibição na UI
```

---

## Glossário

- **Camada**: nível de responsabilidade dentro da arquitetura do sistema.
- **Gateway**: ponto de passagem entre host, roteamento interno e hardware.
- **Arquitetura**: organização estrutural e funcional das partes do SimulDIESEL.

## Próximas camadas

Após compreender o fluxo de comunicação, siga para a área que deseja aprofundar.

### Protocolos

* [Protocolos e Contratos](../06-protocolos/README.md)
