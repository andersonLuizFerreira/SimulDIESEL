# Link Layer – Handshake (Feature)

**Projeto:** SimulDIESEL  
**Branch:** feature/link-handshake  
**Escopo:** Implementação da camada de LINK entre API e ESP-32  
**Dependência:** v0.1.0-serial-ok (conexão serial estável)

---

# 1. Objetivo

Adicionar uma camada intermediária entre a conexão serial e o protocolo SGGW,
garantindo que:

- O dispositivo conectado seja validado.
- Lixo de boot seja descartado.
- Haja sincronização explícita antes do início do tráfego binário.
- A API e o hardware entrem em estado confiável.

---

# 2. Conceito da Camada de Link

A conexão serial aberta não significa que o dispositivo está pronto para uso.

O LINK adiciona:

- Validação textual inicial (banner)
- Máquina de estados
- Timeout de sincronização
- Transição controlada para SGGW

---

# 3. Máquina de Estados

Estados definidos:

```
DISCONNECTED
SERIAL_CONNECTED
DRAINING
BANNER_SENT
LINKED
LINK_FAILED
```

---

# 4. Fluxo do Handshake

## 4.1 Abertura da Serial

Quando `ConnectionChanged(true)` ocorre:

1. Estado → SERIAL_CONNECTED
2. Inicia handshake
3. Entra em DRAINING

---

## 4.2 DRAINING

Janela de aproximadamente 300 ms:

- Todos os bytes recebidos são descartados.
- Remove lixo de boot do ESP-32.

Transição automática após o tempo expirar.

---

## 4.3 Envio do Banner

A API envia:

```
\nSIMULDIESELAPI\n
```

Estado → BANNER_SENT

A partir deste momento a API aguarda resposta do ESP.

---

## 4.4 Resposta Esperada

O ESP-32 deve responder com:

```
\nSimulDIESEL ver 1.0\n
```

A validação ocorre por prefixo:

```
"SimulDIESEL ver"
```

Se recebido corretamente:

Estado → LINKED

---

## 4.5 Timeout

Se a resposta não for recebida em até 2000 ms:

Estado → LINK_FAILED

Política recomendada:

- Fechar a serial
- Retornar para DISCONNECTED

---

# 5. Após LINKED

Somente após entrar em LINKED:

- Protocolo SGGW é permitido
- Parser binário pode ser ativado
- Heartbeat pode ser implementado

---

# 6. Responsabilidades por Camada

## DAL (SerialTransport)

Continua responsável apenas por transporte cru.

Não implementa handshake.

---

## BLL (SerialLinkService)

Responsável por:

- Máquina de estados do LINK
- Timer de handshake
- Parser textual temporário
- Transição para LINKED

---

## UI

Pode reagir a:

```
event Action<LinkState> LinkStateChanged;
```

Permitindo exibir:

- Serial Conectada
- Link em andamento
- Link OK
- Link falhou

---

# 7. Benefícios Arquiteturais

- Isola protocolo de transporte
- Garante sincronização determinística
- Prepara base sólida para SGGW
- Permite futura implementação de watchdog

---

# 8. Próximo Passo Após Esta Feature

- Implementar framing SGGW
- Implementar controle de timeout de sessão
- Implementar heartbeat periódico
- Implementar reconexão automática opcional

---

# 9. Status da Feature

Em desenvolvimento na branch:

```
feature/link-handshake
```

Ainda não mesclada à main.
