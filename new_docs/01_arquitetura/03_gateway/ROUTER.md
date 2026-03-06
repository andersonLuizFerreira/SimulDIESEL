# GwRouter --- Roteamento

## Função

Responsável por interpretar o campo `CMD` recebido via SGGW e encaminhar
a mensagem ao destino correto.

CMD = \[ADDR:4\]\[OP:4\]

## Fluxo

1.  Extrair ADDR e OP
2.  Consultar DeviceTable
3.  Determinar barramento
4.  Montar mensagem TLV
5.  Enviar ao dispositivo

## Exemplo

CMD = 0x11

ADDR = 0x1\
OP = 0x1

Destino: GSA
