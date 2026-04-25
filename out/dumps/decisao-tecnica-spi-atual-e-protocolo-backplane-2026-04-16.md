# Decisao tecnica - correcao do SPI atual e definicao do protocolo de backplane SPI

Data: 2026-04-16  
Base oficial obrigatoria: `out/dumps/levantamento-tecnico-spi-bpm-uce-backplane-2026-04-16.md`

## Objetivo

Esta decisao separa dois planos que nao devem mais ser confundidos:

- **Parte A - curto prazo**: corrigir o bug real do SPI BPM <-> UCE sem alterar o protocolo atual
- **Parte B - medio prazo**: definir a camada de transporte SPI de backplane sobre a infraestrutura fisica existente

---

## 1. Validacao do relatorio oficial

O relatorio anterior foi revalidado contra os trechos criticos do codigo real:

- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp`
- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwRouter/GwRouter.cpp`

Conclusao validada:

1. O reporte anterior esta tecnicamente correto ao separar:

- fragilidade de implementacao do enlace SPI atual
- fragilidade estrutural do protocolo de transporte inexistente

2. O `Link` da UCE monta um frame coerente antes do envio. Logo, o CRC invalido observado no runtime nao aponta primeiro para erro semantico de `TLV` nem para erro no calculo do CRC em si.

3. O ponto critico real continua sendo o transporte da response no slave combinado com a leitura da BPM em dois bursts.

---

## PARTE A - DIAGNOSTICO FINAL DO BUG SPI

## 2. Causa mais provavel do CRC invalido

### 2.1 Causa principal

A causa mais provavel do `CRC` invalido e:

- **quebra entre os dois bursts da response na BPM**
- **somada ao pipeline de preload do slave na UCE depender de timing e da sobrevivencia do byte ja armado no `SPI_TDR` atraves de `NSSR`**

Em termos práticos:

- a BPM le `header` em um burst
- a UCE, ainda durante esse burst, pre-carrega o primeiro byte do payload seguinte
- o firmware atual assume implicitamente que esse byte ja armado no `TDR` vai continuar valido quando o `CS` subir e depois descer de novo
- essa suposicao e fraca e dependente de timing/comportamento do periferico slave

### 2.2 O que e menos provavel

Menos provavel como causa raiz principal:

- **erro de CRC em `Link.cpp`**: o CRC e calculado em buffer continuo e coerente antes de chamar `setTx(...)`
- **erro sistematico no campo `L`**: o `L` nasce do `Tlv::build(...)` e do `Service`; o problema principal aparece depois, na entrega da response
- **leitura prematura da BPM antes da IRQ**: quando `spiUseIrq == true`, a BPM so le depois da IRQ ir a `LOW`
- **race principal ISR vs loop na montagem da response**: a response ja esta pronta e staged antes da IRQ; o problema maior nao e o CRC ser calculado em paralelo, e sim a transmissao fisica dos bytes staged

### 2.3 Papel da race ISR vs loop

Existe acoplamento entre ISR e loop, mas como fator secundario:

- o `request` so vira `response` depois que `Link::poll()` roda no loop principal
- isso deixa o tempo request -> response dependente do loop

Mas isso explica melhor:

- latencia
- timeout esperando IRQ

Nao explica tao bem o caso especifico de:

- `IRQ` baixa
- header lido
- pacote final recebido com `CRC` invalido

Para esse caso, o melhor encaixe continua sendo:

- **preload/pipeline de TX do slave na fronteira entre os dois bursts**

## 3. O problema e deterministico ou intermitente?

Classificacao:

- **intermitente**
- **dependente de timing**
- **dependente do acoplamento entre burst 1, `NSSR`, persistencia do preload e burst 2**

Nao parece ser:

- erro deterministico de algoritmo
- erro estrutural em todo frame

Se fosse deterministico de algoritmo, o `CRC` falharia sempre da mesma forma. O desenho atual, ao contrario, tem assinatura classica de erro temporal:

- depende de quando o slave conseguiu deixar o proximo byte realmente pronto
- depende de como o hardware trata o byte armado no fim do burst anterior

## 4. Ponto exato do codigo onde o erro nasce

### 4.1 UCE - origem fisica do erro

O ponto mais sensivel esta em:

- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp:135-142`
- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/transport/Transport.cpp:158-177`

Trecho relevante:

```cpp
if (_txPending) {
  _mode = ModeSending;
  if (_txIndex < _txLen) {
    loadNextTxByte();
  }
}
```

e depois:

```cpp
if (status & SPI_SR_NSSR) {
  ...
  if (_txPending && _txIndex >= _txLen) {
    clearTxState();
    setIrqActive(false);
  }
  _mode = ModeIdle;
}
```

Diagnostico:

