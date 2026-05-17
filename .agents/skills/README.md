# Agent Skills - SimulDIESEL

Estas skills estruturadas orientam agentes de IA que trabalham no projeto SimulDIESEL.

A fonte comum de governanca para agentes e `.agents/`.

Antes de executar qualquer ETAPA, leia:

1. `README.md`
2. `.agents/instructions.md`
3. `.agents/README.md`
4. a skill relevante em `.agents/skills/`
5. a documentacao aplicavel em `docs/`

Em caso de divergencia entre `.agents/skills/`, `docs/` e o codigo, adote a regra mais conservadora e registre a divergencia como `pendente de confirmacao`.

| Skill | Quando usar |
| --- | --- |
| `simuldiesel-architecture` | Arquitetura geral, fronteiras e congelamentos. |
| `winforms-ui` | Telas WinForms, controles e FormsLogic. |
| `bll-dal-dtl` | Camadas BLL, DAL e DTL do host C#. |
| `sdh-contract` | Comandos, validacao e contrato semantico SDH. |
| `sdctp-contract` | Massa CAN RX/TX, mirror e output buffer. |
| `sdgw-transport` | Transporte/gateway SDGW no host e BPM. |
| `j1939-decode` | Decodificacao J1939, PGN, SPN e catalogos. |
| `module-database` | Banco de Modulos, schema, perfis e comandos SDH armazenados. |
| `firmware-uce` | Firmware UCE, SPI, LED, CAN e SDCTP embarcado. |
| `firmware-bpm` | Firmware BPM como SDGW e roteador fisico. |
| `git-checkpoint` | Status, diff, rollback e consolidacao Git autorizada. |
| `build-validation` | Builds, scripts e validacoes reproduziveis. |
| `dump-generation` | Dumps de ETAPA, inventarios e registros de decisoes. |
