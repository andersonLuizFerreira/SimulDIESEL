# Camadas do Sistema

## Camada de apresentação

Arquivos principais:

- `DashBoard`
- `frmPortaSerial_UI`
- `frmGSA_UI`

Responsabilidades:

- exibir estado de serial e link
- acionar conexão e desconexão
- consumir operações funcionais expostas pelas FormsLogic

A UI não manipula framing ou protocolo diretamente.

## Camada de aplicação / BLL

Componentes centrais:

- `BpmSerialService`
- `GsaClient`
- `BpmClient`
- `BackplaneService`
- `XConnService`
- `FrmBpmLogic`
- `FrmGsaLogic`

Responsabilidades:

- orquestrar o uso funcional do link serial da BPM
- refletir estado do link para a UI
- expor operações por board
- concentrar a composição atual do host em `BpmSerialService`

Ponto global transitório atual:

    BpmSerialService.Shared

## Camada de protocolo / DAL

Componentes centrais:

- `SerialTransport`
- `SdGwLinkEngine`
- `SdGwTxScheduler`
- `SdgwSession`
- `SdhClient`
- `SdhToSdgwMapper`
- `SdGwLinkSupervisor`

Responsabilidades:

- I/O serial bruto
- framing SDGW
- timeout/retry e stop-and-wait
- arbitragem de TX com prioridade
- sessão de alto nível
- tradução semântica SDH para SDGW compacto
- supervisão lógica do link por silêncio de RX válido

## Camada de gateway embarcada

Componentes principais:

- `SggwLink`
- `GatewayApp`
- `GwRouter`
- `GwI2cBus`
- `GwSpiBus`

Responsabilidades:

- handshake textual inicial
- parsing e validação de frames SDGW
- keepalive por atividade SDGW válida
- tratamento de comandos locais da BPM
- roteamento para barramentos internos

## Camada de módulos

No estado atual, a implementação funcional mais exercitada é a GSA, usando transação TLV curta sobre I2C.

## Dependências entre camadas

A ordem operacional do sistema continua sendo:

1. UI solicita conexão
2. `BpmSerialService` abre a serial
3. bootstrap textual até `Linked`
4. operação binária SDGW
5. envio funcional via scheduler

Depois do primeiro `Linked` da conexão atual, o host continua podendo entregar tráfego binário SDGW ao engine mesmo se o estado lógico cair temporariamente para `LinkFailed`, desde que a porta continue aberta.

[Retornar ao README principal](../README.md)
