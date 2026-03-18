#include "GatewayApp.h"
#include "SggwLink.h"
#include "Sggw.defs.h"
#include "GwTlv.h"

void GatewayApp::onCommand(uint8_t cmd,
                           uint8_t /*flags*/,
                           uint8_t /*seq*/,
                           const uint8_t* data,
                           uint8_t dataLen)
{
    // O host resolve SDH para o formato compacto [ADDR:4][OP:4].
    // A BPM apenas trata ADDR local ou roteia o restante para a baby board.
    const uint8_t addr = GW_CMD_ADDR(cmd);

    if (addr == GW_ADDR_BPM) {
        handleGatewayLocal(cmd, data, dataLen);
        return;
    }

    if (addr == GW_ADDR_BROADCAST) {
        // broadcast (sem resposta) - opcional implementar depois
        return;
    }

    uint8_t resp[300];
    size_t respLen = 0;
    uint16_t timeoutMs = (uint16_t)SGGW_GATEWAY_ROUTE_TIMEOUT_MS;

    GwErr r = _router.route(cmd, data, dataLen, resp, sizeof(resp), respLen, timeoutMs);
    if (r != GWERR_OK) {
        sendGatewayErrAsEvent(cmd, r);
        return;
    }

    // O gateway apenas valida o pacote interno da baby board e o encaminha.
    if (!GwTlv::validatePacket(resp, respLen)) {
        sendGatewayErrAsEvent(cmd, GWERR_BAD_FRAME);
        return;
    }

    _link.sendEvent(cmd, resp, (uint8_t)respLen);
}

void GatewayApp::handleGatewayLocal(uint8_t cmd,
                                    const uint8_t* /*data*/,
                                    uint8_t /*dataLen*/)
{
    const uint8_t opcode = GW_CMD_OP(cmd);

    if (opcode == GW_OP_BPM_PING) {
        const uint8_t ok = 1;
        _link.sendEvent(cmd, &ok, 1);
        return;
    }

    // desconhecido: opcional ignorar
}

void GatewayApp::sendGatewayErrAsEvent(uint8_t cmd, GwErr err)
{
    // Payload SGGW = SOMENTE TLV (inclusive para erros)
    // TLV: T=0xFE, L=1, V=err
    uint8_t tlv[3];
    tlv[0] = 0xFE;
    tlv[1] = 0x01;
    tlv[2] = (uint8_t)err;

    _link.sendEvent(cmd, tlv, sizeof(tlv));
}
