# Nome

Build Validation

## Objetivo

Orientar builds, scripts de contrato, validadores J1939/SDCTP e validadores do Banco de Modulos.

## Quando usar

Use para validar API C#, firmware PlatformIO, scripts de contrato, scripts J1939/SDCTP e validadores de banco.

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

- [ ] Comando executado no diretorio correto.
- [ ] Resultado registrado.
- [ ] Warnings e erros preservados.
- [ ] Validacao nao aplicavel foi justificada.
- [ ] Novos arquivos C# aparecem corretamente na solucao/projeto.
- [ ] `.csproj` sincronizado com arquivos C# criados, removidos ou movidos.
- [ ] Integridade da Solution Explorer conferida quando houver mudanca C#.
- [ ] Ausencia de arquivos C# orfaos confirmada.
- [ ] Build utiliza os arquivos C# corretos.

## Checklist de entrega

- [ ] Comandos executados.
- [ ] Resultado resumido.
- [ ] Falhas e proximos passos.
- [ ] Dump/relatorio se exigido.

## Riscos comuns

- Rodar build desnecessario em ETAPA documental.
- Confundir falha de ambiente com falha de codigo sem evidencias.
- Omitir warnings.
- Declarar build valido enquanto arquivos C# novos ficaram fora do projeto.

## Regras de nao regressao

- Validacao deve ser reproduzivel.
- Build/teste nao deve alterar codigo-fonte funcional.
- Falhas devem permanecer visiveis.
- Validacao C# deve conferir coerencia entre filesystem, `.csproj` e solucao.


