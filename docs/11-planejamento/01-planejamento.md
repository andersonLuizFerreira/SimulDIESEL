⬅ [Retornar para Pai Imediato (Índice Geral)](../00-INDICE.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Planejamento

## Situação após o fechamento desta etapa

O planejamento vivo do projeto muda de patamar depois desta rodada.

O que fica consolidado:

- arquitetura documental oficial estabilizada
- host local aprofundado sobre o código real
- firmware BPM, GSA e UCE aprofundados sobre o código real
- hardware físico documentado no limite real do repositório

## Marco de encerramento

Esta etapa é considerada fechada com a seguinte leitura oficial:

- **IMPLEMENTADO**: base documental transversal do projeto
- **IMPLEMENTADO**: trilhas `ONDE` e `COMO` fora e dentro da API
- **IMPLEMENTADO**: host, BPM, GSA e UCE possuem caminhos observáveis no código versionado
- **PARCIALMENTE IMPLEMENTADO**: CAN/J1939 e Banco Local API existem com limites de validação, catálogo e UI ainda explícitos
- **PRONTO PARA PRÓXIMA ETAPA**: ampliar validação de bancada, catálogos e fluxos operacionais sobre a base UCE/SDCTP já existente

## Linha de continuidade

```text
Consolidação documental
  -> alinhamento docs com código atual
  -> validação ampliada da UCE e do SDCTP
  -> expansão controlada de catálogos, banco e casos de bancada
```

## Critério de priorização daqui para frente

1. validar em bancada os fluxos UCE/SDCTP já presentes no host e no firmware
2. ampliar catálogos J1939 e Banco Local API sem romper a fronteira UI -> BLL -> DAL
3. expandir catálogo de boards apenas onde houver implementação real

## Glossário

- **Marco**: ponto formal de fechamento de uma etapa.
- **Pendência remanescente**: item reconhecido como ainda não concluído, mas já delimitado.
- **UCE**: Unidade de Comunicação Externa, board remota SPI com LED, CAN e SDCTP presentes no código.
- **SDCTP**: SimulDIESEL CAN Transport Protocol, camada de massa CAN RX/TX sobre a rota UCE.

## Próximas camadas

- [Próximas Funcionalidades](02-proximas-funcionalidades.md)
