# Fluxo Git

## Estado atual

O repositório evidencia organização técnica por domínio, mas não traz uma política Git formal e completa versionada na documentação oficial atual. Não foram identificados, no material analisado, regras operacionais fechadas para branching, revisão, versionamento semântico ou promoção entre ambientes.

Ainda assim, a estrutura do projeto permite inferir os pontos de rastreabilidade que cada alteração precisa respeitar.

## Funcionamento técnico

### Acoplamentos que devem permanecer rastreáveis

Uma mudança coerente no SimulDIESEL normalmente afeta pelo menos um destes grupos:

- `docs/`: descrição oficial do comportamento;
- `local-api/`: comportamento do software local;
- `hardware/firmware/`: protocolo, roteamento ou serviço embarcado;
- `hardware/boards/`: artefatos físicos;
- `docs/legacy-docs/adr/`: contexto histórico de decisões já registradas.

### Fluxo de alteração observado como necessidade técnica

```text
Mudança de protocolo
  -> atualizar firmware do gateway
  -> revisar cliente local
  -> ajustar documentação oficial

Mudança de dispositivo
  -> atualizar tabela do gateway
  -> revisar firmware do módulo
  -> atualizar casos de uso e testes
```

Esse é o fluxo mínimo que o código atual impõe, independentemente de uma política de branch específica.

## Limitações

Sem uma política Git normativa no próprio repositório, não é seguro documentar nomes de branch, regras de merge ou obrigatoriedade de revisão como se fossem práticas institucionais já adotadas. O que existe hoje é uma necessidade clara de versionar em conjunto código, firmware e documentação, mas não uma convenção formal fechada.

## Evolução prevista

Quando o fluxo Git for formalizado, a documentação oficial deve registrar:

- estratégia de branch por release ou por feature;
- regra para mudanças de protocolo entre host e firmware;
- critério de atualização obrigatória da documentação oficial;
- relação entre ADRs, roadmap e entregas incrementais.

[Retornar ao README principal](../README.md)
