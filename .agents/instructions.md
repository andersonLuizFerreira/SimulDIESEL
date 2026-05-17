# SimulDIESEL Agents Bootstrap

Este arquivo e o bootstrap oficial para agentes de IA no projeto SimulDIESEL.

## Ordem obrigatoria de leitura

1. `README.md`
2. `.agents/instructions.md`
3. `.agents/README.md`
4. `.agents/skills/` conforme o tema da tarefa
5. `docs/` conforme o escopo da ETAPA

## Visao geral

O SimulDIESEL e uma plataforma de bancada para simulacao, diagnostico e validacao de modulos diesel.

O projeto combina:

- aplicacao local C# WinForms em `local-api/src/SimulDIESEL/SimulDIESEL`;
- camada host organizada em UI, BLL, DAL e DTL;
- gateway embarcado BPM;
- firmware UCE para execucao fisica CAN;
- firmware GSA para geracao de sinais analogicos;
- Banco de Modulos em `Data/Modules`;
- contratos de protocolo em SDH, SDGW, SDCTP e J1939.

## Arquitetura base

Use como modelo mental conservador:

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

Responsabilidades principais:

- `UI`: formularios WinForms, apresentacao e interacao do operador.
- `BLL`: casos de uso, FormsLogic, clients de boards, servicos de aplicacao e decoders de alto nivel.
- `DAL`: SDH, SDGW, sessao, framing, scheduler, supervisor, transportes, banco e repositories.
- `DTL`: DTOs, enums e contratos compartilhados.
- `SDH`: contrato semantico de comandos de operacao/configuracao.
- `SDGW`: gateway/transporte confiavel, framing, COBS, CRC, ACK/ERR, sequencia, retry e eventos.
- `SDCTP`: protocolo de massa CAN RX/TX, mirror, buffers, compactacao e sincronizacao.
- `J1939`: camada de decodificacao automotiva sobre `CanFrameDto`; nao deve poluir SDGW.
- `BPM`: gateway embarcado entre host e boards.
- `UCE`: execucao fisica CAN, LED e servicos de hardware por SPI.
- `GSA`: geracao de sinais analogicos.
- `Banco de Modulos`: base para perfis de modulos, comandos SDH, sequencias de teste, capturas CAN/J1939 e configuracoes.

## Fontes de verdade

Quando houver divergencia, use a ordem:

1. codigo-fonte implementado;
2. contratos tecnicos efetivos;
3. documentacao consolidada em `docs/`;
4. dumps recentes em `out/dumps/` apenas como evidencia temporaria;
5. historico via Git.

Regras:

- `docs/` e a unica fonte documental oficial do projeto.
- Historico e legado pertencem ao Git.
- `out/dumps/` nao e documentacao oficial.
- Se a divergencia nao puder ser resolvida com leitura, registre como `pendente de confirmacao`.
- Nao escolha arbitrariamente entre fontes divergentes.

## Regras de escopo

- Use sempre o termo `ETAPA`; nunca use `FASE`.
- Nao altere nada fora do escopo solicitado.
- Nao modifique contratos consolidados sem autorizacao explicita.
- Nao remova codigo legado sem pedido explicito.
- Nao altere firmware ao trabalhar na API, salvo pedido explicito.
- Nao altere UI ao trabalhar em BLL/DAL, salvo pedido explicito.
- Nao altere banco ao trabalhar em UI, salvo pedido explicito.
- Nao misture UI, BLL, DAL, DTL, firmware e protocolo em uma mesma ETAPA sem autorizacao.
- Nao invente comportamento inexistente; marque como `PLANEJADO`, `LEGADO`, `PARCIALMENTE IMPLEMENTADO` ou `pendente de confirmacao`.
- Nao use mocks/fakes sem deixar explicito.
- Nao esconda warnings, erros, falhas de build ou validacoes incompletas.
- Nunca declare uma implementacao como concluida sem validacao compativel com o escopo.

## Regras de consistencia

- Nao invente arquivos inexistentes.
- Nao invente APIs inexistentes.
- Nao invente endpoints.
- Nao invente suporte de protocolo.
- Nao assuma comportamento nao validado.
- Se houver duvida, registre como `pendente de confirmacao`.

