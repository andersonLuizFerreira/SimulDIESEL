# ETAPA 03 - Roteamento SDCTP bruto no Dispatcher UCE

## 1. Objetivo

Esta ETAPA reduz o conhecimento detalhado de massa CAN dentro de `UceClient` e `UceParsers`.

O Dispatcher UCE da API passa a identificar apenas que um TLV pertence ao dominio SDCTP e a publicar um envelope bruto por `SdctpRawEventReceived`. O parsing detalhado de eventos de massa CAN passa para o dominio SDCTP/CanService da API, preservando contratos TLV, firmware, SDGW e comportamento funcional.

## 2. Arquivos alterados

| Arquivo | Alteracao |
| --- | --- |
| `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/SDCTP/SdctpRawEventDto.cs` | Criado DTO bruto SDCTP com `Type`, `Payload` e `TimestampUtc`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventParser.cs` | Criado parser SDCTP para identificar TLVs do dominio e interpretar eventos de massa CAN fora do Dispatcher. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/SDCTP/SdctpEventProcessor.cs` | Ajustado para processar `SdctpRawEventDto` e delegar parsing de `CAN_RX_EVENT` ao parser SDCTP. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs` | Passou a disparar `SdctpRawEventReceived` para eventos SDCTP, sem parsing profundo no fluxo principal. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceDispatcher.cs` | Passou a expor e encaminhar `SdctpRawEventReceived`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/CAN/ApiCanService.cs` | Passou a assinar evento bruto SDCTP e a processar massa CAN via `SdctpEventProcessor`. |
| `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj` | Incluiu os novos arquivos de DTO e parser no projeto C#. |

## 3. Antes

O fluxo principal de eventos recebidos da UCE estava concentrado em `UceClient.OnEventReceived`.

Nesse fluxo, `UceClient` recebia o frame vindo do SDGW e tentava parsear diretamente:

- `UceParsers.TryReadCanRxEvent` para `CAN_RX_EVENT`.
- `UceParsers.TryReadCanCrudEvent` para eventos de CRUD/mirror CAN.
- Eventos ja parseados eram expostos em `CanRxEventReceived` e `CanCrudEventReceived`.

Com isso, `UceClient` e `UceParsers` conheciam detalhes internos do payload de massa CAN, como indices, flags, mascara de dados, TIC e estrutura de linhas RX.

## 4. Depois

Fluxo atual apos a ETAPA:

```text
UCE TLV
-> SDGW
-> UceClient
-> identifica Type como dominio SDCTP
-> SdctpRawEventReceived
-> UceDispatcher
-> SdctpRawEventReceived
-> ApiCanService
-> SdctpEventProcessor / SdctpEventParser
-> CanRxMirrorManager / CanRxOutputBuffer
```

`UceClient` continua validando o envelope TLV generico via `SdgwFrameCodec`, pois isso pertence ao recebimento do frame. A classificacao SDCTP fica limitada ao `Type`. O payload bruto segue em `SdctpRawEventDto.Payload`.

`ApiCanService` agora assina `IUceDispatcher.SdctpRawEventReceived`. Quando recebe um evento bruto:

- `CAN_RX_EVENT` e convertido em `UceCanRxEvent` por `SdctpEventParser.TryReadCanRxEvent`.
- Eventos CRUD/mirror sao processados por `SdctpEventProcessor.ProcessEvent(SdctpRawEventDto)`.
- `CanRxOutputBuffer` continua sendo alimentado pela mesma regra funcional ja existente.

## 5. Eventos SDCTP roteados como dominio

| Codigo | Nome |
| --- | --- |
| `0x28` | `CAN_RX_EVENT` |
| `0x40` | `CAN_CREATE` |
| `0x41` | `CAN_EDIT` |
| `0x42` | `CAN_DELETE` |
| `0x44` | `CAN_ROW` |
| `0x45` | `CAN_READ_ALL_DONE` |
| `0x46` | `CAN_TIC` |

## 6. Parsing que saiu do Dispatcher

O fluxo principal deixou de depender de:

- `UceParsers.TryReadCanRxEvent` dentro de `UceClient.OnEventReceived`.
- `UceParsers.TryReadCanCrudEvent` dentro de `UceClient.OnEventReceived`.
- Eventos parseados `CanRxEventReceived` e `CanCrudEventReceived` para alimentar `ApiCanService`.

O parsing equivalente foi movido para:

- `SdctpEventParser.TryReadRawEvent`
- `SdctpEventParser.TryReadCanRxEvent`
- `SdctpEventProcessor.ProcessEvent(SdctpRawEventDto)`

## 7. Compatibilidades mantidas

Foram mantidos temporariamente:

- `UceClient.CanRxEventReceived`
- `UceClient.CanCrudEventReceived`
- `IUceDispatcher.CanRxEventReceived`
- `IUceDispatcher.CanCrudEventReceived`
- `UceDispatcher.CanRxEventReceived`
- `UceDispatcher.CanCrudEventReceived`

Esses eventos foram marcados com comentario:

```csharp
// TODO ETAPA 03: evento mantido apenas para compatibilidade temporaria. Preferir SdctpRawEventReceived.
```

Eles permanecem para evitar quebra de contrato interno durante a transicao, mas nao sao mais o caminho usado por `ApiCanService`.

`UceParsers.TryReadCanRxEvent` e `UceParsers.TryReadCanCrudEvent` tambem foram preservados no arquivo original para compatibilidade e rastreabilidade. Nao houve remocao fisica nesta ETAPA.

## 8. Contratos preservados

- TLVs nao alterados.
- Firmware UCE nao alterado.
- Firmware BPM nao alterado.
- SDGW nao alterado.
- UI nao alterada visualmente.
- `CanRxOutputBuffer` continua sendo a saida oficial da massa CAN.
- TX CAN nao foi reclassificado.
- `CAN_READ_ALL` nao foi reclassificado.
- Wrappers temporarios da ETAPA 02 nao foram removidos.

## 9. Validacao

Comando executado:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'G:\PROJETOS\SIMULADORES\SimulDIESEL\local-api\src\SimulDIESEL\SimulDIESEL.sln' /t:Build /p:Configuration=Debug /p:OutDir='G:\PROJETOS\SIMULADORES\SimulDIESEL\out\build-etapa03\'
```

