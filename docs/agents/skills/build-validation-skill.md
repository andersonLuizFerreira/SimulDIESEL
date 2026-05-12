# Skill: Build Validation

## Quando usar

Use para validar build da API C#, firmware PlatformIO, scripts de contrato, scripts J1939/SDCTP e validadores de banco.

## Quando nao usar

Nao use para corrigir codigo automaticamente quando a ETAPA e somente validacao.

## Escopo permitido

- Executar comandos de build/teste.
- Registrar saida relevante.
- Criar dump/relatorio, se pedido.

## Escopo proibido

- Esconder warnings/erros.
- Alterar codigo para passar build sem autorizacao.

## Arquivos/pastas provaveis

- `local-api/src/SimulDIESEL/SimulDIESEL.sln`
- `hardware/firmware/*/platformio.ini`
- `tools/testes/`
- `tools/dumps/`
- `out/dumps/`

## Padroes do projeto

- API C# usa .NET Framework 4.7.2.
- Firmware usa PlatformIO.
- Banco usa scripts em `tools/dumps/module_database_model/`.
- Contratos SDH possuem scripts em `tools/dumps/sdh_contract_export/`.

## Checklist de validacao

- [ ] Comando executado no diretório correto.
- [ ] Resultado registrado.
- [ ] Warnings e erros preservados.
- [ ] Validacao nao aplicavel foi justificada.

## Checklist de entrega

- [ ] Comando(s) executado(s).
- [ ] Resultado resumido.
- [ ] Falhas e proximos passos.
- [ ] Dump/relatorio se exigido.

## Riscos comuns

- Rodar build desnecessario em ETAPA documental.
- Confundir falha de ambiente com falha de codigo sem evidencias.
- Omitir warnings.

## Regras de nao regressao

- Validacao deve ser reproduzivel.
- Build/teste nao deve alterar codigo-fonte funcional.
- Falhas devem permanecer visiveis.
