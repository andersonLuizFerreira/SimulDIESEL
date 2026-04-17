#include <Arduino.h>
#include "GwSpiBus.h"
#include "GwDeviceTable.h"
#include "GwTlv.h"

namespace {
constexpr uint8_t kCurrentUceMinPayloadLen = 1;
constexpr uint8_t kCurrentUceMaxPayloadLen = (uint8_t)(GwSpiDiagnostic::kMaxFrameBytes - 3);
}

void GwSpiBus::csLow(int cs){ digitalWrite(cs, LOW); }
void GwSpiBus::csHigh(int cs){ digitalWrite(cs, HIGH); }

void GwSpiBus::resetSnapshot(uint8_t addr)
{
    _lastSnapshot = GwSpiDiagnostic::Snapshot{};
    _lastSnapshot.valid = true;
    _lastSnapshot.addr = addr;
    _lastSnapshot.layer = GwSpiDiagnostic::LayerGwSpiBus;
}

void GwSpiBus::captureBytes(uint8_t* dest, const uint8_t* src, size_t len, uint8_t& lenOut)
{
    lenOut = 0;
    if (!dest || !src) return;

    const size_t count = (len > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : len;

    for (size_t index = 0; index < count; index++) {
        dest[index] = src[index];
    }

    lenOut = (uint8_t)count;
}

void GwSpiBus::begin(uint32_t hz, int8_t sckPin, int8_t misoPin, int8_t mosiPin)
{
    _hz = hz;
    _sckPin = sckPin;
    _misoPin = misoPin;
    _mosiPin = mosiPin;

    // Inicializa o barramento SPI com pinagem explicita para evitar o
    // mapeamento padrao do ESP32, que conflita com o reset global em GPIO23.
    _spi.begin(_sckPin, _misoPin, _mosiPin, -1);
}

bool GwSpiBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;
    _lastError = TransactError::None;
    resetSnapshot(addr);
    captureBytes(_lastSnapshot.tx, tx, txLen, _lastSnapshot.txLen);
    _lastSnapshot.phase = GwSpiDiagnostic::PhaseWrite;

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) {
        _lastError = TransactError::AddrUnmapped;
        return false;
    }
    if (e.bus != GW_BUS_SPI) {
        _lastError = TransactError::WrongBus;
        return false;
    }
    if (e.spiCsPin < 0) {
        _lastError = TransactError::MissingCs;
        return false;
    }

    const int cs = e.spiCsPin;
    pinMode(cs, OUTPUT);
    csHigh(cs);

    SPISettings st(_hz, SPI_MSBFIRST, SPI_MODE0);

    // FASE 1: WRITE
    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t i = 0; i < txLen; i++) _spi.transfer(tx[i]);
    csHigh(cs);
    _spi.endTransaction();

    // A UCE sinaliza resposta pronta via IRQ, mas a leitura continua
    // síncrona e curta em dois bursts SPI para preservar o contrato atual.
    uint32_t t0 = millis();
    if (e.spiUseIrq && e.spiIrqPin >= 0) {
        _lastSnapshot.phase = GwSpiDiagnostic::PhaseWaitResponseReady;
        pinMode(e.spiIrqPin, INPUT_PULLUP);
        while (digitalRead(e.spiIrqPin) == HIGH) {
            if ((uint32_t)(millis() - t0) > timeoutMs) {
                _lastSnapshot.cause = GwSpiDiagnostic::CauseTimeoutWaitingIrq;
                _lastError = TransactError::TimeoutWaitingIrq;
                return false;
            }
            delay(1);
        }
    } else {
        delay(1);
    }

    // FASE 2: READ header (CMD, LEN)
    uint8_t hdr[2] = {0,0};

    _lastSnapshot.phase = GwSpiDiagnostic::PhaseReadHeader;
    _spi.beginTransaction(st);
    csLow(cs);
    hdr[0] = _spi.transfer(0x00);
    hdr[1] = _spi.transfer(0x00);
    csHigh(cs);
    _spi.endTransaction();

    _lastSnapshot.rx[0] = hdr[0];
    _lastSnapshot.rx[1] = hdr[1];
    _lastSnapshot.rxLen = 2;
    _lastSnapshot.receivedLen = 2;

    if (hdr[0] == 0x00) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseFirstByteMisaligned;
        _lastError = TransactError::HeaderInvalid;
        return false;
    }

    const uint8_t len = hdr[1];
    if (len < kCurrentUceMinPayloadLen) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::HeaderInvalid;
        return false;
    }
    if (len > kCurrentUceMaxPayloadLen) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::LengthInvalid;
        return false;
    }

    const size_t total = (size_t)2 + (size_t)len + (size_t)1;
    _lastSnapshot.expectedLen = (uint8_t)total;
    if (total > rxMax) {
        _lastSnapshot.cause = GwSpiDiagnostic::CauseLengthMismatch;
        _lastError = TransactError::LengthInvalid;
        return false;
    }

    rx[0] = hdr[0];
    rx[1] = hdr[1];

    // READ payload + CRC
    _lastSnapshot.phase = GwSpiDiagnostic::PhaseReadPayload;
    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t i = 0; i < (size_t)len + 1; i++) {
        rx[2 + i] = _spi.transfer(0x00);
        if ((size_t)(2 + i) < GwSpiDiagnostic::kMaxFrameBytes) {
            _lastSnapshot.rx[2 + i] = rx[2 + i];
        }
    }
    csHigh(cs);
    _spi.endTransaction();

    rxLen = total;
    _lastSnapshot.rxLen = (uint8_t)((total > GwSpiDiagnostic::kMaxFrameBytes)
        ? GwSpiDiagnostic::kMaxFrameBytes
        : total);
    _lastSnapshot.receivedLen = (uint8_t)total;
    if (total >= 1) _lastSnapshot.crcReceived = rx[total - 1];
    if (total >= 3) _lastSnapshot.crcCalculated = GwTlv::crc8(rx, total - 1);

    return true;
}
