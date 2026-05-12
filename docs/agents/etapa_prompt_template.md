# ETAPA Prompt Template - SimulDIESEL

Use estes modelos como base para pedir trabalho a agentes. Preencha os campos e mantenha o escopo fechado.

## 1. ETAPA de UI

```text
TEMA:
ETAPA de UI - <nome>

OBJETIVO:
<resultado esperado na interface WinForms>

ESCOPO PERMITIDO:
- local-api/src/SimulDIESEL/SimulDIESEL/UI/
- BLL/FormsLogic apenas se necessario para adaptar evento/caso de uso.

FORA DE ESCOPO:
- DAL, DTL, SDGW, SDCTP, firmware e banco.
- Alteracao de contratos.

ARQUIVOS PROVAVEIS:
- UI/<form>.cs
- UI/<form>.Designer.cs
- BLL/FormsLogic/<area>/*.cs

REGRAS DE IMPLEMENTACAO:
- UI nao consome TLV bruto nem SDGW direto.
- UI chama FormsLogic/servicos de alto nivel.
- Nao alterar protocolo para resolver problema visual.

VALIDACAO OBRIGATORIA:
- Build C# quando houver codigo.
- Conferencia visual/manual quando aplicavel.

DUMP OBRIGATORIO:
- Sim, se a ETAPA pedir.

ENTREGA ESPERADA:
- Arquivos alterados, resumo, validacao, pendencias.

RESTRICOES:
- Nao alterar firmware, banco ou contratos sem autorizacao.
```

## 2. ETAPA de BLL

```text
TEMA:
ETAPA de BLL - <nome>

OBJETIVO:
<caso de uso, client ou servico de aplicacao>

ESCOPO PERMITIDO:
- BLL/FormsLogic/
- BLL/Boards/
- BLL/Services/
- BLL/Protocols/ quando for decodificador de aplicacao.

FORA DE ESCOPO:
- UI visual, DAL de transporte, firmware, banco e schema.

ARQUIVOS PROVAVEIS:
- BLL/Boards/<board>/*.cs
- BLL/Services/<dominio>/*.cs

REGRAS DE IMPLEMENTACAO:
- BLL nao implementa COBS, CRC, retry ou SerialPort.
- BLL usa DTOs da DTL.
- Nao colocar regra de tela na BLL.

VALIDACAO OBRIGATORIA:
- Build C#.
- Teste/script especifico, se existir.

DUMP OBRIGATORIO:
- Quando houver consolidacao ou alteracao arquitetural.

ENTREGA ESPERADA:
- Arquivos, comportamento, validacao e rollback.

RESTRICOES:
- Nao alterar DAL/firmware por conveniencia.
```

## 3. ETAPA de DAL

```text
TEMA:
ETAPA de DAL - <nome>

OBJETIVO:
<sessao, transporte, SDH, SDGW ou adapter>

ESCOPO PERMITIDO:
- DAL/Protocols/SDGW/
- DAL/Transport/
- DTL somente para contratos necessarios e autorizados.

FORA DE ESCOPO:
- UI, FormsLogic visual, firmware, banco e decoders automotivos.

ARQUIVOS PROVAVEIS:
- SdhClient.cs
- SdhValidator.cs
- SdhToSdgwMapper.cs
- SdgwSession.cs
- SdgwLinkEngine.cs
- Transport/*.cs

REGRAS DE IMPLEMENTACAO:
- DAL nao conhece UI.
- SDGW permanece transporte puro.
- Alteracao de contrato exige congelamento e dump.

VALIDACAO OBRIGATORIA:
- Build C#.
- Testes de contrato/transporte, se existirem.

DUMP OBRIGATORIO:
- Sim para alteracao de contrato.

ENTREGA ESPERADA:
- Arquivos, comandos de validacao, divergencias.

RESTRICOES:
- Nao alterar firmware sem ETAPA propria.
```

## 4. ETAPA de DTO/DTL

