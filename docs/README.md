# SimulDIESEL — Documentação Oficial

Bem-vindo à documentação oficial do **SimulDIESEL**.

Esta pasta é o ponto de entrada para a documentação consolidada do projeto. Ela não substitui o código, o histórico do Git ou os registros de validação, mas organiza a leitura humana sobre a arquitetura, os protocolos, o hardware, o firmware, o software e os casos de uso.

A documentação está organizada como uma **árvore hierárquica de aprofundamento progressivo**. A ideia é começar por um mapa geral, entrar na visão do projeto e avançar para documentos mais específicos somente quando necessário.

## Como navegar

Use o [índice geral](./00-INDICE.md) como ponto de partida. Ele funciona como o mapa global da documentação.

Dentro de cada página oficial:

* use o link de retorno ao pai imediato para subir um nível na árvore;
* use o link de retorno ao índice geral para voltar ao mapa principal;
* siga a seção **Próximas camadas** quando quiser continuar para documentos mais específicos.

Essa estrutura evita caminhos duplicados e mantém a leitura previsível.

## Visões de leitura

A documentação pode ser lida por duas perspectivas complementares:

* **visão física**: placas, conexões, sinais, barramentos e interfaces reais da bancada;
* **visão lógica**: camadas de software, protocolos, comandos, contratos e responsabilidades internas.

A visão geral do projeto está documentada em [Visão Geral do Projeto](./01-visao-geral/01-visao-geral-projeto.md).

## Governança documental

A governança oficial para agentes e manutenção documental está em [`.agents/README.md`](../.agents/README.md) e na skill [docs-governance](../.agents/skills/docs-governance/SKILL.md).

Para leitores humanos, a regra prática é simples: a pasta `docs/` deve explicar o projeto de forma clara, didática e atual, sem se confundir com logs de execução, dumps, prompts ou relatórios operacionais.

## Papel desta página

Esta página apresenta a lógica de navegação da pasta `docs/` e direciona o leitor para o índice global da documentação viva.

O acervo histórico removido da árvore viva continua preservado pelo Git e por registros em `out/dumps/`. Esses registros podem ser úteis para auditoria ou recuperação de versões antigas, mas não substituem a documentação consolidada desta pasta.

## Glossário

- **Documentação oficial**: árvore viva usada como referência atual do projeto.
- **Índice geral**: mapa global clicável da navegação documental.
- **Pai imediato**: documento estrutural acima da página atual.
- **Governança documental**: conjunto de regras que controla estrutura, navegação, estilo e revisões da documentação.
- **Visão física**: leitura orientada a placas, sinais, conectores, barramentos e módulos reais.
- **Visão lógica**: leitura orientada a camadas de software, protocolos, comandos, contratos e fluxos internos.

## Próximas camadas

* [00-INDICE — Índice Geral da Navegação](./00-INDICE.md)
