⬅ [Retornar para Visão Lógica do Projeto](03-visao-logica.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

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
Operador / UI
        ↓
FormsLogic e clients funcionais
        ↓
SDH / SDGW / sessão
        ↓
Gateway BPM
        ↓
Serviços embarcados das boards
```

---

## Camada de apresentação e operação

Responsável pela interação com o operador e pelos formulários WinForms já implementados.

Inclui:

* `DashBoard`
* `frmPortaSerial_UI`
* `frmBluetoothConnect`
* `frmGSA_UI`

---

## Camada de aplicação host

Responsável por transformar ação de tela em caso de uso operacional.

Inclui:

* `FrmBpmLogic`
* `FrmGsaLogic`
* `BpmClient`
* `GsaClient`
* `BpmSerialService`

---

## Camada de sessão e comunicação

Responsável por sessão, framing, confiabilidade e transporte.

Inclui:

* `SdhClient`
* `SdgwSession`
* `SdGwTxScheduler`
* `SdGwLinkEngine`
* `SdGwLinkSupervisor`
* `SwitchableTransport`
* `SerialTransport`
* `BluetoothTransport`

---

## Camada de gateway

Responsável por receber SDGW, manter sessão binária e rotear para a board correta.

Inclui:

* `SggwLink`
* `SggwEndpointMux`
* `GatewayApp`
* `GwRouter`
* `GwDeviceTable`

---

## Camada de serviços embarcados

Responsável pela execução funcional nas boards remotas.

Inclui:

* `Service` e `AnalogService` da GSA
* TLV curto com CRC próprio
* geração de sinais analógicos
* fault reset, status e offsets por canal
* eventos assíncronos de fault e de resultado físico


---

## Fluxo entre camadas

```text
UI
→ FormsLogic / clients
→ SDH / SDGW
→ Gateway BPM
→ serviço embarcado
→ resposta
```

---

## Glossário

- **Camada**: nível de responsabilidade dentro da arquitetura do sistema.
- **Gateway**: ponto de passagem entre host, roteamento interno e hardware.
- **Arquitetura**: organização estrutural e funcional das partes do SimulDIESEL.

## Próximas camadas

A partir desta divisão lógica, o próximo aprofundamento oficial segue pelo fluxo operacional do sistema.

* [Fluxo de Comunicação](03-fluxo-de-comunicacao.md)
