# Dump técnico - SPI BPM <-> UCE - 2026-04-24

## Caminho do dump

- Caminho solicitado pelo pedido: `C:\PROJETOS\SimulDIESEL\out\dumps`
- Caminho real disponível neste ambiente: `G:\PROJETOS\SIMULADORES\SimulDIESEL\out\dumps`
- Motivo: `C:\PROJETOS\SimulDIESEL\out\dumps` não existe no workspace atual.

## Arquivos alterados

- `hardware/firmware/UCE - Unidade de comunicacao externa/include/defs.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/service/Service.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/service/Service.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/diag/trace/DiagTrace.h`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/diag/trace/DiagTrace.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/include/SdgwDefs.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwDiag/GwSpiDiagnostic.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.h`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`
- `tests/commissioning_spi_v1/simulate_spi_handshake_log.mjs`
- `tests/commissioning_spi_v1/simulate_spi_handshake_log.py`

## Resumo técnico da mudança

- A UCE deixou de depender de polling em `PIO_PDSR` para detectar `CS LOW` no fluxo principal. O início de sessão agora é disparado por interrupção de borda de descida registrada no `NPCS0/PA28`, mantendo a pinagem original.
- O request e a resposta foram desacoplados em duas sessões SPI: uma de escrita do request e outra de leitura da resposta.
- A resposta da UCE fica staged em `_txBuf` até a sessão de leitura. O primeiro byte só é liberado quando o `CS` da leitura cai, o `SPI_TDR` é preloadado e então a `IRQ` vai para LOW contínuo.
- A UCE passou a emitir pulso `IRQ 1-0-1` quando há resposta pendente.
- A BPM passou a:
  - escrever o request;
  - aguardar o pulso/atenção de `IRQ`;
  - baixar `CS` para a leitura;
  - esperar `IRQ LOW` antes do primeiro clock;
  - abortar se `IRQ` subir antes do fim esperado;
  - pedir diagnóstico em clock seguro;
  - reduzir clock em 10% quando o diagnóstico indicar erro compatível com clock excessivo;
  - persistir o clock validado em `Preferences` (`gwspi/uce_hz`).
- Foi adicionado o comando de diagnóstico `CMD_TRANSPORT_DIAG (0x7E)` para a BPM consultar o último erro de transporte da UCE.

## Fluxo BPM

1. Monta o TLV com CRC, se necessário.
2. Faz um burst de escrita do request com `CS LOW`.
3. Solta `CS` e aguarda atenção da UCE via `IRQ`.
4. Ao detectar atenção, baixa `CS` para a sessão de leitura.
5. Espera `IRQ LOW` antes de clockar o header.
6. Lê `T` e `L`.
7. Continua lendo payload + CRC apenas enquanto `IRQ` permanecer LOW.
8. Se `IRQ` subir antes do comprimento esperado, aborta, descarta o frame e solicita diagnóstico em `BPM_SPI_SAFE_CLOCK_HZ`.
9. Se o diagnóstico apontar `CLOCK_TOO_FAST`, `PRELOAD_FAIL` ou `TX_UNDERRUN`, reduz o clock em 10% e tenta de novo.
10. Ao receber frame com CRC válido, salva a frequência validada em NVS (`Preferences`).

## Fluxo UCE

1. `CS` caindo na sessão de request inicia captura RX por interrupção.
2. O `SPI0_Handler` recebe bytes via `SPI_RDR` até o fim do burst (`NSSR`).
3. O request capturado sobe para `Link/Service`.
4. `Service` monta a resposta TLV e chama `Transport::setTx`.
5. `Transport::setTx` apenas stageia o buffer e emite o pulso `IRQ 1-0-1`.
6. Quando a BPM inicia a sessão de leitura, o novo `CS falling` faz:
   - reset do índice TX;
   - preload do primeiro byte em `SPI_TDR`;
   - `IRQ LOW` contínuo somente após slave-ready.
7. Enquanto a sessão TX está ativa, a UCE fica no caminho crítico de alimentar o próximo byte no `SPI_TDR`.
8. Qualquer falha detectada no transporte derruba `IRQ` para o estado idle e registra o erro para consulta em `CMD_TRANSPORT_DIAG`.

## Evidência de compilação

- UCE:
  - comando: `py -3 -m platformio run`
  - ambiente: `dueUSB`
  - resultado: `SUCCESS`
  - RAM: `4.2% (4112 / 98304 bytes)`
  - Flash: `2.8% (14632 / 524288 bytes)`
  - evidência salva em `out/dumps/uce-build-2026-04-24.txt`
- BPM:
  - comando: `py -3 -m platformio run`
  - ambiente: `esp32dev`
  - resultado: `SUCCESS`
  - RAM: `12.8% (41908 / 327680 bytes)`
  - Flash: `86.6% (1134689 / 1310720 bytes)`
  - evidência salva em `out/dumps/bpm-build-2026-04-24.txt`

## Evidência dos testes

- Teste lógico do protocolo existente:
  - comando: `node tests/commissioning_spi_v1/test_spi_v1_protocol.mjs`
  - resultado: `[COMMISSIONING_SPI_V1] protocol checks passed`
  - evidência salva em `out/dumps/spi-protocol-test-2026-04-24.txt`
- Log determinístico do handshake novo:
  - comando: `node tests/commissioning_spi_v1/simulate_spi_handshake_log.mjs`
  - evidência salva em `out/dumps/spi-handshake-log-2026-04-24.txt`
  - pontos cobertos no log:
    - `CS falling detectado`
    - `IRQ LOW somente apos slave-ready`
    - `primeiro byte pre-carregado antes do clock`
    - `BPM aguardando/validando IRQ LOW antes de clockar`
    - pacote `12 01 01 66` com CRC correto
    - aborto quando `IRQ` sobe antes do fim esperado

## Pendências e limitações

- A validação de handshake foi feita por compilação e simulação determinística em script; não houve captura em hardware real com osciloscópio/analisador lógico neste ambiente.
- O core Arduino Due já possui `PIOA_Handler`; por isso a implementação final usa `attachInterrupt(..., FALLING)` sobre o `NPCS0/PA28`, preservando o evento de borda sem substituir o handler global do core.
- O comando `CMD_TRANSPORT_DIAG` foi mantido mínimo e retorna apenas o último código de erro de transporte.
- Existem outras alterações não relacionadas no worktree (`UceApp.*`, `Link.*`, `platformio.ini`, diretórios `lib/protocol/spi`, `lib/GwSpiV1Protocol`, etc.) que não foram revertidas nem alteradas por este ajuste.
