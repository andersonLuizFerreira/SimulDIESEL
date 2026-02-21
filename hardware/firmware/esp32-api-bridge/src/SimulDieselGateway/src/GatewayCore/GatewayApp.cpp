#include "GatewayApp.h"
#include "src/Sggw/SggwLink.h"
#include "src/Sggw/Sggw.defs.h"
#include "src/GatewayCore/GwTlv.h"

static inline uint8_t hiNib(uint8_t b){ return (uint8_t)(b >> 4); }

void GatewayApp::onCommand(uint8_t cmd,
                           uint8_t /*flags*/,
                           uint8_t /*seq*/,
                           const uint8_t* data,
                           uint8_t dataLen)
{
    const uint8_t addr = hiNib(cmd);

    if (addr == 0x00) {
        handleGatewayLocal(cmd, data, dataLen);
        return;
    }

    if (addr == 0x0F) {
        // broadcast (sem resposta) - opcional implementar depois
        return;
    }

    uint8_t resp[300];
    size_t respLen = 0;
    uint16_t timeoutMs = 30;

    GwErr r = _router.route(cmd, data, dataLen, resp, sizeof(resp), respLen, timeoutMs);
    if (r != GWERR_OK) {
        sendGatewayErrAsEvent(cmd, r);
        return;
    }

    // resp = frame interno: [CMD][LEN][TLV...][CRC8]
    // A API deve receber SOMENTE o TLV (sem CMD/LEN/CRC8).
    uint8_t rcCmd = 0;
    const uint8_t* tlvPtr = nullptr;
    uint8_t tlvLen = 0;

    if (!GwTlv::validateFrame(resp, respLen, rcCmd, tlvPtr, tlvLen) || rcCmd != cmd) {
        sendGatewayErrAsEvent(cmd, GWERR_BAD_FRAME);
        return;
    }

    _link.sendEvent(cmd, tlvPtr, tlvLen);
}

void GatewayApp::handleGatewayLocal(uint8_t cmd,
                                    const uint8_t* /*data*/,
                                    uint8_t /*dataLen*/)
{
    const uint8_t opcode = (uint8_t)(cmd & 0x0F);

    // 0x00 = PING gateway
    if (opcode == 0x00) {
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