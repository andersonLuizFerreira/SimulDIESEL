# Skill: Firmware UCE

## Quando usar

Use para firmware da UCE, SPI slave, LedService, CanService, SDCTP embarcado, driver CAN e execucao fisica.

## Quando nao usar

Nao use para alterar UI/API/banco sem pedido ponta a ponta.

## Escopo permitido

- `hardware/firmware/UCE - Unidade de comunicacao externa/`
- documentos e dumps de UCE.

## Escopo proibido

- Alterar BPM/host para mascarar problema da UCE sem autorizacao.
- Mudar pinagem ou TLVs consolidados sem congelamento.

## Arquivos/pastas provaveis

- `src/main.cpp`
- `include/config.h`
- `include/defs.h`
- `lib/core/transport/`
- `lib/core/services/`
- `lib/services/can/`
- `lib/services/led/`

## Padroes do projeto

- UCE e board remota por SPI.
- Executa LED e CAN fisico.
- CAN atual agrega controle e massa em `CanService`, com wrappers SDCTP.
- Eventos sobem para BPM por IRQ/event queue.

## Checklist de validacao

- [ ] `platformio run` em UCE quando houver codigo.
- [ ] Contratos TLV preservados.
- [ ] Pinos SPI/IRQ/reset preservados.
- [ ] Build host/BPM somente se a ETAPA tambem tocar contrato ponta a ponta.

## Checklist de entrega

- [ ] Servicos alterados.
- [ ] TLVs impactados.
- [ ] Resultado PlatformIO.
- [ ] Validacao de bancada, se executada.

## Riscos comuns

- Quebrar SPI nativo do Due.
- Misturar controle CAN e massa sem documentar.
- Remover fakes/legados sem decisao.

## Regras de nao regressao

- UCE continua executora fisica.
- SDGW nao entra na logica interna da UCE alem do contrato recebido via BPM.
- SDCTP preserva RX/TX validado.
