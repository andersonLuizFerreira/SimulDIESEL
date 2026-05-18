⬅ [Retornar para Backplane](01-backplane.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Alimentação

Esta página documenta apenas o que a árvore atual permite afirmar com segurança sobre alimentação.

## Estado real

- **PARCIALMENTE IMPLEMENTADO**: a separação entre alimentação da bancada e lógica das boards aparece na arquitetura física e nas faixas de saída da GSA.
- **IMPLEMENTADO**: a GSA possui firmware e hardware orientados a canais `0..5 V` e `0..12 V`.
- **PARCIALMENTE IMPLEMENTADO**: a placa raiz da bancada ainda não expõe um esquema populado que permita documentar toda a distribuição de energia com pinagem e netlist.
- **PLANEJADO**: documentação completa de domínios, proteções e orçamento de potência do backplane.

## O que o código confirma

Em `config.h` da GSA:

- canais `1..8` usam faixa máxima de `5000 mV`
- canais `9..16` usam faixa máxima de `12000 mV`

Isso confirma a existência funcional de dois grupos de saída analógica.

## O que o hardware confirma parcialmente

- a GSA possui esquemático próprio e circuito local dedicado;
- a bancada possui placa raiz reservada em `hardware/boards/SimulDIESEL`;
- ainda falta material equivalente para descrever toda a distribuição de energia da placa mãe com o mesmo rigor da GSA.

## Leitura correta desta página

Esta não é uma ficha elétrica completa. Ela apenas registra que:

- há mais de um domínio funcional relevante na bancada;
- a GSA já opera com duas faixas de saída documentadas;
- a documentação de alimentação do backplane ainda está em maturação.

## Glossário

- **Domínio de alimentação**: conjunto de circuitos que compartilha uma mesma faixa ou propósito de energia.
- **Faixa de saída**: intervalo elétrico que um canal é projetado para produzir.
- **Placa mãe da bancada**: backplane físico principal do projeto.

## Próximas camadas

- Esta é uma página terminal deste ramo da documentação.