Resultado:

- Build C# compilou com sucesso.
- Erros: 0.
- Avisos: 2.

Avisos registrados:

| Arquivo | Aviso |
| --- | --- |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs` | `CS0067`: evento `UceClient.CanRxEventReceived` nunca e usado. |
| `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Boards/UCE/UceClient.cs` | `CS0067`: evento `UceClient.CanCrudEventReceived` nunca e usado. |

Observacao de validacao:

- A primeira tentativa de build no sandbox falhou por acesso negado a cache do SDK em `C:\Users\Escritorio\AppData\Local\Microsoft SDKs`.
- O build foi repetido com permissao escalada e `OutDir` dedicado em `out/build-etapa03`, compilando com sucesso.

## 10. Resultado

A ETAPA 03 foi aplicada com mudancas pequenas e reversiveis.

Resultado tecnico:

- `UceClient` passou a atuar como roteador por dominio para massa CAN SDCTP.
- `UceDispatcher` passou a expor `SdctpRawEventReceived`.
- `ApiCanService` deixou de depender dos eventos CAN parseados pelo Dispatcher.
- Parsing de massa CAN foi movido para classes no namespace SDCTP.
- `CanRxOutputBuffer` e mirror table mantiveram o papel existente.
- Nao houve alteracao funcional intencional.

Pendencias:

- Remover eventos parseados antigos somente em ETAPA futura, depois de confirmar que nao ha consumidores restantes.
- Avaliar em ETAPA futura se `UceParsers` deve manter parsers CAN legados ou se tudo deve migrar fisicamente para SDCTP.
- TX CAN permanece ambiguo.
- `CAN_READ_ALL` permanece ambiguo.
- A fronteira final entre fachada de controle e fachada de massa continua dependendo das decisoes pendentes ja congeladas.
