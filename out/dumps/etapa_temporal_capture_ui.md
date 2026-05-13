# ETAPA - Integracao UI da captura temporal J1939

Data: 2026-05-13

## Objetivo

Integrar a captura temporal J1939 na aba `Dados J1939` da tela UCE.

## Arquivos alterados

- `local-api/src/SimulDIESEL/SimulDIESEL/UI/frmUCE_UI.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/FormsLogic/UCE/FrmUceLogic.cs`
- `local-api/src/SimulDIESEL/SimulDIESEL/SimulDIESEL.csproj`

## UI criada

Na aba `Dados J1939` foram adicionados:

- botao `Iniciar Captura`;
- botao `Finalizar Captura`;
- label de estado `Captura: Parado` / `Captura: Capturando`.

## Fluxo implementado

Iniciar captura:

```text
frmUCE_UI
  -> FrmUceLogic.ClearJ1939TemporalCapture()
  -> FrmUceLogic.StartJ1939TemporalCapture()
```

Finalizar captura:

```text
frmUCE_UI
  -> FrmUceLogic.StopJ1939TemporalCapture()
  -> SaveFileDialog
  -> FrmUceLogic.ExportJ1939TemporalCapture()
  -> J1939CaptureExportService
```

Registro de frames:

```text
DrainCanRxOutputBuffer()
  -> ProcessJ1939Frame()
  -> FrmUceLogic.TryDecodeJ1939Frame()
  -> UpdateJ1939DataRow()
  -> FrmUceLogic.RegisterJ1939TemporalCaptureMessage()
  -> J1939TemporalCaptureService
```

## Confirmacoes de escopo

- A captura recebe somente `J1939DataMonitorMessageDto` ja produzido pela pipeline atual da aba `Dados J1939`.
- Nao houve reprocessamento de CAN bruto.
- Nao houve recriacao de decoder.
- Nao houve alteracao no parser J1939.
- Nao houve alteracao em SDGW, SDCTP ou firmware.
- A grade `Dados J1939` nao e limpa automaticamente ao iniciar/finalizar captura.
- Exportacao ocorre apos escolha do usuario por `SaveFileDialog`.

## Validacao

Build C# completo:

- 0 erros
- 0 avisos

Validacao automatizada de `SaveFileDialog` nao executada por depender de interacao desktop. O fluxo compila e o handler `BtnJ1939CaptureStop_Click` instancia `SaveFileDialog` com filtros `.md` e `.txt`.

## Limitacoes

- Responsividade sob trafego real continuo ainda depende de validacao de bancada.
- Teste manual com EST nao executado neste ambiente.

## Rollback

Rollback preservado: alteracoes concentradas na aba `Dados J1939` e em metodos novos de `FrmUceLogic`.
