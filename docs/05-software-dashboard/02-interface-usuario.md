# Interface de Usuário

## Estado atual

A interface de usuário presente no repositório é uma aplicação WinForms orientada à operação de bancada. Ela não é um dashboard analítico amplo; ela é um conjunto de telas de apoio à conexão serial, ao diagnóstico do link e ao teste de funcionalidades expostas pelo gateway.

As telas diretamente observadas no código são:

- `DashBoard`: ponto central de inicialização e navegação;
- `frmPortaSerial_UI`: abertura e controle da porta serial;
- `frmLedGw`: teste de LED via gateway.

## Funcionamento técnico

### Fluxo principal de uso

```text
Operador abre a aplicação
  -> seleciona porta serial
  -> inicia o link com o gateway
  -> monitora saúde da conexão
  -> executa ações de teste
```

### Responsabilidades por tela

- `DashBoard`: organiza o acesso às funcionalidades de bancada e concentra o contexto da aplicação.
- `frmPortaSerial_UI`: aciona serviços de comunicação, normalmente ligados a `SerialLinkService` e ao cliente do gateway.
- `frmLedGw`: fornece um caso de uso controlado para enviar comandos e observar resposta do gateway ou do periférico.

### Relação com as camadas inferiores

A interface não conversa diretamente com byte streams nem com barramentos. Ela delega para classes de negócio e infraestrutura:

- a UI solicita uma ação;
- a `BLL` monta a intenção funcional;
- o cliente de gateway transforma a intenção em frame;
- a `DAL` transporta bytes pela serial.

Essa separação reduz o acoplamento com o hardware e permite evoluir a interface sem reescrever o protocolo.

## Limitações

O conjunto atual de telas é focado em operação técnica e não em experiência de uso ampla. Não há evidência no código de painéis completos para múltiplos módulos, histórico persistente de sessões, editor de payloads arbitrários ou visualizações avançadas de telemetria. A interface atual cumpre papel de instrumento de validação e bancada.

## Evolução prevista

A base atual favorece a expansão da UI em três direções:

- adicionar novos casos de teste consumindo `SdGgwClient` sem alterar o transporte;
- expor estado do link, erros e eventos com maior clareza operacional;
- incorporar novas telas à medida que novos serviços roteáveis forem adicionados ao gateway.

[Retornar ao README principal](../README.md)
