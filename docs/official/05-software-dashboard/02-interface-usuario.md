⬅ [Retornar para Arquitetura do Software Dashboard (Local API)](01-arquitetura-software.md)

# Interface de Usuário

## Estado atual

A interface de usuário presente no repositório é uma aplicação WinForms orientada à operação de bancada. Ela não é um dashboard analítico amplo; ela é um conjunto de telas de apoio à conexão serial, à seleção de Bluetooth, ao diagnóstico do link e ao teste de funcionalidades expostas pelo gateway.

As telas diretamente observadas no código são:

- `DashBoard`: ponto central de inicialização e navegação;
- `frmPortaSerial_UI`: abertura e controle da porta serial;
- `frmBluetoothConnect`: seleção e conexão de dispositivo Bluetooth SPP;
- `frmGSA_UI`: operação e teste dos recursos atuais da GSA.

## Funcionamento técnico

### Fluxo principal de uso

```text
Operador abre a aplicação
  -> escolhe serial ou Bluetooth
  -> inicia o link com o gateway
  -> monitora saúde da conexão
  -> executa ações de teste
```

### Responsabilidades por tela

- `DashBoard`: organiza o acesso às funcionalidades de bancada e concentra o contexto da aplicação.
- `frmPortaSerial_UI`: aciona a sessão serial via `FrmBpmLogic` e `BpmSerialService`.
- `frmBluetoothConnect`: lista dispositivos detectados e usa `FrmBpmLogic` para abrir a sessão Bluetooth da BPM.
- `frmGSA_UI`: fornece o caso de uso funcional mais completo hoje, consumindo `FrmGsaLogic` e `GsaClient`.

### Relação com as camadas inferiores

A interface não conversa diretamente com byte streams nem com barramentos. Ela delega para classes de negócio e infraestrutura:

- a UI solicita uma ação;
- a `BLL` monta a intenção funcional ou a conexão;
- `BpmSerialService` compõe a sessão e o transporte ativo;
- o client funcional transforma a intenção em frame;
- a `DAL` transporta bytes pela interface ativa.

Essa separação reduz o acoplamento com o hardware e permite evoluir a interface sem reescrever o protocolo.

## Papel deste ramo na árvore

Nesta trilha, a interface é lida como o ponto de entrada dos **casos de uso operacionais** já sustentados pelo software local.

Por isso, as próximas camadas não detalham componentes visuais adicionais. Elas aprofundam as duas famílias de uso que a UI precisa materializar hoje:

* simulação de módulos em bancada
* manutenção e diagnóstico de módulos

O objetivo é mostrar o que o operador realmente consegue fazer a partir da interface atual, e não apenas quais formulários existem.

## Limitações

O conjunto atual de telas é focado em operação técnica e não em experiência de uso ampla. Não há evidência no código de painéis completos para múltiplos módulos, histórico persistente de sessões, editor de payloads arbitrários ou visualizações avançadas de telemetria. A interface atual cumpre papel de instrumento de validação e bancada.

## Evolução prevista

A base atual favorece a expansão da UI em três direções:

- adicionar novos casos de teste consumindo `GsaClient`, `BpmClient` e clients equivalentes sem reescrever a sessão;
- expor estado do link, erros e eventos com maior clareza operacional;
- incorporar novas telas à medida que novos serviços roteáveis forem adicionados ao gateway e a novas interfaces de acesso forem consolidadas.

## Próximas camadas

- [Simulação de Módulos](../07-simulacoes/01-simulacao-modulos.md)
- [Manutenção de Módulos](../08-casos-de-uso/01-manutencao-modulos.md)


