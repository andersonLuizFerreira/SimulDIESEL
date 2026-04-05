⬅ [Retornar para Construção Física das Boards](../../05-boards-fisicas.md)

# GSA — Gerador de Sinais Analógicos

A **GSA (Gerador de Sinais Analógicos)** é uma baby board especializada na geração, condicionamento e monitoramento de sinais analógicos aplicados ao módulo em teste.

Atualmente, esta é a board física mais madura do projeto SimulDIESEL, servindo como referência para a arquitetura eletrônica das demais boards.

Sua principal função é reproduzir sinais elétricos analógicos que simulam sensores, entradas e condições reais de operação.

---

## Função principal

A GSA é responsável por:

* geração de tensões analógicas controladas
* simulação de sensores
* condicionamento de níveis de tensão
* monitoramento das saídas
* proteção elétrica dos canais
* interface com a backplane

Ela permite que o módulo em teste receba sinais equivalentes aos encontrados no ambiente real de funcionamento.

---

## Características físicas

A estrutura física da GSA é composta por blocos funcionais principais.

```text id="w8z43f"
Interface com Gateway
        ↓
Controle lógico
        ↓
Barramento I2C interno
        ↓
DACs / HUB I2C
        ↓
Circuitos analógicos
        ↓
Saídas para módulo em teste
```

---

## Características elétricas

A GSA possui:

* 16 canais de saída, sendo:

  * 8 canais de geração de 0–5 V
  * 8 canais de geração de 0–12 V
* conversão digital-analógica por DAC
* condicionamento por amplificadores operacionais
* leitura e monitoramento de tensão e corrente de saída
* proteção contra falhas elétricas
* capacidade de corrente de saída de até 80 mA em modo source/sink


---

## Arquitetura interna

A placa é dividida em duas grandes áreas.

### Controle lógico

Responsável por:

* interpretação dos comandos
* controle dos canais
* shadow RAM
* interface com a BPM (Gateway)

---

### Circuito eletrônico

Responsável por:

* DAC
* HUB I2C
* multiplexação
* condicionamento analógico
* proteção elétrica
* monitoramento

---

## Papel dentro da bancada

A GSA atua como a unidade responsável pela geração de sinais analógicos do sistema.

Ela é utilizada para simular sensores e condições operacionais do módulo conectado via X-CONN.

---

## Próximas camadas

### Funcionamento eletrônico detalhado

* [Funcionamento eletrônico](01-funcionamento-eletronico.md)
