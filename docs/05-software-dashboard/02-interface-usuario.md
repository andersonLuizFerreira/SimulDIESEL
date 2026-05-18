⬅ [Retornar para API e Host Local](../02-arquitetura/04-api-e-host-local.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Interface de Usuário

## Estado atual

A interface de usuário presente no repositório é uma aplicação WinForms orientada à operação de bancada. Nesta trilha física, o foco não é o caso de uso do operador, mas **onde a UI se posiciona no host real**.

As telas diretamente observadas no código são:

- `DashBoard`: ponto central de inicialização e navegação;
- `frmPortaSerial_UI`: abertura e controle da porta serial;
- `frmBluetoothConnect`: seleção e conexão de dispositivo Bluetooth SPP;
- `frmGSA_UI`: operação e teste dos recursos atuais da GSA;
- `frmUCE_UI`: operação UCE, monitor CAN, abas J1939 de diagnósticos, dados, identificação e captura temporal;
- `FrmRedeCan`: janela MDI-child somente leitura para módulos J1939/81 detectados na rede CAN.

## Posição na pilha do host

```text
Operador
  ↓
DashBoard e formulários WinForms
  ↓
FormsLogic / BLL
  ↓
Sessão e transporte
```

## Blocos implementados

- `DashBoard`: organiza o acesso às funcionalidades de bancada e concentra o contexto da aplicação.
- `frmPortaSerial_UI`: aciona a sessão serial via `FrmBpmLogic` e `BpmSerialService`.
- `frmBluetoothConnect`: lista dispositivos detectados e usa `FrmBpmLogic` para abrir a sessão Bluetooth da BPM.
- `frmGSA_UI`: fornece o caso de uso funcional mais completo hoje, consumindo `FrmGsaLogic` e `GsaClient`.
- `frmUCE_UI`: consome `FrmUceLogic` para controle UCE, monitor CAN, leitura J1939, diagnóstico DM1/DM2 e captura temporal J1939.
- `FrmRedeCan`: consome snapshots read-only do registry J1939/81 e usa BLL para enriquecimento por catálogo, sem acessar banco diretamente.

## Captura temporal J1939

Status: `PARCIALMENTE IMPLEMENTADO`.

A aba `Dados J1939` possui comandos para iniciar e finalizar captura temporal. Ao finalizar, a UI abre `SaveFileDialog` e permite salvar `.md` ou `.txt`. A exportação é feita por serviço BLL e registra uma sessão reduzida para engenharia reversa posterior, agrupando mensagens repetitivas em eventos de periodicidade e destacando `Address Claim` com `NAME`, `ClaimedSA` e `RawCanId`.

A UI não acessa TLV bruto, SDGW, SDCTP, firmware, SQLite ou parser J1939 diretamente para essa captura. Ela registra apenas mensagens J1939 válidas já produzidas pela pipeline atual da aba `Dados J1939`.

## Interface com a camada inferior

A interface não conversa diretamente com byte streams nem com barramentos. Ela delega para classes de negócio e infraestrutura:

- a UI solicita uma ação;
- a `BLL` monta a intenção funcional ou a conexão;
- `BpmSerialService` compõe a sessão e o transporte ativo;
- o client funcional transforma a intenção em frame;
- a `DAL` transporta bytes pela interface ativa.

Essa separação reduz o acoplamento com o hardware e permite evoluir a interface sem reescrever o protocolo.

## Limitações

O conjunto atual de telas é focado em operação técnica e não em experiência de uso ampla. Não há evidência no código de painéis completos para múltiplos módulos, histórico persistente de sessões, editor de payloads arbitrários ou visualizações avançadas de telemetria. A interface atual cumpre papel de instrumento de validação e bancada.

## Evolução prevista

A base atual favorece a expansão da UI em três direções:

- adicionar novos casos de teste consumindo `GsaClient`, `BpmClient` e clients equivalentes sem reescrever a sessão;
- expor estado do link, erros e eventos com maior clareza operacional;
- incorporar novas telas à medida que novos serviços roteáveis forem adicionados ao gateway e a novas interfaces de acesso forem consolidadas.

## Glossário

- **Host**: lado do software local que coordena a bancada a partir do PC.
- **UI**: camada visual usada pelo operador para interagir com o sistema.
- **Sessão**: estado lógico que mantém o enlace ativo entre host e gateway.
- **Client funcional**: classe que encapsula operações de um domínio específico, como GSA ou BPM.

## Próximas camadas

- Esta é uma página terminal do ramo físico da API.
