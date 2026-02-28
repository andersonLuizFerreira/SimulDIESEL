# Serial Connection – Versão Base (Main)

**Projeto:** SimulDIESEL  
**Status:** Estável e funcional  
**Baseline:** v0.1.0-serial-ok  
**Escopo:** Conexão serial USB (sem protocolo SGGW)

---

# 1. Objetivo

Implementar uma conexão serial estável entre a API (WinForms) e o hardware (ESP-32), garantindo:

- A porta permanece aberta independentemente de Forms.
- A UI é atualizada automaticamente quando a conexão muda.
- A arquitetura segue o modelo em camadas (UI → BLL → DAL).
- Nenhum protocolo de aplicação (SGGW) é implementado neste estágio.

---

# 2. Arquitetura Vertical Atual

```
UI (DashBoard / frmPortaSerial_UI)
        ↓
BLL (SerialLinkService)
        ↓
DAL (SerialTransport)
        ↓
System.IO.Ports.SerialPort
```

---

# 3. Responsabilidades por Camada

## 3.1 DAL – SerialTransport

Responsável exclusivamente por:

- Listar portas
- Abrir conexão
- Fechar conexão
- Enviar bytes crus
- Receber bytes crus
- Disparar eventos de erro
- Disparar evento de mudança de conexão

### Eventos expostos

```csharp
event Action<byte[]> BytesReceived;
event Action<bool> ConnectionChanged;
event Action<string[]> Error;
```

### Observações importantes

- Não há framing.
- Não há checksum.
- Não há protocolo.
- Apenas transporte cru.

---

## 3.2 BLL – SerialLinkService

Responsável por:

- Ser o intermediário entre UI e DAL.
- Repassar eventos do DAL.
- Expor `Connect()`, `Disconnect()`.
- Expor propriedade `IsConnected`.

### Eventos expostos

```csharp
event Action<bool> ConnectionChanged;
event Action<string[]> Error;
event Action<byte[]> BytesReceived;
```

### Comportamento

- Assina eventos do `SerialTransport`.
- Repassa para UI.
- Não mantém estado de protocolo.
- Não executa handshake.

---

## 3.3 Facade – SerialLink

Classe estática que fornece:

```csharp
public static SerialLinkService Service { get; }
public static bool IsConnected { get; }
public static void Close();
```

Função:

- Ponto único de acesso ao link.
- Mantém a conexão viva independentemente de Forms.
- Evita múltiplas instâncias.

---

## 3.4 UI – DashBoard

Responsável por:

- Abrir form de conexão.
- Fechar conexão se já estiver aberta.
- Atualizar botão e ícone dinamicamente.
- Assinar evento `ConnectionChanged`.

### Atualização segura de thread

O evento utiliza:

```csharp
if (InvokeRequired)
{
    BeginInvoke(...);
}
```

Garantindo atualização segura da UI.

---

# 4. Fluxo de Conexão

### Conectar

1. Usuário clica em "Conectar".
2. Form de conexão abre.
3. `SerialLinkService.Connect()` é chamado.
4. DAL abre a porta.
5. DAL dispara `ConnectionChanged(true)`.
6. BLL repassa evento.
7. UI atualiza botão e ícone.

### Desconectar

1. Usuário clica em "Desconectar".
2. `SerialLink.Close()` é chamado.
3. DAL fecha a porta.
4. DAL dispara `ConnectionChanged(false)`.
5. UI atualiza estado.

---

# 5. Características Técnicas Atuais

- Comunicação USB (porta COM).
- Controle manual de DTR/RTS configurável.
- Timeout configurável.
- Eventos propagados corretamente entre camadas.
- Conexão permanece ativa após fechamento do Form de conexão.
- Atualização visual do botão com ícone dinâmico.

---

# 6. Limitações da Versão Atual

- Não detecta remoção física do cabo USB automaticamente.
- Não possui handshake.
- Não valida identidade do dispositivo conectado.
- Não implementa protocolo SGGW.
- Não possui heartbeat/watchdog.

---

# 7. Próximo Passo Planejado

Implementação da **Camada de Link (Handshake)**:

Estados planejados:

```
DISCONNECTED
SERIAL_CONNECTED
DRAINING
BANNER_SENT
LINKED
LINK_FAILED
```

Objetivo:

- Validar dispositivo via banner textual.
- Descartar lixo de boot.
- Garantir sincronização antes de iniciar SGGW.

---

# 8. Tag de Referência

Esta documentação corresponde à tag:

```
v0.1.0-serial-ok
```

Representa a versão estável da conexão serial antes da introdução da camada de LINK.
