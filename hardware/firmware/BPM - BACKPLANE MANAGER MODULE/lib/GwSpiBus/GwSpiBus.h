#pragma once

#include "GwBus.h"
#include <SPI.h>

class GwSpiBus : public IGwBus {
public:
    static constexpr size_t FixedLedRequestLen = 4;
    static constexpr size_t FixedLedResponseLen = 4;
    static constexpr size_t FixedLedBurstLen = FixedLedRequestLen + FixedLedResponseLen;

    struct DiagnosticSnapshot {
        bool valid = false;
        uint8_t phase = 0x00;
        uint8_t cause = 0x00;
        uint8_t txLen = 0x00;
        uint8_t rxLen = 0x00;
        uint8_t expectedLength = 0x00;
        uint8_t receivedLength = 0x00;
        uint8_t crcCalculated = 0x00;
        uint8_t crcReceived = 0x00;
        uint8_t responseStart = 0xFF;
        uint8_t tx[FixedLedRequestLen] = {0};
        uint8_t rx[FixedLedBurstLen] = {0};
    };

    explicit GwSpiBus(SPIClass& spi = SPI)
        : _spi(spi), _hz(1000000UL), _sckPin(-1), _misoPin(-1), _mosiPin(-1) {}

    void begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin);

    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override;

    bool isOk() const override { return true; }
    const DiagnosticSnapshot& lastDiagnostic() const { return _lastDiagnostic; }

private:
    SPIClass& _spi;
    uint32_t _hz;
    int8_t _sckPin;
    int8_t _misoPin;
    int8_t _mosiPin;
    DiagnosticSnapshot _lastDiagnostic;

    static void csLow(int cs);
    static void csHigh(int cs);
    static bool isFixedUceLedRequest(uint8_t addr, const uint8_t* tx, size_t txLen);
};
