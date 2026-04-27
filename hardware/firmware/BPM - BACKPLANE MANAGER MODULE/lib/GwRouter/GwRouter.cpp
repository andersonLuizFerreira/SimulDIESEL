#include "GwRouter.h"
#include "GwDeviceTable.h"
#include "GwI2cBus.h"
#include "GwTlv.h"
#include "SdgwDefs.h"

namespace {
constexpr uint8_t kDiagVersion = 0x01;
constexpr uint8_t kDiagLayerGwSpiBus = 0x02;
constexpr uint8_t kDiagPhaseReadPayload = 0x04;
constexpr uint8_t kDiagPhaseFinalCrcValidation = 0x05;
constexpr size_t kDiagHeaderLen = 11;
}

GwErr GwRouter::route(uint8_t cmd,
                      const uint8_t* reqTlv, uint8_t reqTlvLen,
                      uint8_t* respBuf, size_t respMax, size_t& respLen,
                      uint16_t timeoutMs)
{
    respLen = 0;

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr == GW_ADDR_BPM) return GWERR_ADDR_UNMAPPED; // nao roteia BPM local

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;

    // Escolha concreta neste estado saneado:
    // apenas a GSA permanece roteada ativamente.
    IGwBus* bus = nullptr;
    switch (e.bus) {
        case GW_BUS_I2C:
            bus = &_i2c;
            break;
        case GW_BUS_SPI:
            bus = &_spi;
            break;
        default:
            return GWERR_BUS_DOWN;
    }

    if (e.bus == GW_BUS_I2C && !GwTlv::validatePacket(reqTlv, reqTlvLen))
        return GWERR_BAD_FRAME;

    if (!bus->isOk()) {
        // ainda pode tentar, mas aqui sinaliza já
        // (ou você decide tentar mesmo assim)
    }

    size_t rxLen = 0;
    if (!bus->transact(addr, reqTlv, reqTlvLen, respBuf, respMax, rxLen, timeoutMs))
        return GWERR_TIMEOUT;

    if (!GwTlv::validatePacket(respBuf, rxLen))
        return GWERR_BAD_CRC;

    respLen = rxLen;
    return GWERR_OK;
}

bool GwRouter::buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const
{
    outLen = 0;
    if (!out || outMax < 3) return false;

    const bool isUce = GW_CMD_ADDR(cmd) == GW_ADDR_UCE;
    const GwSpiBus::DiagnosticSnapshot& diag = _spi.lastDiagnostic();
    if (isUce && diag.valid &&
        (err == GWERR_TIMEOUT || err == GWERR_BAD_CRC || err == GWERR_BAD_FRAME)) {
        const size_t payloadLen = kDiagHeaderLen + diag.txLen + diag.rxLen;
        if (outMax < payloadLen + 2) {
            return false;
        }

        out[0] = SDGW_TLV_GATEWAY_ERR;
        out[1] = (uint8_t)payloadLen;
        out[2] = (uint8_t)err;
        out[3] = kDiagVersion;
        out[4] = kDiagLayerGwSpiBus;
        out[5] = (diag.phase != 0x00) ? diag.phase : kDiagPhaseReadPayload;
        out[6] = diag.cause;
        out[7] = diag.txLen;
        out[8] = diag.rxLen;
        out[9] = diag.expectedLength;
        out[10] = diag.receivedLength;
        out[11] = diag.crcCalculated;
        out[12] = diag.crcReceived;

        size_t cursor = 13;
        for (size_t index = 0; index < diag.txLen; ++index) {
            out[cursor++] = diag.tx[index];
        }
        for (size_t index = 0; index < diag.rxLen; ++index) {
            out[cursor++] = diag.rx[index];
        }

        outLen = cursor;
        return true;
    }

    out[0] = SDGW_TLV_GATEWAY_ERR;
    out[1] = 0x01;
    out[2] = (uint8_t)err;
    outLen = 3;
    return true;
}

bool GwRouter::pollGsaEvent(uint8_t* respBuf, size_t respMax, size_t& respLen)
{
    respLen = 0;

    GwI2cBus* i2cBus = static_cast<GwI2cBus*>(&_i2c);
    if (!i2cBus) return false;

    if (!i2cBus->pollEvent(GW_ADDR_GSA, respBuf, respMax, respLen))
        return false;

    return GwTlv::validatePacket(respBuf, respLen);
}

bool GwRouter::pollUceEvent(uint8_t* respBuf, size_t respMax, size_t& respLen)
{
    respLen = 0;

    GwSpiBus* spiBus = static_cast<GwSpiBus*>(&_spi);
    if (!spiBus) return false;

    return spiBus->pollEvent(GW_ADDR_UCE, respBuf, respMax, respLen, SDGW_GATEWAY_ROUTE_TIMEOUT_MS);
}
