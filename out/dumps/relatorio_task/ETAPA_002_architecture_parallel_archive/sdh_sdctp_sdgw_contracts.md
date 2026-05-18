# Contratos Arquiteturais SDH / SDCTP / SDGW

Documento oficial de congelamento arquitetural da ETAPA 01.

Base de referencia: levantamento arquitetural corrente gerado antes da ETAPA 01.

## 1. Nomenclatura oficial

- SDH: SimulDIESEL Hardware Language
- SDCTP: SimulDIESEL CAN Transport Protocol
- SDGW: SimulDIESEL Gateway

O termo correto e oficial e SDCTP.

Qualquer ocorrencia antiga com letras invertidas deve ser tratada como erro de digitacao em documentacao historica.

## 2. Responsabilidade de cada camada

### SDH

SDH e a linguagem de configuracao e operacao do hardware.

Pertencem ao SDH:

- Configurar bitrate CAN
- Configurar modo CAN
- Abrir, fechar, habilitar ou desabilitar CAN
- Solicitar status CAN
- Resetar servico CAN
- Operar LED e service commands
- Comandos futuros de configuracao ou operacao de hardware

Nao pertencem ao SDH:

- Trafego continuo de massa CAN RX
- CAN_CREATE
- CAN_EDIT
- CAN_DELETE
- CAN_ROW
- CAN_TIC
- Sincronizacao da tabela RX CAN
- Compactacao DATA_MASK de massa RX

### SDCTP

SDCTP e o protocolo de transporte, compactacao e sincronizacao de massa de dados CAN.

Pertencem ao SDCTP:

- CAN_RX_EVENT
- CAN_CREATE
- CAN_EDIT
- CAN_DELETE
- CAN_ROW
- CAN_READ_ALL_DONE
- CAN_TIC
- CAN_TX
- CAN_TX_STOP
- CAN_TX_DIRECT
- CAN_TX_CREATE
- CAN_TX_EDIT
- CAN_TX_DELETE
- Mirror table RX
- Mirror/buffer table TX
- CanRxOutputBuffer
- Compactacao por DATA_MASK
- Sincronizacao de massa CAN RX/TX entre UCE e API

TX CAN segue o mesmo paradigma de massa/sincronizacao do RX CAN. O CanService possui lado RX e lado TX: RX alimenta mirror table e `CanRxOutputBuffer`; TX utiliza tabela/buffer local para envio direto, envio ciclico e manutencao de linhas TX.

`CAN_READ_ALL` e legado. O fluxo oficial nao deve solicitar `CAN_READ_ALL` para funcionamento da UI/API; consumidores devem usar `CanRxOutputBuffer`, snapshot local da mirror table e eventos SDCTP. `CAN_READ_ALL_DONE` pode permanecer apenas como compatibilidade tecnica de evento recebido enquanto o firmware ainda puder emiti-lo.

Nao pertencem ao SDCTP:

- Framing SDGW
- ACK/ERR SDGW
- COBS/CRC SDGW
- Regra de UI
- Decodificacao J1939, K-LINE ou outros protocolos automotivos de aplicacao

### SDGW

SDGW e o gateway/transporte puro entre API, BPM e UCE.

Pertencem ao SDGW:

- Framing
- COBS
- CRC
- ACK/ERR
- Sequencia
- Retry
- Timeout
- Transporte de TLVs
- Entrega de eventos

Nao pertencem ao SDGW:

- Regra CAN
- Regra J1939
- Regra K-LINE
- Decodificacao de massa CAN
- Decodificacao de PGN
- Regra de UI
- Regra de aplicacao

## 3. Fluxos oficiais

### Fluxo de controle/operação

```text
UI / Aplicacao
-> caso de uso BLL
-> comando SDH
-> Dispatcher UCE API
-> conversao SDH para TLV
-> SDGW
-> BPM gateway
-> UCE dispatcher
-> servico de hardware
-> resposta
```

### Fluxo de massa CAN RX

```text
Interface CAN UCE
-> CanService UCE
-> SDCTP
-> TLV de massa CAN
-> SDGW
-> Dispatcher UCE API
-> roteamento para CanService API / SDCTP
-> mirror table / CanRxOutputBuffer
-> aplicacao/UI consome CanFrameDto
```

## 4. Regras de fronteira

1. UI nao deve consumir TLV bruto.
2. UI nao deve conhecer SDGW para massa CAN.
3. UI deve consumir massa CAN por DTO/buffer de aplicacao.
4. SDGW nao deve conhecer CAN, J1939, K-LINE ou protocolos automotivos.
5. Dispatcher UCE deve rotear por dominio, nao concentrar regra de massa CAN.
6. SDH deve ser usado para operacao/configuracao de hardware.
7. SDCTP deve ser usado para massa/sincronizacao CAN.
8. CanService API deve ser o dono da mirror table e do CanRxOutputBuffer.
9. Decoders como J1939 devem consumir CanFrameDto, nunca TLV bruto.
10. Protocolos futuros devem entrar como dominios proprios, sem poluir SDGW.

## 5. Classificação inicial dos TLVs

