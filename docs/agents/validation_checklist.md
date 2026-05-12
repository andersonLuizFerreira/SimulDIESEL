# Validation Checklist - SimulDIESEL

## Checklist geral

- [ ] O escopo da ETAPA foi respeitado.
- [ ] Nao houve alteracao fora do escopo autorizado.
- [ ] Arquivos funcionais nao foram alterados em ETAPA documental.
- [ ] Contratos SDH, SDGW e SDCTP foram preservados ou a alteracao foi explicitamente autorizada.
- [ ] UI, BLL, DAL, DTL, firmware e banco nao foram misturados sem autorizacao.
- [ ] Warnings, erros e limitacoes foram relatados.
- [ ] Lista de arquivos alterados foi gerada.
- [ ] Resultado de build/teste foi registrado ou marcado como nao aplicavel.
- [ ] Documentacao oficial impactada foi revisada/atualizada ao concluir a ETAPA.
- [ ] Dump foi gerado quando a ETAPA pediu.
- [ ] Rollback foi preservado.

## API C# WinForms

- [ ] Build da solucao `local-api/src/SimulDIESEL/SimulDIESEL.sln`, quando aplicavel.
- [ ] Arquivos C# criados, removidos ou movidos estao sincronizados com `.csproj`.
- [ ] Nao ha arquivos C# orfaos fora do carregamento da solucao.
- [ ] Solution Explorer/projeto Visual Studio reflete o filesystem.
- [ ] Nenhuma dependencia direta indevida da UI para TLV/SDGW bruto foi introduzida.
- [ ] BLL nao recebeu framing/COBS/CRC.
- [ ] DAL nao recebeu regra de apresentacao.
- [ ] DTL permaneceu sem IO ou logica de execucao.

## Firmware

- [ ] `platformio run` na pasta da board alterada, quando aplicavel.
- [ ] Pinos, barramentos e contratos de TLV foram preservados.
- [ ] BPM permaneceu gateway/roteador.
- [ ] UCE permaneceu executora fisica.
- [ ] GSA permaneceu geradora de sinais analogicos.

## Protocolos

- [ ] SDH alterado somente com contrato e validacao.
- [ ] SDGW permaneceu transporte puro.
- [ ] SDCTP permaneceu massa CAN RX/TX/sync.
- [ ] J1939 permaneceu sobre `CanFrameDto` e catalogos proprios.
- [ ] Legados foram preservados se ainda forem compatibilidade.

## Banco de Modulos

- [ ] Schema SQLite/PostgreSQL nao foi alterado sem pedido explicito.
- [ ] Comandos SDH armazenados seguem `SdhValidator`.
- [ ] Banco nao executa comandos automaticamente.
- [ ] Dados reais e exemplos ficam separados.

## Documentacao

- [ ] Arquivos Markdown criados no local previsto.
- [ ] Links e caminhos estao coerentes.
- [ ] `/docs/` reflete o estado consolidado quando a ETAPA muda para concluida.
- [ ] Divergencias foram registradas.
- [ ] Informacoes ausentes foram marcadas como `pendente de confirmacao`.
