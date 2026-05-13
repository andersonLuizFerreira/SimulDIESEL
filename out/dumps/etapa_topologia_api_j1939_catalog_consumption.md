# ETAPA - Topologia da API para consumo dos catalogos J1939/81

Data: 2026-05-13

## 1. Resumo executivo

Esta ETAPA fez apenas levantamento arquitetural. Nenhum codigo funcional, schema, migration, JSON, banco, UI, firmware, SDGW, SDCTP ou fluxo CAN RX foi alterado.

### O que ja existe

- Core de banco local SQLite no DAL:
  - factories de caminho e conexao;
  - initializer;
  - runner de migrations;
  - `IBdServiceProvider` / `SqliteBdServiceProvider`;
  - factory de runtime para montar initializer e repository piloto.
- CRUD piloto de `module_profiles`:
  - DTOs em `DTL/Modules`;
  - service BLL `ModuleProfileService`;
  - interface DAL `IModuleProfileRepository`;
  - repository SQLite `SqliteModuleProfileRepository`;
  - criacao via `LocalDatabaseService` e `ModuleDatabaseRuntimeFactory`.
- J1939 implementado em BLL/DTL para:
  - parsing de CAN ID J1939;
  - camada Data Link;
  - Transport Protocol;
  - diagnosticos DM1/DM2 e catalogos estaticos;
  - Address Claim J1939/81;
  - parser de NAME J1939/81;
  - registro em memoria de Source Address;
  - Working Set ISOBUS.
- Catalogos J1939/81 recem-criados:
  - migration `0003_j1939_reference_catalogs.sql`;
  - JSONs locais em `Data/Protocols/J1939/catalogs/`;
  - seed importer `J1939CatalogSeedImporter`;
  - chamada do importer em `SqliteDatabaseInitializer.Initialize()`.

### O que nao existe

- Nao existe repository de consulta para `j1939_industry_groups`, `j1939_manufacturers`, `j1939_functions`, `j1939_vehicle_systems`, `j1939_preferred_addresses` ou `j1939_name_field_definitions`.
- Nao existe interface DAL para catalogos J1939/81.
- Nao existe service BLL para resolver codigos J1939/81 pelo banco.
- Nao existe DTO publico especifico para resultado de resolucao de identidade J1939/81 enriquecida por catalogo.
- Nao existe integracao entre `J1939NameParser` / `J1939AddressClaimDecoder` e o banco local de catalogos.
- Nao existe resolvedor reutilizavel para Manufacturer Code, Function Code, Industry Group, Vehicle System ou Preferred Source Address.
- A UI atual ainda exibe alguns campos de identificacao J1939/81 como `Desconhecido` diretamente.

### O que pode ser reaproveitado

- `J1939NameParser` deve ser reaproveitado para decodificar o NAME bruto. Nao recriar parser de bitfield.
- `J1939AddressClaimDecoder` deve ser reaproveitado para detectar PGN 60928 e montar `J1939AddressClaimDto`.
- `J1939AddressRegistry` deve ser reaproveitado para estado em memoria de Address Claim.
- `J1939NameDto` deve ser reaproveitado como entrada ja decodificada para uma futura resolucao por catalogo.
- `IBdServiceProvider` e `SqliteBdServiceProvider` devem ser reaproveitados para acesso SQL.
- O padrao de `IModuleProfileRepository` + `SqliteModuleProfileRepository` + BLL service deve ser seguido.
- `ModuleDatabaseRuntimeFactory` deve ser ampliado na proxima ETAPA para criar o repository/service de catalogos, em vez de instanciar SQLite na UI.

### O que nao deve ser recriado

- Nao recriar `SqliteConnectionFactory`.
- Nao recriar `SqliteBdServiceProvider`.
- Nao recriar `SqliteMigrationRunner`.
- Nao recriar parser de NAME J1939/81.
- Nao recriar Address Claim decoder.
- Nao criar acesso direto a SQLite na UI.
- Nao misturar catalogo de referencia com `module_profiles` ou cadastros operacionais.
- Nao acoplar logica de resolucao de catalogo ao SDGW, SDCTP ou firmware.

## 2. Arvore de arquivos relevantes

### Banco local