```text
TEMA:
ETAPA de DTO/DTL - <nome>

OBJETIVO:
<contrato de dados ou enum>

ESCOPO PERMITIDO:
- DTL/
- Ajustes minimos de consumidores autorizados.

FORA DE ESCOPO:
- Logica de execucao, IO, UI, firmware e banco.

ARQUIVOS PROVAVEIS:
- DTL/Common/
- DTL/Boards/
- DTL/Protocols/

REGRAS DE IMPLEMENTACAO:
- DTL nao abre conexao, nao agenda fila, nao executa protocolo.
- Preservar compatibilidade quando contrato ja esta consolidado.

VALIDACAO OBRIGATORIA:
- Build C#.

DUMP OBRIGATORIO:
- Sim se contrato for congelado.

ENTREGA ESPERADA:
- Contratos alterados e consumidores impactados.

RESTRICOES:
- Nao renomear tipos publicos sem autorizacao.
```

## 5. ETAPA de Firmware

```text
TEMA:
ETAPA de Firmware - <BPM/UCE/GSA> - <nome>

OBJETIVO:
<efeito embarcado esperado>

ESCOPO PERMITIDO:
- hardware/firmware/<board>/
- docs/dumps relacionados.

FORA DE ESCOPO:
- UI/API/banco, salvo pedido explicito de contrato ponta a ponta.

ARQUIVOS PROVAVEIS:
- src/main.cpp
- lib/<servico>/
- include/config.h
- include/defs.h

REGRAS DE IMPLEMENTACAO:
- BPM e gateway/roteador.
- UCE executa servicos fisicos CAN/LED.
- GSA executa sinais analogicos.
- Nao alterar pinos/contratos sem evidencia.

VALIDACAO OBRIGATORIA:
- `platformio run` da board alterada.
- Teste de bancada se pedido.

DUMP OBRIGATORIO:
- Sim para mudanca de firmware/protocolo.

ENTREGA ESPERADA:
- Arquivos, build PlatformIO, impacto em contrato.

RESTRICOES:
- Nao alterar host/API por conveniencia.
```

## 6. ETAPA de Protocolo

```text
TEMA:
ETAPA de Protocolo - <SDH/SDGW/SDCTP/J1939>

OBJETIVO:
<contrato ou fluxo a consolidar>

ESCOPO PERMITIDO:
- docs/architecture/
- docs/official/06-protocolos/
- DTL/DAL/BLL/firmware apenas se explicitamente autorizado.

FORA DE ESCOPO:
- Mudanca visual, refatoracao ampla, banco.

ARQUIVOS PROVAVEIS:
- GwProtocol.cs
- SdhValidator.cs
- SdhToSdgwMapper.cs
- SDCTP/* ou firmware correspondente.

REGRAS DE IMPLEMENTACAO:
- SDH semantico.
- SDGW transporte.
- SDCTP massa CAN.
- J1939 decodificacao sobre `CanFrameDto`.

VALIDACAO OBRIGATORIA:
- Build/testes de contrato.
- Dump de contrato.

DUMP OBRIGATORIO:
- Sim.

ENTREGA ESPERADA:
- Matriz de contratos, arquivos, testes, pendencias.

RESTRICOES:
- Nao quebrar compatibilidade legada sem autorizacao.
```

## 7. ETAPA de Banco de Dados

```text
TEMA:
ETAPA de Banco de Dados - <nome>

OBJETIVO:
<schema, dump ou modelo de dados>

ESCOPO PERMITIDO:
- Data/Modules/
- tools/dumps/module_database_model/
- docs relacionados.

FORA DE ESCOPO:
- UI, executor SDH, firmware, cloud/autenticacao sem pedido.

ARQUIVOS PROVAVEIS:
- Data/Modules/schema/*.sql
- Data/Modules/docs/*.md
- Data/Modules/modules.db

REGRAS DE IMPLEMENTACAO:
- Banco nao executa comandos.
- Comandos SDH devem ser validaveis.
- Dados reais e exemplos separados.

VALIDACAO OBRIGATORIA:
- Validadores de schema/dump quando aplicavel.

DUMP OBRIGATORIO:
- Sim para schema/modelo.

ENTREGA ESPERADA:
- Schema, dump, validacao, limitacoes.

RESTRICOES:
- Nao alterar UI/firmware/protocolo sem autorizacao.
```

## 8. ETAPA de Validacao

