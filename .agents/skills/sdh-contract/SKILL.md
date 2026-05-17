# Nome

SDH Contract

## Objetivo

Orientar ETAPAS no contrato semantico SDH, incluindo comandos, validacao, serializacao e mapeamento para comandos compactos.

## Quando usar

Use para `SdhCommand`, `SdhValidator`, `SdhClient`, serializacao textual/JSON, catalogo de comandos e relacao do Banco de Modulos com SDH.

## Quando nao usar

Nao use para massa CAN continua, mirror table, COBS/CRC, SDGW puro ou decodificacao J1939.

## Escopo permitido

- `DTL/Protocols/SDGW/SdhCommand.cs`
- `DTL/Protocols/SDGW/SdhResponse.cs`
- `DAL/Protocols/SDGW/Sdh*.cs`
- docs de protocolo SDH e dumps de contrato.

## Escopo proibido

- Alterar TLVs de fio sem autorizacao.
- Incluir regra de UI.
- Executar comandos a partir do Banco de Modulos sem ETAPA propria.

## Arquivos/pastas provaveis

- `docs/`
- `out/dumps/sdh_contract_export/`
- `tools/dumps/sdh_contract_export/`
- `DAL/Protocols/SDGW/SdhValidator.cs`

## Padroes do projeto

- Comando: `version`, `target`, `op`, `args`, `meta`.
- Forma textual: `sdh/1 <target> <op> chave=valor`.
- Target e logico, nao fisico.
- Argumentos devem ser explicitos.

## Checklist de validacao

- [ ] Build C#.
- [ ] Export/check de contrato, se alterado.
- [ ] Validacao de comandos aceitos/rejeitados.
- [ ] Dump de contrato para mudancas.

## Checklist de entrega

- [ ] Comandos novos/alterados.
- [ ] Compatibilidade legada.
- [ ] Evidencia de validacao.
- [ ] Decisoes pendentes.

## Riscos comuns

- Colocar massa CAN dentro de SDH sem decisao.
- Declarar comando documental como implementado.
- Usar nomenclatura diferente de `SDCTP` para o SimulDIESEL CAN Transport Protocol.

## Regras de nao regressao

- SDH continua semantico.
- Comandos do Banco de Modulos devem ser validaveis por `SdhValidator`.
- Nao quebrar comandos ja aceitos sem autorizacao.


