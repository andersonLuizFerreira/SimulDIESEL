# Skill: WinForms UI

## Quando usar

Use para telas WinForms, controles visuais, eventos de usuario, apresentacao de estado e integracao da UI com FormsLogic.

## Quando nao usar

Nao use para alterar SDGW, SDCTP, firmware, schema do banco ou contratos de fio.

## Escopo permitido

- `local-api/src/SimulDIESEL/SimulDIESEL/UI/`
- `DashBoard.cs` e `DashBoard.Designer.cs`
- `BLL/FormsLogic/` apenas como adaptador entre tela e caso de uso.

## Escopo proibido

- Acesso direto da UI a TLV bruto, SDGW ou `SerialPort`.
- Regras de COBS, CRC, retry, mirror table ou protocolo de baixo nivel.
- Mudancas em firmware ou banco sem pedido explicito.

## Arquivos/pastas provaveis

- `UI/frmUCE_UI.cs`
- `UI/frmGSA_UI.cs`
- `UI/frmPortaSerial_UI.cs`
- `UI/Controls/`
- `BLL/FormsLogic/`

## Padroes do projeto

- UI chama `Frm*Logic`.
- UI consome DTOs e resultados tratados.
- Para massa CAN, UI deve consumir `TryReadRxFrame`/`CanFrameDto`, nao TLV.

## Checklist de validacao

- [ ] Build C# quando houver alteracao funcional.
- [ ] Nenhum acoplamento novo com SDGW/TLV bruto.
- [ ] Eventos de UI continuam chamando BLL/FormsLogic.
- [ ] Textos e estados visuais permanecem coerentes.

## Checklist de entrega

- [ ] Forms/controles alterados.
- [ ] FormsLogic impactada.
- [ ] Resultado de build ou motivo de nao execucao.
- [ ] Capturas/manual QA, se aplicavel.

## Riscos comuns

- Resolver problema visual alterando protocolo.
- Colocar parser de baixo nivel no form.
- Alterar `.Designer.cs` manualmente sem necessidade.

## Regras de nao regressao

- UI nao conhece SDGW bruto.
- UI nao deve ser fonte de verdade de contrato.
- UI nao altera banco, firmware ou contratos sem ETAPA propria.
