# [COMMISSIONING_SPI_V1] Comissionamento SPI V1

Testes obrigatorios cobertos por `test_spi_v1_protocol.py` e `test_spi_v1_protocol.mjs`:

- baseline logico de frame fisico COBS + delimitador `0x00`
- frame DATA com multiplas mensagens
- ACK com status `1`, `2` e `3`
- distribuicao de prioridade `6:3:1`
- regra de nao fragmentar e nao pular mensagem quando nao cabe
- pausa/resume por IRQ
- timeout de 500 ms sem progresso
- ciclo de janela com ACK, RETRY e DROP

Baseline fisico antes de bancada:

1. Gravar BPM e UCE com logs habilitados.
2. Validar CS, IRQ, CLK, MOSI e MISO em osciloscopio/analisador logico.
3. Enviar comando LED toggle pela rota UCE.
4. Confirmar no log da BPM os bytes COBS transmitidos e recebidos.
5. Confirmar no `SerialUSB` da UCE a captura do frame, parse DATA e emissao de ACK/DATA.

Aceite fisico minimo:

- CS permanece LOW durante a sessao.
- CLK ocorre apenas com IRQ LOW.
- IRQ HIGH pausa o clock e IRQ LOW retoma.
- A sessao aborta apos 500 ms sem progresso.
- O comando LED alterna e a resposta TLV valida retorna ao host.