- `local-api/src/SimulDIESEL/SimulDIESEL/Program.cs`
  - Inicializa o banco no startup com `new LocalDatabaseService().Initialize()`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseConnectionFactory.cs`
  - Interface de factory de conexao.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteConnectionFactory.cs`
  - Implementacao SQLite; resolve arquivo, cria diretorio e abre `SQLiteConnection`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IDatabaseInitializer.cs`
  - Interface de inicializacao.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteDatabaseInitializer.cs`
  - Habilita FKs, aplica migrations e executa `J1939CatalogSeedImporter`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IMigrationRunner.cs`
  - Interface de runner de migrations.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteMigrationRunner.cs`
  - Aplica baseline `0001_sqlite_schema_v1`, arquivos `.sql` em ordem e registra `schema_migrations`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/IBdServiceProvider.cs`
  - Contrato para `ExecuteNonQuery`, `ExecuteScalar`, `Query` e transacao.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/SqliteBdServiceProvider.cs`
  - Provider SQLite usado por repositories; abre conexoes via factory e parametriza comandos.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/BdCommandParameter.cs`
  - Parametro simples de comando SQL.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/DatabaseInitializationResult.cs`
  - Resultado com caminho do banco e migrations aplicadas.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabasePaths.cs`
  - Resolve caminhos de `modules.db`, schema, migrations e catalogos J1939/81.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/ModuleDatabaseRuntimeFactory.cs`
  - Monta initializer e repository piloto de `module_profiles`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/J1939CatalogSeedImporter.cs`
  - Importa JSONs locais J1939/81 para tabelas SQLite usando UPSERT.

### Repositories

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/IModuleProfileRepository.cs`
  - Interface do CRUD piloto de `module_profiles`.
- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Repositories/SqliteModuleProfileRepository.cs`
  - Repository concreto SQLite do CRUD piloto; usa somente `IBdServiceProvider`.

### BLL

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseService.cs`
  - Inicializa banco e cria service/repository default do CRUD piloto.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/ModuleProfileService.cs`
  - Service BLL do CRUD piloto; valida entrada, gera id/timestamps e chama repository.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Services/Database/LocalDatabaseStatus.cs`
  - DTO de status interno da BLL para inicializacao.

### DTL / DTOs

- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileDto.cs`
  - DTO persistido e retornado pelo CRUD piloto.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileCreateRequest.cs`
  - Request de criacao do CRUD piloto.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Modules/ModuleProfileUpdateRequest.cs`
  - Request de update do CRUD piloto.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NameDto.cs`
  - DTO com campos decodificados do NAME J1939/81.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939AddressClaimDto.cs`
  - DTO de Address Claim.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939AddressRegistryEntryDto.cs`
  - DTO de entrada do registry em memoria.
- `local-api/src/SimulDIESEL/SimulDIESEL/DTL/Protocols/J1939/NetworkManagement/J1939NetworkEventDto.cs`
  - DTO de evento de Network Management.

### J1939 atual

- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/J1939ProtocolService.cs`
  - Fachada para processar `CanFrameDto`, `CanRowDto`, snapshots e timeouts.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939IdParser.cs`
  - Parser de CAN ID e PGN.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939DataLinkService.cs`
  - Processamento de Data Link.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/DataLink/J1939TransportProtocolService.cs`
  - TP.CM / TP.DT.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NameParser.cs`
  - Parser do NAME J1939/81 de 64 bits.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939AddressClaimDecoder.cs`
  - Decoder de PGN 60928 Address Claimed.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939NetworkManagementService.cs`
  - Orquestra Address Claim e Working Set.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939AddressRegistry.cs`
  - Registry em memoria por Source Address.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/NetworkManagement/J1939WorkingSetService.cs`
  - Decodificacao parcial de Working Set ISOBUS.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Common/J1939PgnStandardCatalog.cs`
  - Catalogo JSON estatico de PGNs, independente do banco local.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Application/J1939PgnCatalog.cs`
  - Catalogo/decodificacao de aplicacao J1939-71.
- `local-api/src/SimulDIESEL/SimulDIESEL/BLL/Protocols/J1939/Diagnostics/J1939FmiCatalog.cs`
  - Catalogo estatico de FMI.

### Catalogos J1939/81 criados

- `Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql`
  - Cria seis tabelas de referencia J1939/81.
- `Data/Protocols/J1939/catalogs/j1939_industry_groups.json`
  - Seed local de Industry Groups.
- `Data/Protocols/J1939/catalogs/j1939_manufacturers.json`
  - Seed local de fabricantes.
- `Data/Protocols/J1939/catalogs/j1939_functions.json`
  - Seed local de funcoes.
- `Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json`
  - Seed local de Source Addresses preferenciais.
- `Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json`
  - Seed local das definicoes dos campos do NAME.

## 3. Topologia real das camadas

### Inicializacao atual do banco

```text
Program
  -> BLL/Services/Database/LocalDatabaseService.Initialize()
  -> DAL/Database/ModuleDatabaseRuntimeFactory.CreateInitializer()
  -> DAL/Database/SqliteDatabaseInitializer.Initialize()
  -> DAL/Database/SqliteConnectionFactory.CreateOpenConnection()
  -> DAL/Database/SqliteMigrationRunner.ApplyMigrations()
  -> DAL/Database/J1939CatalogSeedImporter.Import()
  -> SQLite Data/Modules/modules.db
