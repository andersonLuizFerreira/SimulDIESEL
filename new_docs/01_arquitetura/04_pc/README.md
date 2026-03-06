# Aplicação PC (.NET) --- Arquitetura de Comunicação

Esta pasta documenta a arquitetura atual da comunicação entre a
aplicação PC e o Gateway do SimulDIESEL.

## Componentes principais

UI → BLL → DAL

Fluxo principal:

DashBoard / Forms\
↓\
SerialLink (fachada)\
↓\
SerialLinkService\
├─ SdGgwClient\
├─ SdGwHealthService\
├─ SdGwLinkEngine\
└─ SerialTransport\
↓\
System.IO.Ports.SerialPort

## Documentos desta pasta

-   architecture-overview.md
-   serial-connection.md
-   serial-transport.md
-   link-handshake.md
-   link-engine.md
-   health-service.md
-   sggw-client.md
