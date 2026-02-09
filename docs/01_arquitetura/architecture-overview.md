# SimulDIESEL – Architecture Overview

**Projeto:** SimulDIESEL  
**Documento:** Visão Geral Arquitetural  
**Status Atual:** Conexão Serial Estável + Camada de Link em Desenvolvimento

---

# 1. Visão Geral

O SimulDIESEL é estruturado em camadas bem definidas para garantir:

- Separação de responsabilidades
- Manutenção simplificada
- Escalabilidade futura
- Testabilidade isolada

A arquitetura segue o modelo:

```
UI → BLL → DAL → Hardware
```

---

# 2. Camadas do Sistema

## 2.1 UI (Interface do Usuário)

Responsável por:

- Interação com o usuário
- Atualização visual de estado
- Abertura de formulários
- Exibição de status de conexão

Não contém lógica de protocolo.

---

## 2.2 BLL (Business Logic Layer)

Responsável por:

- Orquestrar a comunicação
- Gerenciar estados do link
- Controlar transições de estado
- Implementar handshake
- Futuramente implementar protocolo SGGW

Classe central:

```
SerialLinkService
```

---

## 2.3 DAL (Data Access Layer / Transporte)

Responsável exclusivamente por:

- Comunicação serial crua
- Abrir e fechar porta
- Enviar e receber bytes
- Disparar eventos

Classe principal:

```
SerialTransport
```

Não possui lógica de protocolo.

---

## 2.4 Hardware

- ESP-32
- Comunicação via USB (Serial)
- Futuramente CAN / LIN / outros barramentos

---

# 3. Estado Atual do Projeto

## Versão Base (main)

- Conexão serial funcional
- Eventos propagando corretamente
- UI sincronizada
- Sem handshake
- Sem protocolo SGGW

Documentado em:

```
serial-connection.md
```

---

## Feature Atual (feature/link-handshake)

Implementação de:

- Máquina de estados do LINK
- Validação textual do dispositivo
- Timeout de sincronização
- Preparação para SGGW

Documentado em:

```
link-handshake.md
```

---

# 4. Próximas Evoluções Planejadas

1. Implementação do protocolo SGGW
2. Framing binário
3. CRC / Validação de integridade
4. Heartbeat / Watchdog
5. Reconexão automática
6. Expansão para múltiplos transportes (WiFi / Bluetooth)

---

# 5. Princípios Arquiteturais

- Transporte não conhece protocolo.
- Protocolo não conhece UI.
- UI não acessa DAL diretamente.
- Estados são explícitos.
- Eventos propagam mudanças de estado.
- Cada camada pode ser testada isoladamente.

---

# 6. Estrutura Recomendada de Documentação

```
/docs
    /architecture
        architecture-overview.md
        serial-connection.md
        link-handshake.md
```

---

# 7. Versionamento

Modelo adotado:

```
MAJOR.MINOR.PATCH
```

Exemplo:

- v0.1.0 → Serial estável
- v0.2.0 → Link implementado
- v0.3.0 → SGGW implementado

---

# 8. Direção de Longo Prazo

Evoluir para:

- Arquitetura preparada para múltiplos transportes
- Camada de protocolo independente de meio físico
- Expansão para nuvem
- Sincronização remota
- Sistema modular de simulação de ECUs

---

**Documento de referência arquitetural do projeto SimulDIESEL.**
