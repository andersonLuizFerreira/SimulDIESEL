# SimulDIESEL Agents Bootstrap

Este arquivo e o bootstrap oficial para agentes de IA no projeto SimulDIESEL.

## Ordem obrigatoria de leitura

1. `README.md`
2. `.agents/instructions.md`
3. `.agents/README.md`
4. `.agents/skills/`
5. `docs/`

## Visao geral

O SimulDIESEL e uma plataforma de bancada para simulacao, diagnostico e validacao de modulos diesel.

Arquitetura base:

```text
UI -> BLL -> DAL -> DTL -> SDGW -> BPM -> SPI/BT/SERIAL -> UCE/GSA
```

## Regras fundamentais

- `docs/` e a unica fonte documental oficial do projeto.
- Historico e legado pertencem ao Git.
- `out/dumps/` sao evidencias temporarias; nao sao documentacao oficial.
- Nao implementar comportamento nao validado.
- Nao ampliar escopo automaticamente.
- Nao misturar UI, BLL, DAL, DTL, firmware e protocolos sem autorizacao.
- Registrar ambiguidades como `pendente de confirmacao`.
- Usar sempre o termo `ETAPA`; nunca `FASE`.

## Protocolos oficiais

- SDH = contrato semantico.
- SDGW = gateway/transporte.
- SDCTP = transporte CAN RX/TX.
- J1939 = decodificacao sobre CanFrameDto.

## Validacao

Valide apenas o que se aplica ao escopo da ETAPA.

- API C#: build da solucao.
- Firmware: `platformio run` quando permitido.
- Protocolos: scripts em `tools/testes/`.
- Documentacao: coerencia com `docs/`.

Nunca promover `PLANEJADO` para `IMPLEMENTADO` sem evidencia.

## Entrega obrigatoria

Toda entrega deve registrar:

- arquivos alterados;
- resumo objetivo;
- validacoes executadas;
- pendencias;
- rollback preservado.
