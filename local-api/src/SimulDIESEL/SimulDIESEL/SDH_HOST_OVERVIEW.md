# SDH no host SimulDIESEL

## Classes criadas

- `DTL/SdhCommand.cs`
- `DTL/SdhResponse.cs`
- `DTL/SdhTarget.cs`
- `BLL/SDH/SdhTextParser.cs`
- `BLL/SDH/SdhTextSerializer.cs`
- `BLL/SDH/SdhValidator.cs`
- `BLL/SDH/SdhToSggwMapper.cs`
- `BLL/SDH/SdhClient.cs`
- `BLL/Boards/GsaClient.cs`

## Fluxo atual

`GsaClient.SetLedAsync(bool)`
-> monta `sdh/1 GSA.led set state=on|off`
-> `SdhClient`
-> `SdhValidator`
-> `SdhToSggwMapper`
-> `SdGgwClient`
-> `SdGwLinkEngine`
-> transporte serial atual

## Limitações desta primeira versão

- Suporta apenas `sdh/1`.
- Suporta apenas `GSA.led set state=on|off`.
- O parser textual suporta apenas a forma simples `chave=valor`.
- `Meta` e `SdhResponse` ficaram preparados, mas ainda não participam do envio.
- O host não faz binding lógico-físico; isso continua fora do escopo desta camada.

## Próximos passos naturais

- Expandir o catálogo do `SdhValidator`.
- Adicionar novos mapeamentos parciais no `SdhToSggwMapper`.
- Introduzir respostas SDH canônicas quando houver necessidade de ergonomia de leitura.
- Criar projeto de testes unitários para parser, serializer, validator e mapper.
