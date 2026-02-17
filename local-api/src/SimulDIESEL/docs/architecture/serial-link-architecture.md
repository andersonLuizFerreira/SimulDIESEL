# Arquitetura do Subsystema de Comunicação Serial / SGGW

## Propósito
Descrever a arquitetura em camadas do subsistema de comunicação serial e do protocolo SGGW utilizado pelo projeto SimulDIESEL, explicitando responsabilidades, fluxos e integrações entre componentes.

## Escopo
Aplica-se ao código fonte presente na solução aberta (camadas DAL, BLL, DTL, UI) e descreve o fluxo de mensagens entre aplicação e dispositivo ESP32 via porta serial.

## Responsabilidades das camadas
- SimulDIESEL.DAL
  - `SerialTransport`: transporte físico "cru" sobre `System.IO.Ports.SerialPort`. Abre/fecha porta, envia/recebe bytes brutos. Não faz framing nem checksums.
- SimulDIESEL.BLL
  - `SerialLinkService`: serviço de link de alto nível que gerencia conexão serial, handshake de banner, estados do link e integração entre transporte, engine de link e serviço de health.
  - `SdGwLinkEngine`: engine de link/protocolo (framing COBS + CRC, stop-and-wait, ACK/ERR, retransmissão).
  - `SdGwHealthService`: serviço de "ping" periódico para checar saúde do link e detectar falhas de transporte.
  - `SdGgwClient`: cliente de alto nível do protocolo SGGW que expõe API tipada (DTO) para UI/consumidores.
- SimulDIESEL.DTL
  - DTOs e codecs de protocolo (`SggwCmd`, `SggwFrame`, `CanTxRequest`, `CanTxCodec`).

## Arquitetura / Relação entre módulos
A arquitetura é em camadas e sequencial no caminho de dados:

PC Application (UI)
   ↓
`SdGgwClient` (BLL) — API tipada / eventos de frame
   ↓
`SdGwLinkEngine` (BLL) — framing COBS/CRC, controle de sequência, stop-and-wait
   ↓
`SerialTransport` (DAL) — `SerialPort` (envio/recebimento de bytes)
   ↓
ESP32 (firmware SGGW)

## Fluxo completo (resumido)
1. UI chama `SdGgwClient.SendAsync(...)`.
2. `SdGgwClient` converte DTOs para payload e chama `SdGwLinkEngine.SendAsync(...)`.
3. `SdGwLinkEngine` monta frame bruto `[cmd, flags, seq, payload, crc]`, aplica COBS, adiciona delimitador 0x00 e invoca o delegate de escrita que utiliza `SerialTransport.Write(...)`.
4. `SerialTransport` escreve bytes na porta serial.
5. No sentido inverso, `SerialTransport` recebe bytes e dispara `BytesReceived`, `SerialLinkService` roteia para handshake (se não ligado) ou `SdGwLinkEngine.OnBytesReceived(...)` quando link está estabelecido.
6. `SdGwLinkEngine` faz COBS-decode, valida CRC, trata ACK/ERR e entrega `AppFrameReceived` para `SdGgwClient` que converte em `SggwFrame` e dispara eventos públicos.

## Observações
- O handshake de estabelecimento do link (banner) é responsabilidade de `SerialLinkService`.
- Validade do protocolo (framing, CRC, ACK/NACK, retransmissão) é responsabilidade de `SdGwLinkEngine`.
- Nenhuma criptografia, compressão ou fragmentação são implementadas.Leia todo o projeto aberto atualmente na solução Visual Studio e gere a documentação técnica completa do subsistema de comunicação serial e protocolo SGGW, seguindo rigorosamente os padrões abaixo.

Este projeto segue arquitetura em camadas:

SimulDIESEL.DAL   → transporte físico (SerialTransport)
SimulDIESEL.BLL   → lógica de link e protocolo (SerialLinkService, SdGwLinkEngine, SdGwHealthService, SdGgwClient)
SimulDIESEL.DTL   → DTO e codecs de protocolo (SggwCmd, CanTxRequest, codecs)
SimulDIESEL.UI    → interface do usuário (DashBoard, frmPortaSerial_UI)


A documentação deve refletir exatamente o código existente, sem inventar comportamentos.

OBJETIVO DA DOCUMENTAÇÃO

Produzir documentação técnica profissional, estruturada, pronta para versionamento no repositório Git, dentro da pasta:

docs/protocol/
docs/architecture/
docs/components/

PADRÕES OBRIGATÓRIOS

Cada documento deve conter:

• título claro
• propósito
• escopo
• responsabilidades
• arquitetura
• fluxo operacional
• máquina de estados (quando aplicável)
• eventos
• tratamento de erros
• thread safety
• exemplos de uso

Use Markdown (.md).

DOCUMENTOS QUE DEVEM SER GERADOS

Gerar os seguintes arquivos:

docs/architecture/serial-link-architecture.md

Descrever:

• arquitetura completa da comunicação
• relação entre DAL, BLL, DTL e UI
• fluxo completo:

UI → SdGgwClient → SdGwLinkEngine → SerialTransport → ESP32

docs/protocol/sggw-protocol.md

Descrever completamente:

• formato do frame
• CRC
• COBS
• comandos
• flags
• sequência
• ACK / ERR
• retransmissão
• MTU

Basear-se em:

SdGwLinkEngine.cs

docs/components/serial-transport.md

Documentar:

SerialTransport.cs


Incluir:

• responsabilidades
• eventos
• modelo de conexão
• detecção de falha física
• thread safety

docs/components/link-engine.md

Documentar:

SdGwLinkEngine.cs


Incluir:

• stop-and-wait
• controle de sequência
• retransmissão
• timeout
• ACK handling
• ERR handling

docs/components/link-service.md

Documentar:

SerialLinkService.cs


Incluir:

• handshake
• estados
• reconexão automática
• integração com transport e engine
• integração com health service

Incluir máquina de estados:

Disconnected
SerialConnected
Draining
BannerSent
Linked
LinkFailed

docs/components/health-service.md

Documentar:

SdGwHealthService.cs


Incluir:

• sistema de ping
• detecção de falha
• interação com link engine

docs/components/sggw-client.md

Documentar:

SdGgwClient.cs


Incluir:

• API pública
• envio de comandos
• recebimento de eventos
• integração com DTO

PADRÃO DE ESTILO

Use linguagem técnica profissional.

Evite linguagem informal.

Use diagramas ASCII quando necessário.

Exemplo:

PC Application
   ↓
SdGgwClient
   ↓
SdGwLinkEngine
   ↓
SerialTransport
   ↓
ESP32

NÃO INVENTAR COMPORTAMENTO

Documentar apenas o que existe no código.

Se algo não estiver implementado, marcar como:

Not implemented
Reserved for future use

RESULTADO ESPERADO

Gerar arquivos Markdown completos, prontos para commit no repositório Git.

EXECUÇÃO

Analise toda a solução aberta atualmente e gere os arquivos descritos.