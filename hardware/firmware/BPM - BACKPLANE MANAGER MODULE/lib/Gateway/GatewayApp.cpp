#include <Arduino.h>

#include "GatewayApp.h"
#include "SdgwLink.h"
#include "SdgwDefs.h"
#include "GwTlv.h"

namespace {
static const uint8_t MaxEventsPerDrain = 24;
}

GatewayApp* GatewayApp::_self = nullptr;

GatewayApp::GatewayApp(SdgwLink& link, GwRouter& router)
    : _link(link),
      _router(router),
      _gsaIrqLatched(false)
{
}

void GatewayApp::begin()
{
    _self = this;
    _gsaIrqLatched = false;

    pinMode(BPM_GSA_IRQ_PIN, INPUT);
    attachInterrupt(digitalPinToInterrupt(BPM_GSA_IRQ_PIN), onGsaIrqThunk, FALLING);
}

void GatewayApp::onCommand(uint8_t cmd,
                           uint8_t /*flags*/,
                           uint8_t /*seq*/,
                           const uint8_t* data,
                           uint8_t dataLen)
{
    const uint8_t addr = GW_CMD_ADDR(cmd);

    if (addr == GW_ADDR_BPM) {
        handleGatewayLocal(cmd, data, dataLen);
        return;
    }

    if (addr == GW_ADDR_BROADCAST) {
        return;
    }

    uint8_t resp[300];
    size_t respLen = 0;
    uint16_t timeoutMs = (uint16_t)SDGW_GATEWAY_ROUTE_TIMEOUT_MS;

    GwErr r = _router.route(cmd, data, dataLen, resp, sizeof(resp), respLen, timeoutMs);
    if (r != GWERR_OK) {
        sendGatewayErrAsResponse(cmd, r);
        return;
    }

    if (!GwTlv::validatePacket(resp, respLen)) {
        sendGatewayErrAsResponse(cmd, GWERR_BAD_FRAME);
        return;
    }

    _link.sendResponse(cmd, resp, (uint8_t)respLen);
}

void GatewayApp::handleGatewayLocal(uint8_t cmd,
                                    const uint8_t* /*data*/,
                                    uint8_t /*dataLen*/)
{
    const uint8_t opcode = GW_CMD_OP(cmd);

    if (opcode == GW_OP_BPM_PING) {
        const uint8_t ok = 1;
        _link.sendResponse(cmd, &ok, 1);
        return;
    }
}

void GatewayApp::tick()
{
    if (!_gsaIrqLatched && digitalRead(BPM_GSA_IRQ_PIN) != LOW) {
        return;
    }

    drainPendingGsaEvents();
}

void GatewayApp::onGsaIrqThunk()
{
    if (_self != nullptr) {
        _self->onGsaIrq();
    }
}

void GatewayApp::onGsaIrq()
{
    _gsaIrqLatched = true;
}

void GatewayApp::drainPendingGsaEvents()
{
    for (uint8_t index = 0; index < MaxEventsPerDrain; index++) {
        uint8_t eventPacket[32];
        size_t eventLen = 0;
        if (!_router.pollGsaEvent(eventPacket, sizeof(eventPacket), eventLen)) {
            break;
        }

        _link.sendEvent(SDGW_CMD_GSA_TLV, eventPacket, (uint8_t)eventLen);
    }

    _gsaIrqLatched = (digitalRead(BPM_GSA_IRQ_PIN) == LOW);
}

void GatewayApp::sendGatewayErrAsResponse(uint8_t cmd, GwErr err)
{
    uint8_t tlv[96];
    size_t tlvLen = 0;
    if (!_router.buildGatewayErrorPayload(cmd, err, tlv, sizeof(tlv), tlvLen) || tlvLen == 0) {
        tlv[0] = SDGW_TLV_GATEWAY_ERR;
        tlv[1] = 0x01;
        tlv[2] = (uint8_t)err;
        tlvLen = 3;
    }

    _link.sendResponse(cmd, tlv, (uint8_t)tlvLen);
}
