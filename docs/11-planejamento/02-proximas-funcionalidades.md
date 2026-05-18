⬅ [Retornar para Planejamento](01-planejamento.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Próximas Funcionalidades

## Prioridade oficial após esta etapa

As próximas frentes oficiais, respeitando o código real e o fechamento desta rodada, são:

1. ampliar validação de bancada da `UCE (Unidade de Comunicação Externa)`, SDCTP RX/TX e integração CAN/J1939
2. expandir o Banco Local API e catálogos J1939 somente a partir de dados versionados e validáveis
3. ampliar o catálogo de boards somente quando houver firmware ou hardware vivo correspondente

## O que não deve entrar como prioridade falsa

- expansão especulativa de múltiplas boards sem código
- detalhamento elétrico inventado para o backplane
- promoção de parser `SDH` embarcado como se já existisse na BPM
- reintrodução de diretórios documentais paralelos dentro de `docs/`

## Leitura prática

O projeto sai desta etapa com:

- documentação oficial madura
- BPM, GSA e UCE descritas no nível de classes, métodos e trechos reais
- host com clients, FormsLogic e UI para GSA e UCE
- base pronta para validação operacional mais ampla de CAN, J1939 e Banco Local API

## Glossário

- **Prioridade oficial**: direção de evolução compatível com a implementação real.
- **UCE**: Unidade de Comunicação Externa, board remota SPI já presente no firmware e no host.
- **SDCTP**: protocolo de massa CAN RX/TX usado pela UCE e pela API.
- **Prioridade falsa**: item documentalmente sedutor, mas sem evidência real suficiente.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
