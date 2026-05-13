# ETAPA interna 4 - FrmRedeCan MDI-child

## Objetivo

Criar a janela MDI-child `FrmRedeCan` para exibir modulos J1939/81 detectados na rede CAN usando identidades enriquecidas pela BLL.

## Branch utilizada

- `feature/j1939-reference-catalogs`

## Arquivos criados

- `local-api/src/SimulDIESEL/SimulDIESEL/UI/FrmRedeCan.cs`

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/DashBoard.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## Conexao do btnRedeCan

O handler `btnRedeCan_Click` em `DashBoard.cs` passou a:

1. Obter `FrmRedeCan.Instance`.
2. Reutilizar a instancia existente quando ja estiver visivel.
3. Chamar `BringToFront()`, `Activate()` e `RefreshSnapshot()` quando a tela ja estiver aberta.
4. Definir `MdiParent = this`.
5. Abrir com `Show()`.

## Fluxo UI -> BLL

Fluxo implementado:

```text
FrmRedeCan
  -> J1939NodeIdentityService
  -> J1939ReferenceCatalogService
  -> IJ1939ReferenceCatalogRepository
  -> SqliteJ1939ReferenceCatalogRepository
  -> IBdServiceProvider
  -> SQLite
```

`FrmRedeCan` recebe snapshot read-only do registry por:

```text
frmUCE_UI.GetJ1939AddressRegistrySnapshotForRedeCan()
  -> FrmUceLogic.GetJ1939AddressRegistrySnapshot()
  -> J1939AddressRegistry.GetSnapshot()
```

## Colunas da grade

- Status
- SA Hex
- SA Decimal
- Fabricante
- Funcao
- Grupo
- Endereco preferencial
- Ultimo RX
- NAME Hex

## Area de detalhes

A selecao da grade exibe:

- NAME completo
- Manufacturer Code + nome
- Function Code + nome
- Industry Group + nome
- Vehicle System + nome
- ECU Instance
- Function Instance
- Vehicle System Instance
- Identity Number
- Arbitrary Address Capable
- Ultimo RX

## Comportamento dos botoes

- `Atualizar`: coleta novo snapshot do registry e reconstroi a visualizacao usando `J1939NodeIdentityService`.
- `Limpar visualizacao`: limpa apenas `_visibleNodes`, grade e detalhes locais do form. Nao altera registry, banco, dados CAN internos ou servicos.

## Validacoes executadas

### Build completo

Comando:

```powershell
MSBuild.exe local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-rede-can-04/
```

Resultado:

- 0 erros
- 0 avisos

Observacao: tentativas em sandbox encontraram bloqueio em `AppData/Local/Microsoft SDKs`; os builds validos foram executados com permissao elevada para o MSBuild do Visual Studio.

### Smoke test da tela

Banco temporario:

- `out/validation-rede-can-04/frm-rede-can-smoke.db`

Validacoes:

- `FrmRedeCan` instanciou em contexto STA.
- `FormTitle=Rede CAN`.
- `ControlCount=3`.
- Snapshot simulado com SA `0`, Manufacturer `94`, Function `0`, Industry Group `2` foi aceito pela tela.
- `NodesAfterRefresh=1`.
- Acionamento do fluxo de limpar visualizacao deixou `NodesAfterClear=0`.

### Separacao de camadas na UI

Validado por busca textual em `FrmRedeCan.cs`:

- Nao referencia `DAL`.
- Nao referencia `Repository`.
- Nao referencia `Provider`.
- Nao referencia `SQLite` ou `Sqlite`.
- Nao referencia `System.Data`.
- Nao contem SQL.
- Nao referencia `IBdServiceProvider`.

### Reuso J1939

Confirmado por diff:

- `J1939NameParser.cs` nao foi alterado.
- `J1939AddressClaimDecoder.cs` nao foi alterado.
- `J1939AddressRegistry.cs` nao foi alterado.

## Limitacoes

- A tela e somente leitura.
- A fonte viva do snapshot continua sendo o registry existente mantido pela logica da UCE.
- A validacao nao abriu uma janela interativa em desktop; a abertura MDI foi validada por build e por inspecao do handler `btnRedeCan_Click`.

## Confirmacao fora de escopo

- Nao houve CRUD de catalogos.
- Nao houve scraping web.
- Nao houve alteracao em firmware.
- Nao houve alteracao em SDGW.
- Nao houve alteracao em SDCTP.
- Nao houve alteracao no fluxo CAN RX consolidado.