- o proximo byte e carregado em `RDRF`, portanto no limite temporal do clock do master
- quando a BPM encerra o burst do header, o firmware da UCE **nao rearma explicitamente** o primeiro byte do burst seguinte no handler de `NSSR`
- o desenho depende de o byte ja escrito em `SPI_TDR` antes do `CS HIGH` continuar servindo corretamente no burst seguinte

Esse e o ponto mais provavel em que o byte errado nasce.

### 4.2 BPM - condicao que expõe o erro

O outro lado do problema esta em:

- `hardware/firmware/BPM - BACKPLANE MANAGER MODULE/lib/GwSpiBus/GwSpiBus.cpp:90-125`

Trecho relevante:

```cpp
// READ header
csLow(cs);
hdr[0] = _spi.transfer(0x00);
hdr[1] = _spi.transfer(0x00);
csHigh(cs);

// READ payload + CRC
csLow(cs);
for (size_t i = 0; i < (size_t)len + 1; i++) {
    rx[2 + i] = _spi.transfer(0x00);
}
csHigh(cs);
```

Diagnostico:

- a BPM nao causa sozinha o bug
- mas a quebra da response em dois bursts e a condicao que torna o pipeline da UCE vulneravel
- o protocolo atual depende justamente desse comportamento

### 4.3 Link.cpp - por que ele nao parece ser a origem

Trecho relevante:

- `hardware/firmware/UCE - Unidade de comunicacao externa/lib/core/link/Link.cpp:63-70`

```cpp
out[txTlvLen] = Crc8::calc(out, txTlvLen);
_transport.setTx(out, (uint8_t)(txTlvLen + 1));
```

Diagnostico:

- o `Link` monta o buffer completo em RAM
- calcula o CRC sobre o buffer inteiro
- so depois entrega ao `Transport`

Portanto:

- o `Link` confirma que a response logica nasce coerente
- o problema aparece depois, no transporte da response pela SPI slave

## 5. Conclusao objetiva do bug atual

Resposta curta:

- **o CRC invalido nasce mais provavelmente no transporte TX da UCE, no atravessamento entre o burst de header e o burst de payload, por dependencia fragil de preload em `RDRF` e da continuidade do byte armado atraves de `NSSR`**

Classificacao final:

- **bug de implementacao**
- **intermitente**
- **timing-sensitive**
- **potencializado pela resposta em dois bursts do protocolo atual**

---

## PARTE A - PLANO DE CORRECAO DO SPI ATUAL

## 6. O que corrigir na UCE

### 6.1 Reestruturar o pipeline de TX

Corrigir:

- nao depender de que o byte ja escrito no `SPI_TDR` continue implicitamente valido depois de `NSSR`
- explicitar no estado interno a diferenca entre:
  - byte ja enviado
  - byte apenas armado
  - proximo byte ainda nao armado

Direcao concreta:

- transformar o TX da UCE numa pequena maquina de estados explicita
- separar:
  - `txPreparedLen`
  - `txSentCount`
  - `txPrimedValid`
  - `txPrimedByte`

Objetivo:

- o primeiro byte do burst seguinte deve ser rearmado de forma deliberada
- nao apenas por efeito colateral do burst anterior

### 6.2 Tratar `NSSR` como fronteira real de burst

Corrigir no `Transport.cpp`:

- `NSSR` nao pode so colocar `_mode = ModeIdle`
- quando `_txPending` ainda nao acabou, `NSSR` deve:
  - preservar o contexto da response atual
  - preparar conscientemente o proximo inicio de burst
  - nao limpar estado cedo demais

Em outras palavras:

- a UCE precisa assumir formalmente que a response atual sera lida em dois bursts
- nao apenas “torcer” para que o preload sobreviva

### 6.3 Imutabilidade da response enquanto TX estiver pendente

Garantir:

- o buffer TX staged nao pode mudar ate a conclusao do ultimo `NSSR` da response
- o `setTx(...)` deve ser rejeitado ou enfileirado se ainda houver uma response pendente

Motivo:

- o transporte atual usa buffer unico
- ele precisa ser tratado como “response em voo”

### 6.4 Bloqueio de RX enquanto o slave estiver servindo TX

Garantir:

- durante a leitura da response pela BPM, os bytes dummy de MOSI nao podem contaminar nenhum estado de novo request
- hoje isso ja acontece parcialmente pelo ramo `if (_txPending)`, mas o estado precisa ficar explicitamente blindado ate o fim real da response

### 6.5 Mantem o `Link` como produtor da response, nao como responsavel pelo timing

O `Link` deve continuar:

- montando `TLV + CRC`
- entregando o pacote inteiro ao `Transport`

Nao deve ser o lugar da correcao do bug fisico. O ajuste principal e em `Transport.cpp`.

## 7. O que corrigir na BPM

