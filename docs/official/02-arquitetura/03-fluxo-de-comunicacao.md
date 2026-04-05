⬅ [Retornar para Camadas do Sistema](02-camadas-do-sistema.md)

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
Aplicação
    ↓
Camada de Comunicação
    ↓
Gateway Central
    ↓
Barramento Físico
    ↓
Módulo em teste
    ↓
Resposta
```

---

## Etapa 1 — estabelecimento do link

Antes da operação funcional, o sistema realiza o estabelecimento inicial do enlace entre software e hardware.

Nesta etapa ocorre:

* abertura do transporte
* sincronização inicial
* identificação do hardware
* validação do link

Somente após a validação o sistema entra em operação.

---

## Etapa 2 — operação funcional

Após o enlace estar estabelecido, o fluxo passa a operar de forma contínua.

O caminho funcional segue:

```text
UI
→ Aplicação
→ Comunicação
→ Gateway
→ Módulo
→ resposta
```

Cada comando enviado pelo operador percorre essa cadeia até atingir o equipamento em teste.

---

## Papel do gateway

O gateway atua como ponto central de roteamento.

Sua função é:

* receber comandos
* validar integridade
* identificar destino
* encaminhar ao barramento correto
* devolver resposta ao software

---

## Retorno da resposta

Após a execução no módulo, a resposta percorre o caminho inverso até a interface.

```text
Módulo
→ Gateway
→ Comunicação
→ Aplicação
→ UI
```

Isso permite monitoramento e diagnóstico em tempo real.

---

## Robustez do enlace

O sistema foi projetado para manter robustez na comunicação por meio de:

* controle de integridade
* verificação de erros
* supervisão de atividade
* recuperação de falhas
* tolerância a atrasos

---

## Exemplo prático

Um comando de teste percorre o seguinte caminho:

```text
Operador
→ comando na UI
→ envio pela aplicação
→ gateway
→ módulo em teste
→ leitura da resposta
→ exibição na UI
```

---

## Próximas camadas

Após compreender o fluxo de comunicação, siga para a área que deseja aprofundar.

### Protocolos

* [Arquitetura de Comandos](../06-protocolos/00-onboarding-comandos.md)
