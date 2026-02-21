#pragma once
#include "GwBus.h"
#include <SPI.h>

class GwSpiBus : public IGwBus {
public:
    explicit GwSpiBus(SPIClass& spi = SPI) : _spi(spi), _ok(true) {}

    void begin(uint32_t hz = 8000000) { // ajuste
        _spi.begin();
        _hz = hz;
    }

    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override;

    bool isOk() const override { return _ok; }

private:
    SPIClass& _spi;
    bool _ok;
    uint32_t _hz;

    void csLow(int cs);
    void csHigh(int cs);
};