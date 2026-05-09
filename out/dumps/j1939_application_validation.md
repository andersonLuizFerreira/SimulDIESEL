# Validacao J1939 Application Layer

- Resultado: `OK`
- Casos: EEC1/SPN190, ET1/SPN110, EFL/P1/SPN100, PGN nao cadastrado, valores 0xFF/0xFE e mensagem remontada.

```text
EEC1.Decoded=OK
EEC1.SPN190.Status=OK
EEC1.SPN190.Value=OK
ET1.SPN110.Status=OK
ET1.SPN110.Value=OK
EFLP1.SPN100.Status=OK
EFLP1.SPN100.Value=OK
Unsupported.Status=OK
Unsupported.NotDecoded=OK
Special.0xFF=OK
Special.0xFE=OK
Reassembled.PGN=OK
Reassembled.Source=OK
Reassembled.SPN190=OK
failures=0
```
