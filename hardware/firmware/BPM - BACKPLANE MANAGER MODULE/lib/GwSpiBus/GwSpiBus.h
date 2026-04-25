#pragma once

#include "GwBus.h"
#include "SdgwDefs.h"
#include <SPI.h>

class GwSpiBus : public IGwBus {
public:
    static constexpr size_t MaxPacketLen = GW_SPI_PACKET_LIMIT;
    static constexpr size_t MaxBurstLen = MaxPacketLen;

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
        uint8_t tx[MaxPacketLen] = {0};
        uint8_t rx[MaxBurstLen] = {0};
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
    static bool buildTxFrame(const uint8_t* tx, size_t txLen, uint8_t* out, size_t outMax, size_t& packetLen);
    static bool waitIrqLevel(int irq, int level, uint32_t deadline);
    static bool transferFrame(SPIClass& spi, const uint8_t* txFrame, uint8_t* rxFrame, size_t len);
    static bool extractPacket(const uint8_t* frame, size_t frameLen, uint8_t* rx, size_t rxMax, size_t& rxLen);
};

using SpiLink = GwSpiBus;
