#pragma once

#include "GwBus.h"

class NullGwBus : public IGwBus {
public:
    bool transact(uint8_t addr,
                  const uint8_t* tx, size_t txLen,
                  uint8_t* rx, size_t rxMax, size_t& rxLen,
                  uint16_t timeoutMs) override
    {
        (void)addr;
        (void)tx;
        (void)txLen;
        (void)rx;
        (void)rxMax;
        (void)timeoutMs;
        rxLen = 0;
        return false;
    }

    bool isOk() const override { return false; }
};
