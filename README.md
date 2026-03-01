Estruturas das pastas
SimulDIESEL/
├─ README.md
├─ CHANGELOG.md
├─ VERSIONING.md
├─ .editorconfig
├─ .gitattributes
├─ .gitignore
├─ .gitlab-ci.yml
│
├─ docs/                        # Documentação geral do projeto
│  ├─ 00_visao-geral/
│  ├─ 01_arquitetura/
│  ├─ 02_instalacao/
│  ├─ 03_operacao/
│  ├─ 04_desenvolvimento/
│  ├─ 05_hardware/
│  └─ images/
│
├─ tools/                       # Scripts auxiliares (build, flash, release)
│  ├─ build/
│  ├─ flash/
│  ├─ release/
│  └─ dev/
│
├─ infra/                       # Infraestrutura como código e deploy
│  ├─ cloud/
│  │  ├─ docker/
│  │  └─ pipelines/
│  └─ local/
│     ├─ installers/
│     └─ config-templates/
│
├─ cloud/                       # Camada de Nuvem (Monólito)
│  ├─ src/
│  │  └─ SimulDiesel.Cloud/
│  │     ├─ Api/                # Endpoints REST/gRPC
│  │     ├─ Application/        # Casos de uso e serviços
│  │     ├─ Domain/             # Entidades e regras centrais
│  │     ├─ Infrastructure/     # Banco, storage, integrações
│  │     └─ Contracts/          # DTOs internos
│  │
│  ├─ database/
│  │  ├─ migrations/
│  │  ├─ schemas/
│  │  └─ seed/
│  │
│  ├─ api-contracts/            # Contrato oficial OpenAPI
│  │  ├─ openapi.yaml
│  │  └─ schemas/
│  │
│  ├─ deploy/
│  │  └─ docker/
│  │
│  └─ README.md
│
├─ local-api/                   # API Local (.NET)
│  ├─ src/
│  │  ├─ SimulDiesel.LocalApi/        # API principal
│  │  ├─ SimulDiesel.LocalApp/        # UI/Service local
│  │  ├─ SimulDiesel.Domain/          # Núcleo do domínio
│  │  ├─ SimulDiesel.Application/     # Casos de uso (testes)
│  │  ├─ SimulDiesel.Infrastructure/  # Serial/USB/WiFi
│  │  ├─ SimulDiesel.Drivers.Esp32/   # Driver ESP32 bridge
│  │  ├─ SimulDiesel.Protocols/       # Parser/frames/protocolos
│  │  └─ SimulDiesel.Shared/          # Código compartilhado
│  │
│  ├─ tests/
│  │  ├─ unit/
│  │  └─ integration/
│  │
│  ├─ docs/
│  └─ README.md
│
├─ hardware/                    # Hardware e Firmware embarcado
│  ├─ boards/                   # PCBs e produção (KiCad)
│  │  ├─ x-conn/
│  │  │  ├─ kicad/
│  │  │  ├─ bom/
│  │  │  ├─ gerbers/
│  │  │  ├─ assembly/
│  │  │  └─ docs/
│  │  ├─ backplane/
│  │  └─ babyboards/
│  │     ├─ gerador-niveis/
│  │     ├─ reles/
│  │     ├─ fonte-alimentacao/
│  │     └─ comunicacao/
│  │
│  ├─ firmware/                 # Firmwares PlatformIO
│  │  ├─ esp32-api-bridge/      # ESP32: ponte PC ↔ hardware
│  │  ├─ arduino-due-vehicle-bus/ # DUE: CAN/LIN/K-Line
│  │  ├─ arduino-mega-peripherals/ # MEGA: relés, níveis, fonte
│  │  └─ shared/               # Bibliotecas comuns
│  │
│  ├─ test-jigs/                # Gabaritos e testes de bancada
│  └─ README.md
│
└─ tests/                       # Testes de sistema (E2E / HIL)
   ├─ e2e/
   ├─ hardware-in-the-loop/
   └─ performance/
