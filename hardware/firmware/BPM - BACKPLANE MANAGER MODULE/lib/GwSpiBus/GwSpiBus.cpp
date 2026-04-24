#include <Arduino.h>
#include <string.h>

#include "GwSpiBus.h"
#include "GwDeviceTable.h"
#include "GwTlv.h"
#include "SdgwDefs.h"

namespace {
constexpr uint8_t kDiagPhaseWrite = 0x01;
constexpr uint8_t kDiagPhaseWaitResponseReady = 0x02;
constexpr uint8_t kDiagPhaseReadPayload = 0x04;
constexpr uint8_t kDiagPhaseFinalCrcValidation = 0x05;
constexpr uint8_t kDiagCauseFirstByteMisaligned = 0x01;
constexpr uint8_t kDiagCausePreloadFailure = 0x02;
constexpr uint8_t kDiagCauseTimeoutWaitingIrq = 0x06;
constexpr uint8_t kDiagCauseIncompleteFrame = 0x07;
}

void GwSpiBus::begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin)
{
    _hz = hz;
    _sckPin = sckPin;
    _misoPin = misoPin;
    _mosiPin = mosiPin;
    _spi.begin(_sckPin, _misoPin, _mosiPin, -1);
}

bool GwSpiBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;
    memset(&_lastDiagnostic, 0, sizeof(_lastDiagnostic));
    _lastDiagnostic.expectedLength = GwSpiBus::FixedLedResponseLen;
    _lastDiagnostic.responseStart = 0xFF;

    if (!tx || !rx || rxMax < 4 || !GwTlv::validatePacket(tx, txLen)) {
        return false;
    }

    _lastDiagnostic.txLen = (uint8_t)((txLen < GwSpiBus::FixedLedRequestLen) ? txLen : GwSpiBus::FixedLedRequestLen);
    memcpy(_lastDiagnostic.tx, tx, _lastDiagnostic.txLen);

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e) || e.bus != GW_BUS_SPI || e.spiCsPin < 0) {
        return false;
    }

    if (!isFixedUceLedRequest(addr, tx, txLen)) {
        (void)timeoutMs;
        return false;
    }

    const int cs = e.spiCsPin;
    const int irq = e.spiIrqPin;
    pinMode(cs, OUTPUT);
    csHigh(cs);
    if (irq >= 0) {
        pinMode(irq, INPUT_PULLUP);
    }

    SPISettings st(_hz, SPI_MSBFIRST, SPI_MODE0);
    uint8_t burstRx[GwSpiBus::FixedLedBurstLen] = {0};

    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t index = 0; index < GwSpiBus::FixedLedRequestLen; ++index) {
        burstRx[index] = _spi.transfer(tx[index]);
    }

    const uint32_t deadline = millis() + timeoutMs;
    while (irq >= 0 && digitalRead(irq) != LOW) {
        if ((int32_t)(millis() - deadline) >= 0) {
            csHigh(cs);
            _spi.endTransaction();
            memcpy(_lastDiagnostic.rx, burstRx, sizeof(burstRx));
            _lastDiagnostic.rxLen = GwSpiBus::FixedLedBurstLen;
            _lastDiagnostic.valid = true;
            _lastDiagnostic.phase = kDiagPhaseWaitResponseReady;
            _lastDiagnostic.cause = kDiagCauseTimeoutWaitingIrq;
            _lastDiagnostic.receivedLength = 0;
            return false;
        }
        delayMicroseconds(10);
    }

    for (size_t index = GwSpiBus::FixedLedRequestLen; index < GwSpiBus::FixedLedBurstLen; ++index) {
        burstRx[index] = _spi.transfer(0x00);
    }
    csHigh(cs);
    _spi.endTransaction();

    memcpy(_lastDiagnostic.rx, burstRx, sizeof(burstRx));
    _lastDiagnostic.rxLen = GwSpiBus::FixedLedBurstLen;

    size_t responseStart = GwSpiBus::FixedLedRequestLen;
    for (size_t index = GwSpiBus::FixedLedRequestLen; index < GwSpiBus::FixedLedBurstLen; ++index) {
        if (burstRx[index] != 0x00) {
            responseStart = index;
            break;
        }
    }

    if (responseStart < GwSpiBus::FixedLedBurstLen) {
        _lastDiagnostic.responseStart = (uint8_t)responseStart;
    }

    if ((responseStart + GwSpiBus::FixedLedResponseLen) > GwSpiBus::FixedLedBurstLen) {
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseReadPayload;
        _lastDiagnostic.cause = kDiagCauseIncompleteFrame;
        _lastDiagnostic.receivedLength = 0;
        (void)timeoutMs;
        return false;
    }

    for (size_t index = 0; index < GwSpiBus::FixedLedResponseLen; ++index) {
        rx[index] = burstRx[responseStart + index];
    }
    rxLen = GwSpiBus::FixedLedResponseLen;
    _lastDiagnostic.receivedLength = (uint8_t)rxLen;
    _lastDiagnostic.crcReceived = rx[rxLen - 1];
    _lastDiagnostic.crcCalculated = GwTlv::crc8(rx, rxLen - 1);
    (void)timeoutMs;

    const bool packetOk = GwTlv::validatePacket(rx, rxLen);
    if (!packetOk) {
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseFinalCrcValidation;
        _lastDiagnostic.cause = (responseStart > GwSpiBus::FixedLedRequestLen)
            ? kDiagCauseFirstByteMisaligned
            : kDiagCausePreloadFailure;
    }

    return packetOk;
}

void GwSpiBus::csLow(int cs)
{
    digitalWrite(cs, LOW);
}

void GwSpiBus::csHigh(int cs)
{
    digitalWrite(cs, HIGH);
}

bool GwSpiBus::isFixedUceLedRequest(uint8_t addr, const uint8_t* tx, size_t txLen)
{
    if (addr != GW_ADDR_UCE || !tx || txLen != 4) {
        return false;
    }

    return tx[0] == UCE_CMD_SET_LED &&
           tx[1] == 0x01 &&
           (tx[2] == 0x00 || tx[2] == 0x01);
}
