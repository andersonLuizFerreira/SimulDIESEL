\# SD-GW-LINK — Gateway ↔ API Transport Protocol



\*\*Projeto:\*\* SimulDIESEL

\*\*Protocolo:\*\* SD-GW-LINK

\*\*Versão:\*\* 1.0.0

\*\*Status:\*\* Estável



---



\## Visão Geral



O \*\*SD-GW-LINK\*\* é a camada de transporte binária utilizada na comunicação entre:



\- \*\*Gateway embarcado\*\* (ex.: ESP32 Bridge)

\- \*\*API Local / Host PC\*\*



Esta camada é responsável exclusivamente por:



\- Framing e delimitação de frames via \*\*COBS\*\*

\- Integridade de dados via \*\*CRC-8/ATM\*\*

\- Sequenciamento via \*\*SEQ\*\*

\- ACK opcional de transporte

\- Suporte a eventos assíncronos Gateway → API



> A camada não interpreta comandos de aplicação (CAN, periféricos, firmware).



---



\## Documentação Oficial



📄 Especificação completa:



\- \[`spec.pt-BR.md`](spec.pt-BR.md)



---



\## Exemplos e Vetores de Teste



Os exemplos oficiais em hexadecimal estão disponíveis em:



\- \[`examples/`](examples/)

\- \[`examples/README.md`](examples/README.md)



Arquivos incluídos:



\- `ping.hex`

\- `ack.hex`

\- `event-level.hex`

\- `payload-with-zero.hex`



---



\## Decisões de Arquitetura (ADR)



Decisão técnica registrada em:



\- `specs/adr/ADR-0007-cobs-crc8.pt-BR.md`



---



\## Extensões Futuras



Campos reservados para evolução:



\- Fragmentação (`FLAGS.FRAG`)

\- Janela deslizante (modo avançado)

\- CRC16 superior para firmware



---



\*\*Fim do índice do protocolo SD-GW-LINK\*\*

