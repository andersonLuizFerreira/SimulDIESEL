# Levantamento tecnico - SPI BPM <-> UCE e direcao para backplane SPI robusto

Data: 2026-04-16  
Workspace: `C:\PROJETOS\SimulDIESEL`

## Escopo

Este relatorio reconstrui, com base no codigo real e na documentacao oficial relevante, como a comunicacao SPI entre BPM e UCE funciona hoje, onde estao as fragilidades do desenho atual e qual direcao arquitetural faz sentido se o SPI passar a ser o barramento principal entre a BPM e as baby boards.

Regras seguidas nesta entrega:

- nao foi implementada nenhuma mudanca de firmware
- nao foi presumido que firmware update so pode ocorrer via SPI
- a conclusao foi ancorada primeiro no que existe hoje no repositorio

## BLOCO 1 - EVIDENCIAS

### 1.1 Arquivos inspecionados

Arquitetura e hardware oficial:

- `docs/official/03-hardware/03-barramentos.md`
- `docs/official/04-firmware/06-gateway-binding-logico-fisico.md`
- `docs/official/04-firmware/boards/BPM/01-bpm.md`
- `docs/official/04-firmware/boards/UCE/11-uce.md`
- `docs/official/06-protocolos/07-uce-sdh-tlv.md`
- `docs/official/08-casos-de-uso/03-testes-bancada.md`
- `docs/official/10-testes/03-testes-integracao.md`
- `docs/official/12-documentacao-tecnica/03-contratos-software.md`

Firmware BPM:

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/main.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/Gateway/GatewayApp.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwTlv/GwTlv.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDiag/GwSpiDiagnostic.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwErr/GwErr.h`

Firmware UCE:

- `hardware/firmware/UCE - Unidade de comunicacao externa/include/config.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/app/UceApp.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/crc8.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/service/Service.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/service/Service.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/protocol/tlv/Tlv.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/protocol/tlv/Tlv.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/diag/trace/DiagTrace.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/diag/trace/DiagTrace.cpp`

Host e contrato de payload:

- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/GwProtocol.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs`

### 1.2 Trechos mais relevantes

