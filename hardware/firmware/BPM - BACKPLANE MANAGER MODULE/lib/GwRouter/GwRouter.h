#pragma once
#include <stdint.h>
#include <stddef.h>

#include "GwBus.h"
#include "GwErr.h"

class GwRouter {
public:
    GwRouter(IGwBus& i2c, IGwBus& spi)
    : _i2c(i2c), _spi(spi) {}

    // Roteia uma requisicao para a baby board externa indicada por
    // GW_CMD_ADDR(cmd). A BPM local (ADDR 0) nao passa por aqui.
    // Entrada e saida sao os payloads internos da baby board (TLV+CRC).
    GwErr route(uint8_t cmd,
                const uint8_t* reqTlv, uint8_t reqTlvLen,
                uint8_t* respBuf, size_t respMax, size_t& respLen,
                uint16_t timeoutMs);

private:
    IGwBus& _i2c;
    IGwBus& _spi;
};
