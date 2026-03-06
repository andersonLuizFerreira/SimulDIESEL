# Architecture Overview --- Aplicação PC

## Visão geral

A aplicação PC do SimulDIESEL é estruturada em três camadas principais:

UI → BLL → DAL

### UI

Responsável pela interação com o usuário.

Principais componentes:

-   DashBoard
-   frmPortaSerial_UI
-   frmLedGw

### BLL

Responsável pela lógica da comunicação e serviços.

Componentes:

-   SerialLink
-   SerialLinkService
-   SdGwLinkEngine
-   SdGwHealthService
-   SdGgwClient

### DAL

Responsável pelo acesso ao meio físico.

Componentes:

-   IByteTransport
-   SerialTransport

### DTL

Tipos de transferência e contratos leves:

-   SggwFrame
-   SggwCmd
-   DeviceInfo
