# Skill: Module Database

## Quando usar

Use para Banco de Modulos, schemas SQLite/PostgreSQL, perfis de modulos, pinagem, sinais, comandos SDH, sequencias de teste e capturas.

## Quando nao usar

Nao use para implementar UI, executor de comandos, firmware ou cloud/autenticacao sem pedido.

## Escopo permitido

- `Data/Modules/`
- `Data/Modules/schema/`
- `Data/Modules/docs/`
- `tools/dumps/module_database_model/`
- dumps do modelo.

## Escopo proibido

- Alterar `modules.db` com dados reais sem autorizacao.
- Fazer o banco executar comandos SDH.
- Mudar firmware/protocolo para caber no banco.

## Arquivos/pastas provaveis

- `Data/Modules/modules.db`
- `sqlite_schema_v1.sql`
- `postgres_schema_v1.sql`
- `module_database_model_v1.md`
- `module_database_sdh_relation.md`

## Padroes do projeto

- Tabelas em `snake_case`.
- SQLite usa JSON como `TEXT` com `json_valid`.
- PostgreSQL futuro usa `JSONB`.
- Comandos SDH devem ser validaveis por `SdhValidator`.

## Checklist de validacao

- [ ] Validar schema com scripts existentes.
- [ ] Gerar dump de schema quando alterado.
- [ ] Confirmar que banco nao executa comandos.
- [ ] Separar exemplo de dado real.

## Checklist de entrega

- [ ] Schema/documentos alterados.
- [ ] Dump gerado.
- [ ] Compatibilidade SQLite/PostgreSQL.
- [ ] Pendencias de confirmacao.

## Riscos comuns

- Transformar banco em executor operacional.
- Persistir comandos fora do contrato SDH.
- Misturar cloud futura com modelo local v1.

## Regras de nao regressao

- Banco permanece base de configuracao/teste.
- Contrato SDH continua fonte semantica.
- Nao alterar UI/API/firmware sem ETAPA propria.
