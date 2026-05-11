# Contratos JSON relacionados ao SDH

## Resultado da varredura

Nao foi encontrado no host atual um serializador/desserializador JSON SDH funcional. A busca por `Json`, `JSON`, `Serialize`, `Deserialize`, `JavaScriptSerializer` em `local-api/src/SimulDIESEL/SimulDIESEL` encontrou:

- `DAL/Protocols/SDGW/SdhTextSerializer.cs`: serializa `SdhCommand` para texto SDH, nao JSON.
- `DAL/Protocols/SDGW/SdhTextParser.cs`: interpreta texto SDH, nao JSON.
- `BLL/Protocols/J1939/Common/J1939PgnStandardCatalog.cs`: usa `JavaScriptSerializer` para catalogo J1939, fora do contrato SDH.
- Arquivos JSON J1939 em `Data/Protocols/J1939`, fora do contrato SDH.

Conclusao: contratos JSON SDH runtime: **NAO CONFIRMADO NO CODIGO**.

## JSON documental do comando SDH

Documentado em `docs/official/06-protocolos/01-sdh-command-model.md`, mas nao implementado como parser no host atual:

```json
{
  "version": "sdh/1",
  "target": "BPM.gateway.serial",
  "op": "cfg",
  "args": {
    "baudrate": 115200
  },
  "meta": {}
}
```

Relação com o codigo:

- `version` corresponde a `SdhCommand.Version`.
- `target` corresponde a `SdhCommand.Target`.
- `op` corresponde a `SdhCommand.Op`.
- `args` corresponde a `SdhCommand.Args`.
- `meta` corresponde a `SdhCommand.Meta`.

Campos obrigatorios no codigo: `Version`, `Target`, `Op`. `Args` pode ser vazio dependendo do comando. `Meta` existe no DTO, mas nao foi encontrado uso funcional no envio.

## JSON documental da resposta SDH

Documentado em `docs/official/06-protocolos/02-sdh-response-model.md`, mas nao implementado como parser de resposta no pipeline atual:

```json
{
  "version": "sdh/1",
  "ok": true,
  "target": "GSA.channel.status",
  "op": "get",
  "code": "OK",
  "message": "Status do canal lido",
  "data": {
    "channel": 6,
    "setpoint": 128,
    "vout": 134,
    "iread": 17,
    "enabled": true,
    "fault": false
  },
  "meta": {}
}
```

Relação com o codigo:

- `SdhResponse` possui `Version`, `Ok`, `Target`, `Op`, `Code`, `Message`, `Data`, `Meta`.
- O pipeline real de boards usa DTOs especificos (`Gsa*Response`, `Uce*Response`) parseados de TLV, nao `SdhResponse`.

## Modelo textual efetivamente implementado

`SdhTextParser`:

```text
sdh/1 <target> <op> chave=valor ...
```

Regras:

- minimo de 3 tokens;
- argumentos a partir do quarto token no formato `chave=valor`;
- sem suporte a aspas/escaping no parser atual;
- `Args` e case-insensitive (`StringComparer.OrdinalIgnoreCase`).

`SdhTextSerializer`:

- emite `version target op`;
- ordena `Args` por chave com `StringComparer.Ordinal`;
- nao serializa `Meta`.

## Impacto para Banco de Dados de Modulos

O banco futuro pode armazenar comandos em formato JSON usando a forma documental, mas sera necessario implementar ponte JSON -> `SdhCommand` antes de usar em runtime. O modelo mais direto e:

- `version`: string, default `sdh/1`;
- `target`: string;
- `op`: string;
- `args`: objeto chave/valor serializado como string ou tipos fortes;
- `meta`: objeto opcional para origem, perfil, versao de modulo, lote de teste.

Campos como `target`, `op` e `args` devem ser validados contra o catalogo em `SdhValidator` e contra os DTOs/servicos de board antes de execucao.

