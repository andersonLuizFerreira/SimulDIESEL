#pragma once
#include "GwBus.h"
#include <Wire.h>

class GwI2cBus : public IGwBus {
public:
    explicit GwI2cBus(TwoWire& w = Wire) : _w(w), _ok(true) {}

    void begin(uint32_t hz = 400000) {
        _w.begin();
        _w.setClock(hz);
    }

    bool pingTlv(uint8_t i2cAddr, uint8_t& outBoardId);

    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override;

    bool isOk() const override { return _ok; }

    bool transactTlv(uint8_t addr,
        const uint8_t* tx, size_t txLen,
        uint8_t* rx, size_t rxMax, size_t& rxLen,
        uint16_t timeoutMs);

private:
    TwoWire& _w;
    bool _ok;

    bool writeAll(uint8_t i2cAddr, const uint8_t* data, size_t len);
    bool readAll(uint8_t i2cAddr, uint8_t* data, size_t len);
};