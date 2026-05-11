# Relacao Banco de Modulos e SDH

## Papel do SDH

O SDH e o contrato semantico entre a API do SimulDIESEL e as boards fisicas. O Banco de Modulos deve guardar configuracoes e testes em uma forma que possa ser validada contra esse contrato, sem depender de detalhes de transporte como SDGW, TLV, SDCTP, SPI, Serial ou firmware.

Nesta etapa o banco apenas modela e valida dados. Ele nao envia comandos.

## Onde o SDH aparece no modelo

### module_sdh_commands

Armazena comandos de preparacao e configuracao vinculados ao perfil tecnico do modulo.

Uso previsto:

- configurar alimentacao da bancada;
- configurar sinais eletricos;
- configurar redes CAN/J1939;
- preparar canais de simulacao;
- parametrizar leituras ou diagnosticos;
- registrar comandos reutilizaveis para perfis de modulo.

O comando e formado por:

- `target`
- `op`
- `args_json`
- `meta_json`

Esses campos devem ser suficientes para formar um `SdhCommand`:

```json
{
  "version": "sdh/1",
  "target": "UCE.can.config",
  "op": "set",
  "args": {
    "controller": "can0",
    "bitrate": "250",
    "mode": "normal",
    "rxMode": "auto"
  },
  "meta": {}
}
```

### module_test_steps

Armazena passos de teste. Quando `step_type = 'sdh_command'`, o campo `sdh_command_json` deve conter o documento SDH completo:

```json
{
  "version": "sdh/1",
  "target": "UCE.can.rx",
  "op": "readAll",
  "args": {
    "controller": "can0"
  },
  "meta": {}
}
```

Os campos `expected_response_json` e `expected_event_json` descrevem expectativas futuras de validacao, mas nao executam nada nesta etapa.

## Validacao Esperada

Antes de persistir qualquer comando SDH em fluxos futuros, a aplicacao deve:

1. Montar um `SdhCommand` a partir de `target`, `op`, `args_json` e `meta_json`, ou a partir de `sdh_command_json`.
2. Chamar `SdhValidator.ValidateOnly(command)`.
3. Recusar persistencia se `IsValid = false`.
4. Registrar `ErrorCode`, `Message` e argumentos invalidos para feedback ao usuario ou ferramenta.

O catalogo exportado em `out/dumps/sdh_contract_export/sdh_contract_export.json` deve ser usado como referencia documental para UI, importadores e validadores auxiliares. A fonte autoritativa de validacao continua sendo o `SdhValidator`.

## Regras de Integridade SDH

- `target` e `op` nao devem ser livres do ponto de vista funcional: devem existir no contrato aceito pelo `SdhValidator`.
- `args_json` deve ser um objeto JSON cujas chaves e valores possam ser convertidos para `Dictionary<string,string>`.
- `meta_json` deve permanecer opcional e nao deve alterar o comportamento fisico do comando.
- Comandos nao suportados pelo validator nao devem entrar no banco.
- O banco nao deve tentar mapear diretamente TLV, SDGW ou firmware.

## Compatibilidade com Supabase

No SQLite, JSON e armazenado como `TEXT` para manter o banco local simples e portavel.

No PostgreSQL/Supabase, os mesmos campos passam a ser `JSONB`:

- `module_sdh_commands.args_json`
- `module_sdh_commands.meta_json`
- `module_test_steps.sdh_command_json`
- `module_test_steps.expected_response_json`
- `module_test_steps.expected_event_json`
- `module_capture_events.payload_json`

Essa separacao permite usar o mesmo contrato SDH no armazenamento local e, futuramente, na nuvem, sem introduzir dependencia obrigatoria com Supabase nesta etapa.

## Limites Desta Etapa

- Nao ha sincronizacao cloud.
- Nao ha autenticacao.
- Nao ha UI.
- Nao ha executor SDH baseado no banco.
- Nao ha alteracao de UCE, BPM, GSA, SDGW, SDCTP, SPI ou firmware.