## Nomenclatura oficial

- `SDH`: contrato semantico de comandos.
- `SDGW`: SimulDIESEL Gateway, transporte/gateway.
- `SDCTP`: SimulDIESEL CAN Transport Protocol. Esta e a unica nomenclatura oficial para o protocolo de transporte CAN do projeto.
- `UCE`: Unidade de Comunicacao Externa.
- `BPM`: Backplane Manager Module.
- `GSA`: Gerador de Sinais Analogicos.
- `DTL`: camada de DTOs/contratos compartilhados.

## Regras de ETAPA

Toda ETAPA deve declarar:

- tema e objetivo;
- escopo permitido;
- fora de escopo;
- arquivos provaveis;
- regras de implementacao;
- validacao obrigatoria;
- necessidade de dump;
- entrega esperada;
- restricoes e rollback.

ETAPAS de congelamento devem registrar exatamente o que fica congelado, o que permanece legado, o que esta pendente e quais contratos nao podem ser alterados sem nova autorizacao.

## Validacao

Valide apenas o que se aplica ao escopo.

Exemplos:

- API C# WinForms: build da solucao `local-api/src/SimulDIESEL/SimulDIESEL.sln`.
- Firmware BPM/UCE/GSA: `platformio run` na pasta correspondente, quando a ETAPA permitir.
- Protocolos CAN/SDCTP/J1939: scripts em `tools/testes/`, quando pertinentes.
- Documentacao: conferir arquivos criados, links basicos e coerencia com `docs/`.

Para ETAPA exclusivamente documental, nao execute build funcional se nao houver alteracao de codigo; registre que nao se aplica.

Nunca promova automaticamente algo de `PLANEJADO` para `IMPLEMENTADO` sem evidencia concreta no codigo, teste, build ou validacao aplicavel.

## Sincronizacao do ambiente de desenvolvimento

Sempre que codigo C# for criado, removido ou movido:

- mantenha `.sln` e `.csproj` sincronizados;
- garanta que os arquivos aparecam corretamente no ambiente Visual Studio;
- evite codigo orfao fora da solucao;
- evite arquivos nao incluidos no projeto;
- preserve a organizacao da Solution Explorer;
- preserve a coerencia entre filesystem e projeto carregado.

Implementacao incompleta no ambiente de desenvolvimento nao e considerada entrega valida. Criar arquivo fisico sem integra-lo ao projeto e erro de ETAPA. Mudancas C# devem ser visiveis e rastreaveis pelo desenvolvedor no ambiente de desenvolvimento.

## Entrega obrigatoria

Toda entrega deve conter:

- lista de arquivos alterados/criados/removidos;
- resumo objetivo do que mudou;
- validacoes executadas e resultados;
- warnings, erros ou validacoes nao executadas;
- dump gerado, quando aplicavel;
- pontos pendentes;
- confirmacao de que rollback foi preservado.

## Commits, tags e rollback

- Nao faca commit sem autorizacao explicita do usuario.
- Nao crie branch sem autorizacao explicita.
- Nao crie tag sem autorizacao explicita.
- Preserve rollback: mantenha mudancas pequenas, rastreaveis e documentadas.
- Nunca use comandos destrutivos para limpar alteracoes existentes de outro autor.

## Atualizacao obrigatoria de documentacao

Sempre que uma ETAPA for concluida, o agente deve:

- identificar a documentacao impactada;
- atualizar os arquivos relevantes em `docs/`;
- preservar a coerencia da estrutura documental;
- refletir o estado real da implementacao;
- registrar mudancas arquiteturais, contratos, fluxos ou limitacoes;
- evitar documentacao desatualizada apos a conclusao da ETAPA.

A documentacao faz parte da entrega. Dumps registram auditoria temporaria, mas nao substituem a documentacao oficial.

## CODEX

A pasta `.codex/` e apenas adaptador especifico do CODEX.

O fluxo comum de governanca para todos os agentes fica em `.agents/`.

Se `.codex/` divergir de `.agents/`, a divergencia deve ser registrada e corrigida antes de iniciar nova ETAPA.