```text
TEMA:
ETAPA de Validacao - <area>

OBJETIVO:
<provar build, teste, contrato ou regressao>

ESCOPO PERMITIDO:
- Execucao de comandos de validacao.
- Criacao de dumps/relatorios.
- Ajustes somente se explicitamente pedidos.

FORA DE ESCOPO:
- Refatoracao ou correcao automatica.

ARQUIVOS PROVAVEIS:
- out/dumps/<nome>.md

REGRAS DE IMPLEMENTACAO:
- Nao esconder warnings/erros.
- Registrar comandos e resultados.

VALIDACAO OBRIGATORIA:
- A propria validacao solicitada.

DUMP OBRIGATORIO:
- Sim, se a ETAPA pedir relatorio.

ENTREGA ESPERADA:
- Resultado reproduzivel.

RESTRICOES:
- Nao corrigir codigo sem autorizacao.
```

## 9. ETAPA de Refatoracao Controlada

```text
TEMA:
ETAPA de Refatoracao Controlada - <nome>

OBJETIVO:
<reduzir acoplamento sem mudar comportamento>

ESCOPO PERMITIDO:
- Arquivos explicitamente listados.

FORA DE ESCOPO:
- Mudanca de contrato, UI, firmware ou banco nao listada.

ARQUIVOS PROVAVEIS:
- <lista fechada>

REGRAS DE IMPLEMENTACAO:
- Sem mudanca comportamental intencional.
- Preservar nomes publicos, salvo autorizacao.
- Fazer em passos pequenos.

VALIDACAO OBRIGATORIA:
- Build e testes de regressao aplicaveis.

DUMP OBRIGATORIO:
- Sim.

ENTREGA ESPERADA:
- Antes/depois, arquivos, validacao.

RESTRICOES:
- Nao remover legado sem ETAPA propria.
```

## 10. ETAPA de Limpeza de Legado

```text
TEMA:
ETAPA de Limpeza de Legado - <nome>

OBJETIVO:
<remover legado comprovadamente sem uso>

ESCOPO PERMITIDO:
- Somente simbolos/arquivos listados.

FORA DE ESCOPO:
- Contratos de fio, firmware, UI e banco sem pedido.

ARQUIVOS PROVAVEIS:
- <lista fechada>

REGRAS DE IMPLEMENTACAO:
- Provar ausencia de uso por busca/teste.
- Preservar compatibilidade quando contrato ainda existir.

VALIDACAO OBRIGATORIA:
- Busca de referencias.
- Build/testes.

DUMP OBRIGATORIO:
- Sim.

ENTREGA ESPERADA:
- Itens removidos, itens preservados, justificativa.

RESTRICOES:
- Nao remover codigo legado por preferencia estetica.
```

## 11. ETAPA de Congelamento

```text
TEMA:
ETAPA de Congelamento - <contrato/area>

OBJETIVO:
<registrar estado aceito e regras de nao regressao>

ESCOPO PERMITIDO:
- Documentacao e dumps.
- Codigo somente se pedido explicitamente.

FORA DE ESCOPO:
- Alteracao funcional.

ARQUIVOS PROVAVEIS:
- docs/architecture/*.md
- out/dumps/*.md

REGRAS DE IMPLEMENTACAO:
- Registrar decisoes congeladas.
- Registrar decisoes pendentes.
- Usar instrucao mais conservadora em divergencias.

VALIDACAO OBRIGATORIA:
- Conferencia documental e de arquivos.

DUMP OBRIGATORIO:
- Sim.

ENTREGA ESPERADA:
- Documento de congelamento e dump.

RESTRICOES:
- Nao alterar contratos runtime.
```

## 12. ETAPA de Consolidacao Git

```text
TEMA:
ETAPA de Consolidacao Git - <nome>

OBJETIVO:
<preparar checkpoint, commit ou tag com autorizacao>

ESCOPO PERMITIDO:
- Inspecao de status/diff.
- Stage/commit/tag apenas com autorizacao explicita.

FORA DE ESCOPO:
- Alterar codigo para "arrumar" antes do commit sem pedido.

ARQUIVOS PROVAVEIS:
- Nao aplicavel.

REGRAS DE IMPLEMENTACAO:
- Nao criar branch sem autorizacao.
- Nao fazer commit sem autorizacao.
- Nao incluir alteracoes de terceiros por acidente.

VALIDACAO OBRIGATORIA:
- `git status --short`
- Conferencia de arquivos staged.

DUMP OBRIGATORIO:
- Se pedido.

ENTREGA ESPERADA:
- Status, arquivos incluidos, hash/branch/tag se criados.

RESTRICOES:
- Nunca usar reset destrutivo sem pedido explicito.
```
