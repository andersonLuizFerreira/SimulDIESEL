#include "GwRouter.h"
#include "GwDeviceTable.h"
#include "GwSpiBus.h"
#include "GwI2cBus.h"
#include "GwTlv.h"
#include "SdgwDefs.h"

void GwRouter::resetLastDiag()
{
    _lastDiag = GwSpiDiagnostic::Snapshot{};
}

void GwRouter::captureSpiDiag(uint8_t addr, GwErr err, const GwSpiDiagnostic::Snapshot& snapshot)
{
    _lastDiag = snapshot;
    _lastDiag.valid = snapshot.valid;
    _lastDiag.addr = addr;
    _lastDiag.status = (uint8_t)err;
}

GwErr GwRouter::mapSpiTransactFailure(const GwSpiBus& spiBus, bool spiUseIrq) const
{
    switch (spiBus.lastError()) {
        case GwSpiBus::TransactError::TimeoutWaitingIrq:
            return GWERR_TIMEOUT;
        case GwSpiBus::TransactError::HeaderInvalid:
            return GWERR_HEADER_INVALID;
        case GwSpiBus::TransactError::LengthInvalid:
            return GWERR_LENGTH_INVALID;
        case GwSpiBus::TransactError::FrameIncomplete:
            return GWERR_FRAME_INCOMPLETE;
        default:
            break;
    }

    const GwSpiDiagnostic::Snapshot& snapshot = spiBus.lastSnapshot();
    const uint8_t cause = detectPossibleCause(snapshot, spiUseIrq);
    switch (cause) {
        case GwSpiDiagnostic::CauseIncompleteFrame:
            return GWERR_FRAME_INCOMPLETE;
        case GwSpiDiagnostic::CauseLengthMismatch:
            return GWERR_LENGTH_INVALID;
        case GwSpiDiagnostic::CauseFirstByteMisaligned:
            return GWERR_HEADER_INVALID;
        default:
            return GWERR_TIMEOUT;
    }
}

uint8_t GwRouter::detectPossibleCause(const GwSpiDiagnostic::Snapshot& snapshot, bool spiUseIrq) const
{
    if (snapshot.phase == GwSpiDiagnostic::PhaseWaitResponseReady) {
        return spiUseIrq
            ? GwSpiDiagnostic::CauseTimeoutWaitingIrq
            : GwSpiDiagnostic::CauseEarlyReadBeforeResponseReady;
    }

    if (snapshot.expectedLen > 0 && snapshot.receivedLen < snapshot.expectedLen) {
        return GwSpiDiagnostic::CauseIncompleteFrame;
    }

    if (snapshot.expectedLen != 0 && snapshot.receivedLen != 0 && snapshot.expectedLen != snapshot.receivedLen) {
        return GwSpiDiagnostic::CauseLengthMismatch;
    }

    if (snapshot.receivedLen >= 2 && snapshot.rx[0] == 0x00 && snapshot.rx[1] != 0x00) {
        return GwSpiDiagnostic::CauseFirstByteMisaligned;
    }

    if (!spiUseIrq) {
        return GwSpiDiagnostic::CauseEarlyReadBeforeResponseReady;
    }

    return GwSpiDiagnostic::CausePreloadFailure;
}

