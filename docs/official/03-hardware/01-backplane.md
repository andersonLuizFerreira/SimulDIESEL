⬅ [Retornar para Camadas do Sistema](../02-arquitetura/02-camadas-do-sistema.md)

# Backplane

O **Backplane** é a estrutura central de interligação física, elétrica e lógica do SimulDIESEL.

Ele é responsável por conectar o módulo em teste, as baby boards e o gateway, garantindo distribuição segura de sinais, alimentação e comunicação.

Sua arquitetura foi concebida para permitir modularidade, proteção elétrica e expansão progressiva da bancada.

---

## Papel do backplane

O backplane atua como elemento central entre três blocos principais:

```text id="7j9l0v"
Módulo em teste
        ↓
X-CONN
        ↓
Backplane
   ↙    ↓    ↘
Baby Boards  Gateway  Alimentação
```

Ele faz a interligação completa entre esses elementos.

---

## Interface com o módulo em teste

O módulo em teste é conectado por meio de um **chicote específico** para cada aplicação.

Esse chicote é ligado à placa **X-CONN**.

A **X-CONN** é responsável por adaptar e organizar fisicamente os sinais do módulo.

O backplane recebe esses sinais da X-CONN e faz a distribuição para as baby boards responsáveis pela geração, leitura ou monitoramento de cada recurso.

Isso inclui:

* sinais analógicos
* sinais digitais
* linhas de comunicação
* alimentação do módulo

---

## Interface com as baby boards

O backplane realiza a interligação elétrica entre a X-CONN e as baby boards.

Cada baby board executa uma função específica, como:

* geração de sinais
* leitura de entradas
* acionamento de saídas
* monitoramento de estados

O backplane distribui os sinais necessários entre essas unidades.

---

## Distribuição de alimentação

Uma das funções mais importantes do backplane é a distribuição de alimentação.

Essa alimentação é dividida em dois domínios independentes.

### Alimentação das baby boards

Responsável exclusivamente pelo funcionamento interno da bancada.

Essa alimentação é dedicada aos módulos internos do sistema.

---

### Alimentação do módulo em teste

Responsável por alimentar o equipamento externo conectado à bancada.

Essa alimentação é mantida separada da alimentação das baby boards.

O objetivo é garantir proteção contra falhas como:

* curto-circuito no módulo
* sobrecorrente
* falha de alimentação externa

Essa segregação aumenta a segurança do sistema.

---

## Comunicação com o gateway

O backplane também realiza a interligação dos sinais de comunicação entre as baby boards e o gateway central.

Essa comunicação permite que a aplicação Windows envie comandos e receba respostas do hardware.

```text id="1x8xqj"
Aplicação
    ↓
Gateway
    ↓
Backplane
    ↓
Baby Boards
```

---

## Papel estrutural

O backplane é o núcleo físico da bancada.

Ele centraliza:

* interconexão
* alimentação
* proteção
* comunicação
* expansão modular

---

## Próximas camadas

### Módulos especializados

* [Baby Boards](02-baby-boards.md)

### Barramentos internos

* [Barramentos](03-barramentos.md)

### Alimentação

* [Alimentação](04-alimentacao.md)
