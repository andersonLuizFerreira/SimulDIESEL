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

Além desses componentes, o host passou a contar com uma primeira camada semântica SDH, introduzida acima do transporte atual.

## Camada SDH implementada no host

A primeira implementação do SDH no host foi projetada para não substituir o transporte atual. Em vez disso, ela encapsula semanticamente o envio de comandos e adapta esses comandos ao contrato legado já funcional.

Os componentes introduzidos são:

- `SdhCommand`
- `SdhTarget`
- `SdhResponse`
- `SdhTextParser`
- `SdhTextSerializer`
- `SdhValidator`
- `SdhToSggwMapper`
- `SdhClient`
- `GsaClient`

O primeiro caso funcional implementado é:

    sdh/1 GSA.led set state=on
    sdh/1 GSA.led set state=off

## Responsabilidades por classe

### Camada de transporte e sessão

- `IByteTransport`: contrato mínimo para leitura, escrita e ciclo de vida do transporte.
- `SerialTransport`: implementação concreta do acesso à porta serial.
- `SerialLinkService`: sincronização inicial com o firmware, controle de estados e detecção de banner.
- `SdGwLinkEngine`: empacotamento de frames, confirmação por `ACK`, cache de respostas e timeout/retry.
- `SdGgwClient`: API orientada ao envio sobre o contrato atual de transporte.
- `SdGwHealthService`: monitoramento periódico por `ping`, com notificação de mudança de estado do link.

### Camada semântica SDH

- `SdhCommand`: envelope semântico do comando no host.
- `SdhTarget`: decomposição do target lógico.
- `SdhTextParser`: parse da forma textual canônica.
- `SdhTextSerializer`: serialização da forma textual canônica.
- `SdhValidator`: validação estrutural e funcional do comando SDH suportado.
- `SdhToSggwMapper`: adaptação do SDH para o contrato legado atualmente funcional.
- `SdhClient`: cliente central da camada SDH.
- `GsaClient`: ergonomia por board para o caso do GSA.

## Fluxo técnico interno

O fluxo atual do primeiro caso funcional é:

    UI / BLL
      -> GsaClient.SetLedAsync(true)
      -> SdhClient.SendAsync(...)
      -> SdhValidator.Validate(...)
      -> SdhToSggwMapper.Map(...)
      -> SdGgwClient.SendAsync(...)
      -> SdGwLinkEngine monta frame
      -> SerialTransport envia bytes
      -> resposta retorna pela serial
      -> engine valida e libera a resposta

Esse desenho é importante porque a UI não precisa conhecer `COBS`, `CRC8`, sequência ou retransmissão. A aplicação local concentra a complexidade de hardware em serviços especializados e passa a contar também com uma camada semântica formal para envio de comandos.

## Estado atual da integração

Na fase atual:

- a transmissão já usa SDH como camada semântica;
- a recepção ainda permanece no modelo legado de `SggwFrame`;
- o binding entre endereço lógico e físico ainda não é responsabilidade do host;
- o mapper atual cobre apenas o caso `GSA.led set state=on|off`.

Essa adoção incremental foi escolhida para reduzir risco e preservar compatibilidade com o que já estava estável no projeto.

## Exemplo conceitual do fluxo atual

Comando semântico:

    sdh/1 GSA.led set state=on

Adaptação atual no host:

- target lógico: `GSA.led`
- operação: `set`
- argumento: `state=on`
- contrato legado reaproveitado:
  - `SggwCmd.LED`
  - payload de 1 byte
  - `0x01` para `on`
  - `0x00` para `off`

## Limitações

A camada atual está claramente otimizada para um transporte por serial e para uma sessão por vez. O repositório não mostra suporte a múltiplos gateways concorrentes, seleção dinâmica de outros meios físicos ou fila complexa de comandos paralelos. A cobertura funcional também é maior no enlace do que nos serviços de negócio consumidos pela UI.

Do lado do SDH, as limitações atuais são:

- apenas um target/operação suportado no host;
- ausência de adaptação formal de respostas para `SdhResponse`;
- ausência de adaptação formal de eventos para SDH;
- ausência de catálogo amplo de boards;
- ausência de binding lógico-físico.

## Evolução prevista

Os pontos mais prováveis de evolução são:

- encapsular novos casos de uso em clients por board sobre `SdhClient`;
- ampliar telemetria de saúde e diagnóstico do enlace;
- formalizar contratos de resposta e eventos para reduzir lógica ad hoc na interface;
- preparar a implementação do SDH no gateway;
- transferir a resolução lógico-física para a camada embarcada, como previsto na arquitetura.

## Referência adicional

Para o detalhamento formal da implementação atual do SDH no host, consulte:

- `docs/05-software-dashboard/04-sdh-host-architecture.md`

[Retornar ao README principal](../README.md)
