# Comissionamento SPI BPM-UCE - LED Handshake

Data: 2026-04-24
Branch: `feature/uce-foundation`

## Estado validado em bancada

Foi validado com sucesso o fluxo SPI entre BPM ESP32 master e UCE Arduino Due slave para o comando do LED builtin da UCE.

O problema original nao era o clock SPI de `1 MHz` por si so. O problema era o primeiro byte da resposta da UCE nao estar pre-carregado no `SPI_TDR` antes da BPM continuar gerando clocks para leitura. Isso fazia o byte `0x12` sumir do burst e a BPM acusar timeout/CRC por desalinhamento.

## Solucao validada

O fluxo funcional atual ficou assim:

1. A BPM baixa `CS`.
2. A BPM envia o request TLV+CRC do LED: `12 01 00/01 CRC`.
3. A BPM para de gerar clock, mas mantem `CS = LOW`.
4. A UCE recebe o request completo.
5. A UCE processa o comando.
6. A UCE acende/apaga o `LED_BUILTIN`.
7. A UCE monta a resposta.
8. A UCE pre-carrega o primeiro byte da resposta (`0x12`) no `SPI_TDR`.
9. A UCE coloca `IRQ = LOW` apenas quando a resposta estiver pronta para transmissao.
10. A BPM detecta `IRQ = LOW`.
11. A BPM gera os clocks de leitura da resposta.
12. Ao final da sessao, a UCE volta `IRQ = HIGH`.

Esse fluxo foi testado com a SPI da BPM novamente em `1 MHz` e continuou funcionando. Portanto, o fator decisivo foi o handshake, nao a reducao do clock.

## Arquivos principais do estado funcional

### BPM

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/src/main.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`

### UCE

- `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/include/config.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/led/LedService.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/services/led/LedService.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/src/main.cpp`

### API local / diagnostico

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceParsers.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceGatewayDiagnosticLog.cs`

## O que esta implementado

- Mapeamento fisico SPI BPM-UCE restabelecido.
- Handshake com `IRQ` no caminho BPM<->UCE para leitura da resposta.
- `CS` mantido em `LOW` entre o envio do request e a leitura da resposta.
- Diagnostico estendido da BPM com captura do burst cru SPI para o caso de falha.
- Caso funcional implementado apenas para o comando do LED builtin da UCE.

## O que NAO esta implementado ainda

- Generalizacao do protocolo SPI para outros comandos TLV da UCE.
- Camada SPI definitiva com estados/erros padronizados para toda a board.
- Tratamento completo de diagnosticos de transporte da UCE.
- Servicos CAN da UCE reintroduzidos sobre essa nova base SPI.

## Proximo passo recomendado

O proximo Codex deve partir deste ponto e transformar o handshake validado em bancada em uma implementacao consolidada do transporte SPI da UCE, sem perder o comportamento abaixo:

- `IRQ = HIGH` enquanto a resposta nao estiver pronta.
- `IRQ = LOW` somente depois do preload do primeiro byte da resposta.
- BPM nao deve clockar a resposta antes de `IRQ = LOW`.
- `CS` deve permanecer em `LOW` durante essa espera.

Depois disso, expandir do caso do LED para os demais comandos da UCE.

## Observacao importante para continuidade

Se o proximo Codex abrir este arquivo, o entendimento correto do ponto atual e:

"A implementacao minima do LED com handshake SPI BPM-UCE esta funcional em bancada inclusive com clock SPI de 1 MHz. O problema de preload do primeiro byte foi contornado com CS baixo + espera por IRQ baixo antes do burst de leitura. A partir daqui devemos consolidar esse protocolo e ampliar a cobertura de comandos."
