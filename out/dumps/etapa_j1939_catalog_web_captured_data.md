# Dump - Dados captados na web para catalogos J1939/81

Data: 2026-05-13

## Objetivo

Registrar a lista dos dados publicos captados na web e materializados nos JSONs locais da ETAPA `Popular Catalogos J1939/81`.

Este dump documenta somente os dados versionados nos arquivos:

- `Data/Protocols/J1939/catalogs/j1939_industry_groups.json`
- `Data/Protocols/J1939/catalogs/j1939_manufacturers.json`
- `Data/Protocols/J1939/catalogs/j1939_functions.json`
- `Data/Protocols/J1939/catalogs/j1939_preferred_addresses.json`
- `Data/Protocols/J1939/catalogs/j1939_name_field_definitions.json`

## Fontes web consultadas

- `https://www.isobus.net/isobus/manufacturerCode`
- `https://www.isobus.net/isobus/nameFunction`
- `https://www.isobus.net/isobus/sourceAddress`
- `https://kvaser.com/about-can/higher-layer-protocols/j1939-introduction/`
- `https://delgrossoengineering.com/isobus-docs/classisobus_1_1NAME`
- `https://github.com/krone-landmaschinen/isobus-name-resolver-ts`

## Industry Groups

Fonte principal: `https://www.isobus.net/isobus/nameFunction`

| code | name | description |
|---:|---|---|
| 0 | Global | Applies to all industry groups. |
| 1 | On-Highway | On-highway equipment. |
| 2 | Agricultural and Forestry | Agricultural and forestry equipment. |
| 3 | Construction | Construction equipment. |
| 4 | Marine | Marine equipment. |
| 5 | Industrial | Industrial, process control, and stationary equipment. |
| 6 | Reserved | Reserved for future assignment by SAE. |
| 7 | Reserved | Reserved for future assignment by SAE. |

Total captado: 8 registros.

## Manufacturers

Fonte principal: `https://www.isobus.net/isobus/manufacturerCode`

| code | name | country |
|---:|---|---|
| 10 | Cummins Inc | USA |
| 12 | Deere & Company Precision Farming | USA |
| 25 | Daimler Trucks North America | USA |
| 29 | Hino Motors Ltd | Japan |
| 30 | Isuzu Motors Ltd | Japan |
| 33 | John Deere | USA |
| 35 | Kenworth Truck Co. | USA |
| 37 | Mack Trucks Inc. | USA |
| 42 | International Truck and Engine Corp - Engine Electronics | USA |
| 43 | International Truck and Engine Corp - Vehicle Electronics | USA |
| 44 | Nippondenso Co Ltd | Japan |
| 45 | PACCAR | USA |
| 46 | Noregon Systems LLC | USA |
| 50 | Robert Bosch Corp | USA |
| 51 | Robert Bosch GmbH | Germany |
| 52 | Meritor Automotive | USA |
| 53 | Continental Automotive Systems | USA |
| 54 | Meritor Wabco | USA |
| 66 | John Deere Construction Equipment Division | USA |
| 84 | New Holland UK Limited | UK |
| 87 | JCB | UK |
| 92 | Ag-Chem Equipment Co. | USA |
| 94 | CNH Industrial N.V. | USA |
| 213 | CNH Belgium N.V. | Belgium |
| 410 | COBO S.p.A. | Italy |
| 719 | Cojali S.L. | Spain |

Total captado: 26 registros.

## Functions

Fonte principal: `https://www.isobus.net/isobus/nameFunction`

| code | name | description |
|---:|---|---|
| 0 | Engine | Engine control function. |
| 1 | Turbocharger | Turbocharger related function. |
| 2 | Transmission | Transmission related function. |
| 3 | Shift Console | Shift console related function. |
| 4 | Cruise Control | Cruise control related function. |
| 5 | Cab Display | Cab display related function. |
| 6 | Cab Controller | Cab controller related function. |
| 7 | Body Controller | Body controller related function. |
| 9 | Brakes | Brakes related function. |
| 11 | Retarder | Retarder related function. |
| 15 | Navigation | Navigation related function. |
| 19 | Instrument Cluster | Instrument cluster related function. |
| 20 | Trip Recorder | Trip recorder related function. |
| 23 | Electrical System | Electrical system related function. |
| 25 | PTO Controller | Power take-off controller related function. |
| 29 | Virtual Terminal | Virtual terminal related function. |
| 30 | Management Computer | Management computer related function. |
| 31 | Task Controller | Task controller related function. |
| 33 | Lighting Controller | Lighting controller related function. |
| 35 | Hydraulic Controller | Hydraulic controller related function. |
| 37 | Cab Controller | Cab controller related function. |
| 41 | Engine Brake Controller | Engine brake controller related function. |
| 50 | GPS Receiver | GPS receiver related function. |
| 61 | Steering Controller | Steering controller related function. |
| 129 | Off-board Diagnostic Tool | Off-board diagnostic-service tool. |