| Codigo | Nome | Dominio oficial | Situacao | Observacao |
|---|---|---|---|---|
| `0x12` | LED | SDH / Controle | Congelado | Operacao de LED/service command |
| `0x20` | CAN_CONFIG | SDH / Controle | Congelado | Configura bitrate, modo e parametros de operacao CAN |
| `0x21` | CAN_ENABLE | SDH / Controle | Congelado | Habilita, desabilita, abre ou fecha CAN |
| `0x22` | CAN_STATUS | SDH / Controle | Congelado | Solicita estado atual do servico CAN |
| `0x23` | CAN_RESET | SDH / Controle | Congelado | Reseta servico/porta CAN |
| `0x25` | CAN_DRIVER_LOG_POLL | SDH / Controle | Congelado | Diagnostico operacional do driver CAN |
| `0x28` | CAN_RX_EVENT | SDCTP / Massa CAN | Congelado | Evento de frame CAN RX direto |
| `0x40` | CAN_CREATE | SDCTP / Massa CAN | Congelado | Criacao de entrada na mirror table RX |
| `0x41` | CAN_EDIT | SDCTP / Massa CAN | Congelado | Atualizacao compactada de entrada RX |
| `0x42` | CAN_DELETE | SDCTP / Massa CAN | Congelado | Remocao/invalidation de entrada RX |
| `0x44` | CAN_ROW | SDCTP / Massa CAN | Congelado | Linha de snapshot RX |
| `0x45` | CAN_READ_ALL_DONE | SDCTP / Massa CAN | Compatibilidade legada | Marcador recebido de conclusao de sincronizacao RX; nao deve depender de solicitacao nova de CAN_READ_ALL |
| `0x46` | CAN_TIC | SDCTP / Massa CAN | Congelado | Evento de vida/recorrencia de entrada RX |
| `0x24` | CAN_RX_POLL | Ambiguo / Pendente | Pendente | Poll legado de RX; fronteira oficial ainda nao congelada |
| `0x26` | CAN_TX | SDCTP / Massa CAN TX | Legado | TX legado; manter apenas enquanto houver compatibilidade com contrato antigo |
| `0x27` | CAN_TX_STOP | SDCTP / Massa CAN TX | Congelado | Parada de TX periodico |
| `0x43` | CAN_READ_ALL | Legado / Deprecated | Removido do fluxo principal | Nao deve ser usado por UI/API como contrato valido |
| `0x50` | CAN_TX_DIRECT | SDCTP / Massa CAN TX | Congelado | Envio CAN TX direto |
| `0x51` | CAN_TX_CREATE | SDCTP / Massa CAN TX | Congelado | Criacao de linha TX |
| `0x52` | CAN_TX_EDIT | SDCTP / Massa CAN TX | Congelado | Edicao compactada de linha TX |
| `0x53` | CAN_TX_DELETE | SDCTP / Massa CAN TX | Congelado | Remocao de linha TX |
| `0x7E` | TRANSPORT_DIAG | Diagnostico / Transporte | Congelado | Diagnostico de transporte/dispatcher |
| `0x7F` | FUNCTIONAL_ERROR | Diagnostico / Transporte | Congelado | Erro funcional retornado pela UCE |
| `0xFE` | GATEWAY_ERROR | Diagnostico / Transporte | Congelado | Erro gerado pelo gateway BPM |

## 6. Decisões congeladas

- Nome oficial: SDCTP.
- SDH e controle/operacao.
- SDCTP e massa/sincronizacao CAN.
- RX CAN pertence ao SDCTP.
- TX CAN pertence ao SDCTP.
- SDGW e transporte.
- UI nao consome TLV bruto.
- CanRxOutputBuffer e a saida oficial de massa CAN para aplicacao/UI.
- CanService API possui lado RX e lado TX.
- J1939 deve consumir CanFrameDto, nao TLV bruto.
- SDGW nao deve receber regra de protocolo automotivo.
- CAN_READ_ALL e legado e nao deve ser usado no fluxo principal.

## 7. Decisões pendentes

- Se UceClient deve apenas rotear payload bruto SDCTP ou continuar parseando eventos.
- Se UceParsers deve manter parsers CAN ou mover tudo para namespace SDCTP.
- Separacao futura entre CanControlService e SdctpCanDataService no firmware UCE.
- Camada futura para consumidores de protocolo automotivo, como J1939, K-LINE e J1949.

## 8. ETAPAS futuras sugeridas

ETAPA 02 - Separar fachada de controle CAN da fachada SDCTP na API.

ETAPA 03 - Reduzir conhecimento SDCTP dentro de UceClient/UceParsers.

ETAPA 04 - Consolidar SDCTP RX/TX e remover CAN_READ_ALL legado.

ETAPA 05 - Reduzir conhecimento SDCTP dentro de UceClient/UceParsers remanescente.

ETAPA 06 - Revisar mecanismo futuro de snapshot SDCTP sem CAN_READ_ALL.

ETAPA 07 - Remover dependencia da UI em GwProtocol onde for apenas apresentacao.

ETAPA 08 - Preparar camada de consumidores de protocolos automotivos.

ETAPA 09 - Avaliar separacao do CanService firmware em controle CAN e massa SDCTP.

## 9. Validação

Este documento executa somente a ETAPA 01 de congelamento arquitetural.

Validacoes aplicaveis:

- Nao altera codigo funcional.
- Nao renomeia classes.
- Nao move arquivos.
- Nao cria refatoracao.
- Nao altera contratos runtime.
- TX CAN foi congelado como SDCTP na ETAPA 04.
- CAN_READ_ALL foi declarado legado na ETAPA 04.

Ambiguidades registradas:

- Dispatcher UCE API ainda precisa de decisao futura sobre roteamento por payload bruto SDCTP versus parsing local.
- UceParsers ainda precisa de decisao futura sobre permanencia dos parsers CAN ou migracao para namespace SDCTP.
- Firmware UCE ainda precisa de decisao futura sobre separacao entre controle CAN e massa SDCTP.
- O mecanismo futuro de snapshot SDCTP sem `CAN_READ_ALL` ainda precisa ser especificado para recuperacao completa apos perda de mirror table.
