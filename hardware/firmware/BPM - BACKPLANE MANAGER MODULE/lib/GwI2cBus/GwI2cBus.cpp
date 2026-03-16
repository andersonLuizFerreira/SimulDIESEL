#include <Arduino.h>
#include "GwI2cBus.h"
#include "GwDeviceTable.h"
#include <Wire.h>

static const size_t kGwI2cResponseMax = 32;

bool GwI2cBus::writeAll(uint8_t i2cAddr, const uint8_t* data, size_t len) {
    _w.beginTransmission(i2cAddr);
    size_t w = _w.write(data, len);
    uint8_t st = _w.endTransmission(true);
    if (w != len || st != 0) { _ok = false; return false; }
    return true;
}

bool GwI2cBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;
    if (!tx || txLen == 0 || !rx || rxMax < 3) return false;

    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return false;
    if (e.bus != GW_BUS_I2C) return false;

    const uint8_t i2cAddr = e.i2cAddr;

    // O payload interno da baby board ja chega pronto ao gateway.
    if (!writeAll(i2cAddr, tx, txLen)) return false;

    uint32_t t0 = millis();
    const size_t requestLen = (rxMax < kGwI2cResponseMax) ? rxMax : kGwI2cResponseMax;

    while (true) {
        size_t got = _w.requestFrom((int)i2cAddr, (int)requestLen, (int)true);
        if (got >= 2) {
            for (size_t i = 0; i < got; i++) {
                int v = _w.read();
                if (v < 0) {
                    _ok = false;
                    return false;
                }
                rx[i] = (uint8_t)v;
            }

            // "No data" do GSA: aguarda ate o slave publicar a resposta real.
            if (rx[0] == 0xFF && rx[1] == 0x00) {
                if ((uint32_t)(millis() - t0) > timeoutMs) {
                    _ok = false;
                    return false;
                }
                delay(1);
                continue;
            }

            const size_t total = (size_t)2 + (size_t)rx[1] + (size_t)1;
            if (total > rxMax || got < total) {
                _ok = false;
                return false;
            }

            rxLen = total;
            _ok = true;
            return true;
        }

        if ((uint32_t)(millis() - t0) > timeoutMs) {
            _ok = false;
            return false;
        }
        delay(1);
    }
}

bool GwI2cBus::pingTlv(uint8_t i2cAddr, uint8_t& outBoardId)
{
    // TX: [T=0x00][L=0x00]
    Wire.beginTransmission(i2cAddr);
    Wire.write((uint8_t)0x00);
    Wire.write((uint8_t)0x00);
    if (Wire.endTransmission(true) != 0) {
        return false;
    }

    // RX: [0x00][0x01][ID]
    const uint8_t want = 3;
    uint8_t got = Wire.requestFrom((int)i2cAddr, (int)want, (int)true);
    if (got != want) return false;

    uint8_t t = Wire.read();
    uint8_t l = Wire.read();
    uint8_t id = Wire.read();

    if (t != 0x00 || l != 0x01) return false;

    outBoardId = id;
    return true;
}
