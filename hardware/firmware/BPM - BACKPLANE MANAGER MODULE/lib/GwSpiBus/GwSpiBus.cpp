#include <Arduino.h>
#include <string.h>

#include "GwSpiBus.h"
#include "GwDeviceTable.h"
#include "SdgwCobs.h"
#include "GwTlv.h"
#include "SdgwDefs.h"

namespace {
constexpr uint8_t kDiagPhaseWrite = 0x01;
constexpr uint8_t kDiagPhaseWaitResponseReady = 0x02;
constexpr uint8_t kDiagPhaseReadHeader = 0x03;
constexpr uint8_t kDiagPhaseReadPayload = 0x04;
constexpr uint8_t kDiagPhaseFinalCrcValidation = 0x05;
constexpr uint8_t kDiagCauseFirstByteMisaligned = 0x01;
constexpr uint8_t kDiagCausePreloadFailure = 0x02;
constexpr uint8_t kDiagCauseTimeoutWaitingIrq = 0x06;
constexpr uint8_t kDiagCauseIncompleteFrame = 0x07;
constexpr uint8_t kDiagCausePacketTooLarge = 0x08;
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
    _lastDiagnostic.responseStart = 0xFF;

    uint8_t txFrame[GwSpiBus::MaxBurstLen] = {0};
    size_t txPacketLen = 0;
    if (!tx || !rx || rxMax < 3 || !buildTxFrame(tx, txLen, txFrame, sizeof(txFrame), txPacketLen)) {
        return false;
    }

    _lastDiagnostic.txLen = (uint8_t)txPacketLen;
    memcpy(_lastDiagnostic.tx, txFrame, _lastDiagnostic.txLen);

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e) || e.bus != GW_BUS_SPI || e.spiCsPin < 0) {
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
    uint8_t firstRx[GwSpiBus::MaxBurstLen] = {0};
    uint8_t readTx[GwSpiBus::MaxBurstLen] = {0};
    uint8_t responseFrame[GwSpiBus::MaxBurstLen] = {0};

    _spi.beginTransaction(st);
    csLow(cs);
    const uint32_t deadline = millis() + timeoutMs;
    if (!waitIrqLevel(irq, LOW, deadline)) {
        csHigh(cs);
        _spi.endTransaction();
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseWaitResponseReady;
        _lastDiagnostic.cause = kDiagCauseTimeoutWaitingIrq;
        _lastDiagnostic.receivedLength = 0;
        return false;
    }
    transferFrame(_spi, txFrame, firstRx, GwSpiBus::MaxBurstLen);
    csHigh(cs);
    _spi.endTransaction();

    const uint32_t attentionDeadline = millis() + timeoutMs;
    if (!waitIrqLevel(irq, LOW, attentionDeadline)) {
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseWaitResponseReady;
        _lastDiagnostic.cause = kDiagCauseTimeoutWaitingIrq;
        _lastDiagnostic.receivedLength = 0;
        return false;
    }
    (void)waitIrqLevel(irq, HIGH, millis() + timeoutMs);

    _spi.beginTransaction(st);
    csLow(cs);
    if (!waitIrqLevel(irq, LOW, millis() + timeoutMs)) {
        csHigh(cs);
        _spi.endTransaction();
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseWaitResponseReady;
        _lastDiagnostic.cause = kDiagCauseTimeoutWaitingIrq;
        _lastDiagnostic.receivedLength = 0;
        return false;
    }
    transferFrame(_spi, readTx, responseFrame, GwSpiBus::MaxBurstLen);
    csHigh(cs);
    _spi.endTransaction();

    memcpy(_lastDiagnostic.rx, responseFrame, GwSpiBus::MaxBurstLen);
    _lastDiagnostic.rxLen = GwSpiBus::MaxBurstLen;
    _lastDiagnostic.responseStart = 0;

    const bool responseOk = extractPacket(responseFrame, GwSpiBus::MaxBurstLen, rx, rxMax, rxLen);
    _lastDiagnostic.expectedLength = (rxLen >= 2) ? (uint8_t)(2U + rx[1] + 1U) : 0;
    _lastDiagnostic.receivedLength = (uint8_t)rxLen;

    if (!responseOk) {
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = (rxLen < 2) ? kDiagPhaseReadHeader : kDiagPhaseReadPayload;
        _lastDiagnostic.cause = (_lastDiagnostic.expectedLength > GwSpiBus::MaxPacketLen)
            ? kDiagCausePacketTooLarge
            : kDiagCauseIncompleteFrame;
        return false;
    }

    _lastDiagnostic.crcReceived = rx[rxLen - 1];
    _lastDiagnostic.crcCalculated = GwTlv::crc8(rx, rxLen - 1);

    const bool packetOk = GwTlv::validatePacket(rx, rxLen);
    if (!packetOk) {
        _lastDiagnostic.valid = true;
        _lastDiagnostic.phase = kDiagPhaseFinalCrcValidation;
        _lastDiagnostic.cause = (rxLen > 0 && rx[0] == 0x00)
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

bool GwSpiBus::buildTxFrame(const uint8_t* tx, size_t txLen, uint8_t* out, size_t outMax, size_t& packetLen)
{
    packetLen = 0;
    if (!tx || !out || txLen < 2 || outMax != GwSpiBus::MaxBurstLen) {
        return false;
    }

    memset(out, 0, outMax);
    uint8_t raw[GwSpiBus::MaxPacketLen] = {0};
    size_t rawLen = 0;
    if (GwTlv::validatePacket(tx, txLen)) {
        if (txLen > sizeof(raw)) {
            return false;
        }
        memcpy(raw, tx, txLen);
        rawLen = txLen;
    } else {
        const size_t tlvLen = (size_t)2U + tx[1];
        if (txLen != tlvLen || tlvLen + 1U > sizeof(raw)) {
            return false;
        }

        memcpy(raw, tx, tlvLen);
        raw[tlvLen] = GwTlv::crc8(raw, tlvLen);
        rawLen = tlvLen + 1U;
    }

    size_t encodedLen = 0;
    if (!SdgwCobs::encode(raw, rawLen, out, outMax, encodedLen)) {
        return false;
    }

    packetLen = encodedLen;
    return encodedLen > 0;
}

bool GwSpiBus::waitIrqLevel(int irq, int level, uint32_t deadline)
{
    if (irq < 0) {
        return true;
    }
    while (digitalRead(irq) != level) {
        if ((int32_t)(millis() - deadline) >= 0) {
            return false;
        }
        delayMicroseconds(10);
    }
    return true;
}

bool GwSpiBus::transferFrame(SPIClass& spi, const uint8_t* txFrame, uint8_t* rxFrame, size_t len)
{
    if (!txFrame || !rxFrame || len != GwSpiBus::MaxBurstLen) {
        return false;
    }

    for (size_t index = 0; index < len; ++index) {
        rxFrame[index] = spi.transfer(txFrame[index]);
    }
    return true;
}

bool GwSpiBus::extractPacket(const uint8_t* frame, size_t frameLen, uint8_t* rx, size_t rxMax, size_t& rxLen)
{
    rxLen = 0;
    if (!frame || !rx || frameLen != GwSpiBus::MaxBurstLen || frameLen < 3) {
        return false;
    }

    if (frame[0] == 0x00) {
        return false;
    }

    size_t cobsLen = 0;
    while (cobsLen < frameLen && frame[cobsLen] != 0x00) {
        ++cobsLen;
    }

    if (cobsLen == 0) {
        return false;
    }

    return SdgwCobs::decode(frame, cobsLen, rx, rxMax, rxLen) && rxLen >= 3;
}