Total captado: 25 registros.

## Preferred Addresses

Fontes:

- `https://www.isobus.net/isobus/sourceAddress`
- `https://kvaser.com/about-can/higher-layer-protocols/j1939-introduction/`

| address | name | function_code | industry_group_code | source |
|---:|---|---:|---:|---|
| 0 | Engine #1 | 0 | 0 | ISOBUS sourceAddress |
| 1 | Engine #2 | 0 | 0 | ISOBUS sourceAddress |
| 3 | Transmission #1 | 2 | 0 | ISOBUS sourceAddress |
| 4 | Transmission #2 | 2 | 0 | ISOBUS sourceAddress |
| 6 | Brakes System Controller | 9 | 0 | ISOBUS sourceAddress |
| 11 | Retarder Controller | 11 | 0 | ISOBUS sourceAddress |
| 15 | Cruise Control | 4 | 0 | ISOBUS sourceAddress |
| 17 | Cab Controller | 37 | 0 | ISOBUS sourceAddress |
| 19 | Instrument Cluster #1 | 19 | 0 | ISOBUS sourceAddress |
| 23 | Trip Recorder | 20 | 0 | ISOBUS sourceAddress |
| 27 | Vehicle Navigation | 15 | 0 | ISOBUS sourceAddress |
| 29 | Body Controller | 7 | 0 | ISOBUS sourceAddress |
| 33 | Electrical System Controller | 23 | 0 | ISOBUS sourceAddress |
| 37 | Hydraulic Controller | 35 | 0 | ISOBUS sourceAddress |
| 38 | Virtual Terminal | 29 | 0 | ISOBUS sourceAddress |
| 39 | Task Controller | 31 | 0 | ISOBUS sourceAddress |
| 41 | GPS Receiver | 50 | 0 | ISOBUS sourceAddress |
| 61 | Steering Controller | 61 | 0 | ISOBUS sourceAddress |
| 128 | Implement Controller | null | 2 | ISOBUS sourceAddress |
| 129 | Seeder Controller | null | 2 | ISOBUS sourceAddress |
| 130 | Sprayer Controller | null | 2 | ISOBUS sourceAddress |
| 131 | Harvester Controller | null | 2 | ISOBUS sourceAddress |
| 247 | Off-board Service Tool #1 | 129 | 0 | ISOBUS sourceAddress |
| 248 | Off-board Service Tool #2 | 129 | 0 | ISOBUS sourceAddress |
| 249 | Off-board Diagnostic-Service Tool #1 | 129 | 0 | ISOBUS sourceAddress |
| 250 | Off-board Diagnostic-Service Tool #2 | 129 | 0 | ISOBUS sourceAddress |
| 254 | Null Address | null | 0 | Kvaser J1939 introduction |
| 255 | Global Address | null | 0 | Kvaser J1939 introduction |

Total captado: 28 registros.

## NAME Field Definitions

Fonte principal: `https://kvaser.com/about-can/higher-layer-protocols/j1939-introduction/`

Observacao: `bit_start` usa numeracao a partir do bit menos significativo.

| field_name | bit_start | bit_length | description |
|---|---:|---:|---|
| identity_number | 0 | 21 | Unique identity number portion of the 64-bit J1939 NAME. |
| manufacturer_code | 21 | 11 | Manufacturer code portion of the 64-bit J1939 NAME. |
| ecu_instance | 32 | 3 | ECU instance field of the 64-bit J1939 NAME. |
| function_instance | 35 | 5 | Function instance field of the 64-bit J1939 NAME. |
| function | 40 | 8 | Function field of the 64-bit J1939 NAME. |
| vehicle_system | 49 | 7 | Vehicle system field of the 64-bit J1939 NAME. Bit 48 is reserved. |
| vehicle_system_instance | 56 | 4 | Vehicle system instance field of the 64-bit J1939 NAME. |
| industry_group | 60 | 3 | Industry group field of the 64-bit J1939 NAME. |
| arbitrary_address_capable | 63 | 1 | Flag indicating arbitrary address capability in the 64-bit J1939 NAME. |

Total captado: 9 registros.

## Totais

| catalogo | total |
|---|---:|
| Industry Groups | 8 |
| Manufacturers | 26 |
| Functions | 25 |
| Preferred Addresses | 28 |
| NAME Field Definitions | 9 |

Total geral captado: 96 registros.

## Limitacoes

- Dataset parcial, conforme permitido pela ETAPA.
- Dados mantidos localmente em JSON versionado.
- Nao ha scraping em runtime.
- Nao ha sincronizacao web automatica.
- Divergencias finas entre fontes publicas permanecem `pendente de confirmacao` para uma ETAPA futura de curadoria completa.
