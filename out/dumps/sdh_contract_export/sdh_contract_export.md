# SDH Contract Export

Catalogo documental dos comandos aceitos pelo SdhValidator nesta etapa.

| Target | Op | Board | Service | Required Args | Optional Args | Accepted Values | Observacoes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| BPM.gateway | ping | BPM | gateway |  |  |  | Gateway ping sem argumentos. |
| GSA.led | set | GSA | led | state |  | state=on,off | Controle do LED da GSA. |
| GSA.channel.setpoint | set | GSA | channel | channel, value |  | channel=1..16; value=0..255 | Define setpoint de canal GSA. |
| GSA.channel.enable | set | GSA | channel | channel, state |  | channel=1..16; state=on,off | Habilita ou desabilita canal GSA. |
| GSA.channels.enable | set | GSA | channels | state |  | state=on,off | Habilita ou desabilita canais GSA em conjunto. |
| GSA.channel.status | get | GSA | channel | channel |  | channel=1..16 | Le status de canal GSA. |
| GSA.channels.status | get | GSA | channels |  |  |  | Le status consolidado dos canais GSA. |
| GSA.channel.fault | reset | GSA | channel | channel |  | channel=1..16 | Reseta fault de canal GSA. |
| GSA.channel.offset | set | GSA | channel | channel, kind, value |  | channel=1..16; value=-32768..32767; kind=vout,vread,iread | Define offset de canal GSA. |
| GSA.channel.offset | get | GSA | channel | channel, kind |  | channel=1..16; kind=vout,vread,iread | Le offset de canal GSA. |
| GSA.channel.offset | save | GSA | channel | channel |  | channel=1..16 | Salva offset de canal GSA. |
| GSA.channel.offset | reset | GSA | channel | channel |  | channel=1..16 | Reseta offset de canal GSA. |
| GSA.offset | reset | GSA | offset |  |  |  | Reseta offsets GSA. |
| UCE.led | set | UCE | led | state |  | state=on,off | Controle do LED da UCE. |
| UCE.can.config | set | UCE | can | controller, bitrate, mode | rxMode | bitrate=5,10,25,50,125,250,500,800,1000; controller=can0,can1; mode=normal,listen,loopback; rxMode=auto,directOnly | rxMode e opcional para compatibilidade com o contrato antigo. |
| UCE.can.enable | set | UCE | can | controller, state |  | state=on,off; controller=can0,can1 | Habilita ou desabilita controlador CAN UCE. |
| UCE.can.status | get | UCE | can | controller |  | controller=can0,can1 | Le status do controlador CAN UCE. |
| UCE.can.rx | poll | UCE | can | controller |  | controller=can0,can1 | Poll de recepcao CAN. |
| UCE.can.rx | readAll | UCE | can | controller |  | controller=can0,can1 | Contrato semantico exige controller; mapper atual envia TLV CAN_READ_ALL sem value payload. |
| UCE.can.driverLog | poll | UCE | can | controller |  | controller=can0,can1 | Poll do log do driver CAN. |
| UCE.can.tx | send | UCE | can | controller, extended, id, dlc, period, d0, d1, d2, d3, d4, d5, d6, d7 |  | extended=0/1; data=d0..d7 em 0..255; id=STD <= 0x7FF, EXT <= 0x1FFFFFFF; dlc=0..8; controller=can0,can1; period=0..65535 | Envio CAN TX periodico/normal conforme mapper. |
| UCE.can.tx | direct | UCE | can | controller, extended, rtr, id, dlc, d0, d1, d2, d3, d4, d5, d6, d7 |  | extended=0/1; data=d0..d7 em 0..255; id=STD <= 0x7FF, EXT <= 0x1FFFFFFF; dlc=0..8; controller=can0,can1; rtr=0/1 | Envio CAN direto. |
| UCE.can.tx | create | UCE | can | controller, index, extended, rtr, id, dlc, d0, d1, d2, d3, d4, d5, d6, d7, period, enabled |  | controller=can0,can1; extended=0/1; enabled=0/1; id=STD <= 0x7FF, EXT <= 0x1FFFFFFF; data=d0..d7 em 0..255; rtr=0/1; period=0..65535; index=0..99; dlc=0..8 | Cria entrada no espelho TX CAN. |
| UCE.can.tx | edit | UCE | can | controller, index, mask | flags, id, dlc, dataMask, d0, d1, d2, d3, d4, d5, d6, d7, period, enabled | dataMask=1..255 quando dados forem editados; mask=0..63; index=0..99; dlc=0..8; flags=0..3; enabled=0/1; controller=can0,can1; period=0..65535 | Edita campos indicados pela mask CAN_TX_EDIT. |
| UCE.can.tx | delete | UCE | can | controller, index, reason |  | reason=1..4; controller=can0,can1; index=0..99 | Remove entrada TX CAN. |
| UCE.can.tx | stop | UCE | can | controller, slot |  | slot=0,255; controller=can0,can1 | Para transmissao TX por slot. |
| UCE.can | reset | UCE | can | controller |  | controller=can0,can1 | Reset do controlador CAN UCE. |
