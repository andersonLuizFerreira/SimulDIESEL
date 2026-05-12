# Dump - Rastreamento de dados candidatos a persistencia

Data: 2026-05-12

## 1. Resumo executivo

Esta ETAPA foi exclusivamente documental. O rastreamento identificou dados reais ja existentes no SimulDIESEL que podem, em ETAPAS futuras, ser migrados, registrados, versionados ou sincronizados no banco local da API.

Conclusao conservadora:

- o Banco de Modulos ja possui modelagem documental e estrutural para perfis, pinagem, redes CAN/J1939, comandos SDH, sequencias de teste e capturas;
- ha candidatos fortes para persistencia futura em quatro dominios:
  - banco de modulos;
  - banco de capturas;
  - banco de protocolos/catalogos;
  - banco de logs/historico;
- varios estados atuais sao runtime puro e nao devem virar tabela por reflexo, como espelho SDCTP, buffer RX atual, estado de conexao ativo e working set em memoria;
- nao foi encontrado motivo factual para criar nova entidade nesta ETAPA;
- persistencia imediata nao foi recomendada para nenhum candidato, porque o pedido desta ETAPA e analitico e os fluxos consumidores ainda precisam ser decididos ou consolidados.

## 2. Metodologia de rastreamento

Foram usados:

- leitura das regras obrigatorias da ETAPA:
  - `AGENTS.md`;
  - `.codex/instructions.md`;
  - `.codex/skills/bll-dal-dtl/SKILL.md`;
  - `.codex/skills/build-validation/SKILL.md`;
  - `.codex/skills/simuldiesel-architecture/SKILL.md`;
- busca textual por `rg` sobre:
  - `BLL/`, `DAL/`, `DTL/`, `SDGW/`, `UI/`;
  - `Data/`, `Data/Modules/`, `Data/Protocols/`;
  - `docs/`;
  - `out/dumps/`;
- leitura dirigida de arquivos de maior relevancia:
  - `Data/Modules/docs/module_database_model_v1.md`;
  - `Data/Modules/docs/module_database_sdh_relation.md`;
  - `Data/Modules/docs/local_api_database_runtime.md`;
  - `docs/official/02-arquitetura/12-banco-local-api.md`;
  - `out/dumps/bd_local_estrutura.md`;
  - `out/dumps/bd_service_provider.md`;
  - `BLL/Boards/UCE/UceGatewayDiagnosticLog.cs`;
  - `BLL/Protocols/J1939/J1939ProtocolService.cs`;
  - `BLL/Protocols/J1939/NetworkManagement/J1939NetworkManagementService.cs`;
  - `BLL/Protocols/J1939/Diagnostics/J1939DiagnosticRequestService.cs`;
  - `DAL/Transport/Serial/SerialConnectionSettings.cs`;
  - `DAL/Transport/Bluetooth/BluetoothConnectionSettings.cs`;
  - `DTL/Boards/GSA/GsaRequests.cs`;
  - `DTL/Boards/GSA/GsaResponses.cs`.

Toda conclusao abaixo foi baseada em evidencias encontradas no projeto. Quando ha extrapolacao arquitetural, ela esta marcada como inferencia.

