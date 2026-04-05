⬅ [Retornar para GSA — Visão Geral](README.md)

# Funcionamento Eletrônico

Esta seção descreve o funcionamento eletrônico real da GSA, desde o recebimento do comando vindo da BPM até a geração física do sinal analógico na saída.

O fluxo foi concebido com **dois barramentos I2C independentes**, separando a comunicação com o gateway da comunicação interna da própria board.

---

## Fluxo eletrônico principal

O funcionamento da GSA segue o seguinte fluxo:

```text id="k7m2ha"
BPM (Gateway)
        ↓
I2C físico (slave)
        ↓
Controle lógico da GSA
        ↓
I2C lógico interno (master)
        ↓
HUB I2C (8 canais)
        ↓
DAC selecionado
        ↓
Condicionamento analógico
        ↓
Saída do canal
```

---

## 1. Recebimento do comando pela BPM

A GSA recebe os comandos da BPM por meio de um **barramento I2C físico dedicado**.

Neste barramento, a GSA opera como **slave**.

A BPM envia o comando contendo, entre outras informações:

* canal desejado
* valor de tensão solicitado
* habilitação da saída

O firmware da GSA recebe esse pacote e inicia o processamento interno.

---

## 2. Processamento do comando

Após o recebimento, a camada lógica da GSA interpreta o comando.

Nesta etapa são identificados:

* porta / canal de saída
* faixa de tensão
* valor solicitado
* DAC de destino

Essa etapa define qual recurso eletrônico será acionado.

---

## 3. Arquitetura de barramentos I2C

A GSA utiliza **dois barramentos I2C independentes**.

### I2C físico — comunicação com a BPM

Este barramento é utilizado exclusivamente para comunicação entre gateway e board.

```text id="j8s3ta"
BPM → GSA
```

Modo de operação da GSA:

```text id="4pj7wh"
slave
```

---

### I2C lógico — barramento interno da GSA

Para acesso aos componentes internos, a GSA utiliza um segundo barramento I2C.

Neste barramento a GSA opera como **master**.

Esse barramento é responsável por controlar:

* HUB I2C
* DACs
* periféricos internos

```text id="4m40kl"
GSA → HUB → DAC
```

---
---

## Tabela de interligação física

A GSA possui as seguintes interligações físicas principais relacionadas ao funcionamento eletrônico da placa.

| Função       | Tipo                   | Pinos   | Observação                                              |
| ------------ | ---------------------- | ------- | ------------------------------------------------------- |
| I2C físico   | Comunicação com BPM    | A4 / A5 | Barramento I2C onde a GSA opera como **slave**          |
| I2C lógico   | Comunicação interna    | D2 / D3 | Barramento I2C interno onde a GSA opera como **master** |
| Reset do HUB | Controle interno       | D8      | Reset do multiplexador / HUB I2C                        |
| IRQ          | Sinalização assíncrona | D4      | Notificação de eventos para a BPM                       |
| Reset global | Reset da board         | RESET   | Ligado ao reset global vindo pelo backplane             |

---


## 4. HUB I2C

O acesso aos DACs é realizado por meio de um **HUB I2C de 8 canais**.

Cada canal do HUB possui **2 DACs associados**.

Isso permite expandir a quantidade total de canais de saída da board.

A estrutura é:

```text id="p4e58n"
8 canais HUB
×
2 DACs por canal
=
16 canais totais
```

---

## 5. DAC e geração do sinal

Após a seleção do canal no HUB, a GSA acessa o DAC correspondente.

O DAC converte o valor digital recebido em tensão analógica de referência.

Essa tensão segue para o circuito de condicionamento analógico da saída.

É nessa etapa que são geradas as faixas de:

* 0–5 V
* 0–12 V

---

## Estado atual

A leitura de tensão e corrente de saída **ainda não está implementada**.

Por esse motivo, esta etapa ainda não faz parte do fluxo funcional oficial.

---

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