### 7.1 Manter a leitura em dois bursts, mas endurecer o header

Corrigir em `GwSpiBus.cpp`:

- validar o `L` imediatamente apos o burst do header
- antes de partir para o segundo burst, rejeitar comprimentos impossiveis ou incoerentes

Validacoes minimas recomendadas:

- `total >= 3`
- `total <= rxMax`
- `L` dentro do limite esperado para o protocolo atual da UCE

### 7.2 Diferenciar erro de header invalido de timeout

Corrigir em `GwRouter.cpp` + `GwSpiBus.cpp`:

- nem todo `false` de `transact(...)` deve virar `GWERR_TIMEOUT`

Separacoes minimas necessarias:

- timeout esperando IRQ
- header invalido
- comprimento incoerente
- resposta maior que o buffer
- frame incompleto

Motivo:

- o diagnostico atual ja sabe apontar causas finas
- a classificacao final ainda esta grossa demais

### 7.3 Guardar melhor a leitura apos IRQ

Correcao recomendada:

- a BPM deve continuar respeitando `IRQ`
- pode acrescentar uma guarda curta e deterministica logo apos detectar `IRQ LOW`, se medicao mostrar necessidade

Importante:

- nao introduzir `delay(1)` arbitrario como “solucao”
- se houver guarda, ela deve ser minima e justificada por medicao

### 7.4 Melhorar o snapshot diagnostico

Preservar e ampliar:

- bytes lidos no burst 1
- bytes lidos no burst 2
- motivo exato da falha de header

Isso deve ser mantido porque e uma das melhores partes da solucao atual.

## 8. O que nao deve ser alterado agora

Nesta fase de correcao do bug, **nao alterar**:

- o `TLV` atual no fio
- o `CRC8` atual do `TLV`
- a rota `GW_ADDR_UCE` / `GW_OP_UCE_TLV_TRANSACT`
- a arquitetura `SDGW` host <-> BPM
- o uso de `CS`
- o uso de `IRQ`
- o conceito de response em dois bursts, nesta correcao imediata

Justificativa:

- o objetivo de curto prazo e estabilizar o que ja existe
- mudar framing agora misturaria bugfix com redesign de protocolo

## 9. Decisao de curto prazo

Decisao:

- **corrigir primeiro a implementacao da UCE no `Transport.cpp`**
- **endurecer a classificacao e a validacao do lado BPM**
- **nao mudar ainda o protocolo `TLV + CRC` nem o binding atual**

---

## PARTE B - DEFINICAO DO PROTOCOLO SPI DE BACKPLANE

## 10. Objetivo do protocolo de medio prazo

Criar uma camada de transporte SPI real sobre:

```text
SPI fisico + CS + IRQ + RESET
```

preservando:

- `CS` dedicado por board
- `IRQ` dedicado por board
- `RESET` por board ou compartilhado
- payload funcional curto em `TLV`, quando aplicavel
- BPM como roteador fisico, nao como logica de aplicacao

## 11. Principios obrigatorios do protocolo novo

O protocolo de backplane deve:

1. separar **transporte** de **payload funcional**
2. ter **header fixo de transporte**
3. ter **comprimento total explicito**
4. ter **identidade de transacao (`seq`)**
5. ter **status de transporte separado**
6. permitir **retry seguro e deduplicacao**
7. suportar **payloads maiores**
8. suportar **fragmentacao/blocos** quando necessario
9. suportar **bootloader/aplicacao** como estados distintos, se o projeto optar por update via SPI
10. continuar funcionando bem para comandos curtos

## 12. Estrutura recomendada do frame de transporte SPI

### 12.1 Header de transporte fixo

Recomendacao:

```text
[SYNC0][SYNC1][VER][TYPE][FLAGS][SEQ][LEN_L][LEN_H][HDR_CRC]
```

Campos:

- `SYNC0`, `SYNC1`: assinatura fixa de sincronismo e sanidade de header
- `VER`: versao do protocolo de transporte SPI
- `TYPE`: `REQ`, `RESP`, `EVENT`
- `FLAGS`: bits de controle
- `SEQ`: identificador de transacao para retry/deduplicacao
- `LEN_L`, `LEN_H`: tamanho do payload de transporte
- `HDR_CRC`: CRC do header

### 12.2 Payload e integridade

Formato:

```text
[HEADER FIXO][PAYLOAD][PAYLOAD_CRC16]
```

Decisao:

- manter `CRC8` do `TLV` apenas no payload funcional curto legado, quando ele existir
- no transporte SPI novo, usar **CRC proprio de transporte mais forte**, por exemplo `CRC16`, para payload

Motivo:

- `CRC8` atende bem TLVs curtos
- para blocos maiores e backplane padrao, `CRC16` e mais apropriado

### 12.3 O que vai dentro do payload

Opcoes de payload:

