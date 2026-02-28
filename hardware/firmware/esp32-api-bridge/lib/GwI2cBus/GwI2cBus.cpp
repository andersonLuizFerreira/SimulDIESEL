#include <Arduino.h>
#include "GwI2cBus.h"
#include "GwDeviceTable.h"
#include <Wire.h>

bool GwI2cBus::writeAll(uint8_t i2cAddr, const uint8_t* data, size_t len) {
    _w.beginTransmission(i2cAddr);
    size_t w = _w.write(data, len);
    uint8_t st = _w.endTransmission(true);
    if (w != len || st != 0) { _ok = false; return false; }
    return true;
}

bool GwI2cBus::readAll(uint8_t i2cAddr, uint8_t* data, size_t len) {
    size_t got = _w.requestFrom((int)i2cAddr, (int)len, (int)true);
    if (got != len) { _ok = false; return false; }
    for (size_t i = 0; i < len; i++) {
        int v = _w.read();
        if (v < 0) { _ok = false; return false; }
        data[i] = (uint8_t)v;
    }
    return true;
}

bool GwI2cBus::transact(uint8_t addr,
                        const uint8_t* tx, size_t txLen,
                        uint8_t* rx, size_t rxMax, size_t& rxLen,
                        uint16_t timeoutMs)
{
    rxLen = 0;
    GwDeviceEntry e{};
    if (!GwDeviceTable::get(addr, e)) return false;
    if (e.bus != GW_BUS_I2C) return false;

    const uint8_t i2cAddr = e.i2cAddr;

    // WRITE: frame completo
    if (!writeAll(i2cAddr, tx, txLen)) return false;

    // READ: primeiro 2 bytes (CMD, LEN) + depois (LEN + CRC)
    // Simples e determinístico
    uint32_t t0 = millis();
    while (true) {
        // tenta ler header mínimo
        uint8_t hdr[2] = {0,0};
        if (readAll(i2cAddr, hdr, 2)) {
            uint8_t len = hdr[1];
            size_t total = (size_t)2 + (size_t)len + (size_t)1;
            if (total <= rxMax) {
                rx[0] = hdr[0];
                rx[1] = hdr[1];
                if (len > 0) {
                    if (!readAll(i2cAddr, &rx[2], (size_t)len + 1)) return false;
                } else {
                    if (!readAll(i2cAddr, &rx[2], 1)) return false;
                }
                rxLen = total;
                return true;
            }
            return false;
        }

        if ((uint32_t)(millis() - t0) > timeoutMs) return false;
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

bool GwI2cBus::transactTlv(uint8_t addr,
    const uint8_t* tx, size_t txLen,
    uint8_t* rx, size_t rxMax, size_t& rxLen,
    uint16_t /*timeoutMs*/)
{
rxLen = 0;

GwDeviceEntry e{};
if (!GwDeviceTable::get(addr, e)) return false;
if (e.bus != GW_BUS_I2C) return false;

const uint8_t i2cAddr = e.i2cAddr;

// WRITE: manda o frame TLV+CRC para o GSA
if (!writeAll(i2cAddr, tx, txLen)) return false;

// READ: lê tudo de uma vez (evita o problema do _txLen zerar no slave)
if (rxMax < 3) return false;

size_t got = _w.requestFrom((int)i2cAddr, (int)rxMax, (int)true);
if (got < 2) return false;

for (size_t i = 0; i < got; i++) {
int v = _w.read();
if (v < 0) return false;
rx[i] = (uint8_t)v;
}

// “no data” padrão do Transport do GSA
if (rx[0] == 0xFF && rx[1] == 0x00) {
rxLen = 2;
return true;
}

// Enquadra no TLV: total esperado = 2 + L + 1(CRC)
uint8_t L = rx[1];
size_t total = (size_t)2 + (size_t)L + (size_t)1;
if (got < total) return false;

rxLen = total; // corta no tamanho real
return true;
}