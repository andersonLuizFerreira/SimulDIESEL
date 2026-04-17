# Comissionamento completo - Parte A - correcao do SPI BPM <-> UCE

Data de consolidacao: 2026-04-17  
Referencia de estado: 2026-04-16  
Workspace: `C:\PROJETOS\SimulDIESEL`

## BLOCO 1 - OBJETIVO

Este comissionamento consolida o estado atual da **Parte A - correcao do enlace SPI BPM <-> UCE**.

Esta etapa registra a correcao do bug atual de implementacao do enlace SPI existente, sem misturar esta entrega com a evolucao arquitetural futura.

Fica explicito que esta entrega:

- trata do bug atual de implementacao do SPI BPM <-> UCE
- nao trata ainda do novo protocolo SPI de backplane
- mantem separada a futura evolucao arquitetural da Parte B

## BLOCO 2 - BASES OFICIAIS RELACIONADAS

Documentos-base encontrados no workspace e usados como referencia desta consolidacao:

- `C:\PROJETOS\SimulDIESEL\out\dumps\levantamento-tecnico-spi-bpm-uce-backplane-2026-04-16.md`
- `C:\PROJETOS\SimulDIESEL\out\dumps\decisao-tecnica-spi-atual-e-protocolo-backplane-2026-04-16.md`

Esses documentos foram considerados a base formal da separacao entre:

- Parte A: correcao do SPI atual
- Parte B: definicao futura do protocolo SPI de backplane

## BLOCO 3 - ARQUIVOS ALTERADOS

### UCE

- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\lib\core\transport\Transport.cpp`

### BPM

- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwSpiBus\GwSpiBus.cpp`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwErr\GwErr.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.h`
- `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\lib\GwRouter\GwRouter.cpp`

## BLOCO 4 - RESUMO TECNICO DAS ALTERACOES

### UCE

No transporte SPI da UCE, o envio da response deixou de depender de um TX implicito baseado em indice/pending residual e passou a operar com estado explicito de transmissao.

Mudancas consolidadas:

- abandono do TX implicito anteriormente dependente de continuidade residual entre bursts
- criacao de TX explicito com `_txSentCount`, `_txActive`, `_txPrimed` e `_txPrimedByte`
- `_txSentCount`: contador de bytes efetivamente enviados ao longo da response inteira
- `_txActive`: indica que existe uma response em voo e impede reutilizacao prematura do buffer de TX
- `_txPrimed`: indica que o proximo byte ja foi armado no `SPI_TDR`
- `_txPrimedByte`: guarda qual byte foi explicitamente prearmado para diagnostico e rearme consistente
- manutencao da response ativa entre bursts: o estado de TX permanece valido ate todos os bytes serem enviados
- tratamento deliberado de `NSSR`: ao fim de burst, se a response ainda nao terminou, o primeiro byte pendente e rearmado explicitamente para o burst seguinte
- bloqueio de `setTx(...)` enquanto ha TX em voo, evitando sobreposicao de responses
- blindagem da MOSI dummy: durante TX ativo, os bytes recebidos do master sao tratados apenas como clock dummy e nao como novo request

Efeito tecnico pretendido na UCE:

- remover a dependencia de timing implito na sobrevivencia do byte ja carregado entre bursts
- manter integridade da response atual ate o ultimo byte
- evitar que dummy bytes do segundo burst contaminem a recepcao do request

### BPM

No lado da BPM, o barramento SPI passou a desconfiar explicitamente do header e do comprimento antes de continuar a leitura da response.

Mudancas consolidadas:

- validacao do header antes do segundo burst
- rejeicao imediata de `header` com primeiro byte invalido
- endurecimento do campo `L`, com checagens de minimo, maximo e compatibilidade com `rxMax`
- remocao da confianca cega no `L`: o segundo burst so ocorre depois de a BPM considerar o header coerente
- classificacao explicita de falhas de transacao em `TimeoutWaitingIrq`, `HeaderInvalid`, `LengthInvalid` e `FrameIncomplete`
- ampliacao dos codigos `GwErr` para preservar causas distintas do barramento SPI
- melhoria da propagacao da causa no roteador, preservando o snapshot do barramento quando a falha ja chega qualificada
- consolidacao do diagnostico de CRC no roteador, incluindo `expectedLen`, `receivedLen`, `crcCalculated`, `crcReceived` e inferencia da causa mais provavel

Efeito tecnico pretendido na BPM:

- evitar leitura cega de payload quando o header ja nasceu inconsistente
- separar timeout, header invalido, comprimento invalido e frame incompleto
- melhorar rastreabilidade do erro retornado ao host e aos dumps diagnosticos

## BLOCO 5 - ANALISE CONSOLIDADA DO ESTADO

Este estado representa uma correcao pontual e focada no enlace SPI atual BPM <-> UCE.

O que este estado faz:

- estabiliza o comportamento do slave UCE na resposta em dois bursts
- torna a BPM mais defensiva antes de consumir payload e CRC
- melhora a classificacao e a propagacao de falhas de transacao SPI
- prepara o link atual para reteste real em hardware

O que este estado deliberadamente nao faz:

- nao cria um novo protocolo SPI de backplane
- nao altera o contrato `TLV + CRC8`
- nao altera a arquitetura SDGW
- nao altera a rota logica da UCE dentro do gateway
- nao remove `CS`
- nao remove `IRQ`

Em resumo tecnico, a Parte A corrige fragilidades de implementacao do transporte SPI atual, preservando o protocolo e a arquitetura existentes, para permitir reteste de bancada com melhor previsibilidade e melhor diagnostico.

## BLOCO 6 - BUILDS EXECUTADOS

Observacao de rastreabilidade: este bloco registra o estado de build ja validado nesta linha de trabalho. Os builds nao foram rerodados durante a geracao deste comissionamento.

### UCE

- diretorio de build: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa`
- ambiente: `dueUSB`
- comando executado na validacao informada desta rodada: `platformio run`
- referencia do ambiente: `C:\PROJETOS\SimulDIESEL\hardware\firmware\UCE - Unidade de comunicacao externa\platformio.ini`
- resultado conhecido: `OK`