| Evidencia | O que prova |
| --- | --- |
| `docs/official/03-hardware/03-barramentos.md:14-19` | Contrato fisico atual: SPI 18/26/25, CS 33, IRQ 27, reset 23 |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h:16-23` | Pinagem viva da BPM para SPI/CS/IRQ/reset da UCE |
| `hardware/firmware/UCE - Unidade de comunicacao externa/include/config.h:6-13` | Pinagem viva da UCE: CS em D10, IRQ em D2, IRQ ativo em LOW |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.cpp:7-10` | A UCE ja esta publicada como device remoto SPI com CS, IRQ e reset associados |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp:66-125` | Fluxo master atual: write, wait IRQ, read header em 2 bytes, read payload+CRC em segundo burst |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp:92-136` | BPM valida request TLV, roteia para SPI, valida CRC da resposta e converte falhas em `GWERR_*` |
| `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp:84-103` | A resposta da UCE so e considerada pronta quando foi copiada para `_txBuf`, o primeiro byte foi preloaded no TDR e a IRQ foi ativada |
| `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp:129-178` | No slave, o preload do proximo byte e feito em `RDRF`; o fim de burst e detectado em `NSSR` |
| `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.cpp:42-70` | A UCE trata o request como TLV cru, valida T/L/CRC, monta response TLV e adiciona CRC no final |
| `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs:131-181` | O payload enviado da BPM para a UCE ja sai do host como `TLV + CRC`, sem header SPI adicional |
| `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h:4` | Limite atual do transporte da UCE: `TLV_MAX_LEN = 32` |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDiag/GwSpiDiagnostic.h:16-34` | Causas diagnosticas ja reconhecidas para falhas SPI: timeout IRQ, preload failure, first-byte misaligned, incomplete frame, etc. |
| `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/main.cpp:33-48` | O reset 23 e apenas mantido em nivel inativo no boot; nao existe uso funcional do reset no fluxo da transacao SPI |
| `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp:11-15` + `lib/app/UceApp.cpp:37-44` | O parsing e a preparacao de resposta dependem do loop principal da UCE, nao da ISR sozinha |

### 1.3 Observacao importante sobre a documentacao

Existe documentacao oficial consistente com o codigo atual para a rota UCE por SPI, por exemplo:

- `docs/official/04-firmware/06-gateway-binding-logico-fisico.md:12-18`
- `docs/official/04-firmware/boards/UCE/11-uce.md:63-109`

Mas tambem existem paginas mais antigas ainda marcando o SPI do gateway como parcial, por exemplo:

- `docs/official/04-firmware/01-arquitetura-firmware.md:12-16`

Como o codigo atual publica a UCE em `GwDeviceTable.cpp:7-10`, o codigo foi tratado aqui como fonte de verdade.

## BLOCO 2 - DESENHO ATUAL CONSOLIDADO

### 2.1 Contrato fisico real hoje

Contrato confirmado entre BPM e UCE:

- `SCK`: BPM `GPIO18` -> UCE `SPI header SCK`
- `MISO`: BPM `GPIO26` <- UCE `SPI header MISO`
- `MOSI`: BPM `GPIO25` -> UCE `SPI header MOSI`
- `CS`: BPM `GPIO33` -> UCE `D10 / PA28 / NPCS0`
- `IRQ`: UCE `D2` -> BPM `GPIO27`
- `RESET`: BPM `GPIO23` -> reset fisico compartilhado da UCE e da GSA

Sinais obrigatorios alem do SPI puro:

- `CS` dedicado: obrigatorio, porque a UCE fecha o request e encerra cada burst pela borda de `NSSR`
- `IRQ` dedicado: obrigatorio no fluxo vivo, porque a BPM espera `IRQ` ativa antes de iniciar a leitura da response
- `RESET`: existe no contrato fisico, mas nao participa do fluxo normal da transacao; hoje e apenas mantido em nivel inativo no boot da BPM

Niveis e direcoes observaveis:

- `CS`: ativo em LOW; a UCE habilita pull-up no `NPCS0` nativo para manter repouso alto
- `IRQ`: ativo em LOW e idle em HIGH na UCE
- `MISO/MOSI/SCK`: direcao padrao de master/slave SPI

Sobre nivel logico eletrico:

- o repositorio confirma estados logicos HIGH/LOW e pull-up, mas nao formaliza numa pagina unica o nivel em volts da SPI BPM/UCE
- como BPM e UCE sao tratadas no codigo e na documentacao como ESP32 e Arduino Due, 3.3 V e a inferencia mais provavel, mas isso nao esta formalizado no contrato de SPI auditado aqui

### 2.2 Papel de cada lado

Responsabilidades da BPM:

- receber frame SDGW do host
- extrair o payload TLV interno ja pronto para a board remota
- resolver o destino logico em `GwDeviceTable`
- selecionar a board pelo `CS`
- controlar o clock
- esperar a `IRQ` da UCE
- ler a resposta em dois bursts
- validar `T/L/CRC`
- converter falhas do barramento em `GWERR_*` e, quando possivel, anexar diagnostico ao host

Responsabilidades da UCE:

- capturar bytes do master entre `CS LOW` e `NSSR`
- validar `T/L/CRC`
- despachar o `type` para `Service`
- construir a response funcional ou erro funcional `0x7F`
- acrescentar o CRC final
- preload do primeiro byte no `SPI_TDR`
- manter `IRQ` ativa enquanto a response estiver pendente
- servir os bytes da response conforme o master gerar clock

### 2.3 Framing SPI real usado hoje

O frame real no fio hoje nao e um protocolo SPI proprio; ele e o proprio `TLV + CRC`.

Formato generico do request no fio:

```text
[T][L][V0]...[Vn-1][CRC8]
```

Formato generico da response no fio:

```text
[T][L][V0]...[Vn-1][CRC8]
```

Onde:

- `T` = type funcional da UCE, por exemplo `0x12`, `0x20`, `0x21`, `0x22`, `0x23` ou `0x7F`
- `L` = tamanho do payload funcional
- `CRC8` = CRC-8/ATM sobre `T`, `L` e `V`

Relacao entre TLV e frame SPI:

- nao existe encapsulamento extra de transporte sobre SPI
- o payload que o host monta em `SdhToSdgwMapper` ja e exatamente o que a BPM escreve na MOSI
- a UCE responde com outro `TLV + CRC` cru

### 2.4 Sequencia real da transacao

Fluxo atual BPM -> UCE -> BPM:

1. O host monta o payload interno da UCE como `TLV + CRC`.
2. A BPM valida esse pacote com `GwTlv::validatePacket(...)`.
3. A BPM faz um burst de escrita SPI unico com `CS LOW`, enviando todos os bytes do request.
4. A UCE captura esses bytes em `_rxWorkBuf` enquanto `_txPending == false`.
5. No `NSSR`, a UCE fecha o request e marca `_rxPending = true`.
6. No loop principal, `Link::poll()` consome o request, valida o TLV, chama `Service`, monta a response e chama `Transport::setTx(...)`.
7. `Transport::setTx(...)` copia a response para `_txBuf`, preload o primeiro byte em `SPI_TDR` e ativa `IRQ`.
8. A BPM espera `IRQ` ir para LOW.
9. A BPM executa o primeiro burst de leitura, com MOSI `00 00`, para ler apenas `[T][L]`.
10. A BPM usa o `L` recebido para decidir quantos bytes faltam.
11. A BPM executa o segundo burst de leitura, com MOSI preenchida por zeros, para ler `[V...][CRC]`.
12. A BPM valida `T/L/CRC` do pacote recebido.
13. No `NSSR` final da response, a UCE limpa o estado TX e desativa a `IRQ`.

### 2.5 O frame atual no fio, burst por burst

Request:

```text
CS = LOW
MOSI = [T][L][V...][CRC]
MISO = ignorado pela BPM
CS = HIGH
```

Espera de prontidao:

```text
IRQ = HIGH enquanto nao ha resposta pronta
IRQ = LOW quando a response ja esta staged e o primeiro byte ja foi preloadado
```

Response burst 1:

```text
CS = LOW
MOSI = 00 00
MISO = [T][L]
CS = HIGH
```

Response burst 2:

```text
CS = LOW
MOSI = 00 00 00 ... 00   ; quantidade = L + 1
MISO = [V0][V1]...[Vn-1][CRC]
CS = HIGH
```

Conclusoes diretas sobre framing:

- existe tamanho explicito apenas no campo `L`
- nao existe campo de tamanho total
- nao existe `version`
- nao existe `transaction id`
- nao existe `ACK/NACK` do transporte SPI
- nao existe campo de status de transporte separado do payload funcional
- nao existe token de prontidao no proprio frame; a prontidao e externa, pela `IRQ`

### 2.6 Modelo de sincronismo real

Quem inicia:

- a BPM sempre inicia a transacao

Quem controla clock:

- a BPM, como master SPI

Como o slave indica que a resposta esta pronta:

- a UCE baixa a `IRQ` so depois de copiar toda a response para `_txBuf`, preloadar o primeiro byte e entrar em modo de envio

O que acontece se a BPM ler cedo demais:

- com `IRQ` habilitada, a BPM fica esperando ate `timeoutMs` e falha com `GWERR_TIMEOUT`
- sem `IRQ`, o proprio codigo da BPM admite o risco e so faz `delay(1)` antes da leitura; nesse caso o diagnostico pode cair em `CauseEarlyReadBeforeResponseReady`

O que acontece se a UCE ainda nao tiver preloadado o proximo byte:

- o comentario do proprio `Transport.cpp` diz que o ultimo byte do burst pode sair como lixo ou zero se o preload atrasar
- por isso o firmware atual tenta preloadar o proximo byte em `RDRF`, imediatamente apos o byte atual terminar

Acoplamento temporal observado:

- a BPM depende do tempo entre o `NSSR` do request e a proxima chamada de `Link::poll()` na UCE
- a response em dois bursts depende de a UCE manter o pipeline de preload consistente atraves da quebra de `CS`
- portanto o desenho atual e funcional, mas temporalmente acoplado

### 2.7 Tratamento de erros e integridade hoje

O que ja existe:

- validacao `T/L/CRC` na BPM para request e response
- validacao `T/L/CRC` na UCE para request
- erro funcional da UCE `0x7F` para payload invalido, comando nao suportado, estado invalido e CRC invalido do request
- erro de gateway da BPM `0xFE` com diagnostico estendido para falhas SPI/CRC
- timeout de roteamento na BPM: `SDGW_GATEWAY_ROUTE_TIMEOUT_MS = 100`
- espera explicita por `IRQ` na BPM quando `spiUseIrq == true`
- diagnostico SPI da BPM com fase, causa, comprimentos e bytes capturados
- rastros locais na UCE via `DiagTrace` em `SerialUSB`

O que a BPM reconhece explicitamente:

- timeout aguardando IRQ
- frame incompleto
- mismatch de comprimento esperado vs recebido
- primeiro byte desalinhado
- falha possivel de preload
- CRC invalido na response

O que nao existe como tratamento real no barramento SPI:

- retry automatico BPM <-> UCE
- retransmissao por `seq` entre BPM e UCE
- nack do transporte SPI
- reset automatico da board apos falha
- fila de requests na UCE
- deteccao explicita de overrun/underrun
- negociacao de capacidades ou versao do frame

## BLOCO 3 - PONTOS FRAGEIS

### 3.1 Fragilidades de protocolo

1. O transporte SPI atual praticamente nao existe como camada separada; o TLV funcional esta sendo usado como frame de transporte.

2. A response em dois bursts nao tem amarracao estrutural entre os bursts. O segundo burst confia que:

- o `L` do primeiro burst esta correto
- o pipeline de preload da UCE seguiu integro
- o byte staged ao final do burst anterior continua valendo no proximo burst

3. Nao existe `transaction id`, `seq`, `version`, `capability`, `status` de transporte ou `ACK/NACK` do barramento SPI.

4. Nao existe protecao de protocolo contra dessintonia entre master e slave alem de `L` e `CRC`. Isso detecta erro, mas nao organiza recuperacao.

5. Nao existe framing de bloco, fragmentacao ou confirmacao por bloco. Para firmware update isso e uma lacuna estrutural, nao um detalhe.

### 3.2 Fragilidades de implementacao

1. A preparacao da response depende do loop principal da UCE, nao apenas da ISR. Portanto o tempo real entre request e response depende de quanto a UCE demora para voltar a `Link::poll()`.

2. A UCE tem somente um buffer RX e um buffer TX, ambos com limite de `32` bytes. Nao ha fila. Isso serve para comandos curtos, mas nao para operacoes maiores.

3. Se um request maior que `TLV_MAX_LEN` chegar, o transporte da UCE trunca a captura silenciosamente no ISR. O erro funcional que sai depois e generico; nao ha codigo de overrun do transporte.

4. Se `Tlv::build(...)` nao conseguir montar a response por limite de tamanho, `txTlvLen` pode ficar zero e a BPM vai interpretar como timeout por ausencia de resposta. Hoje isso nao aparece nos comandos curtos atuais, mas bloqueia extensoes mais pesadas.

5. `GwSpiBus::transact(...)` retorna `false` para varios casos diferentes, mas `GwRouter::route(...)` converte a maioria deles em `GWERR_TIMEOUT`. Na pratica, falta separacao entre:

- timeout aguardando IRQ
- leitura impossivel por header incoerente
- resposta maior que o buffer do chamador
- device nao pronto

6. O reset existe na tabela e na pinagem, mas nao ha politica de recovery usando reset no fluxo SPI. Na pratica, o reset e um recurso fisico sem orquestracao no protocolo.

### 3.3 Fragilidades para sincronismo

1. O modelo atual depende de `IRQ` fora do frame e de `CS` como delimitador de burst. Sem `IRQ`, o proprio codigo da BPM cai para `delay(1)`, o que e fragil.

2. O preload do slave e critico. O comentario em `Transport.cpp:137-139` deixa isso explicito: se o preload atrasar, sai lixo/zero.

3. O primeiro burst de leitura da response ja consome bytes reais da UCE. Nao existe um `header fixo de transporte` com magic/status/len total que permita re-sincronizar de forma mais forte.

### 3.4 Fragilidades para escalabilidade

1. O contrato atual serve bem para TLVs curtos e sincronos, mas nao para um barramento comum de backplane com multiplas boards e operacoes mais pesadas.

2. O transporte atual nao diferencia claramente:

- payload funcional da board
- controle do barramento
- diagnostico
- recuperacao

3. O modelo da UCE nao oferece evento assincrono proprio, mailbox de notificacao ou canal de progresso.

4. O protocolo nao tem nada que ajude a retomar operacao apos falha parcial, reentrada em bootloader ou transferencia longa interrompida.

## BLOCO 4 - AVALIACAO DE ESCALABILIDADE

### 4.1 O que ja esta bom e deve ser preservado

1. A infraestrutura fisica de barramento ja aponta para um backplane SPI valido:

- linhas SPI compartilhaveis
- `CS` dedicado por board
- `IRQ` dedicado por board
- reset fisico disponivel

2. O gateway ja tem um ponto unico de roteamento por endereco logico em `GwDeviceTable` + `GwRouter`.

3. O uso de `TLV + CRC8` para payload funcional curto e bom e simples para comandos de configuracao, status e operacoes pequenas.

4. O diagnostico da BPM para falhas SPI ja e um ativo forte e nao deveria ser perdido.

5. O padrao "host resolve semantica, BPM apenas roteia" tambem esta bom e deve ser preservado.

### 4.2 O que hoje serve so para a UCE atual

1. A ideia de que o proprio payload funcional da board ja e o frame do barramento.

2. A response em dois bursts guiada so por `L` e `IRQ`.

3. O limite fisico e logico de `32` bytes na UCE.

4. A ausencia de um header comum de transporte SPI para todas as boards.

5. A dependencia de que toda operacao remota relevante seja curta e sincrona.

### 4.3 O modelo atual serve como barramento mestre comum?

Para comandos curtos, sim, parcialmente:

- LED
- configuracoes pequenas
- leituras pequenas de status

Como padrao de backplane SPI para todas as baby boards, nao ainda.

Motivo:

- ele ainda nao separa transporte de payload
- nao suporta crescimento previsivel para blocos maiores
- nao tem mecanismos de transporte para recuperacao, fragmentacao ou retomada
- nao tem envelope comum que permita versao e extensao sem quebrar as boards

### 4.4 GSA futura em SPI e outras baby boards

Se a GSA migrar para SPI mantendo apenas o modelo atual da UCE, o projeto ganharia padronizacao fisica, mas nao ganharia automaticamente um protocolo de backplane robusto.

O que precisaria existir para virar padrao de backplane:

- um header comum de transporte SPI
- separacao entre status de transporte e payload funcional
- forma padronizada de sinalizar prontidao, erro e comprimento total
- regras comuns para timeout, recovery e reset coordenado
- estrategia para payloads maiores que os TLVs curtos atuais

## BLOCO 5 - DIRECAO RECOMENDADA

### 5.1 Resposta objetiva as 10 perguntas

1. Como exatamente a SPI BPM <-> UCE funciona hoje?

- A BPM envia um request `TLV + CRC` bruto em um burst SPI.
- A UCE captura esse request por `NSSR`, processa no loop principal, monta a response, preloada o primeiro byte no `SPI_TDR` e baixa `IRQ`.
- A BPM espera `IRQ`, le `[T][L]` em um primeiro burst e depois le `[V...][CRC]` em um segundo burst.

2. Qual e o frame real atual no fio?

- O frame real e `TLV + CRC8`.
- Request: `[T][L][V...][CRC]`
- Response: `[T][L][V...][CRC]`
- A response e fisicamente lida em dois bursts: primeiro `T/L`, depois `V/CRC`.

3. Onde estao os pontos frageis do desenho atual?

- response em dois bursts sem header de transporte proprio
- dependencia de `IRQ` fora do frame
- dependencia de preload exato no slave
- ausencia de `seq`, `version`, `ACK/NACK`, `status` de transporte
- buffers unicos e pequenos na UCE
- ausencia de retry e recovery BPM <-> UCE

4. O problema atual parece ser de implementacao ou de protocolo?

- dos dois tipos
- ha fragilidades reais de implementacao no preload, no acoplamento com o loop principal e na classificacao de erros
- mas tambem ha lacunas reais de protocolo para qualquer expansao seria do barramento SPI

5. O desenho atual e suficiente para continuar sendo expandido?

- para comandos curtos, sim
- para virar o barramento principal de backplane, nao

6. O modelo atual serve como base para um padrao SPI comum entre BPM e todas as boards?

- serve como base fisica e como prova de conceito funcional
- nao serve, do jeito que esta, como padrao final de transporte SPI comum

7. O protocolo atual e adequado para futura atualizacao de firmware?

- nao diretamente
- ele nao oferece fragmentacao, confirmacao por bloco, retomada, controle de estado bootloader/aplicacao nem recovery robusto

8. Qual deve ser a direcao correta?

- corrigir so a implementacao atual nao basta para o objetivo arquitetural declarado
- a direcao correta e dupla:
- primeiro, estabilizar a implementacao BPM/UCE atual sem quebrar o contrato vigente
- depois, definir uma camada de transporte SPI de backplane mais robusta sobre a infraestrutura fisica existente

9. Quais partes da solucao atual devem ser preservadas obrigatoriamente?

- infraestrutura fisica SPI + CS dedicado + IRQ dedicado + reset disponivel
- roteamento por `GwDeviceTable` e `GwRouter`
- `TLV + CRC8` como payload funcional curto
- diagnostico SPI da BPM
- principio de a BPM continuar sendo roteador e nao regra de negocio da board

10. Quais mudancas futuras parecem inevitaveis se o SPI virar o barramento padrao?

- separar transporte SPI de payload funcional
- incluir metadata minima de transporte
- definir politica de timeout/retry/recovery/reset
- padronizar transferencia de blocos e retomada
- padronizar bootloader/aplicacao e reentrada segura
- ampliar buffers e politica de filas nas boards SPI

### 5.2 Avaliacao especifica para firmware update

O repositorio atual nao mostra um caminho implementado de firmware update para a UCE nem por SPI nem por outro barramento. Portanto:

- nao ha base para afirmar que firmware update sera obrigatoriamente via SPI
- tambem nao ha base para afirmar que ja existe outro caminho pronto

Sobre adequacao do desenho SPI atual para esse uso:

- transferencia de blocos maiores: nao
- confirmacao por bloco: nao
- retomada apos erro: nao
- integridade por bloco: nao
- controle de estado bootloader/aplicacao: nao
- reset coordenado: so fisicamente disponivel; nao orquestrado
- timeout robusto para operacao longa: nao
- reentrada segura apos falha: nao

Conclusao objetiva:

- o desenho atual nao suporta firmware update diretamente
- pequenas extensoes nao parecem suficientes se o objetivo for algo realmente robusto
- o mais coerente e criar uma camada de transporte SPI de backplane sobre a infraestrutura fisica atual e manter o `TLV` curto como payload funcional quando fizer sentido

### 5.3 Recomendacao arquitetural

Recomendacao tecnica:

1. Curto prazo: tratar o problema atual como um problema real de implementacao BPM/UCE e fechar a robustez do fluxo ja existente.

2. Medio prazo: parar de tratar `TLV + CRC` como se ja fosse o protocolo de barramento SPI.

3. Proximo passo arquitetural correto: definir um protocolo SPI de backplane mais robusto sobre a infraestrutura existente de `SPI + CS + IRQ + reset`, preservando:

- pinagem fisica
- tabela de devices
- payload funcional TLV onde ele ja funciona bem
- diagnostico e roteamento atuais

Em outras palavras:

- para corrigir o problema atual imediato, existe trabalho de implementacao
- para cumprir a meta arquitetural do projeto, existe trabalho inevitavel de protocolo

## BLOCO 6 - IMPACTO NO PROJETO

### 6.1 Firmware BPM

Arquivos diretamente impactados por uma futura evolucao do protocolo SPI:

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDeviceTable/GwDeviceTable.*`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDiag/GwSpiDiagnostic.h`

Impactos esperados:

- novo framing de transporte
- nova classificacao de erros
- novos timeouts ou retries internos
- possivel uso real do reset por board
- suporte a multiplas boards SPI vivas

### 6.2 Firmware UCE

Arquivos diretamente impactados:

- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/service/Service.*`
- `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/include/config.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/diag/trace/DiagTrace.*`

