# Project Conventions - SimulDIESEL

## Convenções obrigatorias

- Use `ETAPA`, nunca `FASE`.
- Preserve a arquitetura `UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA`.
- Separe SDH, SDGW, SDCTP e J1939.
- Nao declare suporte implementado sem evidencia no codigo, contrato ou dump recente.
- Nao altere codigo, firmware, banco, UI ou contratos quando o pedido for documental.

## Fronteiras de camada

| Camada | Pode conter | Nao deve conter |
| --- | --- | --- |
| UI | formularios, controles, apresentacao, eventos de operador | TLV bruto, SDGW direto, regra de protocolo de baixo nivel |
| BLL | casos de uso, FormsLogic, clients, servicos de aplicacao | framing, COBS, CRC, acesso direto a SerialPort |
| DAL | SDH, SDGW, sessao, scheduler, transportes | regra de UI, decoders automotivos |
| DTL | DTOs, enums, contratos | logica de execucao, IO, retry |
| SDGW | transporte, framing, ACK/ERR, retry | regra CAN, J1939, UI, negocio |
| SDCTP | massa CAN, mirror, buffers, TX/RX table | apresentacao, regras de tela, framing SDGW |
| Firmware | execucao embarcada e servicos fisicos | dependencias de UI/host |

## Estados documentais

Use estes estados quando houver risco de ambiguidade:

- `IMPLEMENTADO`: confirmado em codigo/contrato.
- `PARCIALMENTE IMPLEMENTADO`: existe, mas com limite conhecido.
- `PLANEJADO`: descrito como futuro ou placeholder.
- `LEGADO`: preservado por compatibilidade ou historico.
- `pendente de confirmacao`: nao ha evidencia suficiente.

## Documentacao viva

- `/docs/` representa o estado consolidado do projeto.
- Dumps registram historico e auditoria, mas nao substituem a documentacao oficial.
- Documentacao oficial deve ser revisada e atualizada apos ETAPAS concluidas.
- Nao deixe documentacao contradizendo o codigo consolidado.
- Nao dependa apenas de dumps para explicar a arquitetura atual.

## Ambiente de desenvolvimento C#

- Arquivos C# criados, removidos ou movidos devem manter `.sln` e `.csproj` sincronizados.
- O Solution Explorer deve refletir o estado real do filesystem.
- Arquivo C# fisico fora do projeto carregado e erro de ETAPA.
- Build valido deve compilar os arquivos corretos, nao apenas o subconjunto incluido no projeto.

## Regras para contratos

- SDH e contrato semantico de comandos.
- SDGW e transporte/gateway.
- SDCTP e contrato de massa CAN.
- J1939 deve consumir `CanFrameDto` e catalogos proprios; nao deve entrar em SDGW.
- Banco de Modulos guarda configuracao e sequencias; nao executa comandos por si so nesta etapa.

## Regras de entrega

Sempre entregue:

- arquivos criados/alterados;
- validacao executada;
- validacao nao executada e motivo;
- divergencias encontradas;
- dump quando exigido;
- confirmacao sobre escopo e rollback.
