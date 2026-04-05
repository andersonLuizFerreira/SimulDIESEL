⬅ [Retornar para Camadas do Sistema](../02-arquitetura/02-camadas-do-sistema.md)

# Arquitetura do Software Dashboard (Local API)

O software local do SimulDIESEL é a camada que transforma a operação de bancada em ações executáveis sobre o gateway e as boards.

Nesta altura da árvore, o objetivo é entender **o papel do software**, não o detalhe de cada classe.

Ele atua como ponte entre:

* operador
* interface gráfica
* lógica de aplicação
* sessão de comunicação com o hardware
* casos de uso funcionais de bancada

---

## Papel do software local

O software local concentra três responsabilidades principais.

### 1. Operação

Disponibilizar telas e fluxos para que o operador:

* abra a conexão com a bancada
* acompanhe o estado do link
* execute ações de simulação, diagnóstico e manutenção

---

### 2. Orquestração funcional

Transformar ações da interface em intenções funcionais coerentes com o domínio do projeto.

Isso inclui:

* decidir qual caso de uso está sendo executado
* selecionar o transporte ativo
* coordenar a sessão de comunicação
* encaminhar comandos aos clients funcionais

---

### 3. Integração com o hardware

Isolar da interface os detalhes do enlace com o gateway e da troca de dados com as boards.

Essa separação evita que a UI precise conhecer framing, temporização ou protocolo físico.

---

## Estrutura conceitual

Em nível alto, a aplicação local segue o fluxo:

```text
Operador
  -> Interface de usuário
  -> Lógica de aplicação
  -> Sessão host/gateway
  -> Gateway BPM
  -> Board remota
```

Essa estrutura existe para que a aplicação continue evoluindo sem misturar:

* operação visual
* coordenação funcional
* transporte e protocolo
* integração com o hardware

---

## Estado atual

O repositório confirma que a aplicação atual é uma base WinForms voltada à operação de bancada local.

O caso mais maduro hoje continua sendo a interação com a GSA, mas a arquitetura do software já separa a camada de interface da camada que efetivamente conversa com o gateway.

Por isso, os próximos aprofundamentos deste ramo se dividem em duas frentes:

* **Interface de usuário**: como o operador interage com a bancada
* **Camada hardware do software**: como a aplicação materializa essa interação no enlace com o gateway

Os detalhes de classes, services, scheduler, supervisor e transporte ficam para a camada inferior imediatamente dedicada ao hardware do software.

## Próximas camadas

- [Interface de Usuário](02-interface-usuario.md)
- [Camada Hardware do Software](03-camada-hardware.md)
