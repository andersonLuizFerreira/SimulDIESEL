# ETAPA 05 - Remoção dos eventos legados do Dispatcher UCE

## 1. Objetivo

Remover os eventos parseados legados `CanRxEventReceived` e `CanCrudEventReceived` do Dispatcher UCE, porque o fluxo oficial de massa CAN e o evento bruto `SdctpRawEventReceived`.

O fluxo oficial permanece:

```text
UCE TLV
-> UceClient
-> SdctpRawEventReceived
-> ApiCanService
-> SdctpEventProcessor / SdctpEventParser
-> CanRxMirrorManager / CanRxOutputBuffer
```

## 2. Arquivos alterados

| Arquivo | Alteracao |
| --- | --- |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs` | Removidos os eventos legados `CanRxEventReceived` e `CanCrudEventReceived`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs` | Removidos eventos, inscricoes e repasses legados do dispatcher. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs` | Confirmado uso de `SdctpRawEventReceived`; metodos privados foram renomeados para nao parecerem eventos legados. |

## 3. Antes

- `UceClient` expunha `CanRxEventReceived` e `CanCrudEventReceived`.
- `IUceDispatcher` tambem declarava esses eventos.
- `UceDispatcher` assinava os eventos do `UceClient` e os repassava.
- Esses eventos ja nao alimentavam o `ApiCanService`, que desde a ETAPA 03 usa `SdctpRawEventReceived`.
- O build gerava avisos `CS0067` para os eventos antigos no `UceClient`.

## 4. Depois

- Dispatcher UCE expõe apenas `SdctpRawEventReceived` para massa CAN.
- `ApiCanService` consome somente `IUceDispatcher.SdctpRawEventReceived`.
- Eventos parseados antigos foram removidos de `UceClient`, `IUceDispatcher` e `UceDispatcher`.
- Inscricoes `_client.CanRxEventReceived` e `_client.CanCrudEventReceived` foram removidas.
- Handlers de repasse `OnCanRxEventReceived` e `OnCanCrudEventReceived` foram removidos do `UceDispatcher`.

## 5. Verificação de dependências

Busca executada:

```powershell
rg -n "CanRxEventReceived|CanCrudEventReceived" local-api/src/SimulDIESEL/SimulDIESEL/BLL local-api/src/SimulDIESEL/SimulDIESEL/UI local-api/src/SimulDIESEL/SimulDIESEL/DTL
```

Resultado:

- Nenhuma ocorrencia restante.

Busca adicional por inscricoes:

```powershell
rg -n "_uceDispatcher\.(CanRxEventReceived|CanCrudEventReceived)|_client\.(CanRxEventReceived|CanCrudEventReceived)" local-api/src/SimulDIESEL/SimulDIESEL
```

Resultado:

- Nenhuma ocorrencia restante.

## 6. Parsers legados ainda presentes

Ainda existem em `UceParsers`:

- `UceParsers.TryReadCanRxEvent`
- `UceParsers.TryReadCanCrudEvent`

Eles ficaram sem uso no fluxo principal do Dispatcher UCE. A remocao fisica desses parsers fica como pendencia para ETAPA futura, porque esta ETAPA remove apenas eventos e repasses legados, sem limpar parsers por rastreabilidade.

Tambem existem parsers SDCTP oficiais:

- `SdctpEventParser.TryReadCanRxEvent`
- `SdctpEventProcessor.TryReadCanRxEvent`

Esses permanecem como parte do fluxo oficial SDCTP.

## 7. Contratos preservados

- TLVs nao alterados.
- SDGW nao alterado.
- Firmware UCE nao alterado.
- Firmware BPM nao alterado.
- UI visual nao alterada.
- `CanRxOutputBuffer` continua saida oficial de massa CAN.
- TX CAN continua SDCTP.
- `CAN_READ_ALL` nao foi reintroduzido.

## 8. Validação

Primeira tentativa de build:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug /p:OutDir='G:\PROJETOS\SIMULADORES\SimulDIESEL\out\build-etapa05\'
```

Resultado:

- Falhou por sandbox: acesso negado a `C:\Users\Escritorio\AppData\Local\Microsoft SDKs`.

Segunda tentativa:

- Mesmo comando, com permissao escalada.

Resultado:

- Build C# compilou com sucesso.
- Avisos: 0.
- Erros: 0.
- Os avisos `CS0067` de `CanRxEventReceived` e `CanCrudEventReceived` desapareceram.

## 9. Resultado

A ETAPA 05 compilou com sucesso.

Resultado tecnico:

- Eventos legados removidos.
- Dispatcher UCE ficou alinhado ao fluxo bruto SDCTP.
- `ApiCanService` segue consumindo `SdctpRawEventReceived`.
- UI segue consumindo `TryReadRxFrame`, `CanRxOutputBuffer` e snapshots.
- Nao houve alteracao funcional intencional no fluxo SDCTP bruto.

Pendencias reais:

- Remover `UceParsers.TryReadCanRxEvent` e `UceParsers.TryReadCanCrudEvent` em ETAPA futura, se nao houver necessidade de rastreabilidade.
- Continuar reduzindo conhecimento de massa CAN em areas antigas que ainda existam fora do fluxo principal.