- `TLV funcional legado`, para comandos curtos como LED e CAN
- payload binario de servicos futuros
- blocos de transferencia para eventual bootloader

Conclusao:

- o `TLV` deixa de ser o transporte
- ele passa a ser **um tipo de payload de aplicacao**

## 13. Fluxo de transacao recomendado

### 13.1 Request

1. A BPM seleciona a board por `CS`
2. Envia `HEADER + PAYLOAD + CRC`
3. A board valida header e payload
4. Se aceitou a request, processa e prepara a response
5. Quando a response estiver staged, baixa `IRQ`

### 13.2 Response

1. A BPM espera `IRQ`
2. Le um **header fixo de response** em burst 1
3. Valida `SYNC`, `VER`, `TYPE`, `SEQ`, `LEN`, `HDR_CRC`
4. Se o header for valido, le `PAYLOAD + PAYLOAD_CRC16` em burst 2
5. Valida integridade do payload

### 13.3 Retry e deduplicacao

Decisao:

- a BPM passa a manter `SEQ` por board
- em falha recuperavel, a BPM repete a request com o mesmo `SEQ`
- a board precisa deduplicar e poder reenviar a ultima response daquele `SEQ`

Isso traz para o backplane SPI a robustez que hoje existe no `SDGW` host <-> BPM, mas nao existe entre BPM <-> board.

## 14. Tipos e status minimos recomendados

### 14.1 Tipos

Minimo:

- `REQ`
- `RESP`
- `EVENT`

### 14.2 Status de transporte na response

Minimo:

- `OK`
- `BAD_HEADER`
- `BAD_PAYLOAD_CRC`
- `BAD_LEN`
- `BUSY`
- `UNSUPPORTED`
- `INTERNAL_ERROR`

Importante:

- isso fica no **transporte**
- erro funcional da aplicacao continua dentro do payload funcional

## 15. Uso de IRQ, CS e RESET no protocolo novo

### 15.1 IRQ

Manter:

- `IRQ` dedicada por board

Papel:

- indicar `response ready`
- opcionalmente indicar `event pending`

### 15.2 CS

Manter:

- `CS` dedicado por board

Papel:

- delimitar bursts SPI
- selecionar fisicamente a board

### 15.3 RESET

No protocolo novo, o `RESET` deixa de ser apenas fio presente e passa a ter politica clara:

- retry de transporte antes de reset
- reset coordenado apos numero limitado de falhas consecutivas
- possibilidade de entrar em `bootloader mode`, se o projeto optar por update via SPI

## 16. Compatibilidade com firmware update

Sem assumir que SPI sera o unico caminho de update:

- o protocolo novo deve ser **compativel** com update via SPI
- o projeto ainda pode escolher outro canal no futuro

Para suportar update via SPI, o transporte precisa permitir:

- bloco maior que `TLV_MAX_LEN`
- confirmacao por bloco
- `SEQ` e retry por bloco
- integridade por bloco
- estados distintos `application` / `bootloader`
- retomada segura apos falha

O protocolo atual nao atende isso. O protocolo novo passa a nao bloquear esse cenario.

## 17. Plano de migracao recomendado

### Fase 1 - agora

- corrigir o SPI atual BPM <-> UCE
- manter `TLV + CRC8` legado

### Fase 2 - depois da estabilizacao

- introduzir camada de transporte SPI V1
- encapsular o `TLV` atual dentro do novo payload

### Fase 3 - expansao

- publicar novas baby boards SPI usando o transporte V1
- avaliar migracao futura da GSA para SPI ja nesse novo transporte

### Fase 4 - usos avancados

- blocos maiores
- eventos SPI
- eventual suporte a bootloader/update via SPI, se o projeto confirmar essa direcao

---

## 18. Decisao final

### 18.1 Curto prazo

Decisao:

- **o bug atual deve ser tratado como falha de implementacao do enlace SPI**
- **a causa mais provavel e a fragilidade do preload/transicao entre bursts no slave UCE**
- **a correcao deve focar primeiro no `Transport.cpp` da UCE e no endurecimento do lado BPM**

### 18.2 Medio prazo

Decisao:

- **o protocolo atual nao deve ser expandido como padrao de backplane**
- **o caminho correto e definir um transporte SPI de backplane sobre a infraestrutura fisica ja existente**
- **o `TLV` atual deve ser preservado apenas como payload funcional curto e legado**

### 18.3 Sintese executiva

Escolha tecnica recomendada:

- **primeiro: corrigir a implementacao atual sem mudar o protocolo**
- **depois: introduzir um protocolo SPI de backplane real, com header fixo, `seq`, `len` explicito, status de transporte e retry/deduplicacao**

Essa e a separacao correta entre:

- **bugfix imediato**
- **evolucao arquitetural**