void GwRouter::finalizeSpiCrcDiag(uint8_t addr, GwErr err, const uint8_t* respBuf, size_t respLen, bool spiUseIrq)
{
    GwSpiDiagnostic::Snapshot snapshot = _lastDiag;
    snapshot.valid = true;
    snapshot.addr = addr;
    snapshot.layer = GwSpiDiagnostic::LayerCrcValidation;
    snapshot.phase = GwSpiDiagnostic::PhaseFinalCrcValidation;
    snapshot.status = (uint8_t)err;
    snapshot.receivedLen = (uint8_t)respLen;
    snapshot.rxLen = (uint8_t)((respLen > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : respLen);

    for (size_t index = 0; index < snapshot.rxLen; index++) {
        snapshot.rx[index] = respBuf[index];
    }

    if (respLen >= 2) {
        snapshot.expectedLen = (uint8_t)(2 + respBuf[1] + 1);
    }
    if (respLen >= 1) {
        snapshot.crcReceived = respBuf[respLen - 1];
    }
    if (respLen >= 3) {
        snapshot.crcCalculated = GwTlv::crc8(respBuf, respLen - 1);
    }
    snapshot.cause = detectPossibleCause(snapshot, spiUseIrq);
    _lastDiag = snapshot;
}

GwErr GwRouter::route(uint8_t cmd,
                      const uint8_t* reqTlv, uint8_t reqTlvLen,
                      uint8_t* respBuf, size_t respMax, size_t& respLen,
                      uint16_t timeoutMs)
{
    respLen = 0;
    resetLastDiag();

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr == GW_ADDR_BPM) return GWERR_ADDR_UNMAPPED; // nao roteia BPM local

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return GWERR_ADDR_UNMAPPED;

    // O host resolve o contrato interno da board; o gateway apenas roteia.
    if (!GwTlv::validatePacket(reqTlv, reqTlvLen))
        return GWERR_BAD_FRAME;

    // Escolha concreta hoje:
    // GSA (0x1) -> I2C
    // UCE (0x2) -> SPI
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

    if (!bus->isOk()) {
        // ainda pode tentar, mas aqui sinaliza já
        // (ou você decide tentar mesmo assim)
    }

    size_t rxLen = 0;
    if (!bus->transact(addr, reqTlv, reqTlvLen, respBuf, respMax, rxLen, timeoutMs)) {
        if (e.bus == GW_BUS_SPI) {
            GwSpiBus* spiBus = static_cast<GwSpiBus*>(&_spi);
            const GwErr mappedErr = mapSpiTransactFailure(*spiBus, e.spiUseIrq);
            captureSpiDiag(addr, mappedErr, spiBus->lastSnapshot());
            if (_lastDiag.cause == GwSpiDiagnostic::CauseUnknown) {
                _lastDiag.cause = detectPossibleCause(_lastDiag, e.spiUseIrq);
            }
            return mappedErr;
        }
        return GWERR_TIMEOUT;
    }

    if (!GwTlv::validatePacket(respBuf, rxLen)) {
        if (e.bus == GW_BUS_SPI) {
            GwSpiBus* spiBus = static_cast<GwSpiBus*>(&_spi);
            captureSpiDiag(addr, GWERR_BAD_CRC, spiBus->lastSnapshot());
            finalizeSpiCrcDiag(addr, GWERR_BAD_CRC, respBuf, rxLen, e.spiUseIrq);
        }
        return GWERR_BAD_CRC;
    }

    respLen = rxLen;
    return GWERR_OK;
}

bool GwRouter::buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const
{
    outLen = 0;
    if (!out || outMax < 3) return false;

    out[0] = SDGW_TLV_GATEWAY_ERR;
    out[1] = 0x01;
    out[2] = (uint8_t)err;
    outLen = 3;

    const uint8_t addr = GW_CMD_ADDR(cmd);
    if (addr != GW_ADDR_UCE || !_lastDiag.valid) {
        return true;
    }

    const size_t valueLen = (size_t)11 + (size_t)_lastDiag.txLen + (size_t)_lastDiag.rxLen;
    const size_t totalLen = (size_t)2 + valueLen;
    if (totalLen > outMax || valueLen > 0xFF) {
        return true;
    }

    out[0] = SDGW_TLV_GATEWAY_ERR;
    out[1] = (uint8_t)valueLen;
    out[2] = (uint8_t)err;
    out[3] = GwSpiDiagnostic::kVersion;
    out[4] = _lastDiag.layer;
    out[5] = _lastDiag.phase;
    out[6] = _lastDiag.cause;
    out[7] = _lastDiag.txLen;
    out[8] = _lastDiag.rxLen;
    out[9] = _lastDiag.expectedLen;
    out[10] = _lastDiag.receivedLen;
    out[11] = _lastDiag.crcCalculated;
    out[12] = _lastDiag.crcReceived;

    size_t cursor = 13;
    for (uint8_t index = 0; index < _lastDiag.txLen; index++) {
        out[cursor++] = _lastDiag.tx[index];
    }
    for (uint8_t index = 0; index < _lastDiag.rxLen; index++) {
        out[cursor++] = _lastDiag.rx[index];
    }

    outLen = cursor;
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
