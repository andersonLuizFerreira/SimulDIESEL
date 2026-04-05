⬅ [Retornar para Visão Arquitetural](01-visao-arquitetural.md)

# Camadas do Sistema

O SimulDIESEL é estruturado em camadas lógicas, com responsabilidades bem definidas em cada nível.

Essa organização facilita:

* manutenção
* escalabilidade
* isolamento de falhas
* evolução modular

---

## Estrutura em camadas

```text
Camada de Apresentação
        ↓
Camada de Aplicação
        ↓
Camada de Comunicação
        ↓
Camada de Gateway
        ↓
Camada de Módulos
```

---

## Camada de apresentação

Responsável pela interação com o operador.

Inclui:

* interface gráfica
* formulários
* dashboards
* comandos operacionais
* monitoramento

---

## Camada de aplicação

Responsável pela lógica funcional do sistema.

Inclui:

* regras de operação
* orquestração dos recursos
* controle de fluxo
* integração entre software e hardware

---

## Camada de comunicação

Responsável pelo transporte seguro de dados entre software e hardware.

Inclui:

* sessão de comunicação
* integridade de dados
* framing
* controle de erros
* supervisão do enlace

---

## Camada de gateway

Responsável pelo roteamento interno da bancada.

Inclui:

* recepção de comandos
* validação
* decisão de destino
* encaminhamento aos módulos

---

## Camada de módulos

Responsável pelos recursos físicos e funcionais da bancada aplicados ao equipamento em teste.

Inclui:

* geração e monitoramento de alimentação
* geração de sinais analógicos e digitais
* leitura de sinais analógicos e digitais
* leitura de dados, códigos de erro e programação
* simulação de condições reais de funcionamento


---

## Fluxo entre camadas

```text
UI
→ Aplicação
→ Comunicação
→ Gateway
→ Módulo
→ resposta
```

---

## Próximas camadas

A partir desta divisão em camadas, escolha a área que deseja aprofundar.

### Fluxo entre as camadas

Para entender como a informação percorre o sistema, desde a interface até o módulo físico.

* [Fluxo de Comunicação](03-fluxo-de-comunicacao.md)

---

### Estrutura física da bancada

Para entender a organização física do hardware e sua distribuição modular.

* [Backplane](../03-hardware/01-backplane.md)

---

### Camada embarcada

Para aprofundar a arquitetura de firmware e gateway.

* [Arquitetura de Firmware](../04-firmware/01-arquitetura-firmware.md)

---

### Camada da aplicação

Para aprofundar a arquitetura do software local e interface com o operador.

* [Arquitetura do Software](../05-software-dashboard/01-arquitetura-software.md)
