# Próximas Funcionalidades

## Estado atual

As próximas funcionalidades mais sustentadas pelo repositório não são especulações amplas; elas surgem diretamente dos espaços já preparados pela arquitetura e ainda pouco ocupados pelo código. O projeto já possui transporte, link, roteamento e um primeiro dispositivo. O passo seguinte natural é aumentar densidade funcional sobre essa base.

## Funcionamento técnico

### Frentes que o código já prepara

1. Mais serviços no GSA ou em dispositivos equivalentes.
2. Ampliação de `GwDeviceTable` com módulos adicionais.
3. Casos de uso novos no software local sobre `SdGgwClient`.
4. Formalização dos contratos externos de software.

### Sinais concretos de evolução possível

- `cloud/api-contracts/openapi.yaml` existe, mas ainda sem rotas operacionais;
- a tabela de dispositivos do gateway já comporta múltiplos destinos;
- o padrão `Transport -> Link -> Service` é replicável;
- os documentos legados já discutem protocolo e roadmap em mais de uma etapa.

### Prioridade arquitetural sugerida pelo estado atual

```text
Mais serviços reais
  -> mais testes integrados
  -> mais documentação por dispositivo
  -> mais valor de bancada
```

Esse encadeamento é coerente porque o sistema já superou a fase de transporte básico; agora o gargalo é a riqueza funcional dos módulos.

## Limitações

Não há, no repositório analisado, um backlog versionado e detalhado por issue ou sprint que permita afirmar ordem fechada de implementação. Também não é possível prometer funcionalidades automotivas específicas sem código correspondente. As próximas funcionalidades listadas aqui são as mais compatíveis com a arquitetura e com os artefatos já presentes.

## Evolução prevista

As evoluções mais prováveis e tecnicamente justificadas são:

- expansão do conjunto de comandos roteáveis;
- documentação formal de payloads e contratos por dispositivo;
- amadurecimento do software local para diagnóstico e operação;
- introdução futura de protocolos adicionais quando houver implementação real no repositório.

[Retornar ao README principal](../README.md)
