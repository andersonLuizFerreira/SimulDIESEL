# Validacao J1939 Diagnostics J1939-73

- Resultado: `OK`
- Casos: DM1 sem DTC, DM1/DM2 com DTC, DM1 remontado, FMI, SPN conhecido/desconhecido e Request PGN 59904.

```text
DM1.NoFault.Status=OK
DM1.NoFault.Count=OK
DM1.GrandfatheredNoFault.Status=OK
DM1.GrandfatheredNoFault.Count=OK
DM1.OneDtc.Count=OK
DM1.OneDtc.SPN=OK
DM1.OneDtc.FMI=OK
DM1.OneDtc.OC=OK
DM1.OneDtc.CM=OK
DM2.OneDtc.Type=OK
DM2.OneDtc.SPN=OK
DM2.OneDtc.FMI=OK
DM2.OneDtc.OC=OK
DM1.Reassembled.Decoded=OK
DM1.Reassembled.Count=OK
DM1.Reassembled.Flag=OK
FMI.3.Description=OK
SPN.Known.190=OK
SPN.Unknown=OK
Request.DM1.Payload=OK
Request.DM2.Payload=OK
Request.PGN59904=OK
failures=0
```
