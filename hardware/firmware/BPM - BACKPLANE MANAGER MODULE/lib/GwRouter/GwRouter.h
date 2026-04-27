#pragma once
#include <stdint.h>
#include <stddef.h>

#include "GwBus.h"
#include "GwErr.h"
#include "GwI2cBus.h"
#include "GwSpiBus.h"

class GwRouter {
public:
    GwRouter(GwI2cBus& i2c, GwSpiBus& spi)
    : _i2c(i2c), _spi(spi) {}

    // Roteia uma requisicao para a baby board externa indicada por
    // GW_CMD_ADDR(cmd). A BPM local (ADDR 0) nao passa por aqui.
    // Entrada e saida sao os payloads internos da baby board (TLV+CRC).
    GwErr route(uint8_t cmd,
                const uint8_t* reqTlv, uint8_t reqTlvLen,
                uint8_t* respBuf, size_t respMax, size_t& respLen,
                uint16_t timeoutMs);

    bool pollGsaEvent(uint8_t* respBuf, size_t respMax, size_t& respLen);
    bool pollUceEvent(uint8_t* respBuf, size_t respMax, size_t& respLen);
    bool buildGatewayErrorPayload(uint8_t cmd, GwErr err, uint8_t* out, size_t outMax, size_t& outLen) const;

private:
    GwI2cBus& _i2c;
    GwSpiBus& _spi;
};
