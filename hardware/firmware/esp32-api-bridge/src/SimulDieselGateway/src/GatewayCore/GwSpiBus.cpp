#include <Arduino.h>
#include "GwSpiBus.h"
#include "GwDeviceTable.h"

void GwSpiBus::csLow(int cs){ digitalWrite(cs, LOW); }
void GwSpiBus::csHigh(int cs){ digitalWrite(cs, HIGH); }

bool GwSpiBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return false;
    if (e.bus != GW_BUS_SPI) return false;
    if (e.spiCsPin < 0) return false;

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

    // Espera por IRQ (se houver)
    uint32_t t0 = millis();
    if (e.spiIrqPin >= 0) {
        pinMode(e.spiIrqPin, INPUT_PULLUP);
        while (digitalRead(e.spiIrqPin) == HIGH) {
            if ((uint32_t)(millis() - t0) > timeoutMs) return false;
            delay(1);
        }
    } else {
        delay(1);
    }

    // FASE 2: READ header (CMD, LEN)
    uint8_t hdr[2] = {0,0};

    _spi.beginTransaction(st);
    csLow(cs);
    hdr[0] = _spi.transfer(0x00);
    hdr[1] = _spi.transfer(0x00);
    csHigh(cs);
    _spi.endTransaction();

    const uint8_t len = hdr[1];
    const size_t total = (size_t)2 + (size_t)len + (size_t)1;
    if (total > rxMax) return false;

    rx[0] = hdr[0];
    rx[1] = hdr[1];

    // READ payload + CRC
    _spi.beginTransaction(st);
    csLow(cs);
    for (size_t i = 0; i < (size_t)len + 1; i++) {
        rx[2 + i] = _spi.transfer(0x00);
    }
    csHigh(cs);
    _spi.endTransaction();

    rxLen = total;
    return true;
}