### BPM

- diretorio de build: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE`
- ambiente: `esp32dev`
- comando executado na validacao informada desta rodada: `platformio run`
- referencia do ambiente: `C:\PROJETOS\SimulDIESEL\hardware\firmware\BPM - BACKPLANE MANAGER MODULE\platformio.ini`
- resultado conhecido: `OK`

## BLOCO 7 - VALIDACAO FUNCIONAL

### 1. Validacao logica / build

Validado nesta rodada:

- inspecao tecnica dos arquivos alterados da UCE e da BPM
- confirmacao de que a Parte A atua no transporte atual sem alterar o protocolo `TLV + CRC8`
- confirmacao de que a BPM agora valida `header` e `L` antes do segundo burst
- confirmacao de que a UCE mantem o estado de TX entre bursts e rearma explicitamente o proximo inicio de envio em `NSSR`
- build da UCE no ambiente `dueUSB`: `OK`
- build da BPM no ambiente `esp32dev`: `OK`

### 2. Validacao fisica em bancada

Nao executado nesta rodada:

- validacao fisica completa em bancada do enlace BPM <-> UCE
- repeticao funcional real dos comandos LED e CAN com observacao de estabilidade
- confirmacao em hardware de ausencia de `CRC` invalido, `header` invalido espurio ou frame truncado

Conclusao deste bloco:

- a implementacao da Parte A esta concluida em codigo
- os builds estao `OK`
- a confirmacao funcional final em hardware real continua pendente

## BLOCO 8 - ROTEIRO DE VALIDACAO FISICA PENDENTE

Sequencia recomendada de bancada:

1. LED on/off repetido
2. CAN status repetido
3. CAN enable on/off repetido
4. CAN config alternando bitrate e modo

O que observar em cada etapa:

### 1. LED on/off repetido

- resposta correta e coerente com o comando enviado
- comportamento repetivel em repeticoes consecutivas
- ausencia de `CRC` invalido
- ausencia de `header` invalido
- ausencia de frame truncado

### 2. CAN status repetido

- leitura coerente do estado do CAN em repeticoes sucessivas
- ausencia de variacao espuria entre leituras equivalentes
- ausencia de `CRC` invalido
- ausencia de `header` invalido
- ausencia de frame truncado

### 3. CAN enable on/off repetido

- transicoes corretas entre habilitado e desabilitado
- resposta consistente a cada alternancia
- ausencia de falha intermitente apos repeticoes
- ausencia de `CRC` invalido
- ausencia de `header` invalido
- ausencia de frame truncado

### 4. CAN config alternando bitrate e modo

- aplicacao correta da configuracao solicitada
- resposta coerente para cada combinacao exercitada
- comportamento repetivel apos varias alternancias
- ausencia de `CRC` invalido
- ausencia de `header` invalido
- ausencia de frame truncado

Complemento recomendado de observacao:

- se houver recorrencia de erro, registrar dump da BPM com os bytes de header e payload recebidos
- se o erro persistir de forma intermitente, observar a response byte a byte na bancada para confirmar alinhamento entre os dois bursts

## BLOCO 9 - CRITERIOS DE ACEITE

A Parte A so deve ser considerada validada em bancada se todos os criterios abaixo forem satisfeitos:

- zero `CRC` invalido
- zero frame truncado
- zero `header` invalido espurio
- zero falha intermitente nos testes repetidos
- resposta consistente nos testes de LED
- resposta consistente nos testes de CAN

## BLOCO 10 - LIMITES DESTA ENTREGA

Nao foi feito nesta rodada:

- nao foi criado protocolo SPI de backplane novo
- nao foi alterado o `TLV + CRC8`
- nao foi alterada a rota da UCE
- nao foi alterada a arquitetura SDGW
- nao foi implementado retry SPI
- nao foi implementado header de transporte novo
- nao foi feita validacao fisica completa em bancada
- a Parte B continua pendente

## BLOCO 11 - RISCOS / PENDENCIAS

Pendencias e riscos registrados de forma objetiva:

- a principal pendencia continua sendo a validacao fisica real em bancada
- o bug pode estar efetivamente corrigido em codigo, mas isso ainda nao esta comprovado em hardware
- como a falha historica era associada a temporizacao e fronteira entre bursts, a comprovacao final depende do comportamento no enlace fisico real
- se ainda houver `CRC` invalido em bancada, o proximo foco deve ser a analise byte a byte do header e da response durante os dois bursts SPI
- se necessario, a investigacao deve priorizar captura de bytes e temporizacao no barramento, e nao mudancas arquiteturais prematuras

## BLOCO 12 - STATUS FINAL

Status consolidado desta Parte A:

- implementacao da Parte A: `OK`
- builds BPM/UCE: `OK`
- documentacao de base: `OK`
- validacao fisica: `PENDENTE`
- pronto para retomada em outra maquina: `SIM`

Leitura objetiva do estado:

- o codigo da Parte A esta fechado para esta rodada
- o estado esta suficientemente documentado para retomada tecnica em outra maquina ou equipe
- a liberacao funcional final ainda depende da bancada

## BLOCO 13 - AVALIACAO DE VERSIONAMENTO

Avaliacao objetiva deste estado:

- condicao para checkpoint local: `SIM`
- condicao para commit tematico: `SIM`, desde que a mensagem deixe explicito que a validacao fisica ainda esta pendente
- condicao para congelar como entrega concluida e encerrada: `NAO`

Recomendacao de versionamento:

- este estado ja comporta um commit tematico ou checkpoint local de consolidacao da Parte A
- esse congelamento deve ser tratado como estado implementado e compilando, mas ainda nao homologado em bancada
- nao ha recomendacao para marcar esta entrega como concluida definitiva antes da validacao fisica

## ENCERRAMENTO

Este documento consolida o estado atual da Parte A de forma portatil e rastreavel, suficiente para retomada em outra maquina sem perda do contexto tecnico principal:

- o que mudou
- por que mudou
- o que ja foi validado
- o que ainda falta validar
- como executar a bancada pendente
- quais sao os criterios de aceite
