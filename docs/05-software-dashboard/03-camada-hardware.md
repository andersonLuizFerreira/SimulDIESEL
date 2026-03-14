# Camada Hardware do Software

## Estado atual

A camada de hardware do software local é a porção da aplicação que faz a ponte entre a UI e o gateway físico. No repositório, ela está distribuída principalmente entre `DAL` e `BLL`, com interfaces e serviços que isolam a serial, o handshake e o protocolo binário da interface de usuário.

Os componentes centrais são:

- `IByteTransport`
- `SerialTransport`
- `SerialLinkService`
- `SdGwLinkEngine`
- `SdGgwClient`
- `SdGwHealthService`

## Funcionamento técnico

### Responsabilidades por classe

- `IByteTransport`: contrato mínimo para leitura, escrita e ciclo de vida do transporte.
- `SerialTransport`: implementação concreta do acesso à porta serial.
- `SerialLinkService`: sincronização inicial com o firmware, controle de estados e detecção de banner.
- `SdGwLinkEngine`: empacotamento de frames, confirmação por `ACK`, cache de respostas e timeout/retry.
- `SdGgwClient`: API orientada a comando para a camada superior.
- `SdGwHealthService`: monitoramento periódico por `ping`, com notificação de mudança de estado do link.

### Fluxo técnico interno

```text
UI
  -> SdGgwClient.SendAsync(...)
  -> SdGwLinkEngine monta frame
  -> SerialTransport envia bytes
  -> resposta retorna pela serial
  -> engine valida, correlaciona SEQ e libera a resposta
```

Esse desenho é importante porque a UI não precisa conhecer `COBS`, `CRC8`, sequência ou retransmissão. A aplicação local concentra a complexidade de hardware em serviços especializados.

### Exemplo conceitual de payload

Para uma operação de teste no gateway:

```text
CMD   = 0x01   ; gateway/ping ou echo conforme opcode
FLAGS = 0x01   ; requer ACK
SEQ   = 0x2A
DATA  = payload opcional
```

O payload só chega ao nível de UI já convertido em objetos ou eventos compreensíveis pela camada superior.

## Limitações

A camada atual está claramente otimizada para um transporte por serial e para uma sessão por vez. O repositório não mostra suporte a múltiplos gateways concorrentes, seleção dinâmica de outros meios físicos ou fila complexa de comandos paralelos. A cobertura funcional também é maior no enlace do que nos serviços de negócio consumidos pela UI.

## Evolução prevista

Os pontos mais prováveis de evolução são:

- encapsular novos casos de uso em clientes de mais alto nível sobre `SdGgwClient`;
- ampliar telemetria de saúde e diagnóstico do enlace;
- formalizar contratos de resposta e eventos para reduzir lógica ad hoc na interface.

[Retornar ao README principal](../README.md)
