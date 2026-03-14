# SimulDIESEL – Technical Roadmap

**Projeto:** SimulDIESEL  
**Tipo:** Roadmap Técnico  
**Objetivo:** Planejamento evolutivo do sistema por versões

---

# 1. Fase Atual

## v0.1.0 – Serial Base (Concluído)

✔ Conexão serial estável  
✔ Arquitetura UI → BLL → DAL consolidada  
✔ Eventos propagando corretamente  
✔ Facade SerialLink implementada  
✔ Atualização automática da UI  

---

# 2. Próxima Fase

## v0.2.0 – Camada de Link (Em desenvolvimento)

Objetivos:

- Implementar máquina de estados do LINK
- Descartar lixo de boot (DRAINING)
- Implementar banner textual de validação
- Implementar timeout de handshake
- Garantir estado LINKED antes do SGGW

Resultado esperado:

- Comunicação determinística
- Base sólida para protocolo binário

---

# 3. Evolução de Protocolo

## v0.3.0 – SGGW Framing

Objetivos:

- Definir framing binário
- Implementar delimitadores
- Implementar CRC
- Validar integridade de pacotes

---

## v0.4.0 – Dispatcher & Command Layer

Objetivos:

- Interpretador de comandos
- Tabela de mensagens
- Identificação de módulos simulados
- Respostas estruturadas

---

# 4. Confiabilidade

## v0.5.0 – Heartbeat & Watchdog

Objetivos:

- Ping periódico
- Timeout de sessão
- Detecção automática de cabo removido
- Reconexão controlada

---

# 5. Expansão de Transporte

## v0.6.0 – Multi-Transport Support

Objetivos:

- Separar transporte do protocolo
- Adicionar WiFi
- Adicionar Bluetooth
- Abstração de transporte comum

---

# 6. Camada de Simulação

## v0.7.0 – Simulação de Módulos

Objetivos:

- Implementar gerador CAN
- Simular sensores
- Simular estados de ECU
- Integração com hardware modular

---

# 7. Camada Cloud

## v1.0.0 – Infraestrutura Completa

Objetivos:

- Integração com backend em nuvem
- Autenticação por assinatura
- Sincronização remota
- Atualização OTA
- Banco de dados de testes

---

# 8. Princípios do Roadmap

- Cada versão deve ser estável antes da próxima.
- Features grandes devem ser divididas em branches específicas.
- Cada marco deve possuir documentação própria.
- Nenhuma camada deve violar responsabilidade arquitetural.

---

# 9. Direção Estratégica

Evoluir o SimulDIESEL para:

- Plataforma modular de diagnóstico
- Sistema escalável
- Suporte multi-ECU
- Integração com IA futura

---

**Roadmap técnico oficial do projeto SimulDIESEL.**
