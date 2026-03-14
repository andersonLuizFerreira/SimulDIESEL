# Padrões de Código

## Estado atual

O repositório já exibe padrões claros de organização, ainda que nem todos estejam formalizados em um guia separado. Os padrões mais consistentes aparecem na separação de responsabilidades e no uso de classes pequenas com propósito definido.

No `local-api`, a estrutura principal é:

- `UI`: telas e interação com o operador;
- `BLL`: regras de negócio, sessão e coordenação de comandos;
- `DAL`: acesso ao transporte físico;
- `DTL`: estruturas de dados trocadas entre camadas.

Nos firmwares, a separação também é explícita:

- gateway: parser, link, roteador, tabela de dispositivos e barramentos;
- GSA: transporte, link, serviço e atuação física.

## Funcionamento técnico

### Padrões observáveis no C#

- abstração por interface (`IByteTransport`);
- máquina de estados explícita para o enlace;
- serviços com responsabilidade única (`SdGwHealthService`, `SdGwLinkEngine`);
- modelos de dados pequenos para frames e comandos.

### Padrões observáveis no C++

- separação entre protocolo e acesso ao barramento;
- tratamento de erro próximo da camada onde ele ocorre;
- constantes de protocolo centralizadas em headers dedicados;
- encapsulamento de dispositivos por serviço.

### Decisão arquitetural recorrente

O repositório favorece protocolos pequenos, validação local e baixo acoplamento:

```text
Transporte físico != Sessão != Regra funcional
```

Esse padrão aparece no host, no gateway e no periférico, o que facilita manutenção e expansão incremental.

## Limitações

Nem todo o código segue convenções idênticas de nomenclatura ou acabamento. Há diferenças de estilo entre áreas do projeto e a documentação desses padrões ainda é posterior ao código existente, não anterior. Também não há evidência, no estado atual analisado, de um pipeline automatizado forte de lint, análise estática ou padronização contínua entre C# e C++.

## Evolução prevista

Os padrões que merecem consolidação formal são:

- nomenclatura consistente de comandos, frames e serviços;
- documentação de estados de máquina por componente;
- política comum de códigos de erro;
- critérios de teste para cada camada antes da integração.

[Retornar ao README principal](../README.md)
