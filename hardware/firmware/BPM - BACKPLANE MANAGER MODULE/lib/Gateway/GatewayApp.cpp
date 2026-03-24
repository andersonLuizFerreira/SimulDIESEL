#include <Arduino.h>

#include "GatewayApp.h"
#include "SggwLink.h"
#include "Sggw.defs.h"
#include "GwTlv.h"

namespace {
static const uint32_t GsaBusyPollGuardMs = 20;
static const uint32_t GsaBusyPollIntervalMs = 10;
}

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

    if (addr == GW_ADDR_GSA && _gsaState == GsaRemoteState::Busy) {
        sendGatewayErrAsResponse(cmd, GWERR_BUSY);
        return;
    }

    uint8_t resp[300];
    size_t respLen = 0;
    uint16_t timeoutMs = (uint16_t)SGGW_GATEWAY_ROUTE_TIMEOUT_MS;

    GwErr r = _router.route(cmd, data, dataLen, resp, sizeof(resp), respLen, timeoutMs);
    if (r != GWERR_OK) {
        sendGatewayErrAsResponse(cmd, r);
        return;
    }

    // O gateway apenas valida o pacote interno da baby board e o encaminha.
    if (!GwTlv::validatePacket(resp, respLen)) {
        sendGatewayErrAsResponse(cmd, GWERR_BAD_FRAME);
        return;
    }

    uint8_t eventType = 0;
    if (addr == GW_ADDR_GSA && isGsaBusEvent(resp, respLen, &eventType)) {
        _gsaState = (eventType == GSA_EVENT_BUSY) ? GsaRemoteState::Busy : GsaRemoteState::Idle;
        if (_gsaState == GsaRemoteState::Busy) {
            const uint32_t now = millis();
            _gsaBusySinceMs = now;
            _gsaNextPollAtMs = now + GsaBusyPollGuardMs;
        } else {
            _gsaBusySinceMs = 0;
            _gsaNextPollAtMs = 0;
        }
        _link.sendEvent(cmd, resp, (uint8_t)respLen);
        if (eventType == GSA_EVENT_BUSY) {
            sendGatewayErrAsResponse(cmd, GWERR_BUSY);
        }
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

    // desconhecido: opcional ignorar
}

void GatewayApp::tick()
{
    if (_gsaState != GsaRemoteState::Busy) {
        return;
    }

    const uint32_t now = millis();
    if ((int32_t)(now - _gsaNextPollAtMs) < 0) {
        return;
    }

    _gsaNextPollAtMs = now + GsaBusyPollIntervalMs;

    uint8_t eventPacket[32];
    size_t eventLen = 0;
    if (!_router.pollGsaEvent(eventPacket, sizeof(eventPacket), eventLen)) {
        return;
    }

    uint8_t eventType = 0;
    if (!isGsaBusEvent(eventPacket, eventLen, &eventType)) {
        _link.sendEvent(SGGW_CMD_GSA_TLV, eventPacket, (uint8_t)eventLen);
        return;
    }

    _gsaState = (eventType == GSA_EVENT_BUSY) ? GsaRemoteState::Busy : GsaRemoteState::Idle;
    if (_gsaState == GsaRemoteState::Busy) {
        _gsaBusySinceMs = now;
        _gsaNextPollAtMs = now + GsaBusyPollGuardMs;
    } else {
        _gsaBusySinceMs = 0;
        _gsaNextPollAtMs = 0;
    }
    _link.sendEvent(SGGW_CMD_GSA_TLV, eventPacket, (uint8_t)eventLen);
}

bool GatewayApp::isGsaBusEvent(const uint8_t* payload, size_t payloadLen, uint8_t* eventTypeOut) const
{
    if (!payload || payloadLen < 6) {
        return false;
    }

    if (payload[0] != GSA_CMD_EVENT || payload[1] != 0x03) {
        return false;
    }

    uint8_t eventType = payload[2];
    if (eventType != GSA_EVENT_BUSY && eventType != GSA_EVENT_IDLE) {
        return false;
    }

    if (eventTypeOut) {
        *eventTypeOut = eventType;
    }

    return true;
}

void GatewayApp::sendGatewayErrAsResponse(uint8_t cmd, GwErr err)
{
    uint8_t tlv[3];
    tlv[0] = SGGW_TLV_GATEWAY_ERR;
    tlv[1] = 0x01;
    tlv[2] = (uint8_t)err;

    _link.sendResponse(cmd, tlv, sizeof(tlv));
}
