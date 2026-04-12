#pragma once
#include <stdint.h>
#include <stddef.h>

#include "GwBus.h"
#include "GwErr.h"
#include "GwSpiDiagnostic.h"

class GwRouter {
public:
    GwRouter(IGwBus& i2c, IGwBus& spi)
    : _i2c(i2c), _spi(spi), _lastDiag() {}

    // Roteia uma requisicao para a baby board externa indicada por
    // GW_CMD_ADDR(cmd). A BPM local (ADDR 0) nao passa por aqui.
    // Entrada e saida sao os payloads internos da baby board (TLV+CRC).
    GwErr route(uint8_t cmd,
                const uint8_t* reqTlv, uint8_t reqTlvLen,
                uint8_t* respBuf, size_t respMax, size_t& respLen,
                uint16_t timeoutMs);

    bool pollGsaEvent(uint8_t* respBuf, size_t respMax, size_t& respLen);
    bool buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const;

private:
    void resetLastDiag();
    void captureSpiDiag(uint8_t addr, GwErr err, const GwSpiDiagnostic::Snapshot& snapshot);
    void finalizeSpiCrcDiag(uint8_t addr, GwErr err, const uint8_t* respBuf, size_t respLen, bool spiUseIrq);
    uint8_t detectPossibleCause(const GwSpiDiagnostic::Snapshot& snapshot, bool spiUseIrq) const;

    IGwBus& _i2c;
    IGwBus& _spi;
    GwSpiDiagnostic::Snapshot _lastDiag;
};
