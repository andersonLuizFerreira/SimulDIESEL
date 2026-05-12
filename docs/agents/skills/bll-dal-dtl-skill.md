# Skill: BLL/DAL/DTL

## Quando usar

Use para trabalhos no host C# que envolvam casos de uso, clients, servicos, validadores, mapeadores, DTOs, transporte e sessao.

## Quando nao usar

Nao use para redesenhar UI, alterar firmware ou modificar Banco de Modulos sem pedido especifico.

## Escopo permitido

- `BLL/`
- `DAL/`
- `DTL/`
- Documentos e dumps relacionados.

## Escopo proibido

- Mudar firmware para acomodar mudanca host sem autorizacao.
- Alterar contratos publicos sem congelamento.
- Colocar regra de apresentacao na DAL/DTL.

## Arquivos/pastas provaveis

- `BLL/Boards/`
- `BLL/Services/CAN/`
- `BLL/Protocols/J1939/`
- `DAL/Protocols/SDGW/`
- `DAL/Transport/`
- `DTL/`

## Padroes do projeto

- BLL decide casos de uso e usa DTOs.
- DAL faz SDH/SDGW/transporte.
- DTL contem contratos sem IO.
- `SdgwHostSession` esta fisicamente na BLL, mas estruturalmente compoe a borda superior da DAL.
- Ao criar, mover ou remover arquivos C#, verificar integracao correta em `.csproj`.
- Garantir que a Solution Explorer reflita o estado real do projeto.
- Nao deixar arquivos C# orfaos fora do carregamento da solucao.
- Preservar organizacao estrutural das camadas no filesystem e no projeto.

## Checklist de validacao

- [ ] Build C#.
- [ ] Novos arquivos C# incluidos no `.csproj` correto.
- [ ] Nenhum arquivo C# orfao fora do carregamento da solucao.
- [ ] Solution Explorer/projeto Visual Studio coerente com o filesystem.
- [ ] BLL nao recebeu framing/COBS/CRC.
- [ ] DAL nao conhece UI.
- [ ] DTL permanece sem IO.
- [ ] Contratos alterados foram documentados.

## Checklist de entrega

- [ ] Classes alteradas por camada.
- [ ] Contratos impactados.
- [ ] Testes/build.
- [ ] Pendencias e rollback.

## Riscos comuns

- Misturar controle SDH com massa SDCTP sem documentar.
- Fazer DTL executar logica.
- Colocar decoder automotivo dentro de SDGW.
- Criar arquivo C# fisico sem incluir no projeto.

## Regras de nao regressao

- Manter separacao BLL/DAL/DTL.
- Preservar contratos legados ainda usados.
- Nao ampliar acoplamento entre UI e DAL.
- Manter `.sln` e `.csproj` sincronizados com arquivos C# criados, removidos ou movidos.
