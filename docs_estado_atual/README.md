# SimulDIESEL

**SimulDIESEL** é uma plataforma modular para simulação e diagnóstico de módulos eletrônicos automotivos, projetada com arquitetura em camadas e foco em escalabilidade, robustez e evolução incremental.

---

# 📐 Arquitetura

O projeto segue uma estrutura vertical bem definida:

```
UI → BLL → DAL → Hardware
```

- **UI**: Interface do usuário (WinForms)
- **BLL**: Lógica de negócio e controle de estados
- **DAL**: Transporte serial cru
- **Hardware**: ESP-32 e módulos simulados

Documentação detalhada:

- 📘 [Architecture Overview](01_arquitetura/04_pc/architecture-overview.md)
- 📘 [Serial Connection (Baseline)](01_arquitetura/04_pc/serial-connection.md)
- 📘 [Link Handshake (Feature)](01_arquitetura/04_pc/link-handshake.md)
- 📘 [Technical Roadmap](04_desenvolvimento/technical-roadmap.md)

---

# 🚀 Estado Atual

## ✅ v0.1.0 – Serial Base

- Conexão serial estável
- Eventos propagando corretamente
- UI sincronizada automaticamente
- Arquitetura em camadas consolidada

## 🔄 Em desenvolvimento – v0.2.0

- Implementação da camada de LINK
- Handshake textual com validação de dispositivo
- Máquina de estados
- Preparação para protocolo SGGW

---

# 🧱 Estrutura do Projeto

```
/docs
    /architecture
/src
    /UI
    /BLL
    /DAL
```

---

# 🎯 Próximos Marcos

- Implementação do protocolo SGGW
- Heartbeat e watchdog
- Multi-transporte (WiFi / Bluetooth)
- Camada de simulação CAN
- Integração com infraestrutura em nuvem

---

# 📌 Princípios do Projeto

- Transporte não conhece protocolo
- Protocolo não conhece UI
- Estados são explícitos
- Evolução incremental por versão
- Cada feature em branch própria

---

# 📄 Licença

Definir conforme estratégia futura do projeto.

---

**SimulDIESEL – Plataforma de Simulação Modular Automotiva**