Impactos esperados:

- ampliacao de buffers
- revisao do sincronismo TX/RX
- possivel separacao entre transporte SPI e payload funcional
- tratamento explicito de oversize, abort, retry e recovery
- eventual suporte a bootloader/aplicacao

### 6.3 Host e software local

Arquivos potencialmente impactados:

- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDGW/GwProtocol.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Protocols/SDGW/SdhToSdgwMapper.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs`

Impactos esperados:

- adaptacao dos parsers da UCE
- possivel envelope novo para erro de transporte
- possivel separacao entre resposta funcional curta e transacoes longas

### 6.4 Documentacao, testes e futuras boards

Tambem seriam impactados:

- contratos oficiais de protocolo da UCE
- testes de bancada e de integracao
- futura migracao da GSA para SPI
- projeto de quaisquer baby boards SPI novas

## Conclusao final

Decisao com alta confianca com base no codigo atual:

- se o objetivo fosse apenas estabilizar LED/CAN da UCE no estado atual, daria para tratar como correcao de implementacao sobre o desenho existente
- mas esse nao e o objetivo arquitetural declarado
- para transformar o SPI no barramento principal do backplane, inclusive com horizonte de operacoes mais pesadas, o projeto precisara evoluir de `TLV cru no fio` para uma camada de transporte SPI de backplane mais robusta, preservando a infraestrutura fisica e o payload funcional ja consolidados

Sintese da recomendacao:

- **nao parar em correcao de implementacao**
- **preservar SPI/CS/IRQ/reset, roteamento e TLV funcional**
- **evoluir para um protocolo SPI de backplane robusto sobre a infraestrutura existente**