```

### CRUD piloto de `module_profiles`

```text
UI/API futura
  -> BLL/Services/Database/ModuleProfileService
  -> DAL/Repositories/IModuleProfileRepository
  -> DAL/Repositories/SqliteModuleProfileRepository
  -> DAL/Database/IBdServiceProvider
  -> DAL/Database/SqliteBdServiceProvider
  -> SQLite
```

### J1939 runtime atual

```text
UI frmUCE_UI / FormsLogic UCE
  -> BLL/Protocols/J1939/J1939ProtocolService
  -> BLL/Protocols/J1939/DataLink
  -> BLL/Protocols/J1939/NetworkManagement
  -> DTL/Protocols/J1939 DTOs
```

Observacao: o J1939 runtime atual nao usa DAL/Database para resolver identidade. O registry de Address Claim e mantido em memoria.

### Fluxo ideal para catalogos J1939/81 na proxima ETAPA

```text
UI/API
  -> BLL/Services/Database ou BLL/Protocols/J1939 service de catalogo
  -> DAL/Repositories/IJ1939ReferenceCatalogRepository
  -> DAL/Repositories/SqliteJ1939ReferenceCatalogRepository
  -> DAL/Database/IBdServiceProvider
  -> SQLite
```

## 4. Padrao atual do CRUD piloto

O CRUD piloto `module_profiles` estabelece o padrao a seguir:

1. DTOs publicos em `DTL`.
2. Service BLL com validacoes e regras de uso.
3. Interface de repository no DAL.
4. Repository SQLite concreto no DAL.
5. SQL fica no repository.
6. Execucao SQL passa pelo `IBdServiceProvider`.
7. Factories default ficam em `LocalDatabaseService` / `ModuleDatabaseRuntimeFactory`.
8. UI/API nao instanciam provider, connection factory ou repository concreto.

Arquivos envolvidos:

- `DTL/Modules/ModuleProfileDto.cs`
- `DTL/Modules/ModuleProfileCreateRequest.cs`
- `DTL/Modules/ModuleProfileUpdateRequest.cs`
- `BLL/Services/Database/ModuleProfileService.cs`
- `DAL/Repositories/IModuleProfileRepository.cs`
- `DAL/Repositories/SqliteModuleProfileRepository.cs`
- `DAL/Database/ModuleDatabaseRuntimeFactory.cs`
- `BLL/Services/Database/LocalDatabaseService.cs`

Para J1939 catalog, o padrao deve ser copiado conceitualmente, mas sem criar CRUD completo se a proxima ETAPA for apenas consulta. A menor implementacao deve ser read-only.

## 5. Estado atual do J1939/81

### Implementado

- `J1939NameParser` decodifica:
  - `identity_number`;
  - `manufacturer_code`;
  - `ecu_instance`;
  - `function_instance`;
  - `function`;
  - `reserved`;
  - `vehicle_system`;
  - `vehicle_system_instance`;
  - `industry_group`;
  - `arbitrary_address_capable`;
  - `raw bytes`, `raw UInt64` e `NameHex`.
- `J1939AddressClaimDecoder`:
  - reconhece PGN 60928;
  - usa `J1939NameParser`;
  - identifica Address Claimed, Cannot Claim e Source Address invalido;
  - usa `J1939PgnStandardCatalog` apenas para PGN label/acronym.
- `J1939AddressRegistry`:
  - registra Source Address em memoria;
  - detecta refresh e conflito por prioridade numerica do NAME.
- `J1939NetworkManagementService`:
  - orquestra Address Claim e Working Set.
- `frmUCE_UI`:
  - possui aba de identificacao J1939;
  - gera linhas de identificacao a partir de `J1939AddressRegistryEntryDto`;
  - atualmente usa textos como `Desconhecido` para Manufacturer Code, Function, Vehicle System e Industry Group.

### Nao implementado

- Resolucao de Manufacturer Code para fabricante pelo banco.
- Resolucao de Function Code para nome de funcao pelo banco.
- Resolucao de Industry Group para nome pelo banco.
- Resolucao de Vehicle System pelo banco.
- Resolucao de Source Address preferencial pelo banco.
- DTO enriquecido de identidade J1939/81.
- Service BLL para enriquecer `J1939NameDto`.
- Repository de leitura dos catalogos J1939/81.

### Reaproveitamento recomendado

- `J1939NameDto` deve permanecer como DTO de NAME cru/decodificado.
- A proxima ETAPA deve criar DTOs de catalogo/resolucao sem alterar o parser.
- O enriquecimento deve acontecer depois do parsing, como camada de resolucao de nomes/codigos.

## 6. Estado atual dos catalogos J1939/81

### Tabelas criadas

Migration: `Data/Modules/schema/migrations/0003_j1939_reference_catalogs.sql`

- `j1939_industry_groups`
- `j1939_manufacturers`
- `j1939_functions`
- `j1939_vehicle_systems`
- `j1939_preferred_addresses`
- `j1939_name_field_definitions`

### JSONs existentes

- `Data/Protocols/J1939/catalogs/j1939_industry_groups.json`
- `Data/Protocols/J1939/catalogs/j1939_manufacturers.json`
- `Data/Protocols/J1939/catalogs/j1939_functions.json`
- `Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json`
- `Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json`

### Importador existente

- `local-api/src/SimulDIESEL/SimulDIESEL/DAL/Database/J1939CatalogSeedImporter.cs`

Responsabilidade atual:

- Ler JSONs locais.
- Inserir/atualizar registros por UPSERT.
- Nao apagar dados manuais.
- Rodar em transacao dentro de uma conexao ja aberta.

### Ponto de execucao do importador

- `SqliteDatabaseInitializer.Initialize()`:

```text
EnableForeignKeys(connection)
ApplyMigrations(connection)
J1939CatalogSeedImporter.Import(connection)
return DatabaseInitializationResult
```

### Existe repository/service de consulta?

Nao. A busca por `j1939_`, `J1939Catalog`, `Reference`, `Resolver`, `ManufacturerCode`, `Function`, `IndustryGroup` em `DAL/Repositories`, `BLL/Services` e `DTL` nao encontrou repository ou service de consulta para as tabelas de catalogo.

## 7. Recomendacao para a proxima ETAPA

Menor ETAPA possivel: criar consumo read-only dos catalogos J1939/81, sem UI e sem alterar parsers.

### DTOs realmente necessarios

Criar DTOs publicos em `DTL/Protocols/J1939/NetworkManagement` ou `DTL/Modules/J1939Catalogs`:

- `J1939CatalogEntryDto`
  - `Code`
  - `Name`
  - `Description`
  - `Source`
  - `Notes`
- `J1939PreferredAddressDto`
  - `Address`
  - `Name`
  - `Description`
  - `FunctionCode`
  - `IndustryGroupCode`
  - `Source`
  - `Notes`
- `J1939NameFieldDefinitionDto`
  - `FieldName`
  - `BitStart`
  - `BitLength`
  - `Description`
  - `Source`
  - `Notes`
- Opcional, se a ETAPA autorizar enriquecimento de NAME:
  - `J1939NameResolutionDto`
  - Deve conter `J1939NameDto RawName` ou campos equivalentes e nomes resolvidos opcionais.

Evitar DTOs de create/update/delete nesta proxima ETAPA, pois os catalogos sao read-only para consumo.

### Service BLL recomendado

Criar um service read-only, por exemplo:

- `BLL/Services/Database/J1939ReferenceCatalogService.cs`

Responsabilidades:

- Validar ranges basicos de codigo.
- Consultar repository.
- Retornar DTOs com `null`/status conhecido para codigos ausentes, sem falhar.
- Opcionalmente enriquecer `J1939NameDto` sem alterar o parser.

Nao reaproveitar `ModuleProfileService`, pois ele e de dominio operacional de perfis de modulo.

### Repository recomendado

Criar:

- `DAL/Repositories/IJ1939ReferenceCatalogRepository.cs`
- `DAL/Repositories/SqliteJ1939ReferenceCatalogRepository.cs`

Operacoes minimas:

- `GetIndustryGroupByCode(int code)`
- `GetManufacturerByCode(int code)`
- `GetFunctionByCode(int code)`
- `GetVehicleSystemByCode(int code, int? industryGroupCode)`
- `GetPreferredAddress(int address, int? industryGroupCode)`
- `GetNameFieldDefinition(string fieldName)`

Todas as consultas devem usar `IBdServiceProvider.Query` ou `ExecuteScalar`, nunca `SQLiteConnection` diretamente.

### Factory/injecao

Ampliar `ModuleDatabaseRuntimeFactory` com:

- `CreateJ1939ReferenceCatalogRepository()`

Ampliar `LocalDatabaseService` com:

- `CreateDefaultJ1939ReferenceCatalogService()`

Isso preserva o padrao atual sem exigir container de DI.

### Arquivos que nao devem ser tocados na proxima ETAPA

- `SqliteConnectionFactory.cs`
- `SqliteBdServiceProvider.cs`
- `SqliteMigrationRunner.cs`
- `J1939NameParser.cs`
- `J1939AddressClaimDecoder.cs`
- `J1939DataLinkService.cs`
- `SDGW/*`
- `SDCTP/*`
- `hardware/firmware/*`
- UI, salvo ETAPA propria posterior

### Validacoes recomendadas

- Build completo da solucao.
- Teste/smoke em banco temporario:
  - inicializar banco;
  - confirmar seed;
  - consultar fabricante existente e inexistente;
  - consultar funcao existente e inexistente;
  - consultar industry group existente e inexistente;
  - consultar preferred address `0`, `254`, `255` e desconhecido;
  - consultar field definition existente e inexistente.
- Validar que codigo desconhecido nao causa excecao.
- Validar que BLL nao referencia `System.Data.SQLite`.
- Validar que UI nao referencia repository/provider.

## 8. Riscos

- Duplicar logica de parsing J1939/81 em vez de reaproveitar `J1939NameParser`.
- Acessar SQLite diretamente pela UI para preencher a aba de identificacao.
- Criar um service que misture catalogo de referencia com dados operacionais de `module_profiles`.
- Transformar o `BD_Service_Provider` em repository generico de dominio.
- Criar CRUD completo antes de haver necessidade funcional.
- Fazer o parser J1939 depender de banco, dificultando testes e uso offline.
- Quebrar a tolerancia a codigos desconhecidos, que deve continuar retornando codigo cru com resolucao ausente.
- Misturar catalogos J1939/81 com PGN/SPN catalogs ja existentes em JSON estatico.

## 9. Validacao executada

Comando:

```text
MSBuild local-api/src/SimulDIESEL/SimulDIESEL.sln /t:Build /p:Configuration=Debug /p:OutDir=out/build-etapa-topologia-j1939-catalog/
```

Resultado:

```text
Compilacao com exito.
0 Aviso(s)
0 Erro(s)
```

## 10. Confirmacao de escopo

- Nenhum codigo funcional novo foi implementado nesta ETAPA.
- Nenhum schema foi alterado nesta ETAPA.
- Nenhum JSON foi alterado nesta ETAPA.
- Nenhum dado foi alterado nesta ETAPA.
- Nenhuma UI foi alterada nesta ETAPA.
- Nenhum firmware, SDGW, SDCTP ou fluxo CAN RX foi alterado nesta ETAPA.
- Unico arquivo criado por esta ETAPA: este dump.
