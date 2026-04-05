⬅ [Retornar para 00-INDICE — Mapa da árvore documental](../../00-INDICE.md)

# Introdução

O **SimulDIESEL** é uma plataforma modular de bancada desenvolvida para **diagnóstico, simulação e manutenção de módulos eletrônicos automotivos e industriais**, com foco especial em aplicações diesel, veículos pesados e máquinas agrícolas.

O projeto integra **software local**, **hardware modular especializado** e **firmware embarcado**, permitindo reproduzir condições reais de operação em ambiente controlado.

Seu propósito é oferecer ao operador uma estrutura segura e escalável para:

* testar módulos eletrônicos
* simular sinais e comandos
* validar funcionamento de periféricos
* diagnosticar falhas
* executar manutenção assistida em bancada

A arquitetura foi concebida para crescimento progressivo, com separação clara entre:

* operação no software local
* comunicação entre camadas
* gateway central
* barramentos internos
* módulos especializados

Essa organização permite expansão contínua do sistema sem ruptura da base existente.

## Estado atual

Atualmente o projeto possui como núcleo funcional consolidado:

* aplicação Windows para operação local
* enlace confiável com gateway
* roteamento interno de barramentos
* primeiros módulos funcionais de bancada

A base de comunicação e roteamento encontra-se mais madura que o catálogo funcional de módulos.

## Evolução prevista

A evolução natural do projeto inclui:

* ampliação do catálogo de boards
* expansão dos serviços funcionais
* aumento da cobertura de testes
* inclusão de novos protocolos e cenários de bancada

## Próximas camadas

* [Objetivos](02-objetivos.md)
