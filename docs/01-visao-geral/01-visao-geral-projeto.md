⬅ [Retornar para Pai Imediato (Índice Geral)](../00-INDICE.md)
⬅ [Retornar para Índice Geral](../00-INDICE.md)

# Visão Geral do Projeto

O **SimulDIESEL** é uma plataforma de bancada voltada à manutenção, diagnóstico, análise, simulação e validação de módulos eletrônicos da linha Diesel, com foco em caminhões, máquinas agrícolas e equipamentos pesados.

A proposta do projeto é permitir que um módulo eletrônico seja analisado fora do veículo, em uma bancada controlada, recebendo sinais, comandos e mensagens semelhantes aos que encontraria em uma máquina real.

Com isso, o técnico pode observar o comportamento do módulo, testar hipóteses de falha, validar respostas elétricas e de comunicação, além de documentar resultados de manutenção e diagnóstico.

## O que o projeto é hoje

O SimulDIESEL é constituído por um software local que controla uma bancada eletrônica modular. Essa bancada pode ser entendida como um rack composto por placas com funções específicas, interligadas por uma estrutura comum.

Essa modularidade permite que o projeto cresça de forma organizada: novas placas podem ser adicionadas conforme novas funções forem necessárias, mantendo a escalabilidade da bancada e separando responsabilidades entre os componentes.

A bancada é formada por alguns blocos principais:

- **Backplane**: estrutura central de interligação elétrica e lógica da bancada. Ela distribui sinais, alimentação e comunicação entre as placas conectadas ao rack.
- **X-CONN**: placa de conexão entre o chicote do módulo em teste e o backplane. Ela faz a ponte entre os pinos reais do módulo e os recursos disponíveis na bancada.
- **BPM (Backplane Manager Module)**: placa responsável por gerenciar a comunicação entre a aplicação Windows local e as demais placas da bancada. Dentro da arquitetura, a BPM atua como gateway e hub de mensagens, comandos e configurações vindas da aplicação.
- **UCE (Unidade de Comunicação Externa)**: placa responsável por concentrar as interfaces de comunicação com os módulos em teste. Ela é voltada a protocolos automotivos e industriais usados em módulos Diesel, como CAN/J1939 e outros protocolos previstos para evolução do projeto.
- **GSA (Gerador de Sinais Analógicos)**: placa responsável por gerar níveis elétricos contínuos para simular sinais como temperatura, pressão, nível de combustível e outras grandezas analógicas. A GSA conta com canais de saída de `0–5 V` e `0–12 V`, permitindo simular diferentes faixas de sensores.

As demais placas da bancada ainda não foram implementadas. A documentação oficial será atualizada conforme novas placas forem desenvolvidas, validadas e integradas ao projeto.

## Glossário

- **Backplane**: estrutura central da bancada, responsável por interligar eletricamente as placas e distribuir sinais, alimentação e comunicação.
- **BPM**: Backplane Manager, placa que atua como gateway e hub de mensagens entre a aplicação local e as placas conectadas ao backplane.
- **GSA**: Gerador de Sinais Analógicos, placa responsável por gerar níveis elétricos contínuos para simulação de sensores e grandezas analógicas.
- **Host local**: software executado no computador, responsável por controlar a bancada, enviar comandos e apresentar informações ao operador.
- **Módulo em teste**: central ou módulo eletrônico Diesel conectado à bancada para diagnóstico, simulação ou validação.
- **UCE**: Unidade de Comunicação Externa, placa responsável pelas interfaces de comunicação com os módulos em teste.
- **X-CONN**: placa de conexão entre o chicote do módulo em teste e o backplane da bancada.

## Próximas camadas

- [Visão Arquitetural](../02-arquitetura/01-visao-arquitetural.md)