## 3. Areas analisadas

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/`
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/`
- `local-api/src/SimulDIESEL/SimulDIESEL/SDGW/`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/`
- `Data/`
- `Data/Modules/`
- `Data/Protocols/`
- `docs/`
- `out/dumps/`
- `hardware/firmware/`, apenas como referencia documental indireta em dumps e docs, sem alteracao

## 4. Lista de candidatos encontrados

Foram encontrados candidatos agrupados em:

- configuracao e identidade tecnica de modulos;
- comandos SDH reutilizaveis;
- sequencias e passos de teste;
- sessoes e eventos de captura CAN/J1939;
- catalogos e definicoes J1939;
- diagnosticos e logs operacionais;
- preferencias de transporte;
- configuracoes de canais e offsets GSA;
- estados runtime que devem permanecer fora do banco.

## 5. Tabela de classificacao

| Candidato | Local atual | Tipo de dado | Consumidor atual | Persistencia sugerida | Dominio sugerido | Prioridade | Justificativa | Risco |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Perfis de modulo e versoes | `Data/Modules/docs/module_database_model_v1.md`, schema `module_profiles` e `module_profile_versions` | configuracao/catalogo | Banco de Modulos estrutural | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Alta | Ja existe modelagem oficial e valor claro para versionamento | Persistir fluxo sem CRUD funcional real |
| Conectores, pinos e alimentacao | `module_connectors`, `module_pins`, `module_power_requirements` | configuracao fisica | Banco de Modulos estrutural | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Alta | Dados centrais para preparacao de bancada | Amarrar cedo demais hardware ainda em evolucao |
| Redes CAN/J1939 por modulo | `module_can_networks`; `UCE.can.config`; `UceCanProtocol` | configuracao | perfis de modulo e configuracao UCE | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Alta | Bitrate, controlador e SA esperado sao configuracoes reusaveis | Misturar config persistida com estado runtime do barramento |
| Comandos SDH parametrizados | `module_sdh_commands`; dumps de inventario SDH; `SdhCommand` | contrato/configuracao executavel | validacao SDH e futuros perfis | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Alta | Ja existe ponte conceitual entre perfil tecnico e comando SDH | Persistir comando invalido ou ultrapassar o `SdhValidator` |
| Sequencias e passos de teste | `module_test_sequences`, `module_test_steps` | fluxo configuravel | ainda sem executor de banco nesta leitura | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Media | Modelo ja previsto e util para repetibilidade | Transformar desenho documental em motor de execucao sem ETAPA propria |
| Sessoes e eventos de captura | `module_capture_sessions`, `module_capture_events` | captura/historico | modelo estrutural do Banco de Modulos | PLANEJAR PARA ETAPA FUTURA | banco de capturas | Alta | Distingue bem evento encerrado de frame ao vivo | Volume, retenção e indice precisam de decisao |
| Catalogo J1939 padrao | `Data/Protocols/J1939/j1939-pgn-standard-catalog.json` | catalogo/versionavel | decodificacao e documentacao J1939 | PLANEJAR PARA ETAPA FUTURA | banco de protocolos/catalogos | Media | Ja existe em JSON e tende a pedir versionamento | O catalogo possui itens com `pending verification` |
| Catalogo J1939-71 mini | `Data/Protocols/J1939/j1939-71-mini-catalog.json` | catalogo/versionavel | decodificacao J1939 | DOCUMENTAR APENAS | banco de protocolos/catalogos | Baixa | Existe como apoio reduzido e ainda nao se provou necessidade de persistencia | Duplicar fonte de verdade cedo demais |
| PGNs observados e proprietarios em capturas | `module_j1939_pgns`, `module_capture_events`, dumps SDH/database relevance | catalogo/captura | aprendizado de modulo futuro | PLANEJAR PARA ETAPA FUTURA | banco de modulos + capturas | Media | Faz sentido vincular achados a perfil e sessao | Incluir ruído de captura como conhecimento validado |
| Address claiming decodificado | `J1939NetworkManagementService`, DTOs de network management | evento/estado runtime | rede J1939 host | DEPENDE DE DECISAO ARQUITETURAL | capturas ou logs | Media | Eventos sao semanticamente ricos; snapshots historicos podem ser uteis | Registrar working state como se fosse cadastro permanente |
| Working set J1939 atual | `J1939NetworkManagementService` | estado runtime | decodificacao J1939 | NAO PERSISTIR | runtime | Baixa | E estado de sessao/processamento atual | Persistir cache dinamico e quebrar semantica |
| DM1/DM2 requisicoes e respostas diagnosticas | `J1939DiagnosticRequestService`, DTOs de diagnostics | contrato + historico eventual | fluxo diagnostico J1939 | PLANEJAR PARA ETAPA FUTURA | logs/historico ou capturas | Media | Historico diagnostico pode ser util se existir fluxo de atendimento/teste | Criar log permanente antes de politica de retenção e privacidade operacional |
| Offset e calibracao de canais GSA | `GsaChannelOffset*Request/Response`, `GwProtocol` GSA offset TLVs | configuracao | GSA dispatcher/client | PLANEJAR PARA ETAPA FUTURA | banco de modulos | Media | Sao parametros reusaveis e associados a perfis/bench setup | Confundir configuracao persistente com valor temporario de ensaio |
| Setpoint e enable de canais GSA | `GsaChannelSetpointRequest`, `GsaChannelEnableRequest`, status/response | estado operacional/configuracao | GSA runtime | DEPENDE DE DECISAO ARQUITETURAL | banco de modulos ou testes | Media | Alguns parametros podem compor um plano de ensaio | Persistir estado momentaneo da bancada sem semantica de perfil |
| Preferencias de conexao Serial | `SerialConnectionSettings` | configuracao local de transporte | DAL Transport/Serial | DEPENDE DE DECISAO ARQUITETURAL | configuracao local | Baixa | PortName, baud, parity e timeout sao persistiveis como preferencia de operador | Encaixe ruim no Banco de Modulos; pode pertencer a settings da aplicacao |
| Preferencias de conexao Bluetooth | `BluetoothConnectionSettings`, `BluetoothDeviceCatalog` | configuracao local/descoberta | DAL Transport/Bluetooth | DEPENDE DE DECISAO ARQUITETURAL | configuracao local | Baixa | DeviceName, PortName e BaudRate podem ser preferencias | Persistir descoberta transitoria do Windows |
| Eventos de diagnostico UCE/BPM/SPI | `UceGatewayDiagnosticLog`, `DispatcherFifoOverflow`, `AppendCanMirrorOutOfSync` | log/historico | diagnostico operacional | PLANEJAR PARA ETAPA FUTURA | banco de logs/historico | Media | Ja ha escrita em arquivo e valor de auditoria tecnica | Banco pode duplicar logs textuais sem politica unificada |
| Health/protocol events do link SDGW | eventos de `SdgwLinkEngine` e dumps de arquitetura | evento/log | supervisao do link | DOCUMENTAR APENAS | logs/historico | Baixa | Bons sinais operacionais, mas persistencia nao foi demonstrada | Inflar banco com telemetria efemera |
| TLVs compactos e mapeamentos de fio | `GwProtocol`, `SdhToSdgwMapper`, `BoardTlvDispatcher` | contrato/protocolo | mapping runtime | NAO PERSISTIR | nao aplicavel | Baixa | Sao contrato de transporte, nao dado de negocio | Transformar wire format em dado mutavel por engano |
| Espelho CAN/SDCTP e buffers RX | `CanRxMirrorManager`, `SdctpRxMirrorManager`, `CanRxOutputBuffer` | estado runtime | fluxo CAN atual | NAO PERSISTIR | runtime | Baixa | Sao caches e estruturas de sincronizacao | Congelar estado transitorio e piorar consistencia |
| Estado atual de conexao e diagnostico em memoria | `SwitchableTransport`, `ApiCanService` (`IsMirrorOutOfSync`, `LastDiagnosticAt`) | estado runtime | apresentacao/servico atual | NAO PERSISTIR | runtime | Baixa | Representa saude da sessao atual | Reabrir aplicacao com estado obsoleto |
| Dumps gerados em `out/dumps/` | `out/dumps/*` | auditoria documental | agentes e revisao tecnica | DOCUMENTAR APENAS | fora do banco | Baixa | Dumps sao historico de ETAPA, nao entidade operacional | Perder separacao entre auditoria e dominio |

## 6. Detalhamento por dominio

### 6.1 Banco de modulos

Evidencias:

- `Data/Modules/docs/module_database_model_v1.md` ja define perfis, versoes, conectores, pinos, alimentacao, redes CAN, PGNs, sinais, SDH commands, test sequences, test steps, capture sessions e capture events.
- `Data/Modules/docs/module_database_sdh_relation.md` descreve o uso futuro de comandos SDH armazenados com validacao previa via `SdhValidator`.
- `docs/official/02-arquitetura/12-banco-local-api.md` consolida o banco local como `PARCIALMENTE IMPLEMENTADO`.

Leitura arquitetural:

- perfis, conectores, pinagem, redes CAN/J1939 e comandos SDH sao os candidatos mais maduros para persistencia futura;
- offset/calibracao GSA e alguns parametros de preparacao de bancada parecem coerentes com esse dominio, mas a associacao exata com perfil, sequencia ou calibracao geral ainda e `pendente de confirmacao`.

### 6.2 Banco de capturas

Evidencias:

- `module_capture_sessions` e `module_capture_events` ja existem no schema documental/estrutural;
- o sistema possui fluxo de massa CAN via SDCTP, output buffer oficial e decodificacao J1939 por cima de `CanFrameDto`.

Leitura arquitetural:

- sessao de captura encerrada e seus eventos sao bons candidatos para banco;
- frame CAN bruto em tempo real nao deve virar entidade permanente automaticamente;
- PGNs proprietarios observados podem ser persistidos quando vinculados a uma captura ou perfil e explicitamente classificados como observados, nao validados.

### 6.3 Banco de protocolos/catalogos

Evidencias:

- existem arquivos JSON em `Data/Protocols/J1939/`;
- `j1939-pgn-standard-catalog.json` contem entradas catalogadas, varias com `pending verification`;
- o host ja possui servicos e DTOs J1939 implementados em `BLL/Protocols/J1939/` e `DTL/Protocols/J1939/`.

Leitura arquitetural:

- catalogo PGN padrao pode, no futuro, ganhar persistencia/versionamento;
- isso nao deve acontecer enquanto a politica de fonte de verdade entre JSON, codigo e banco nao estiver decidida;
- inferencia: o banco pode ser util para enriquecer catalogos com versao, curadoria e sincronizacao futura, desde que nao substitua silenciosamente contratos consolidados.

### 6.4 Logs e historico

Evidencias:

- `UceGatewayDiagnosticLog.cs` grava arquivo textual em `out/error_logs`;
- ha diagnosticos de CRC/SPI, overflow de FIFO do dispatcher e inconsistencias do espelho CAN;
- `ApiCanService` e fluxos SDCTP mantem estado diagnostico recente em memoria.

Leitura arquitetural:

- diagnosticos relevantes podem futuramente migrar para um banco de logs/historico;
- diagnostico atual em arquivo nao deve ser apagado nem automaticamente redirecionado;
- telemetria de saude de sessao precisa de criterio antes de ganhar persistencia.

## 7. Itens recomendados para banco de modulos

Recomendacao conservadora:

1. perfis e versoes de modulo;
2. conectores, pinos e alimentacao;
3. redes CAN/J1939 previstas por modulo;
4. comandos SDH parametrizados vinculados a perfil;
5. sequencias e passos de teste;
6. PGNs conhecidos/observados do modulo, com clara separacao entre catalogado e observado;
7. parametros GSA persistiveis, especialmente offset/calibracao, apos decisao de escopo.

## 8. Itens recomendados para banco de capturas

1. sessoes de captura encerradas;
2. eventos capturados associados a sessao;
3. quadros CAN/J1939 relevantes ao historico de aprendizado/teste;
4. PGNs proprietarios observados durante captura;
5. diagnosticos decodificados associados a uma sessao quando fizer sentido.

## 9. Itens recomendados para banco de protocolos/catalogos

1. catalogo J1939 padrao, somente apos decidir fonte de verdade;
2. extensoes proprietarias classificadas e versionadas;
3. possivel indexacao futura de SPNs/PGNs, desde que sustentada pelo fluxo funcional;
4. nenhum mapeamento TLV de fio deve ser deslocado para banco nesta fase.

## 10. Itens recomendados para logs/historico

1. diagnosticos UCE/BPM/SPI com severidade, timestamp e payload interpretado;
2. overflow de FIFO do dispatcher;
3. inconsistencias de mirror CAN;
4. leituras diagnosticas J1939 DM1/DM2, se houver fluxo de atendimento, teste ou historico;
5. eventos de link SDGW apenas se uma ETAPA futura comprovar valor de auditoria.

## 11. Itens que NAO devem ir para banco

- TLVs compactos e constantes de wire protocol;
- estado atual do `SwitchableTransport`;
- lista descoberta de portas/dispositivos enquanto scan esta em andamento;
- working set J1939 em memoria;
- estado atual do address registry como cache de sessao;
- mirror SDCTP;
- buffers RX ativos;
- flags temporarias como `IsMirrorOutOfSync`;
- timestamp de ultimo diagnostico apenas como estado da tela/servico atual;
- dumps de ETAPA como entidade de dominio.

## 12. Riscos arquiteturais

1. **Confundir runtime com historico persistivel**  
   O maior risco e transformar cache, mirror, sessao ativa ou estado momentaneo em cadastro permanente.

2. **Duplicar fonte de verdade**  
   Catalogos JSON, contratos em codigo e banco nao podem divergir sem regra explicita.

3. **Persistir protocolo antes de estabilizar responsabilidade**  
   TLV, SDGW e estruturas de transporte devem continuar em contrato/codigo, nao como configuracao mutavel.

4. **Declarar suporte documental acima do estado real**  
   Ha indicio de documentacao oficial de J1939 defasada em `docs/official/06-protocolos/05-j1939.md`, pois o repositorio atual contem servicos e DTOs J1939 implementados. Nesta ETAPA isso foi apenas registrado como risco documental; nenhuma documentacao foi alterada.

5. **Promover dado observado a dado validado**  
   PGN proprietario ou evento de captura precisa preservar origem, confianca e estado de validacao.

6. **Criar entidade sem consumidor funcional real**  
   O proprio bootstrap atual do banco local ja proibe crescimento especulativo de repositories/entidades.

## 13. Proximas ETAPAS sugeridas

1. **Curadoria do dominio de capturas**
   - definir ciclo de vida de `module_capture_sessions` e `module_capture_events`;
   - separar captura bruta, evento interpretado e resumo derivado.

2. **Governanca de catalogos J1939**
   - decidir fonte de verdade entre JSON, banco e codigo;
   - tratar itens com `pending verification`.

3. **Persistencia de configuracoes de modulo**
   - abrir ETAPA funcional para CRUD real de perfis, conectores, CAN/J1939 e comandos SDH quando houver uso concreto.

4. **Historico diagnostico**
   - decidir se logs hoje textuais devem virar eventos estruturados em banco;
   - manter arquivo atual ate decisao formal.

5. **Revisao documental de J1939**
   - reconciliar documentacao oficial com o estado implementado atualmente identificado no codigo.

## 14. Nenhuma implementacao realizada

- nenhum codigo-fonte funcional foi alterado;
- nenhum schema foi alterado;
- nenhuma migration foi criada;
- nenhum banco novo foi criado;
- nenhum repository/provider/service foi implementado;
- nenhuma UI, firmware, SDH, SDGW, SDCTP, UCE, BPM ou GSA foi alterado;
- nenhum commit, branch ou tag foi criado.

## Validacao documental executada

- rastreamento textual e leitura dirigida concluidos;
- relatorio consolidado em `out/dumps/bd_persistence_candidates.md`;
- build nao executado, por esta ETAPA ser exclusivamente documental;
- `git status --short` deve ser consultado na entrega final desta ETAPA.
