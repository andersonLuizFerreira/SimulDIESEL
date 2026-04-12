#pragma once
#include "GwBus.h"
#include "GwSpiDiagnostic.h"
#include <SPI.h>

class GwSpiBus : public IGwBus {
public:
    explicit GwSpiBus(SPIClass& spi = SPI)
        : _spi(spi), _ok(true), _hz(8000000UL), _sckPin(-1), _misoPin(-1), _mosiPin(-1) {}

    void begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin);

    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override;

    bool isOk() const override { return _ok; }
    const GwSpiDiagnostic::Snapshot& lastSnapshot() const { return _lastSnapshot; }

private:
    SPIClass& _spi;
    bool _ok;
    uint32_t _hz;
    int8_t _sckPin;
    int8_t _misoPin;
    int8_t _mosiPin;
    GwSpiDiagnostic::Snapshot _lastSnapshot;

    void csLow(int cs);
    void csHigh(int cs);
    void resetSnapshot(uint8_t addr);
    void captureBytes(uint8_t* dest, const uint8_t* src, size_t len, uint8_t& lenOut);
};
